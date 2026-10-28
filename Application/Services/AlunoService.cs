using Application.Interfaces;
using Application.Request;
using Application.Response;
using Application.Validacoes;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;


namespace Application.Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IContratoService _contratoService;
    private readonly IUnitOfWork _unitOfWork;

    public AlunoService(IAlunoRepository alunoRepository, ICategoriaRepository categoriaRepository,
                        ITurmaRepository turmaRepository,
                        IContratoService contratoService, IUnitOfWork unitOfWork)
    {
        _alunoRepository = alunoRepository;
        _categoriaRepository = categoriaRepository;
        _turmaRepository = turmaRepository;
        _contratoService = contratoService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AlunoResponseDTO> CriarAsync(CriarAlunoRequestDTO dto)
    {
        var mensagemDeDadosInvalidos = ValidarDadosDoAluno(dto.NomeCompleto, dto.DataNascimento, dto.NivelTecnico,
                                                           dto.ResponsavelNome, dto.Telefone, dto.Endereco);
        if (mensagemDeDadosInvalidos != null)
        {
            return new AlunoResponseDTO { Status = false, Message = mensagemDeDadosInvalidos };
        }

        if (dto.Posicao == null || !Enum.IsDefined(dto.Posicao.Value))
        {
            return new AlunoResponseDTO { Status = false, Message = "Informe a posição em que o aluno joga." };
        }

        var existe = await _alunoRepository.ExisteComMesmoNomeEDataNascimentoAsync(dto.NomeCompleto, dto.DataNascimento);
        var mensagemDoCpf = ValidarCpfDoResponsavel(dto.ResponsavelCpf, out var cpfDoResponsavel);

        if (mensagemDoCpf != null)
        {
            return new AlunoResponseDTO { Status = false, Message = mensagemDoCpf };
        }

        if (existe)
        {
            return new AlunoResponseDTO { Status = false, Message = "Aluno já cadastrado no sistema." };
        }

        var turma = await _turmaRepository.ObterPorIdAsync(dto.TurmaId);
        if (turma == null)
        {
            return new AlunoResponseDTO { Status = false, Message = "Turma não encontrada." };
        }

        int idade = CalcularIdade(dto.DataNascimento);

        var categoria = await _categoriaRepository.ObterPorIdadeAsync(idade);
        if (categoria == null)
        {
            return new AlunoResponseDTO { Status = false, Message = $"Nenhuma categoria cadastrada compatível com a idade de {idade} anos." };
        }

        if (turma.CategoriaId != categoria.Id)
        {
            return new AlunoResponseDTO { Status = false, Message = $"A turma selecionada não é da categoria do aluno ({categoria.Nome})." };
        }

        var aluno = new Aluno(dto.NomeCompleto, dto.DataNascimento, dto.NivelTecnico,
                              dto.FotoUrl, dto.ResponsavelNome, cpfDoResponsavel, dto.Telefone,
                              dto.Endereco, dto.TurmaId, categoria.Id, dto.Posicao);

        try
        {
            await _unitOfWork.ExecutarEmTransacaoAsync(async () =>
            {
                await _alunoRepository.AdicionarAsync(aluno);

                var resultadoDoContrato = await _contratoService.CriarAsync(aluno.Id);
                if (!resultadoDoContrato.Status)
                {
                    throw new ContratoNaoCriadoException(resultadoDoContrato.Message);
                }
            });
        }
        catch (ContratoNaoCriadoException excecao)
        {
            return new AlunoResponseDTO { Status = false, Message = $"Não foi possível cadastrar o aluno: {excecao.Message}" };
        }

        return new AlunoResponseDTO
        {
            Status = true,
            Message = "Aluno cadastrado com sucesso.",
            Data = new { aluno.Id, Categoria = categoria.Nome }
        };
    }

    public async Task<AlunoResponseDTO> AtualizarAsync(Guid id, AtualizarAlunoRequestDTO dto)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id);
        if (aluno == null) return new AlunoResponseDTO { Status = false, Message = "Aluno não encontrado." };

        var mensagemDeDadosInvalidos = ValidarDadosDoAluno(dto.NomeCompleto, dto.DataNascimento, dto.NivelTecnico,
                                                           dto.ResponsavelNome, dto.Telefone, dto.Endereco);
        if (mensagemDeDadosInvalidos != null) return new AlunoResponseDTO { Status = false, Message = mensagemDeDadosInvalidos };

        if (dto.Posicao.HasValue && !Enum.IsDefined(dto.Posicao.Value))
            return new AlunoResponseDTO { Status = false, Message = "Posição inválida." };

        var mensagemDoCpf = ValidarCpfDoResponsavel(dto.ResponsavelCpf, out var cpfDoResponsavel);
        if (mensagemDoCpf != null) return new AlunoResponseDTO { Status = false, Message = mensagemDoCpf };

        var existe = await _alunoRepository.ExisteComMesmoNomeEDataNascimentoAsync(dto.NomeCompleto, dto.DataNascimento, id);
        if (existe) return new AlunoResponseDTO { Status = false, Message = "Outro aluno já possui este nome e data de nascimento." };

        var turma = await _turmaRepository.ObterPorIdAsync(dto.TurmaId);
        if (turma == null)
        {
            return new AlunoResponseDTO { Status = false, Message = "Turma não encontrada." };
        }

        int idade = CalcularIdade(dto.DataNascimento);
        var categoria = await _categoriaRepository.ObterPorIdadeAsync(idade);
        if (categoria == null)
        {
            return new AlunoResponseDTO { Status = false, Message = $"Nenhuma categoria cadastrada compatível com a idade de {idade} anos." };
        }

        if (turma.CategoriaId != categoria.Id)
        {
            return new AlunoResponseDTO { Status = false, Message = $"A turma selecionada não é da categoria do aluno ({categoria.Nome})." };
        }

        aluno.AtualizarDados(dto.NomeCompleto, dto.DataNascimento, dto.NivelTecnico, dto.FotoUrl, dto.ResponsavelNome, cpfDoResponsavel, dto.Telefone, dto.Endereco, dto.TurmaId, categoria.Id, dto.Posicao);
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

    private sealed class ContratoNaoCriadoException : Exception
    {
        public ContratoNaoCriadoException(string mensagem) : base(mensagem) { }
    }

    private string? ValidarDadosDoAluno(string? nomeCompleto, DateOnly dataNascimento, NivelTecnico nivelTecnico,
                                        string? responsavelNome, string? telefone, string? endereco)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto)) return "Informe o nome completo do aluno.";
        if (string.IsNullOrWhiteSpace(responsavelNome)) return "Informe o nome do responsável.";
        if (string.IsNullOrWhiteSpace(telefone)) return "Informe o telefone do responsável.";
        if (string.IsNullOrWhiteSpace(endereco)) return "Informe o endereço do aluno.";
        if (!Enum.IsDefined(nivelTecnico)) return "Nível técnico inválido.";
        if (dataNascimento == default) return "Informe a data de nascimento.";
        if (dataNascimento > DateOnly.FromDateTime(DateTime.Now)) return "A data de nascimento não pode estar no futuro.";

        return null;
    }

    private static string? ValidarCpfDoResponsavel(string? cpfInformado, out string cpfNormalizado)
    {
        cpfNormalizado = CpfValidador.Normalizar(cpfInformado);

        if (string.IsNullOrEmpty(cpfNormalizado)) return "Informe o CPF do responsável.";
        if (!CpfValidador.EhValido(cpfNormalizado)) return "CPF do responsável inválido.";

        return null;
    }
    private int CalcularIdade(DateOnly dataNascimento)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        int idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento > hoje.AddYears(-idade)) idade--;
        return idade;
    }
}