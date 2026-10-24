namespace Application.Request;

public class RegistrarReposicaoRequestDTO
{
    public Guid AlunoId { get; set; }
    public Guid TurmaOrigemId { get; set; }
    public Guid TurmaDestinoId { get; set; }
    public DateOnly DataOriginal { get; set; }
    public DateOnly DataReposicao { get; set; }
}
