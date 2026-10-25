namespace Application.Response;

public class AlunoResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public bool Status { get; set; }
    public object? Data { get; set; }
}