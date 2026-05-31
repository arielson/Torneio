using Microsoft.Extensions.Configuration;
using Torneio.Application.DTOs.Asaas;
using Torneio.Application.Services.Interfaces;
using Torneio.Asaas;
using Torneio.Asaas.Enums;
using Torneio.Asaas.Models.Customers;
using Torneio.Asaas.Models.Payments;
using Torneio.Domain.Entities;
using Torneio.Domain.Enums;
using Torneio.Domain.Interfaces.Repositories;

namespace Torneio.Infrastructure.Services;

public class CobrancaAsaasServico : ICobrancaAsaasServico
{
    private readonly ICobrancaAsaasRepositorio _repositorio;
    private readonly IConfiguracaoAsaasRepositorio _configRepositorio;
    private readonly IParcelaTorneioRepositorio _parcelaRepositorio;
    private readonly IMembroRepositorio _membroRepositorio;
    private readonly IAsaasClientFactory _clientFactory;
    private readonly CalculadoraTaxaAsaas _calculadoraTaxa;
    private readonly CalculadoraPrevisaoCredito _calculadoraPrevisao;
    private readonly string _nomePlataforma;

    public CobrancaAsaasServico(
        ICobrancaAsaasRepositorio repositorio,
        IConfiguracaoAsaasRepositorio configRepositorio,
        IParcelaTorneioRepositorio parcelaRepositorio,
        IMembroRepositorio membroRepositorio,
        IAsaasClientFactory clientFactory,
        CalculadoraTaxaAsaas calculadoraTaxa,
        CalculadoraPrevisaoCredito calculadoraPrevisao,
        IConfiguration configuration)
    {
        _repositorio = repositorio;
        _configRepositorio = configRepositorio;
        _parcelaRepositorio = parcelaRepositorio;
        _membroRepositorio = membroRepositorio;
        _clientFactory = clientFactory;
        _calculadoraTaxa = calculadoraTaxa;
        _calculadoraPrevisao = calculadoraPrevisao;
        _nomePlataforma = configuration["Plataforma:NomePlataforma"] ?? "Torvia";
    }

    public async Task<CobrancaAsaasDto> GerarCobranca(GerarCobrancaDto dto)
    {
        var existente = await _repositorio.ObterPorParcelaId(dto.ParcelaTorneioId);
        if (existente is not null &&
            existente.Status is not (StatusCobrancaAsaas.Excluido
                or StatusCobrancaAsaas.Estornado
                or StatusCobrancaAsaas.RecusadoCartao))
            throw new InvalidOperationException("Já existe uma cobrança Asaas ativa para esta parcela.");

        var config = await _configRepositorio.ObterPorTorneioId(dto.TorneioId);
        if (config is null || config.StatusChave != StatusChaveAsaas.Ativa || string.IsNullOrEmpty(config.ChaveApiAsaas))
            throw new InvalidOperationException("Integração Asaas não está ativa para este torneio.");

        ValidarFormaPagamento(dto.FormaPagamento, config);

        var parcela = await _parcelaRepositorio.ObterPorId(dto.ParcelaTorneioId)
            ?? throw new InvalidOperationException("Parcela não encontrada.");

        var membro = await _membroRepositorio.ObterPorId(parcela.MembroId)
            ?? throw new InvalidOperationException("Membro não encontrado.");

        // Se o membro não tem CPF e foi informado um via formulário, salva antes de criar o customer
        if (string.IsNullOrWhiteSpace(membro.Cpf) && !string.IsNullOrWhiteSpace(dto.CpfOverride))
        {
            var digits = new string(dto.CpfOverride.Where(char.IsDigit).ToArray());
            if (digits.Length != 11)
                throw new InvalidOperationException("CPF informado inválido. Informe os 11 dígitos.");
            membro.AtualizarCpf(dto.CpfOverride);
            await _membroRepositorio.Atualizar(membro);
        }

        if (string.IsNullOrWhiteSpace(membro.Cpf))
            throw new InvalidOperationException(
                "O Asaas exige CPF para emitir a cobrança. Informe o CPF do membro.");

        var client = _clientFactory.Criar(config.ChaveApiAsaas);

        var customerId = await ResolverClienteAsaas(client, membro, dto.TorneioId);

        var taxa = dto.FormaPagamento == FormaPagamentoAsaas.Pix
            ? _calculadoraTaxa.CalcularTaxaPix(parcela.Valor)
            : _calculadoraTaxa.CalcularTaxaCartao(parcela.Valor);

        var billingType = dto.FormaPagamento == FormaPagamentoAsaas.Pix ? BillingType.PIX : BillingType.CREDIT_CARD;
        var paymentRequest = new PaymentRequest
        {
            Customer = customerId,
            BillingType = billingType,
            Value = parcela.Valor + taxa,
            DueDate = parcela.Vencimento.ToString("yyyy-MM-dd"),
            Description = parcela.Descricao,
            ExternalReference = parcela.Id.ToString()
        };

        var payment = await client.Payments.CreateAsync(paymentRequest)
            ?? throw new InvalidOperationException("Falha ao criar cobrança no Asaas.");

        var previsaoCredito = ResolverPrevisaoCredito(dto.FormaPagamento, parcela.Vencimento, payment.EstimatedCreditDate);

        var cobranca = CobrancaAsaas.Criar(
            dto.TorneioId,
            parcela.MembroId,
            dto.ParcelaTorneioId,
            payment.Id,
            customerId,
            payment.InvoiceUrl,
            parcela.Valor,
            parcela.Vencimento);

        cobranca.AtualizarStatus(
            StatusCobrancaAsaas.Pendente,
            dto.FormaPagamento,
            taxa,
            previsaoCredito);

        await _repositorio.Adicionar(cobranca);
        return Mapear(cobranca);
    }

