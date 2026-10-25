namespace Domain.Entities;

public class Reposicao
{
    public Guid Id { get; private set; }
    public Guid AlunoId { get; private set; }
    public Guid TurmaOrigemId { get; private set; }
    public Guid TurmaDestinoId { get; private set; }
    public DateOnly DataOriginal { get; private set; }
    public DateOnly DataReposicao { get; private set; }
    public DateTime DataRegistro { get; private set; }

    protected Reposicao() { }

    public Reposicao(Guid alunoId, Guid turmaOrigemId, Guid turmaDestinoId, DateOnly dataOriginal, DateOnly dataReposicao)
    {
        Id = Guid.NewGuid();
        AlunoId = alunoId;
        TurmaOrigemId = turmaOrigemId;
        TurmaDestinoId = turmaDestinoId;
        DataOriginal = dataOriginal;
        DataReposicao = dataReposicao;
        DataRegistro = DateTime.UtcNow;
    }
}
