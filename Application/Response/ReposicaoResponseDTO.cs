namespace Application.Response;

public class ReposicaoResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public bool Status { get; set; }
    public object? Data { get; set; }
}

public class ReposicaoDataDTO
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public Guid TurmaOrigemId { get; set; }
    public Guid TurmaDestinoId { get; set; }
    public DateOnly DataOriginal { get; set; }
    public DateOnly DataReposicao { get; set; }
    public DateTime DataRegistro { get; set; }
}