    public async Task<CobrancaAsaasDto?> ObterPorParcelaId(Guid parcelaTorneioId)
    {
        var cobranca = await _repositorio.ObterPorParcelaId(parcelaTorneioId);
        return cobranca is null ? null : Mapear(cobranca);
    }

    public async Task<IEnumerable<CobrancaAsaasDto>> ListarPorMembro(Guid torneioId, Guid membroId)
    {
        var lista = await _repositorio.ListarPorMembro(torneioId, membroId);
        return lista.Select(Mapear);
    }

    public async Task<PixQrCodeDto> ObterQrCodePix(Guid parcelaTorneioId)
    {
        var cobranca = await _repositorio.ObterPorParcelaId(parcelaTorneioId)
            ?? throw new InvalidOperationException("Cobrança Asaas não encontrada para esta parcela.");

        if (cobranca.FormaPagamento != FormaPagamentoAsaas.Pix)
            throw new InvalidOperationException("Esta cobrança não é do tipo PIX.");

        var config = await _configRepositorio.ObterPorTorneioId(cobranca.TorneioId);
        if (config?.ChaveApiAsaas is null)
            throw new InvalidOperationException("Configuração Asaas não disponível.");

        var client = _clientFactory.Criar(config.ChaveApiAsaas);
        var qrCode = await client.Payments.GetPixQrCodeAsync(cobranca.AsaasPaymentId)
            ?? throw new InvalidOperationException("Falha ao obter QR Code PIX do Asaas.");

        return new PixQrCodeDto
        {
            EncodedImage = qrCode.EncodedImage,
            Payload = qrCode.Payload,
            ExpirationDate = qrCode.ExpirationDate
        };
    }

