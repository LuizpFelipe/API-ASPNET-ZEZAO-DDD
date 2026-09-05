using Domain.Enums;

namespace Domain.Entities;

public class Aluno
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; }
    public DateOnly DataNascimento { get; private set; }
    public NivelTecnico NivelTecnico { get; private set; }
    public string? FotoUrl { get; private set; }
    public string ResponsavelNome { get; private set; }
    public string Telefone { get; private set; }
    public string Endereco { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public StatusAluno Status { get; private set; }
    public Guid TurmaId { get; private set; }

    public Aluno(string nomeCompleto, DateOnly dataNascimento, NivelTecnico nivelTecnico,
                 string? fotoUrl, string responsavelNome, string telefone,
                 string endereco, Guid turmaId)
    {
        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto;
        DataNascimento = dataNascimento;
        NivelTecnico = nivelTecnico;
        FotoUrl = fotoUrl;
        ResponsavelNome = responsavelNome;
        Telefone = telefone;
        Endereco = endereco;
        DataCadastro = DateTime.UtcNow;
        Status = StatusAluno.Ativo;
        TurmaId = turmaId;
    }

    public void AtualizarDados(string nomeCompleto, DateOnly dataNascimento, NivelTecnico nivelTecnico,
                               string? fotoUrl, string responsavelNome, string telefone, string endereco, Guid turmaId)
    {
        NomeCompleto = nomeCompleto;
        DataNascimento = dataNascimento;
        NivelTecnico = nivelTecnico;
        if (!string.IsNullOrEmpty(fotoUrl)) FotoUrl = fotoUrl;
        ResponsavelNome = responsavelNome;
        Telefone = telefone;
        Endereco = endereco;
        TurmaId = turmaId;
    }

    public void Desativar()
    {
        Status = StatusAluno.Inativo;
    }
}