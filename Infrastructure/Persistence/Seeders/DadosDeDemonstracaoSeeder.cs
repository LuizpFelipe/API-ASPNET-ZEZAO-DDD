using Application.Campeonatos;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence.Seeders
{
    public static class DadosDeDemonstracaoSeeder
    {
        private const string NomeDeUsuarioDeControle = "prof.carlos";
        private const decimal ValorDaParcela = 150m;
        private const int QuantidadeDeParcelas = 12;
        private const int AlunosPorCategoria = 15;
        private const int TurmasPorCategoria = 2;
        private const int SemanasDeHistoricoDePresenca = 8;
        private const int AlunosPorTimeNoCampeonato = 5;
        private const string PrefixoCopa = "Copa Zezão ";
        private const string PrefixoTorneio = "Torneio Interclasses ";

        private static readonly string[] NomesDosProfessores =
        {
            "Carlos Eduardo Lima", "Fernanda Souza Alves", "Ricardo Menezes", "Juliana Prado", "Marcos Vinícius Rocha"
        };

        private static readonly string[] NomesDeUsuarioDosProfessores =
        {
            "prof.carlos", "prof.fernanda", "prof.ricardo", "prof.juliana", "prof.marcos"
        };

        private static readonly (string Nome, int IdadeMinima, int IdadeMaxima)[] CategoriasExtras =
        {
            ("Sub-9", 6, 8), ("Sub-17", 15, 16), ("Sub-19", 17, 18)
        };

        private static readonly string[] PadroesDeDias =
        {
            "Segunda,Quarta", "Terça,Quinta", "Sexta", "Sábado", "Segunda,Quarta,Sexta", "Terça,Quinta,Sábado"
        };

        private static readonly string[] Horarios = { "9h", "10h", "14h", "15h", "16h", "17h", "18h", "19h" };

        private static readonly string[] PrimeirosNomes =
        {
            "Miguel", "Arthur", "Heitor", "Theo", "Davi", "Gabriel", "Bernardo", "Samuel", "João", "Pedro",
            "Lucas", "Matheus", "Enzo", "Nicolas", "Rafael", "Gustavo", "Felipe", "Henrique", "Vinícius", "Leonardo",
            "Helena", "Alice", "Laura", "Manuela", "Sophia", "Isabella", "Luiza", "Júlia", "Maria Clara", "Beatriz",
            "Valentina", "Lívia", "Cecília", "Eloá", "Yasmin", "Ana Clara", "Lorena", "Mariana", "Gabriela", "Rebeca"
        };

        private static readonly string[] Sobrenomes =
        {
            "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira", "Lima", "Gomes",
            "Costa", "Ribeiro", "Martins", "Carvalho", "Almeida", "Lopes", "Soares", "Fernandes", "Vieira", "Barbosa",
            "Rocha", "Dias", "Nascimento", "Andrade", "Moreira", "Nunes", "Marques", "Machado", "Mendes", "Freitas"
        };

        private static readonly string[] NomesDosResponsaveis =
        {
            "Ana", "Carlos", "Patrícia", "Roberto", "Sandra", "Marcelo", "Cláudia", "Fernando", "Adriana", "Rogério",
            "Simone", "Paulo", "Luciana", "André", "Renata", "Eduardo", "Tatiane", "Sérgio", "Débora", "Wagner"
        };

        private static readonly string[] Ruas =
        {
            "Rua das Flores", "Avenida Brasil", "Rua Sete de Setembro", "Rua XV de Novembro", "Avenida Paulista",
            "Rua das Acácias", "Rua São João", "Avenida Getúlio Vargas", "Rua Santos Dumont", "Rua da Paz"
        };

        private static readonly string[] Bairros =
        {
            "Centro", "Jardim América", "Vila Nova", "Bela Vista", "Santa Mônica", "Parque das Árvores", "Boa Vista", "Vila Esperança"
        };

        public static async Task<string> SeedAsync(
            AppDbContext contexto,
            ISecurityService servicoDeSeguranca,
            DateOnly hoje,
            string senhaDosProfessores)
        {
            if (await contexto.Usuarios.AnyAsync(usuario => usuario.NomeUsuario == NomeDeUsuarioDeControle))
            {
                return await CompletarAsync(contexto, hoje);
            }

            var categorias = await GarantirCategoriasAsync(contexto);
            var professores = CriarProfessores(contexto, servicoDeSeguranca, senhaDosProfessores);
            await contexto.SaveChangesAsync();

            var turmas = CriarTurmas(contexto, categorias, professores);
            await contexto.SaveChangesAsync();

            var alunos = new List<Aluno>();
            var contratos = new List<Contrato>();
            var parcelas = new List<Parcela>();
            var taxaDePresencaPorAluno = new Dictionary<Guid, double>();
            CriarAlunosComContratos(contexto, categorias, turmas, hoje, alunos, contratos, parcelas, taxaDePresencaPorAluno);
            await contexto.SaveChangesAsync();

            var presencas = CriarPresencas(turmas, alunos, taxaDePresencaPorAluno, hoje);
            var reposicoes = CriarReposicoes(categorias, turmas, alunos, presencas, hoje);
            contexto.Set<Presenca>().AddRange(presencas.Values);
            contexto.Set<Reposicao>().AddRange(reposicoes);
            await contexto.SaveChangesAsync();

            var quantidadeDeCampeonatos = CriarCampeonatos(contexto, categorias, alunos, hoje);
            await contexto.SaveChangesAsync();

            var quantidadeDeInadimplentes = alunos.Count(aluno => aluno.Status == StatusAluno.Inadimplente);
            var quantidadeDeInativos = alunos.Count(aluno => aluno.Status == StatusAluno.Inativo);

            return $"{professores.Count} professores, {turmas.Count} turmas, {alunos.Count} alunos " +
                   $"({quantidadeDeInadimplentes} inadimplentes, {quantidadeDeInativos} inativos), " +
                   $"{contratos.Count} contratos, {parcelas.Count} parcelas, {presencas.Count} presenças, " +
                   $"{reposicoes.Count} reposições, {quantidadeDeCampeonatos} campeonatos.";
        }

        private static async Task<List<Categoria>> GarantirCategoriasAsync(AppDbContext contexto)
        {
            var categorias = await contexto.Set<Categoria>().ToListAsync();

            foreach (var (nome, idadeMinima, idadeMaxima) in CategoriasExtras)
            {
                var jaExiste = categorias.Any(categoria =>
                    categoria.Nome == nome
                    || (idadeMinima <= categoria.IdadeMax && idadeMaxima >= categoria.IdadeMin));

                if (jaExiste) continue;

                var novaCategoria = new Categoria(nome, idadeMinima, idadeMaxima);
                contexto.Set<Categoria>().Add(novaCategoria);
                categorias.Add(novaCategoria);
            }

            await contexto.SaveChangesAsync();

            return categorias.OrderBy(categoria => categoria.IdadeMin).ToList();
        }

        private static List<Professor> CriarProfessores(AppDbContext contexto, ISecurityService servicoDeSeguranca, string senhaDosProfessores)
        {
            var professores = new List<Professor>();

            for (var indice = 0; indice < NomesDosProfessores.Length; indice++)
            {
                var nomeDeUsuario = NomesDeUsuarioDosProfessores[indice];
                var usuario = new Usuario(
                    nomeDeUsuario,
                    $"{nomeDeUsuario}@zezaofutsal.demo",
                    servicoDeSeguranca.HashPassword(senhaDosProfessores),
                    PerfilUsuario.Professor);
                contexto.Set<Usuario>().Add(usuario);

                var professor = new Professor(NomesDosProfessores[indice], $"(11) 98888-000{indice + 1}", usuario.Id);
                contexto.Set<Professor>().Add(professor);
                professores.Add(professor);
            }

            return professores;
        }

        private static List<Turma> CriarTurmas(AppDbContext contexto, List<Categoria> categorias, List<Professor> professores)
        {
            var turmas = new List<Turma>();
            var indiceDaTurma = 0;

            foreach (var categoria in categorias)
            {
                for (var posicao = 0; posicao < TurmasPorCategoria; posicao++)
                {
                    var padraoDeDias = PadroesDeDias[indiceDaTurma % PadroesDeDias.Length];
                    var horario = Horarios[indiceDaTurma % Horarios.Length];
                    var professor = professores[indiceDaTurma % professores.Count];
                    var nome = $"{categoria.Nome} - {padraoDeDias.Replace(",", "/")} {horario}";

                    var turma = new Turma(nome, padraoDeDias, horario, categoria.Id, professor.Id);
                    contexto.Set<Turma>().Add(turma);
                    turmas.Add(turma);
                    indiceDaTurma++;
                }
            }

            return turmas;
        }

        private sealed record CenarioDeContrato(
            DateOnly PrimeiroVencimento,
            int ParcelasEmAtraso,
            bool ContratoCancelado,
            bool ContratoEncerrado,
            bool AlunoInativo);

        private static CenarioDeContrato EscolherCenario(int indiceDoAluno, DateOnly hoje)
        {
            var diasAteVencer = 1 + (indiceDoAluno * 3) % 25;
            var mesesDecorridos = 2 + indiceDoAluno % 7;

            return (indiceDoAluno % 20) switch
            {
                <= 7 => new CenarioDeContrato(hoje.AddDays(diasAteVencer).AddMonths(-mesesDecorridos), 0, false, false, false),
                <= 10 => new CenarioDeContrato(hoje.AddDays(diasAteVencer).AddMonths(-(3 + indiceDoAluno % 4)), 1, false, false, false),
                <= 12 => new CenarioDeContrato(hoje.AddDays(diasAteVencer).AddMonths(-(4 + indiceDoAluno % 3)), 2, false, false, false),
                13 => new CenarioDeContrato(hoje.AddDays(diasAteVencer).AddMonths(-6), 3, false, false, false),
                14 or 15 => new CenarioDeContrato(hoje.AddDays(indiceDoAluno % 4).AddMonths(-mesesDecorridos), 0, false, false, false),
                16 => new CenarioDeContrato(hoje.AddDays(5).AddMonths(-11), 0, false, false, false),
                17 => new CenarioDeContrato(hoje.AddMonths(-13).AddDays(-5), 0, false, true, true),
                18 => new CenarioDeContrato(hoje.AddDays(5 + indiceDoAluno % 20), 0, false, false, false),
                _ => new CenarioDeContrato(hoje.AddDays(8).AddMonths(-4), 0, true, false, true)
            };
        }

        private static void CriarAlunosComContratos(
            AppDbContext contexto,
            List<Categoria> categorias,
            List<Turma> turmas,
            DateOnly hoje,
            List<Aluno> alunos,
            List<Contrato> contratos,
            List<Parcela> parcelas,
            Dictionary<Guid, double> taxaDePresencaPorAluno,
            Dictionary<Guid, int>? jaExistentesPorCategoria = null,
            int indiceInicial = 0,
            HashSet<(string Nome, DateOnly Nascimento)>? nomesJaUsados = null)
        {
            var taxasDePresenca = new[] { 0.35, 0.50, 0.70, 0.90, 0.97 };
            var indiceDoAluno = indiceInicial;
            nomesJaUsados ??= new HashSet<(string, DateOnly)>();

            foreach (var categoria in categorias)
            {
                var turmasDaCategoria = turmas.Where(turma => turma.CategoriaId == categoria.Id).ToList();
                var quantidadeDeIdades = categoria.IdadeMax - categoria.IdadeMin + 1;

                var jaExistentes = jaExistentesPorCategoria != null && jaExistentesPorCategoria.TryGetValue(categoria.Id, out var quantidade) ? quantidade : 0;

                for (var indiceNaCategoria = jaExistentes; indiceNaCategoria < AlunosPorCategoria; indiceNaCategoria++)
                {
                    var idade = categoria.IdadeMin + indiceNaCategoria % quantidadeDeIdades;
                    var dataDeNascimento = hoje.AddYears(-idade).AddDays(-((indiceDoAluno * 37) % 360));

                    var primeiroNome = PrimeirosNomes[(indiceDoAluno * 7) % PrimeirosNomes.Length];
                    var primeiroSobrenome = Sobrenomes[(indiceDoAluno * 11) % Sobrenomes.Length];
                    var segundoSobrenome = Sobrenomes[(indiceDoAluno * 5 + 3) % Sobrenomes.Length];
                    if (segundoSobrenome == primeiroSobrenome)
                    {
                        segundoSobrenome = Sobrenomes[(indiceDoAluno * 5 + 4) % Sobrenomes.Length];
                    }

                    var nomeCompleto = $"{primeiroNome} {primeiroSobrenome} {segundoSobrenome}";
                    if (!nomesJaUsados.Add((nomeCompleto, dataDeNascimento)))
                    {
                        indiceDoAluno++;
                        continue;
                    }

                    var nomeDoResponsavel = NomesDosResponsaveis[(indiceDoAluno * 3) % NomesDosResponsaveis.Length];
                    var telefone = $"(11) 9{1000 + (indiceDoAluno * 137) % 9000}-{1000 + (indiceDoAluno * 251) % 9000}";
                    var endereco = $"{Ruas[indiceDoAluno % Ruas.Length]}, {10 + (indiceDoAluno * 13) % 890} - {Bairros[(indiceDoAluno * 3) % Bairros.Length]}";

                    var aluno = new Aluno(
                        nomeCompleto,
                        dataDeNascimento,
                        (NivelTecnico)(indiceDoAluno % 3),
                        null,
                        $"{nomeDoResponsavel} {primeiroSobrenome}",
                        GerarCpfValido(indiceDoAluno + 1),
                        telefone,
                        endereco,
                        turmasDaCategoria[indiceNaCategoria % turmasDaCategoria.Count].Id,
                        categoria.Id,
                        (PosicaoJogador)(indiceNaCategoria % 5));

                    var cenario = EscolherCenario(indiceDoAluno, hoje);
                    var dataDeInicio = cenario.PrimeiroVencimento.AddDays(-10);

                    var contrato = new Contrato(aluno.Id, dataDeInicio, dataDeInicio.AddYears(1), ValorDaParcela, QuantidadeDeParcelas);
                    if (cenario.ContratoCancelado) contrato.Cancelar();
                    if (cenario.ContratoEncerrado) DefinirValor(contrato, nameof(Contrato.Status), StatusContrato.Encerrado);

                    var parcelasDoContrato = CriarParcelas(contrato, cenario, hoje);

                    DefinirValor(aluno, nameof(Aluno.DataCadastro), dataDeInicio.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc));
                    if (cenario.AlunoInativo)
                    {
                        aluno.Desativar();
                    }
                    else if (parcelasDoContrato.Any(parcela => parcela.Status == StatusParcela.Pendente && parcela.DataVencimento < hoje))
                    {
                        DefinirValor(aluno, nameof(Aluno.Status), StatusAluno.Inadimplente);
                    }

                    taxaDePresencaPorAluno[aluno.Id] = taxasDePresenca[indiceDoAluno % taxasDePresenca.Length];

                    alunos.Add(aluno);
                    contratos.Add(contrato);
                    parcelas.AddRange(parcelasDoContrato);
                    indiceDoAluno++;
                }
            }

            contexto.Set<Aluno>().AddRange(alunos);
            contexto.Set<Contrato>().AddRange(contratos);
            contexto.Set<Parcela>().AddRange(parcelas);
        }

        private static List<Parcela> CriarParcelas(Contrato contrato, CenarioDeContrato cenario, DateOnly hoje)
        {
            var parcelas = new List<Parcela>();

            for (var numero = 1; numero <= QuantidadeDeParcelas; numero++)
            {
                parcelas.Add(new Parcela(contrato.Id, numero, cenario.PrimeiroVencimento.AddMonths(numero - 1), ValorDaParcela));
            }

            var parcelasJaVencidas = parcelas.Where(parcela => parcela.DataVencimento < hoje).OrderBy(parcela => parcela.Numero).ToList();
            var quantidadeEmAtraso = Math.Min(cenario.ParcelasEmAtraso, parcelasJaVencidas.Count);

            foreach (var parcela in parcelasJaVencidas.Take(parcelasJaVencidas.Count - quantidadeEmAtraso))
            {
                parcela.MarcarComoPaga(parcela.DataVencimento.AddDays(-(parcela.Numero % 4)));
            }

            return parcelas;
        }

        private static Dictionary<(Guid AlunoId, Guid TurmaId, DateOnly Data), Presenca> CriarPresencas(
            List<Turma> turmas,
            List<Aluno> alunos,
            Dictionary<Guid, double> taxaDePresencaPorAluno,
            DateOnly hoje)
        {
            var presencas = new Dictionary<(Guid AlunoId, Guid TurmaId, DateOnly Data), Presenca>();
            var aleatorio = new Random(2026);

            for (var indiceDaTurma = 0; indiceDaTurma < turmas.Count; indiceDaTurma++)
            {
                var turma = turmas[indiceDaTurma];
                var alunosDaTurma = alunos
                    .Where(aluno => aluno.TurmaId == turma.Id && aluno.Status != StatusAluno.Inativo)
                    .ToList();

                var datasDeAula = Enumerable.Range(1, SemanasDeHistoricoDePresenca * 7)
                    .Select(dias => hoje.AddDays(-dias))
                    .Where(data => turma.TemAulaEm(data))
                    .OrderBy(data => data)
                    .ToList();

                // Nas turmas de índice par, a última aula fica sem chamada para testar o lançamento.
                if (indiceDaTurma % 2 == 0 && datasDeAula.Count > 0)
                {
                    datasDeAula.RemoveAt(datasDeAula.Count - 1);
                }

                foreach (var data in datasDeAula)
                {
                    foreach (var aluno in alunosDaTurma)
                    {
                        if (data < DateOnly.FromDateTime(aluno.DataCadastro)) continue;

                        var compareceu = aleatorio.NextDouble() < taxaDePresencaPorAluno[aluno.Id];
                        presencas[(aluno.Id, turma.Id, data)] = new Presenca(aluno.Id, turma.Id, data, compareceu);
                    }
                }
            }

            return presencas;
        }

        private static List<Reposicao> CriarReposicoes(
            List<Categoria> categorias,
            List<Turma> turmas,
            List<Aluno> alunos,
            Dictionary<(Guid AlunoId, Guid TurmaId, DateOnly Data), Presenca> presencas,
            DateOnly hoje)
        {
            var reposicoes = new List<Reposicao>();

            foreach (var categoria in categorias)
            {
                var turmasDaCategoria = turmas.Where(turma => turma.CategoriaId == categoria.Id).ToList();
                if (turmasDaCategoria.Count < 2) continue;

                var turmaDeOrigem = turmasDaCategoria[0];
                var turmaDeDestino = turmasDaCategoria[1];
                var alunosDaOrigem = alunos
                    .Where(aluno => aluno.TurmaId == turmaDeOrigem.Id && aluno.Status == StatusAluno.Ativo)
                    .ToList();
                if (alunosDaOrigem.Count < 2) continue;

                foreach (var (aluno, jaAconteceu) in new[] { (alunosDaOrigem[0], true), (alunosDaOrigem[1], false) })
                {
                    var datas = EncontrarDatasDaReposicao(turmaDeOrigem, turmaDeDestino, hoje, jaAconteceu);
                    if (datas == null) continue;

                    var (dataOriginal, dataDaReposicao) = datas.Value;
                    reposicoes.Add(new Reposicao(aluno.Id, turmaDeOrigem.Id, turmaDeDestino.Id, dataOriginal, dataDaReposicao));

                    if (dataOriginal <= hoje)
                    {
                        var chaveDaFalta = (aluno.Id, turmaDeOrigem.Id, dataOriginal);
                        if (presencas.TryGetValue(chaveDaFalta, out var presencaDaFalta))
                            presencaDaFalta.Marcar(false);
                        else
                            presencas[chaveDaFalta] = new Presenca(aluno.Id, turmaDeOrigem.Id, dataOriginal, false);
                    }

                    if (dataDaReposicao <= hoje)
                    {
                        presencas[(aluno.Id, turmaDeDestino.Id, dataDaReposicao)] =
                            new Presenca(aluno.Id, turmaDeDestino.Id, dataDaReposicao, true);
                    }
                }
            }

            return reposicoes;
        }

        private static (DateOnly DataOriginal, DateOnly DataDaReposicao)? EncontrarDatasDaReposicao(
            Turma turmaDeOrigem,
            Turma turmaDeDestino,
            DateOnly hoje,
            bool jaAconteceu)
        {
            var datasPossiveisDaReposicao = Enumerable.Range(2, 40)
                .Select(dias => jaAconteceu ? hoje.AddDays(-dias) : hoje.AddDays(dias))
                .Where(data => turmaDeDestino.TemAulaEm(data));

            foreach (var dataDaReposicao in datasPossiveisDaReposicao)
            {
                var diasNoMes = DateTime.DaysInMonth(dataDaReposicao.Year, dataDaReposicao.Month);
                var dataOriginal = Enumerable.Range(1, diasNoMes)
                    .Select(dia => new DateOnly(dataDaReposicao.Year, dataDaReposicao.Month, dia))
                    .LastOrDefault(data => data < dataDaReposicao && turmaDeOrigem.TemAulaEm(data));

                if (dataOriginal == default) continue;

                return (dataOriginal, dataDaReposicao);
            }

            return null;
        }

        private static int CriarCampeonatos(AppDbContext contexto, List<Categoria> categorias, List<Aluno> alunos, DateOnly hoje)
        {
            var quantidadeDeCampeonatos = 0;
            var indiceDaCategoria = 0;
            var aleatorio = new Random(2026);

            foreach (var categoria in categorias.OrderBy(categoria => categoria.IdadeMin))
            {
                var idsElegiveis = ElegibilidadeDeCategoria.ObterCategoriasElegiveis(categoria, categorias);
                var alunosElegiveis = alunos
                    .Where(aluno => idsElegiveis.Contains(aluno.CategoriaId) && aluno.Status != StatusAluno.Inativo && aluno.Posicao.HasValue)
                    .ToList();

                var alunosDaCategoria = alunosElegiveis.Where(aluno => aluno.CategoriaId == categoria.Id).ToList();
                if (alunosDaCategoria.Count < AlunosPorTimeNoCampeonato * 2) continue;

                var campeonatoRealizado = new Campeonato(
                    $"{PrefixoCopa}{categoria.Nome} {hoje.Year}",
                    hoje.AddDays(-30 - indiceDaCategoria * 3),
                    categoria.Id);
                var campeonatoAgendado = new Campeonato(
                    $"{PrefixoTorneio}{categoria.Nome}",
                    hoje.AddDays(20 + indiceDaCategoria * 3),
                    categoria.Id);
                contexto.Set<Campeonato>().AddRange(campeonatoRealizado, campeonatoAgendado);
                quantidadeDeCampeonatos += 2;

                SimularCampeonatoFinalizado(contexto, campeonatoRealizado, alunosDaCategoria, aleatorio);

                // O torneio agendado fica só com as inscrições (inclui a categoria abaixo), sem times, para demonstrar a criação.
                foreach (var aluno in alunosElegiveis.Take(alunosElegiveis.Count - alunosElegiveis.Count % 2))
                {
                    contexto.Set<ParticipacaoCampeonato>().Add(new ParticipacaoCampeonato(campeonatoAgendado.Id, aluno.Id, null));
                }

                indiceDaCategoria++;
            }

            return quantidadeDeCampeonatos;
        }

        private static void SimularCampeonatoFinalizado(AppDbContext contexto, Campeonato campeonato, List<Aluno> inscritos, Random aleatorio)
        {
            var candidatos = inscritos.Select(aluno => new CandidatoAoSorteio(aluno.Id, aluno.Posicao!.Value)).ToList();
            var sorteio = SorteioDeEquipes.Sortear(candidatos, null, aleatorio);
            if (sorteio.Erro != null || sorteio.Equipes.Count < 2) return;

            var participacoes = new Dictionary<Guid, ParticipacaoCampeonato>();
            foreach (var aluno in inscritos)
            {
                var participacao = new ParticipacaoCampeonato(campeonato.Id, aluno.Id, null);
                participacoes[aluno.Id] = participacao;
                contexto.Set<ParticipacaoCampeonato>().Add(participacao);
            }

            var equipes = new List<EquipeCampeonato>();
            var jogadoresPorEquipe = new Dictionary<Guid, List<Aluno>>();
            for (var indice = 0; indice < sorteio.Equipes.Count; indice++)
            {
                var equipe = new EquipeCampeonato(campeonato.Id, $"Time {indice + 1}");
                equipes.Add(equipe);
                jogadoresPorEquipe[equipe.Id] = new List<Aluno>();

                foreach (var alunoId in sorteio.Equipes[indice])
                {
                    participacoes[alunoId].DefinirEquipe(equipe.Id);
                    jogadoresPorEquipe[equipe.Id].Add(inscritos.First(aluno => aluno.Id == alunoId));
                }
            }
            contexto.Set<EquipeCampeonato>().AddRange(equipes);

            var jogos = new List<JogoCampeonato>();
            foreach (var planejado in ChaveamentoMataMata.Gerar(equipes.Select(equipe => equipe.Id).ToList(), aleatorio))
            {
                var jogo = new JogoCampeonato(campeonato.Id, planejado.Fase, planejado.Ordem);
                if (planejado.EquipeAId.HasValue) jogo.DefinirEquipe(true, planejado.EquipeAId.Value);
                if (planejado.EquipeBId.HasValue) jogo.DefinirEquipe(false, planejado.EquipeBId.Value);
                jogos.Add(jogo);
            }

            foreach (var jogoComBye in jogos.Where(jogo => jogo.Fase == 1 && (jogo.EquipeAId == null) != (jogo.EquipeBId == null)).ToList())
            {
                jogoComBye.FinalizarPorBye((jogoComBye.EquipeAId ?? jogoComBye.EquipeBId)!.Value);
                ChaveamentoMataMata.Avancar(jogos, jogoComBye);
            }

            var estatisticas = new List<EstatisticaJogo>();
            while (true)
            {
                var proximo = jogos
                    .Where(jogo => jogo.Status == StatusJogo.Pendente)
                    .OrderBy(jogo => jogo.Fase).ThenBy(jogo => jogo.Ordem)
                    .FirstOrDefault();
                if (proximo == null) break;

                var linhas = new List<(Guid EquipeId, Aluno Aluno, int Gols, int Assistencias, int Passes, int Desarmes, int Defesas, int Faltas, int Amarelos, int Vermelhos)>();
                foreach (var equipeId in new[] { proximo.EquipeAId!.Value, proximo.EquipeBId!.Value })
                {
                    foreach (var aluno in jogadoresPorEquipe[equipeId])
                    {
                        var ehGoleiro = aluno.Posicao == PosicaoJogador.Goleiro;
                        var ehPivo = aluno.Posicao == PosicaoJogador.Pivo;
                        linhas.Add((
                            equipeId, aluno,
                            ehGoleiro ? 0 : (aleatorio.Next(0, 100) < (ehPivo ? 45 : 25) ? aleatorio.Next(1, 3) : 0),
                            aleatorio.Next(0, 100) < 25 ? 1 : 0,
                            aleatorio.Next(6, 25),
                            ehGoleiro ? 0 : aleatorio.Next(0, 5),
                            ehGoleiro ? aleatorio.Next(2, 9) : 0,
                            aleatorio.Next(0, 4),
                            aleatorio.Next(0, 100) < 10 ? 1 : 0,
                            aleatorio.Next(0, 100) < 2 ? 1 : 0));
                    }
                }

                int GolsDe(Guid equipeId) => linhas.Where(linha => linha.EquipeId == equipeId).Sum(linha => linha.Gols);
                if (GolsDe(proximo.EquipeAId!.Value) == GolsDe(proximo.EquipeBId!.Value))
                {
                    var indiceDoArtilheiro = linhas.FindIndex(linha => linha.EquipeId == proximo.EquipeAId && linha.Aluno.Posicao == PosicaoJogador.Pivo);
                    var linhaDoGolDeOuro = linhas[indiceDoArtilheiro];
                    linhas[indiceDoArtilheiro] = linhaDoGolDeOuro with { Gols = linhaDoGolDeOuro.Gols + 1 };
                }

                foreach (var linha in linhas)
                {
                    estatisticas.Add(new EstatisticaJogo(proximo.Id, linha.Aluno.Id, linha.Gols, linha.Assistencias,
                        linha.Passes, linha.Desarmes, linha.Defesas, linha.Faltas, linha.Amarelos, linha.Vermelhos));
                }

                var golsA = GolsDe(proximo.EquipeAId!.Value);
                var golsB = GolsDe(proximo.EquipeBId!.Value);
                proximo.AtualizarPlacar(golsA, golsB);
                proximo.Finalizar(golsA > golsB ? proximo.EquipeAId.Value : proximo.EquipeBId.Value);
                ChaveamentoMataMata.Avancar(jogos, proximo);
            }

            contexto.Set<JogoCampeonato>().AddRange(jogos);
            contexto.Set<EstatisticaJogo>().AddRange(estatisticas);
        }

        // Completa um banco já populado por uma versão anterior do seeder: posições, mais alunos e campeonatos com times e jogos.
        private static async Task<string> CompletarAsync(AppDbContext contexto, DateOnly hoje)
        {
            var categorias = (await contexto.Set<Categoria>().ToListAsync()).OrderBy(categoria => categoria.IdadeMin).ToList();
            var alunosExistentes = await contexto.Set<Aluno>().ToListAsync();

            var posicoesDefinidas = 0;
            foreach (var grupo in alunosExistentes.GroupBy(aluno => aluno.CategoriaId))
            {
                var indice = 0;
                foreach (var aluno in grupo.OrderBy(a => a.NomeCompleto).ToList())
                {
                    if (aluno.Posicao == null)
                    {
                        DefinirValor(aluno, nameof(Aluno.Posicao), (PosicaoJogador)(indice % 5));
                        posicoesDefinidas++;
                    }
                    indice++;
                }
            }

            var jaExistentesPorCategoria = alunosExistentes.GroupBy(aluno => aluno.CategoriaId).ToDictionary(grupo => grupo.Key, grupo => grupo.Count());
            var turmas = await contexto.Set<Turma>().ToListAsync();
            var nomesJaUsados = alunosExistentes.Select(aluno => (aluno.NomeCompleto, aluno.DataNascimento)).ToHashSet();

            var novosAlunos = new List<Aluno>();
            var contratos = new List<Contrato>();
            var parcelas = new List<Parcela>();
            var taxaDePresencaPorAluno = new Dictionary<Guid, double>();
            CriarAlunosComContratos(contexto, categorias, turmas, hoje, novosAlunos, contratos, parcelas,
                taxaDePresencaPorAluno, jaExistentesPorCategoria, alunosExistentes.Count, nomesJaUsados);
            await contexto.SaveChangesAsync();

            if (novosAlunos.Count > 0)
            {
                contexto.Set<Presenca>().AddRange(CriarPresencas(turmas, novosAlunos, taxaDePresencaPorAluno, hoje).Values);
                await contexto.SaveChangesAsync();
            }

            var quantidadeDeCampeonatos = 0;
            if (!await contexto.Set<EquipeCampeonato>().AnyAsync())
            {
                var antigos = await contexto.Set<Campeonato>()
                    .Where(campeonato => campeonato.Nome.StartsWith(PrefixoCopa) || campeonato.Nome.StartsWith(PrefixoTorneio))
                    .ToListAsync();
                contexto.Set<Campeonato>().RemoveRange(antigos);
                await contexto.SaveChangesAsync();

                var todosOsAlunos = alunosExistentes.Concat(novosAlunos).ToList();
                quantidadeDeCampeonatos = CriarCampeonatos(contexto, categorias, todosOsAlunos, hoje);
                await contexto.SaveChangesAsync();
            }

            if (posicoesDefinidas == 0 && novosAlunos.Count == 0 && quantidadeDeCampeonatos == 0)
            {
                return "Dados de demonstração já existem. Nada foi criado.";
            }

            return $"Dados de demonstração completados: {posicoesDefinidas} posições definidas, {novosAlunos.Count} alunos novos, " +
                   $"{quantidadeDeCampeonatos} campeonatos recriados com times e jogos.";
        }

        private static string GerarCpfValido(int semente)
        {
            var digitosBase = (100000000 + (semente * 7919) % 800000000).ToString();
            var digitos = digitosBase.Select(caractere => caractere - '0').ToList();

            if (digitos.Distinct().Count() == 1) digitos[8] = (digitos[8] + 1) % 10;

            digitos.Add(CalcularDigitoVerificador(digitos));
            digitos.Add(CalcularDigitoVerificador(digitos));

            return string.Concat(digitos);
        }

        private static int CalcularDigitoVerificador(List<int> digitos)
        {
            var peso = digitos.Count + 1;
            var soma = 0;

            foreach (var digito in digitos)
            {
                soma += digito * peso;
                peso--;
            }

            var resto = (soma * 10) % 11;
            return resto == 10 ? 0 : resto;
        }

        private static void DefinirValor(object entidade, string nomeDaPropriedade, object valor)
        {
            entidade.GetType()
                .GetProperty(nomeDaPropriedade, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entidade, valor);
        }
    }
}