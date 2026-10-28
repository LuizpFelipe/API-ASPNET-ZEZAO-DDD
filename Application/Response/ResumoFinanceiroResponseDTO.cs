namespace Application.Response
{
    public class ResumoFinanceiroResponseDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public ResumoFinanceiroData? Data { get; set; }
    }

    public class ResumoFinanceiroData
    {
        public decimal RecebidoMes { get; set; }
        public decimal Pendente { get; set; }
        public decimal Inadimplencia { get; set; }
    }
}