using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class CampeonatoService : ICampeonatoService
    {
        private readonly ICampeonatoRepository _repository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IAlunoRepository _alunoRepository;

        public CampeonatoService(ICampeonatoRepository repository, ICategoriaRepository categoriaRepository, IAlunoRepository alunoRepository)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
            _alunoRepository = alunoRepository;
        }

        public async Task<IEnumerable<CampeonatoDataDTO>> ListarAsync()
        {
            var campeonatos = await _repository.ObterTodosAsync();
            return campeonatos.Select(MapearCampeonato);
        }

        public async Task<CampeonatoResponseDTO> ObterPorIdAsync(Guid id)
        {
            var campeonato = await _repository.ObterPorIdAsync(id);
            if (campeonato == null)
            {
                return new CampeonatoResponseDTO { Status = false, Message = "Campeonato não encontrado." };
            }

            return new CampeonatoResponseDTO
            {
                Status = true,
                Message = "Campeonato recuperado com sucesso.",
                Data = MapearCampeonato(campeonato)
            };
        }

        public async Task<CampeonatoResponseDTO> CriarAsync(CriarCampeonatoRequestDTO dto)
        {
            if (await _categoriaRepository.ObterPorIdAsync(dto.CategoriaId) == null)
            {
                return new CampeonatoResponseDTO { Status = false, Message = "Categoria não encontrada." };
            }

            var campeonato = new Campeonato(dto.Nome, dto.Data, dto.CategoriaId);
            await _repository.AdicionarAsync(campeonato);

            return new CampeonatoResponseDTO
            {
                Status = true,
                Message = "Campeonato criado com sucesso.",
                Data = MapearCampeonato(campeonato)
            };
        }

        public async Task<CampeonatoResponseDTO> AtualizarAsync(Guid id, AtualizarCampeonatoRequestDTO dto)
        {
            var campeonato = await _repository.ObterPorIdAsync(id);
            if (campeonato == null)
            {
                return new CampeonatoResponseDTO { Status = false, Message = "Campeonato não encontrado." };
            }

            if (await _categoriaRepository.ObterPorIdAsync(dto.CategoriaId) == null)
            {
                return new CampeonatoResponseDTO { Status = false, Message = "Categoria não encontrada." };
            }

            campeonato.AtualizarDetalhes(dto.Nome, dto.Data, dto.CategoriaId);
            await _repository.AtualizarAsync(campeonato);

            return new CampeonatoResponseDTO
            {
                Status = true,
                Message = "Campeonato atualizado com sucesso.",
                Data = MapearCampeonato(campeonato)
            };
        }

        public async Task<CampeonatoResponseDTO> RemoverAsync(Guid id)
        {
            var campeonato = await _repository.ObterPorIdAsync(id);
            if (campeonato == null)
            {
                return new CampeonatoResponseDTO { Status = false, Message = "Campeonato não encontrado." };
            }

            await _repository.RemoverAsync(campeonato);

            return new CampeonatoResponseDTO { Status = true, Message = "Campeonato removido com sucesso." };
        }

        public async Task<ParticipacaoCampeonatoResponseDTO> InscreverAlunoAsync(Guid campeonatoId, InscreverAlunoCampeonatoRequestDTO dto)
        {
            if (await _repository.ObterPorIdAsync(campeonatoId) == null)
            {
                return new ParticipacaoCampeonatoResponseDTO { Status = false, Message = "Campeonato não encontrado." };
            }

            if (await _alunoRepository.ObterPorIdAsync(dto.AlunoId) == null)
            {
                return new ParticipacaoCampeonatoResponseDTO { Status = false, Message = "Aluno não encontrado." };
            }

            var participacao = new ParticipacaoCampeonato(campeonatoId, dto.AlunoId, dto.Posicao);
            await _repository.AdicionarParticipacaoAsync(participacao);

            return new ParticipacaoCampeonatoResponseDTO
            {
                Status = true,
                Message = "Aluno inscrito com sucesso.",
                Data = MapearParticipacao(participacao)
            };
        }

        public async Task<ParticipacaoCampeonatoResponseDTO> AtualizarParticipacaoAsync(Guid id, AtualizarParticipacaoCampeonatoRequestDTO dto)
        {
            var participacao = await _repository.ObterParticipacaoPorIdAsync(id);
            if (participacao == null)
            {
                return new ParticipacaoCampeonatoResponseDTO { Status = false, Message = "Participação não encontrada." };
            }

            participacao.Atualizar(dto.Posicao, dto.JogosDisputados, dto.Aproveitamento);
            await _repository.AtualizarParticipacaoAsync(participacao);

            return new ParticipacaoCampeonatoResponseDTO
            {
                Status = true,
                Message = "Participação atualizada com sucesso.",
                Data = MapearParticipacao(participacao)
            };
        }

        public async Task<ParticipacaoCampeonatoResponseDTO> ListarParticipacoesAsync(Guid campeonatoId)
        {
            if (await _repository.ObterPorIdAsync(campeonatoId) == null)
            {
                return new ParticipacaoCampeonatoResponseDTO { Status = false, Message = "Campeonato não encontrado." };
            }

            var participacoes = await _repository.ObterParticipacoesPorCampeonatoAsync(campeonatoId);

            return new ParticipacaoCampeonatoResponseDTO
            {
                Status = true,
                Message = "Participações recuperadas com sucesso.",
                Data = participacoes.Select(MapearParticipacao).ToList()
            };
        }

        private static CampeonatoDataDTO MapearCampeonato(Campeonato c) => new CampeonatoDataDTO
        {
            Id = c.Id,
            Nome = c.Nome,
            Data = c.Data,
            CategoriaId = c.CategoriaId
        };

        private static ParticipacaoCampeonatoDataDTO MapearParticipacao(ParticipacaoCampeonato p) => new ParticipacaoCampeonatoDataDTO
        {
            Id = p.Id,
            CampeonatoId = p.CampeonatoId,
            AlunoId = p.AlunoId,
            Posicao = p.Posicao,
            JogosDisputados = p.JogosDisputados,
            Aproveitamento = p.Aproveitamento
        };
    }
}
