using Domain.Enums;

namespace Domain.Entities;

public class Contrato
{
    public Guid Id { get; private set; }
    public Guid AlunoId { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public decimal ValorParcela { get; private set; }
    public int NumParcelas { get; private set; }
    public StatusContrato Status { get; private set; }
    public string? PdfPath { get; private set; }

    public Contrato(Guid alunoId, DateOnly dataInicio, DateOnly dataFim, decimal valorParcela, int numParcelas)
    {
        Id = Guid.NewGuid();
        AlunoId = alunoId;
        DataInicio = dataInicio;
        DataFim = dataFim;
        ValorParcela = valorParcela;
        NumParcelas = numParcelas;
        Status = StatusContrato.Ativo;
    }

    public void Cancelar()
    {
        Status = StatusContrato.Cancelado;
    }
}
