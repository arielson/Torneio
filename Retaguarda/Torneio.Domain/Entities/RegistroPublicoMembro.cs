using Torneio.Domain.Enums;

namespace Torneio.Domain.Entities;

public class RegistroPublicoMembro
{
    public Guid Id { get; private set; }
    public Guid TorneioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Celular { get; private set; } = null!;
    public string CelularNormalizado { get; private set; } = null!;
    public string? TamanhoCamisa { get; private set; }
    public StatusRegistroPublicoMembro Status { get; private set; }
    public int QuantidadeEnvios { get; private set; }
    public int TentativasValidacao { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime UltimoEnvioEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public DateTime? ConfirmadoEm { get; private set; }
    public Guid? MembroId { get; private set; }

    private RegistroPublicoMembro() { }

    public static RegistroPublicoMembro Criar(
        Guid torneioId,
        string nome,
        string celular,
        string celularNormalizado,
        string? tamanhoCamisa,
        DateTime criadoEm,
        DateTime expiraEm)
    {
        return new RegistroPublicoMembro
        {
            Id = Guid.NewGuid(),
            TorneioId = torneioId,
            Nome = nome.Trim(),
            Celular = celular.Trim(),
            CelularNormalizado = celularNormalizado.Trim(),
            TamanhoCamisa = string.IsNullOrWhiteSpace(tamanhoCamisa) ? null : tamanhoCamisa.Trim(),
            Status = StatusRegistroPublicoMembro.Pendente,
            QuantidadeEnvios = 1,
            TentativasValidacao = 0,
            CriadoEm = criadoEm,
            UltimoEnvioEm = criadoEm,
            ExpiraEm = expiraEm,
        };
    }

    public void AtualizarDados(string nome, string celular, string celularNormalizado, string? tamanhoCamisa)
    {
        Nome = nome.Trim();
        Celular = celular.Trim();
        CelularNormalizado = celularNormalizado.Trim();
        TamanhoCamisa = string.IsNullOrWhiteSpace(tamanhoCamisa) ? null : tamanhoCamisa.Trim();
    }

    public void RegistrarReenvio(DateTime enviadoEm, DateTime expiraEm)
    {
        Status = StatusRegistroPublicoMembro.Pendente;
        UltimoEnvioEm = enviadoEm;
        ExpiraEm = expiraEm;
        QuantidadeEnvios++;
        TentativasValidacao = 0;
        ConfirmadoEm = null;
        MembroId = null;
    }

    public void RegistrarTentativaValidacao()
    {
        TentativasValidacao++;
    }

    public void Confirmar(Guid membroId, DateTime confirmadoEm)
    {
        Status = StatusRegistroPublicoMembro.Confirmado;
        ConfirmadoEm = confirmadoEm;
        MembroId = membroId;
    }

    public void Expirar()
    {
        if (Status == StatusRegistroPublicoMembro.Pendente)
            Status = StatusRegistroPublicoMembro.Expirado;
    }

    public void Cancelar()
    {
        Status = StatusRegistroPublicoMembro.Cancelado;
    }
}
