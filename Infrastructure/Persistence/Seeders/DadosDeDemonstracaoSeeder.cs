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
        private const int AlunosPorCategoria = 10;
        private const int TurmasPorCategoria = 2;
        private const int SemanasDeHistoricoDePresenca = 8;
        private const int AlunosPorTimeNoCampeonato = 4;

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
                return "Dados de demonstração já existem. Nada foi criado.";
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
            Dictionary<Guid, double> taxaDePresencaPorAluno)
        {
            var taxasDePresenca = new[] { 0.35, 0.50, 0.70, 0.90, 0.97 };
            var indiceDoAluno = 0;

            foreach (var categoria in categorias)
            {
                var turmasDaCategoria = turmas.Where(turma => turma.CategoriaId == categoria.Id).ToList();
                var quantidadeDeIdades = categoria.IdadeMax - categoria.IdadeMin + 1;

                for (var indiceNaCategoria = 0; indiceNaCategoria < AlunosPorCategoria; indiceNaCategoria++)
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

                    var nomeDoResponsavel = NomesDosResponsaveis[(indiceDoAluno * 3) % NomesDosResponsaveis.Length];
                    var telefone = $"(11) 9{1000 + (indiceDoAluno * 137) % 9000}-{1000 + (indiceDoAluno * 251) % 9000}";
                    var endereco = $"{Ruas[indiceDoAluno % Ruas.Length]}, {10 + (indiceDoAluno * 13) % 890} - {Bairros[(indiceDoAluno * 3) % Bairros.Length]}";

                    var aluno = new Aluno(
                        $"{primeiroNome} {primeiroSobrenome} {segundoSobrenome}",
                        dataDeNascimento,
                        (NivelTecnico)(indiceDoAluno % 3),
                        null,
                        $"{nomeDoResponsavel} {primeiroSobrenome}",
                        GerarCpfValido(indiceDoAluno + 1),
                        telefone,
                        endereco,
                        turmasDaCategoria[indiceNaCategoria % turmasDaCategoria.Count].Id,
                        categoria.Id);

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

            foreach (var categoria in categorias)
            {
                var alunosDaCategoria = alunos
                    .Where(aluno => aluno.CategoriaId == categoria.Id && aluno.Status != StatusAluno.Inativo)
                    .ToList();

                if (alunosDaCategoria.Count < AlunosPorTimeNoCampeonato * 2) continue;

                var campeonatoRealizado = new Campeonato(
                    $"Copa Zezão {categoria.Nome} {hoje.Year}",
                    hoje.AddDays(-30 - indiceDaCategoria * 3),
                    categoria.Id);
                var campeonatoAgendado = new Campeonato(
                    $"Torneio Interclasses {categoria.Nome}",
                    hoje.AddDays(20 + indiceDaCategoria * 3),
                    categoria.Id);
                contexto.Set<Campeonato>().AddRange(campeonatoRealizado, campeonatoAgendado);
                quantidadeDeCampeonatos += 2;

                for (var ordem = 0; ordem < alunosDaCategoria.Count; ordem++)
                {
                    var aluno = alunosDaCategoria[ordem];
                    var time = ordem / AlunosPorTimeNoCampeonato;

                    var posicao = time switch
                    {
                        0 => "Campeão",
                        1 => "Vice-campeão",
                        2 => "3º lugar",
                        _ => "Fase de grupos"
                    };
                    var jogosDisputados = time <= 1 ? 6 : time == 2 ? 5 : 3;
                    var aproveitamentoBase = time switch { 0 => 90, 1 => 75, 2 => 60, _ => 35 };

                    var participacaoRealizada = new ParticipacaoCampeonato(campeonatoRealizado.Id, aluno.Id, posicao);
                    participacaoRealizada.Atualizar(posicao, jogosDisputados, aproveitamentoBase + (ordem % 3) * 3);
                    contexto.Set<ParticipacaoCampeonato>().Add(participacaoRealizada);

                    if (ordem % 5 != 4)
                    {
                        contexto.Set<ParticipacaoCampeonato>().Add(new ParticipacaoCampeonato(campeonatoAgendado.Id, aluno.Id, null));
                    }
                }

                indiceDaCategoria++;
            }

            return quantidadeDeCampeonatos;
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