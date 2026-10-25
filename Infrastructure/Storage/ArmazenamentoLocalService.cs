using Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Storage;

public class ArmazenamentoLocalService : IArmazenamentoArquivoService
{
    private readonly IWebHostEnvironment _env;

    public ArmazenamentoLocalService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SalvarAsync(Stream conteudo, string nomeArquivo)
    {
        var webRootPath = _env.WebRootPath;

        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        }

        var pastaDestino = Path.Combine(webRootPath, "uploads", "alunos");

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