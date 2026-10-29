using Application.Campeonatos;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class CampeonatoCompeticaoService : ICampeonatoCompeticaoService
    {
        private const int JogadoresPorEquipe = 5;
        private const int MaximoDeJogadoresNoModoLivre = 8;
        private const int LimiteDoNomeDaEquipe = 50;
        private const int LimiteDeUmaEstatistica = 999;

        private readonly ICampeonatoRepository _campeonatoRepository;
        private readonly ICampeonatoCompeticaoRepository _competicaoRepository;
        private readonly IAlunoRepository _alunoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CampeonatoCompeticaoService(
            ICampeonatoRepository campeonatoRepository,
            ICampeonatoCompeticaoRepository competicaoRepository,
            IAlunoRepository alunoRepository,
            ICategoriaRepository categoriaRepository)
        {
            _campeonatoRepository = campeonatoRepository;
            _competicaoRepository = competicaoRepository;
            _alunoRepository = alunoRepository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<RespostaDaCompeticao<List<AlunoElegivelDTO>>> ListarElegiveisAsync(Guid campeonatoId)
        {
            var campeonato = await _campeonatoRepository.ObterPorIdAsync(campeonatoId);
            if (campeonato == null)
                return RespostaDaCompeticao<List<AlunoElegivelDTO>>.Falha("Campeonato não encontrado.", true);

            var categorias = (await _categoriaRepository.ObterTodasAsync()).ToList();
            var categoriaDoCampeonato = categorias.FirstOrDefault(categoria => categoria.Id == campeonato.CategoriaId);
            if (categoriaDoCampeonato == null)
                return RespostaDaCompeticao<List<AlunoElegivelDTO>>.Falha("Categoria do campeonato não encontrada.");

            var categoriasElegiveis = ElegibilidadeDeCategoria.ObterCategoriasElegiveis(categoriaDoCampeonato, categorias);
            var jaInscritos = (await _competicaoRepository.ObterParticipacoesAsync(campeonatoId))
                .Select(participacao => participacao.AlunoId)
                .ToHashSet();

            var alunosAtivos = await _alunoRepository.ObterTodosAsync();
            var elegiveis = alunosAtivos
                .Where(aluno => categoriasElegiveis.Contains(aluno.CategoriaId) && !jaInscritos.Contains(aluno.Id))
                .OrderBy(aluno => aluno.NomeCompleto)
                .Select(aluno => new AlunoElegivelDTO
                {
                    AlunoId = aluno.Id,
                    Nome = aluno.NomeCompleto,
                    Posicao = aluno.Posicao.HasValue ? (int)aluno.Posicao.Value : null,
                    PosicaoNome = NomesDePosicao.Obter(aluno.Posicao),
                    CategoriaNome = categorias.FirstOrDefault(categoria => categoria.Id == aluno.CategoriaId)?.Nome ?? string.Empty
                })
                .ToList();

            return RespostaDaCompeticao<List<AlunoElegivelDTO>>.Ok(elegiveis);
        }

        public async Task<RespostaDaCompeticao<List<EquipeDTO>>> ListarEquipesAsync(Guid campeonatoId)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<List<EquipeDTO>>.Falha("Campeonato não encontrado.", true);

            return RespostaDaCompeticao<List<EquipeDTO>>.Ok(await MontarEquipesAsync(campeonatoId));
        }

        public async Task<RespostaDaCompeticao<EquipeDTO>> CriarEquipeAsync(Guid campeonatoId, CriarEquipeRequestDTO dto)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<EquipeDTO>.Falha("Campeonato não encontrado.", true);

            if (await _campeonatoRepository.PossuiJogosAsync(campeonatoId))
                return RespostaDaCompeticao<EquipeDTO>.Falha("Não é possível alterar os times depois que os jogos foram gerados.");

            var alunoIds = (dto.AlunoIds ?? new List<Guid>()).Distinct().ToList();
            if (dto.ModoLivre)
            {
                if (alunoIds.Count < JogadoresPorEquipe || alunoIds.Count > MaximoDeJogadoresNoModoLivre)
                    return RespostaDaCompeticao<EquipeDTO>.Falha($"No modo livre o time precisa de {JogadoresPorEquipe} a {MaximoDeJogadoresNoModoLivre} alunos.");
            }
            else if (alunoIds.Count != JogadoresPorEquipe)
                return RespostaDaCompeticao<EquipeDTO>.Falha($"Cada time precisa de exatamente {JogadoresPorEquipe} alunos, um para cada posição.");

            var participacoes = await _competicaoRepository.ObterParticipacoesAsync(campeonatoId);
            var participacoesDoTime = new List<ParticipacaoCampeonato>();
            foreach (var alunoId in alunoIds)
            {
                var participacao = participacoes.FirstOrDefault(item => item.AlunoId == alunoId);
                if (participacao == null)
                    return RespostaDaCompeticao<EquipeDTO>.Falha("Há aluno que não está inscrito neste campeonato.");
                if (participacao.EquipeId != null)
                    return RespostaDaCompeticao<EquipeDTO>.Falha("Há aluno que já faz parte de outro time.");
                participacoesDoTime.Add(participacao);
            }

            var alunos = await _alunoRepository.ObterPorIdsAsync(alunoIds);
            if (alunos.Any(aluno => aluno.Posicao == null))
                return RespostaDaCompeticao<EquipeDTO>.Falha("Há aluno sem posição cadastrada. Edite o aluno e escolha a posição.");

            var posicoes = alunos.Select(aluno => aluno.Posicao!.Value).ToList();
            if (!dto.ModoLivre && posicoes.Distinct().Count() != JogadoresPorEquipe)
                return RespostaDaCompeticao<EquipeDTO>.Falha("O time precisa ter uma posição de cada tipo, sem repetir: Goleiro, Fixo, Ala direito, Ala esquerdo e Pivô.");

            var equipesExistentes = await _competicaoRepository.ObterEquipesAsync(campeonatoId);
            var nome = (dto.Nome ?? string.Empty).Trim();
            if (nome.Length == 0)
                nome = GerarNomePadrao(equipesExistentes);
            if (nome.Length > LimiteDoNomeDaEquipe)
                return RespostaDaCompeticao<EquipeDTO>.Falha($"O nome do time deve ter no máximo {LimiteDoNomeDaEquipe} caracteres.");
            if (equipesExistentes.Any(equipe => string.Equals(equipe.Nome, nome, StringComparison.OrdinalIgnoreCase)))
                return RespostaDaCompeticao<EquipeDTO>.Falha("Já existe um time com esse nome neste campeonato.");

            var novaEquipe = new EquipeCampeonato(campeonatoId, nome);
            foreach (var participacao in participacoesDoTime)
                participacao.DefinirEquipe(novaEquipe.Id);

            await _competicaoRepository.AdicionarEquipesAsync(new[] { novaEquipe }, participacoesDoTime);

            var equipes = await MontarEquipesAsync(campeonatoId);
            return RespostaDaCompeticao<EquipeDTO>.Ok(equipes.First(equipe => equipe.Id == novaEquipe.Id), "Time criado com sucesso.");
        }

        public async Task<RespostaDaCompeticao<List<EquipeDTO>>> SortearEquipesAsync(Guid campeonatoId, SortearEquipesRequestDTO dto)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<List<EquipeDTO>>.Falha("Campeonato não encontrado.", true);

            if (await _campeonatoRepository.PossuiJogosAsync(campeonatoId))
                return RespostaDaCompeticao<List<EquipeDTO>>.Falha("Não é possível alterar os times depois que os jogos foram gerados.");

            var participacoesSemTime = (await _competicaoRepository.ObterParticipacoesAsync(campeonatoId))
                .Where(participacao => participacao.EquipeId == null)
                .ToList();

            var alunos = await _alunoRepository.ObterPorIdsAsync(participacoesSemTime.Select(participacao => participacao.AlunoId));
            var candidatos = alunos
                .Where(aluno => aluno.Posicao.HasValue)
                .Select(aluno => new CandidatoAoSorteio(aluno.Id, aluno.Posicao!.Value))
                .ToList();

            var resultado = dto?.IncluirTodos == true
                ? SorteioDeEquipes.SortearTodos(candidatos, dto.Quantidade, Random.Shared)
                : SorteioDeEquipes.Sortear(candidatos, dto?.Quantidade, Random.Shared);
            if (resultado.Erro != null)
                return RespostaDaCompeticao<List<EquipeDTO>>.Falha(resultado.Erro);

            var equipesExistentes = await _competicaoRepository.ObterEquipesAsync(campeonatoId);
            var proximoNumero = ProximoNumeroDeTime(equipesExistentes);

            var novasEquipes = new List<EquipeCampeonato>();
            var participacoesAtualizadas = new List<ParticipacaoCampeonato>();
            foreach (var alunosDoTime in resultado.Equipes)
            {
                var equipe = new EquipeCampeonato(campeonatoId, $"Time {proximoNumero++}");
                novasEquipes.Add(equipe);

                foreach (var alunoId in alunosDoTime)
                {
                    var participacao = participacoesSemTime.First(item => item.AlunoId == alunoId);
                    participacao.DefinirEquipe(equipe.Id);
                    participacoesAtualizadas.Add(participacao);
                }
            }

            await _competicaoRepository.AdicionarEquipesAsync(novasEquipes, participacoesAtualizadas);

            var mensagem = $"{novasEquipes.Count} time(s) criado(s).";
            var quantidadeDeReservas = participacoesAtualizadas.Count - novasEquipes.Count * JogadoresPorEquipe;
            if (quantidadeDeReservas > 0)
                mensagem += $" {quantidadeDeReservas} aluno(s) entraram como reserva.";

            return RespostaDaCompeticao<List<EquipeDTO>>.Ok(await MontarEquipesAsync(campeonatoId), mensagem);
        }

        public async Task<RespostaDaCompeticao<bool>> RemoverEquipeAsync(Guid campeonatoId, Guid equipeId)
        {
            var equipe = await _competicaoRepository.ObterEquipePorIdAsync(equipeId);
            if (equipe == null || equipe.CampeonatoId != campeonatoId)
                return RespostaDaCompeticao<bool>.Falha("Time não encontrado.", true);

            if (await _campeonatoRepository.PossuiJogosAsync(campeonatoId))
                return RespostaDaCompeticao<bool>.Falha("Não é possível remover times depois que os jogos foram gerados.");

            var participacoesLiberadas = (await _competicaoRepository.ObterParticipacoesAsync(campeonatoId))
                .Where(participacao => participacao.EquipeId == equipeId)
                .ToList();
            foreach (var participacao in participacoesLiberadas)
                participacao.DefinirEquipe(null);

            await _competicaoRepository.RemoverEquipeAsync(equipe, participacoesLiberadas);
            return RespostaDaCompeticao<bool>.Ok(true, "Time removido. Os alunos voltaram para a lista sem time.");
        }

        public async Task<RespostaDaCompeticao<bool>> RemoverParticipacaoAsync(Guid campeonatoId, Guid participacaoId)
        {
            var participacao = await _competicaoRepository.ObterParticipacaoPorIdAsync(participacaoId);
            if (participacao == null || participacao.CampeonatoId != campeonatoId)
                return RespostaDaCompeticao<bool>.Falha("Inscrição não encontrada.", true);

            if (await _campeonatoRepository.PossuiJogosAsync(campeonatoId))
                return RespostaDaCompeticao<bool>.Falha("Não é possível remover inscrições depois que os jogos foram gerados.");

            if (participacao.EquipeId != null)
                return RespostaDaCompeticao<bool>.Falha("O aluno faz parte de um time. Remova o time antes de retirar a inscrição.");

            await _competicaoRepository.RemoverParticipacaoAsync(participacao);
            return RespostaDaCompeticao<bool>.Ok(true, "Inscrição removida.");
        }

        public async Task<RespostaDaCompeticao<List<JogoDTO>>> GerarJogosAsync(Guid campeonatoId)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<List<JogoDTO>>.Falha("Campeonato não encontrado.", true);

            if (await _campeonatoRepository.PossuiJogosAsync(campeonatoId))
                return RespostaDaCompeticao<List<JogoDTO>>.Falha("Os jogos deste campeonato já foram gerados.");

            var equipes = await _competicaoRepository.ObterEquipesAsync(campeonatoId);
            if (equipes.Count < 2)
                return RespostaDaCompeticao<List<JogoDTO>>.Falha("Crie ao menos 2 times antes de gerar os jogos.");

            var planejados = ChaveamentoMataMata.Gerar(equipes.Select(equipe => equipe.Id).ToList(), Random.Shared);

            var jogos = new List<JogoCampeonato>();
            foreach (var planejado in planejados)
            {
                var jogo = new JogoCampeonato(campeonatoId, planejado.Fase, planejado.Ordem);
                if (planejado.EquipeAId.HasValue) jogo.DefinirEquipe(true, planejado.EquipeAId.Value);
                if (planejado.EquipeBId.HasValue) jogo.DefinirEquipe(false, planejado.EquipeBId.Value);
                jogos.Add(jogo);
            }

            foreach (var jogoComBye in jogos.Where(jogo => jogo.Fase == 1 && (jogo.EquipeAId == null) != (jogo.EquipeBId == null)).ToList())
            {
                jogoComBye.FinalizarPorBye((jogoComBye.EquipeAId ?? jogoComBye.EquipeBId)!.Value);
                ChaveamentoMataMata.Avancar(jogos, jogoComBye);
            }

            await _competicaoRepository.AdicionarJogosAsync(jogos);
            return await ListarJogosAsync(campeonatoId);
        }

        public async Task<RespostaDaCompeticao<List<JogoDTO>>> ListarJogosAsync(Guid campeonatoId)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<List<JogoDTO>>.Falha("Campeonato não encontrado.", true);

            var jogos = await _competicaoRepository.ObterJogosAsync(campeonatoId);
            var nomesDasEquipes = (await _competicaoRepository.ObterEquipesAsync(campeonatoId)).ToDictionary(equipe => equipe.Id, equipe => equipe.Nome);
            var totalDeFases = jogos.Count == 0 ? 0 : jogos.Max(jogo => jogo.Fase);

            var lista = jogos
                .OrderBy(jogo => jogo.Fase)
                .ThenBy(jogo => jogo.Ordem)
                .Select(jogo => MontarJogo(jogo, nomesDasEquipes, totalDeFases))
                .ToList();

            return RespostaDaCompeticao<List<JogoDTO>>.Ok(lista);
        }

        public async Task<RespostaDaCompeticao<bool>> ReiniciarJogosAsync(Guid campeonatoId)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<bool>.Falha("Campeonato não encontrado.", true);

            var jogos = await _competicaoRepository.ObterJogosAsync(campeonatoId);
            if (jogos.Count == 0)
                return RespostaDaCompeticao<bool>.Falha("Este campeonato ainda não tem jogos.");

            if (jogos.Any(jogo => jogo.Status == StatusJogo.Finalizado && !jogo.Bye))
                return RespostaDaCompeticao<bool>.Falha("Há jogos finalizados. Não é possível refazer o sorteio.");

            await _competicaoRepository.RemoverJogosDoCampeonatoAsync(campeonatoId);
            return RespostaDaCompeticao<bool>.Ok(true, "Jogos removidos. Você já pode alterar os times ou sortear de novo.");
        }

        public async Task<RespostaDaCompeticao<EstatisticasDoJogoDTO>> ObterEstatisticasAsync(Guid jogoId)
        {
            var jogo = await _competicaoRepository.ObterJogoPorIdAsync(jogoId);
            if (jogo == null)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Jogo não encontrado.", true);

            if (!jogo.EquipeAId.HasValue || !jogo.EquipeBId.HasValue)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Este jogo ainda aguarda a definição dos times.");

            return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Ok(await MontarEstatisticasDoJogoAsync(jogo));
        }

        public async Task<RespostaDaCompeticao<EstatisticasDoJogoDTO>> SalvarEstatisticasAsync(Guid jogoId, SalvarEstatisticasRequestDTO dto)
        {
            var jogo = await _competicaoRepository.ObterJogoPorIdAsync(jogoId);
            if (jogo == null)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Jogo não encontrado.", true);

            if (jogo.Status != StatusJogo.Pendente)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha(jogo.Status == StatusJogo.Finalizado
                    ? "Este jogo já foi finalizado e não pode mais ser alterado."
                    : "Este jogo ainda aguarda a definição dos times.");

            var participacoes = await _competicaoRepository.ObterParticipacoesAsync(jogo.CampeonatoId);
            var jogadoresDeA = participacoes.Where(participacao => participacao.EquipeId == jogo.EquipeAId).Select(participacao => participacao.AlunoId).ToHashSet();
            var jogadoresDeB = participacoes.Where(participacao => participacao.EquipeId == jogo.EquipeBId).Select(participacao => participacao.AlunoId).ToHashSet();

            var itens = dto?.Jogadores ?? new List<EstatisticaDoJogadorRequestDTO>();
            if (itens.Select(item => item.AlunoId).Distinct().Count() != itens.Count)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Há jogador repetido na lista.");

            var alunos = (await _alunoRepository.ObterPorIdsAsync(itens.Select(item => item.AlunoId))).ToDictionary(aluno => aluno.Id);

            foreach (var item in itens)
            {
                if (!jogadoresDeA.Contains(item.AlunoId) && !jogadoresDeB.Contains(item.AlunoId))
                    return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Há jogador que não pertence a nenhum dos times deste jogo.");

                var valores = new[] { item.Gols, item.Assistencias, item.PassesCertos, item.Desarmes, item.Defesas, item.Faltas, item.CartoesAmarelos, item.CartoesVermelhos };
                if (valores.Any(valor => valor < 0 || valor > LimiteDeUmaEstatistica))
                    return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha($"As estatísticas devem ficar entre 0 e {LimiteDeUmaEstatistica}.");

                if (item.Defesas > 0 && alunos.TryGetValue(item.AlunoId, out var aluno) && aluno.Posicao != PosicaoJogador.Goleiro)
                    return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Defesas só podem ser lançadas para o goleiro.");
            }

            var estatisticas = itens.Select(item => new EstatisticaJogo(jogo.Id, item.AlunoId, item.Gols, item.Assistencias,
                item.PassesCertos, item.Desarmes, item.Defesas, item.Faltas, item.CartoesAmarelos, item.CartoesVermelhos)).ToList();

            await _competicaoRepository.SubstituirEstatisticasDoJogoAsync(jogo.Id, estatisticas);

            jogo.AtualizarPlacar(
                itens.Where(item => jogadoresDeA.Contains(item.AlunoId)).Sum(item => item.Gols),
                itens.Where(item => jogadoresDeB.Contains(item.AlunoId)).Sum(item => item.Gols));
            await _competicaoRepository.AtualizarJogosAsync(new[] { jogo });

            return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Ok(await MontarEstatisticasDoJogoAsync(jogo), "Estatísticas salvas.");
        }

        public async Task<RespostaDaCompeticao<EstatisticasDoJogoDTO>> FinalizarJogoAsync(Guid jogoId)
        {
            var jogo = await _competicaoRepository.ObterJogoPorIdAsync(jogoId);
            if (jogo == null)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha("Jogo não encontrado.", true);

            if (jogo.Status != StatusJogo.Pendente)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha(jogo.Status == StatusJogo.Finalizado
                    ? "Este jogo já foi finalizado."
                    : "Este jogo ainda aguarda a definição dos times.");

            if (jogo.GolsA == jogo.GolsB)
                return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Falha(
                    "O jogo está empatado. Lance o gol de ouro (o próximo gol) nas estatísticas e finalize novamente.");

            var vencedora = jogo.GolsA > jogo.GolsB ? jogo.EquipeAId!.Value : jogo.EquipeBId!.Value;
            jogo.Finalizar(vencedora);

            var todosOsJogos = await _competicaoRepository.ObterJogosAsync(jogo.CampeonatoId);
            var jogoNaLista = todosOsJogos.First(item => item.Id == jogo.Id);
            if (!ReferenceEquals(jogoNaLista, jogo))
                jogoNaLista.Finalizar(vencedora);

            ChaveamentoMataMata.Avancar(todosOsJogos, jogoNaLista);
            await _competicaoRepository.AtualizarJogosAsync(todosOsJogos);

            return RespostaDaCompeticao<EstatisticasDoJogoDTO>.Ok(await MontarEstatisticasDoJogoAsync(jogoNaLista), "Jogo finalizado.");
        }

        public async Task<RespostaDaCompeticao<RankingDoCampeonatoDTO>> ObterRankingAsync(Guid campeonatoId)
        {
            if (await _campeonatoRepository.ObterPorIdAsync(campeonatoId) == null)
                return RespostaDaCompeticao<RankingDoCampeonatoDTO>.Falha("Campeonato não encontrado.", true);

            var jogos = await _competicaoRepository.ObterJogosAsync(campeonatoId);
            var equipes = (await _competicaoRepository.ObterEquipesAsync(campeonatoId)).ToDictionary(equipe => equipe.Id, equipe => equipe.Nome);
            var participacoes = await _competicaoRepository.ObterParticipacoesAsync(campeonatoId);

            var jogosValidos = jogos.Where(jogo => jogo.Status == StatusJogo.Finalizado && !jogo.Bye).Select(jogo => jogo.Id).ToHashSet();
            var estatisticas = (await _competicaoRepository.ObterEstatisticasDoCampeonatoAsync(campeonatoId))
                .Where(estatistica => jogosValidos.Contains(estatistica.JogoId))
                .ToList();

            var alunos = (await _alunoRepository.ObterPorIdsAsync(participacoes.Select(participacao => participacao.AlunoId)))
                .ToDictionary(aluno => aluno.Id);

            var linhas = CalculadoraDeRanking.Calcular(estatisticas, participacoes.Select(participacao => participacao.AlunoId));

            var itens = linhas.Select((linha, indice) =>
            {
                alunos.TryGetValue(linha.AlunoId, out var aluno);
                var equipeId = participacoes.FirstOrDefault(participacao => participacao.AlunoId == linha.AlunoId)?.EquipeId;
                return new LinhaDoRankingDTO
                {
                    Colocacao = indice + 1,
                    AlunoId = linha.AlunoId,
                    Nome = aluno?.NomeCompleto ?? "—",
                    PosicaoNome = NomesDePosicao.Obter(aluno?.Posicao),
                    EquipeNome = equipeId.HasValue && equipes.TryGetValue(equipeId.Value, out var nomeDaEquipe) ? nomeDaEquipe : null,
                    Jogos = linha.Jogos,
                    Gols = linha.Gols,
                    Assistencias = linha.Assistencias,
                    PassesCertos = linha.PassesCertos,
                    Desarmes = linha.Desarmes,
                    Defesas = linha.Defesas,
                    Faltas = linha.Faltas,
                    CartoesAmarelos = linha.CartoesAmarelos,
                    CartoesVermelhos = linha.CartoesVermelhos,
                    Pontos = linha.Pontos
                };
            }).ToList();

            var totalDeFases = jogos.Count == 0 ? 0 : jogos.Max(jogo => jogo.Fase);
            var final = jogos.FirstOrDefault(jogo => jogo.Fase == totalDeFases && jogo.Ordem == 0);
            var finalizado = final is { Status: StatusJogo.Finalizado, EquipeVencedoraId: not null };

            return RespostaDaCompeticao<RankingDoCampeonatoDTO>.Ok(new RankingDoCampeonatoDTO
            {
                Finalizado = finalizado,
                CampeaId = finalizado ? final!.EquipeVencedoraId : null,
                CampeaNome = finalizado && equipes.TryGetValue(final!.EquipeVencedoraId!.Value, out var nomeDaCampea) ? nomeDaCampea : null,
                Itens = itens
            });
        }

        private async Task<List<EquipeDTO>> MontarEquipesAsync(Guid campeonatoId)
        {
            var equipes = await _competicaoRepository.ObterEquipesAsync(campeonatoId);
            var participacoes = await _competicaoRepository.ObterParticipacoesAsync(campeonatoId);
            var alunos = (await _alunoRepository.ObterPorIdsAsync(participacoes.Select(participacao => participacao.AlunoId)))
                .ToDictionary(aluno => aluno.Id);

            return equipes
                .OrderBy(equipe => equipe.Nome, StringComparer.OrdinalIgnoreCase)
                .Select(equipe => new EquipeDTO
                {
                    Id = equipe.Id,
                    Nome = equipe.Nome,
                    Jogadores = participacoes
                        .Where(participacao => participacao.EquipeId == equipe.Id && alunos.ContainsKey(participacao.AlunoId))
                        .Select(participacao => alunos[participacao.AlunoId])
                        .OrderBy(aluno => aluno.Posicao)
                        .Select(aluno => new JogadorDaEquipeDTO
                        {
                            AlunoId = aluno.Id,
                            Nome = aluno.NomeCompleto,
                            Posicao = aluno.Posicao.HasValue ? (int)aluno.Posicao.Value : null,
                            PosicaoNome = NomesDePosicao.Obter(aluno.Posicao)
                        })
                        .ToList()
                })
                .ToList();
        }

        private async Task<EstatisticasDoJogoDTO> MontarEstatisticasDoJogoAsync(JogoCampeonato jogo)
        {
            var equipes = (await _competicaoRepository.ObterEquipesAsync(jogo.CampeonatoId)).ToDictionary(equipe => equipe.Id, equipe => equipe.Nome);
            var participacoes = await _competicaoRepository.ObterParticipacoesAsync(jogo.CampeonatoId);
            var jogadoresDoJogo = participacoes
                .Where(participacao => participacao.EquipeId.HasValue
                    && (participacao.EquipeId == jogo.EquipeAId || participacao.EquipeId == jogo.EquipeBId))
                .ToList();

            var alunos = (await _alunoRepository.ObterPorIdsAsync(jogadoresDoJogo.Select(participacao => participacao.AlunoId)))
                .ToDictionary(aluno => aluno.Id);
            var registradas = (await _competicaoRepository.ObterEstatisticasDoJogoAsync(jogo.Id))
                .ToDictionary(estatistica => estatistica.AlunoId);

            var totalDeFases = (await _competicaoRepository.ObterJogosAsync(jogo.CampeonatoId)).Max(item => item.Fase);

            var jogadores = jogadoresDoJogo
                .Where(participacao => alunos.ContainsKey(participacao.AlunoId))
                .OrderBy(participacao => participacao.EquipeId == jogo.EquipeAId ? 0 : 1)
                .ThenBy(participacao => alunos[participacao.AlunoId].Posicao)
                .Select(participacao =>
                {
                    var aluno = alunos[participacao.AlunoId];
                    registradas.TryGetValue(aluno.Id, out var registro);
                    return new EstatisticaDoJogadorDTO
                    {
                        AlunoId = aluno.Id,
                        Nome = aluno.NomeCompleto,
                        Posicao = aluno.Posicao.HasValue ? (int)aluno.Posicao.Value : null,
                        PosicaoNome = NomesDePosicao.Obter(aluno.Posicao),
                        EquipeId = participacao.EquipeId!.Value,
                        Gols = registro?.Gols ?? 0,
                        Assistencias = registro?.Assistencias ?? 0,
                        PassesCertos = registro?.PassesCertos ?? 0,
                        Desarmes = registro?.Desarmes ?? 0,
                        Defesas = registro?.Defesas ?? 0,
                        Faltas = registro?.Faltas ?? 0,
                        CartoesAmarelos = registro?.CartoesAmarelos ?? 0,
                        CartoesVermelhos = registro?.CartoesVermelhos ?? 0
                    };
                })
                .ToList();

            return new EstatisticasDoJogoDTO { Jogo = MontarJogo(jogo, equipes, totalDeFases), Jogadores = jogadores };
        }

        private static JogoDTO MontarJogo(JogoCampeonato jogo, Dictionary<Guid, string> nomesDasEquipes, int totalDeFases)
        {
            string? Nome(Guid? equipeId) => equipeId.HasValue && nomesDasEquipes.TryGetValue(equipeId.Value, out var nome) ? nome : null;

            return new JogoDTO
            {
                Id = jogo.Id,
                Fase = jogo.Fase,
                Ordem = jogo.Ordem,
                NomeDaFase = ChaveamentoMataMata.NomeDaFase(jogo.Fase, totalDeFases),
                EquipeAId = jogo.EquipeAId,
                EquipeANome = Nome(jogo.EquipeAId),
                EquipeBId = jogo.EquipeBId,
                EquipeBNome = Nome(jogo.EquipeBId),
                GolsA = jogo.GolsA,
                GolsB = jogo.GolsB,
                Status = jogo.Status.ToString(),
                EquipeVencedoraId = jogo.EquipeVencedoraId,
                EquipeVencedoraNome = Nome(jogo.EquipeVencedoraId),
                Bye = jogo.Bye
            };
        }

        private static int ProximoNumeroDeTime(IEnumerable<EquipeCampeonato> equipesExistentes)
        {
            var numero = equipesExistentes.Count() + 1;
            while (equipesExistentes.Any(equipe => string.Equals(equipe.Nome, $"Time {numero}", StringComparison.OrdinalIgnoreCase)))
                numero++;
            return numero;
        }

        private static string GerarNomePadrao(IEnumerable<EquipeCampeonato> equipesExistentes)
        {
            return $"Time {ProximoNumeroDeTime(equipesExistentes)}";
        }
    }
}
