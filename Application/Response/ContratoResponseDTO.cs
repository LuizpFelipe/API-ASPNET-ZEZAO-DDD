using Domain.Enums;

namespace Application.Response;

public class ContratoResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public bool Status { get; set; }
    public object? Data { get; set; }
}

public class ContratoDataDTO
{
    public Guid Id { get; set; }
    public Guid AlunoId { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public decimal ValorParcela { get; set; }
    public int NumParcelas { get; set; }
    public StatusContrato Status { get; set; }
    public string? PdfPath { get; set; }
}
