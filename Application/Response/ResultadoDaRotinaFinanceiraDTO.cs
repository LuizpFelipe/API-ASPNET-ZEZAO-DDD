namespace Application.Response
{
    public class ResultadoDaRotinaFinanceiraDTO
    {
        public int AlunosMarcadosComoInadimplentes { get; set; }
        public int AlunosRegularizados { get; set; }
        public int AvisosPreviosEnviados { get; set; }
        public int AvisosDeAtrasoEnviados { get; set; }
        public int FalhasDeEnvio { get; set; }
    }
}
