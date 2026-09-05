using Domain.Interfaces;

namespace Infrastructure.Storage;

public class ArmazenamentoLocalService : IArmazenamentoArquivoService
{
    public async Task<string> SalvarAsync(Stream conteudo, string nomeArquivo)
    {
        var pastaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "alunos");

        if (!Directory.Exists(pastaDestino))
            Directory.CreateDirectory(pastaDestino);

        var caminhoCompleto = Path.Combine(pastaDestino, nomeArquivo);

        using (var streamLocal = new FileStream(caminhoCompleto, FileMode.Create))
        {
            await conteudo.CopyToAsync(streamLocal);
        }

        return $"/uploads/alunos/{nomeArquivo}"; 
    }
}