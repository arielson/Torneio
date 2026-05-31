namespace Torneio.Asaas;

public class CalculadoraTaxaAsaas
{
    private readonly AsaasTaxasOptions _taxas;

    public CalculadoraTaxaAsaas(AsaasTaxasOptions taxas)
    {
        _taxas = taxas;
    }

    public decimal CalcularTaxaPix(decimal valor) => _taxas.Pix;

    public decimal CalcularTaxaCartao(decimal valor, bool usarPromocao = false)
    {
        var percentual = usarPromocao && _taxas.PromocaoAtiva
            ? _taxas.CartaoPercentualPromocional
            : _taxas.CartaoPercentual;

        return _taxas.CartaoFixo + Math.Round(valor * percentual / 100m, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalcularValorLiquidoPix(decimal valor) => valor - CalcularTaxaPix(valor);

    public decimal CalcularValorLiquidoCartao(decimal valor, bool usarPromocao = false)
        => valor - CalcularTaxaCartao(valor, usarPromocao);
}
