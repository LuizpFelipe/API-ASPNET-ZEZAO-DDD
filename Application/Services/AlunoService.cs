using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _alunoRepository;

    public AlunoService(IAlunoRepository alunoRepository)
    {
        _alunoRepository = alunoRepository;
    }

    public async Task<AlunoResponseDTO> CriarAsync(CriarAlunoRequestDTO dto)
    {
        var existe = await _alunoRepository.ExisteComMesmoNomeEDataNascimentoAsync(dto.NomeCompleto, dto.DataNascimento);
        if (existe)
        {
            return new AlunoResponseDTO { Status = false, Message = "Aluno já cadastrado no sistema." };
        }

        var aluno = new Aluno(dto.NomeCompleto, dto.DataNascimento, dto.NivelTecnico,
                              dto.FotoUrl, dto.ResponsavelNome, dto.Telefone,
                              dto.Endereco, dto.TurmaId);

        await _alunoRepository.AdicionarAsync(aluno);

        return new AlunoResponseDTO { Status = true, Message = "Aluno cadastrado com sucesso.", Data = new { aluno.Id, Categoria = CalcularCategoria(aluno.DataNascimento) } };
    }

    public async Task<AlunoResponseDTO> AtualizarAsync(Guid id, AtualizarAlunoRequestDTO dto)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id);
        if (aluno == null) return new AlunoResponseDTO { Status = false, Message = "Aluno não encontrado." };

        var existe = await _alunoRepository.ExisteComMesmoNomeEDataNascimentoAsync(dto.NomeCompleto, dto.DataNascimento, id);
        if (existe) return new AlunoResponseDTO { Status = false, Message = "Outro aluno já possui este nome e data de nascimento." };

        aluno.AtualizarDados(dto.NomeCompleto, dto.DataNascimento, dto.NivelTecnico, dto.FotoUrl, dto.ResponsavelNome, dto.Telefone, dto.Endereco, dto.TurmaId);
        await _alunoRepository.AtualizarAsync(aluno);

        return new AlunoResponseDTO { Status = true, Message = "Aluno atualizado com sucesso." };
    }

    public async Task<AlunoResponseDTO> RemoverAsync(Guid id)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id);
        if (aluno == null) return new AlunoResponseDTO { Status = false, Message = "Aluno não encontrado." };

        aluno.Desativar(); 
        await _alunoRepository.AtualizarAsync(aluno);

        return new AlunoResponseDTO { Status = true, Message = "Aluno inativado com sucesso." };
    }

    public async Task<AlunoResponseDTO> ListarAsync()
    {
        var alunos = await _alunoRepository.ObterTodosAsync();
        return new AlunoResponseDTO { Status = true, Data = alunos };
    }

    public async Task<AlunoResponseDTO> ObterPorIdAsync(Guid id)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id);
        if (aluno == null) return new AlunoResponseDTO { Status = false, Message = "Aluno não encontrado." };

        return new AlunoResponseDTO { Status = true, Data = aluno };
    }

    private string CalcularCategoria(DateOnly dataNascimento)
    {
        int idade = DateTime.Now.Year - dataNascimento.Year;
        if (idade <= 11) return "Sub-11";
        if (idade == 12) return "Sub-12";
        if (idade == 13) return "Sub-13";
        return "Sub-14";
    }
}