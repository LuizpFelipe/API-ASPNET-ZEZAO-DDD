using Domain.Enums;

namespace Domain.Entities
{
    public class Parcela
    {
        public Guid Id { get; private set; }
        public Guid ContratoId { get; private set; }
        public int Numero { get; private set; }
        public DateOnly DataVencimento { get; private set; }
        public decimal Valor { get; private set; }
        public StatusParcela Status { get; private set; }
        public DateOnly? DataPagamento { get; private set; }

        public Parcela(Guid contratoId, int numero, DateOnly dataVencimento, decimal valor)
        {
            Id = Guid.NewGuid();
            ContratoId = contratoId;
            Numero = numero;
            DataVencimento = dataVencimento;
            Valor = valor;
            Status = StatusParcela.Pendente;
        }

        public void MarcarComoPaga(DateOnly dataPagamento)
        {
            Status = StatusParcela.Pago;
            DataPagamento = dataPagamento;
        }
    }
}