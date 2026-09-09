namespace Domain.Interfaces;

public interface IArmazenamentoArquivoService
{
    Task<string> SalvarAsync(Stream conteudo, string nomeArquivo);
}