    public async Task CancelarCobranca(Guid parcelaTorneioId)
    {
        var cobranca = await _repositorio.ObterPorParcelaId(parcelaTorneioId);
        if (cobranca is null) return;

        var config = await _configRepositorio.ObterPorTorneioId(cobranca.TorneioId);
        if (config?.ChaveApiAsaas is not null)
        {
            try
            {
                var client = _clientFactory.Criar(config.ChaveApiAsaas);
                await client.Payments.DeleteAsync(cobranca.AsaasPaymentId);
            }
            catch
            {
                // Falha silenciosa na exclusão remota — marca como excluída localmente de qualquer forma
            }
        }

        cobranca.AtualizarStatus(StatusCobrancaAsaas.Excluido);
        await _repositorio.Atualizar(cobranca);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<string> ResolverClienteAsaas(AsaasClient client, Membro membro, Guid torneioId)
    {
        var cpf = NormalizarCpf(membro.Cpf);

        // Otimização: busca customerId em uma cobrança anterior do mesmo membro
        var cobrancasAnteriores = await _repositorio.ListarPorMembro(torneioId, membro.Id);
        var customerIdCached = cobrancasAnteriores
            .FirstOrDefault(c => !string.IsNullOrEmpty(c.AsaasCustomerId))
            ?.AsaasCustomerId;

        if (!string.IsNullOrEmpty(customerIdCached))
        {
            // Se agora há CPF, garante que o customer no Asaas também está atualizado
            if (!string.IsNullOrEmpty(cpf))
            {
                var atual = await client.Customers.GetByIdAsync(customerIdCached);
                if (atual is not null && string.IsNullOrEmpty(NormalizarCpf(atual.CpfCnpj)))
                    await AtualizarCustomerAsaas(client, customerIdCached, membro, cpf);
            }
            return customerIdCached;
        }

        // Busca no Asaas por externalReference
        var lista = await client.Customers.ListAsync(new CustomerListRequest
        {
            ExternalReference = membro.Id.ToString(),
            Limit = 1
        });

        if (lista?.Data?.FirstOrDefault() is { } existente)
        {
            // Atualiza CPF se o customer existente não tem
            if (!string.IsNullOrEmpty(cpf) && string.IsNullOrEmpty(NormalizarCpf(existente.CpfCnpj)))
                await AtualizarCustomerAsaas(client, existente.Id, membro, cpf);

            return existente.Id;
        }

        // Cria novo cliente
        var novo = await client.Customers.CreateAsync(new CustomerRequest
        {
            Name = NomeCliente(membro.Nome),
            CpfCnpj = cpf,
            MobilePhone = membro.Celular ?? string.Empty,
            ExternalReference = membro.Id.ToString(),
            NotificationDisabled = true
        }) ?? throw new InvalidOperationException("Falha ao criar cliente no Asaas.");

        return novo.Id;
    }

    private Task AtualizarCustomerAsaas(AsaasClient client, string customerId, Membro membro, string cpf) =>
        client.Customers.UpdateAsync(customerId, new CustomerRequest
        {
            Name = NomeCliente(membro.Nome),
            CpfCnpj = cpf,
            MobilePhone = membro.Celular ?? string.Empty,
            ExternalReference = membro.Id.ToString(),
            NotificationDisabled = true
        });

    private string NomeCliente(string nome) => $"[{_nomePlataforma}] {nome}";

    private static string NormalizarCpf(string? cpf) =>
        string.IsNullOrWhiteSpace(cpf)
            ? string.Empty
            : new string(cpf.Where(char.IsDigit).ToArray());

    private static void ValidarFormaPagamento(FormaPagamentoAsaas forma, ConfiguracaoAsaasTorneio config)
    {
        if (forma == FormaPagamentoAsaas.Pix && !config.AceitarPix)
            throw new InvalidOperationException("PIX não está habilitado para este torneio.");
        if (forma == FormaPagamentoAsaas.CartaoCredito && !config.AceitarCartaoCredito)
            throw new InvalidOperationException("Cartão de crédito não está habilitado para este torneio.");
    }

    private DateTime? ResolverPrevisaoCredito(FormaPagamentoAsaas forma, DateTime vencimento, string? estimatedCreditDate)
    {
        if (!string.IsNullOrEmpty(estimatedCreditDate) &&
            DateTime.TryParse(estimatedCreditDate, out var dataAsaas))
            return dataAsaas;

        return forma == FormaPagamentoAsaas.Pix
            ? _calculadoraPrevisao.CalcularPrevisaoPix(vencimento)
            : _calculadoraPrevisao.CalcularPrevisaoCartao(vencimento);
    }

    private static CobrancaAsaasDto Mapear(CobrancaAsaas c) =>
        new()
        {
            Id = c.Id,
            TorneioId = c.TorneioId,
            MembroId = c.MembroId,
            ParcelaTorneioId = c.ParcelaTorneioId,
            AsaasPaymentId = c.AsaasPaymentId,
            AsaasInvoiceUrl = c.AsaasInvoiceUrl,
            Status = c.Status.ToString(),
            FormaPagamento = c.FormaPagamento?.ToString(),
            ValorOriginal = c.ValorOriginal,
            TaxaAsaas = c.TaxaAsaas,
            Vencimento = c.Vencimento,
            DataPrevisaoCredito = c.DataPrevisaoCredito,
            DataCreditoEfetivo = c.DataCreditoEfetivo,
            CriadoEm = c.CriadoEm
        };
}
