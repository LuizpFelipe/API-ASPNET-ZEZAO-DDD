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
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoriaDataDTO>> ListarAsync()
        {
            var categorias = await _repository.ObterTodasAsync();
            return categorias.Select(c => new CategoriaDataDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                IdadeMin = c.IdadeMin,
                IdadeMax = c.IdadeMax
            });
        }

        public async Task<CategoriaResponseDTO> ObterPorIdAsync(Guid id)
        {
            var categoria = await _repository.ObterPorIdAsync(id);
            if (categoria == null)
            {
                return new CategoriaResponseDTO { Status = false, Message = "Categoria não encontrada." };
            }

            return new CategoriaResponseDTO
            {
                Status = true,
                Message = "Categoria recuperada com sucesso.",
                Data = new CategoriaDataDTO { Id = categoria.Id, Nome = categoria.Nome, IdadeMin = categoria.IdadeMin, IdadeMax = categoria.IdadeMax }
            };
        }

        public async Task<CategoriaResponseDTO> CriarAsync(CriarCategoriaRequestDTO dto)
        {
            if (await _repository.ExisteSobreposicaoDeIdadeAsync(dto.IdadeMin, dto.IdadeMax))
            {
                return new CategoriaResponseDTO { Status = false, Message = "A faixa de idade se sobrepõe a uma categoria existente." };
            }

            var categoria = new Categoria(dto.Nome, dto.IdadeMin, dto.IdadeMax);
            await _repository.AdicionarAsync(categoria);

            return new CategoriaResponseDTO
            {
                Status = true,
                Message = "Categoria criada com sucesso.",
                Data = new CategoriaDataDTO { Id = categoria.Id, Nome = categoria.Nome, IdadeMin = categoria.IdadeMin, IdadeMax = categoria.IdadeMax }
            };
        }

        public async Task<CategoriaResponseDTO> AtualizarAsync(Guid id, AtualizarCategoriaRequestDTO dto)
        {
            var categoria = await _repository.ObterPorIdAsync(id);
            if (categoria == null)
            {
                return new CategoriaResponseDTO { Status = false, Message = "Categoria não encontrada." };
            }

            if (await _repository.ExisteSobreposicaoDeIdadeAsync(dto.IdadeMin, dto.IdadeMax, id))
            {
                return new CategoriaResponseDTO { Status = false, Message = "A faixa de idade se sobrepõe a outra categoria existente." };
            }

            categoria.Atualizar(dto.Nome, dto.IdadeMin, dto.IdadeMax);
            await _repository.AtualizarAsync(categoria);

            return new CategoriaResponseDTO
            {
                Status = true,
                Message = "Categoria atualizada com sucesso.",
                Data = new CategoriaDataDTO { Id = categoria.Id, Nome = categoria.Nome, IdadeMin = categoria.IdadeMin, IdadeMax = categoria.IdadeMax }
            };
        }

        public async Task<CategoriaResponseDTO> RemoverAsync(Guid id)
        {
            var categoria = await _repository.ObterPorIdAsync(id);
            if (categoria == null)
            {
                return new CategoriaResponseDTO { Status = false, Message = "Categoria não encontrada." };
            }

            await _repository.RemoverAsync(categoria);

            return new CategoriaResponseDTO { Status = true, Message = "Categoria removida com sucesso." };
        }

        public async Task<CategoriaDataDTO?> CalcularCategoriaPorIdadeAsync(DateOnly dataNascimento)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            int idade = hoje.Year - dataNascimento.Year;
            if (hoje < dataNascimento.AddYears(idade))
            {
                idade--;
            }

            var categoria = await _repository.ObterPorIdadeAsync(idade);
            if (categoria == null) return null;

            return new CategoriaDataDTO
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                IdadeMin = categoria.IdadeMin,
                IdadeMax = categoria.IdadeMax
            };
        }
    }
}