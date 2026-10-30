using Application.Configuration;
using Application.Interfaces;
using Application.Validacoes;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Infrastructure.Pdf;

public class ContratoPdfService : IContratoPdfService
{
    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    private const string CorDaMarca = "#E8541E";
    private const string CorDoTexto = "#262626";
    private const string CorSecundaria = "#6B6B6B";
    private const string CorDoFundo = "#F4F1EE";

    private static readonly byte[]? LogoDaEscola = CarregarLogo();

    private readonly IRelogio _relogio;

    public ContratoPdfService(IRelogio relogio)
    {
        _relogio = relogio;
    }

    private static byte[]? CarregarLogo()
    {
        var caminhoDaLogo = Path.Combine(AppContext.BaseDirectory, "Pdf", "Recursos", "logo-completa.png");
        return File.Exists(caminhoDaLogo) ? File.ReadAllBytes(caminhoDaLogo) : null;
    }

    public byte[] Gerar(Contrato contrato, Aluno aluno, string nomeCategoria, string nomeTurma, ConfiguracoesContrato configuracoes)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var valorDaParcela = contrato.ValorParcela.ToString("C", PtBr);
        var valorTotal = (contrato.ValorParcela * contrato.NumParcelas).ToString("C", PtBr);
        var numeroDoContrato = contrato.Id.ToString("N")[..8].ToUpperInvariant();
        var dataDeEmissao = _relogio.AgoraNoBrasil().ToString("dd 'de' MMMM 'de' yyyy", PtBr);

        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(45);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(10.5f).FontColor(CorDoTexto).LineHeight(1.35f));

                pagina.Header().Element(cabecalho => MontarCabecalho(cabecalho, configuracoes, numeroDoContrato));

                pagina.Content().PaddingVertical(18).Column(coluna =>
                {
                    coluna.Spacing(14);

                    coluna.Item().AlignCenter().Text("CONTRATO DE PRESTAÇÃO DE SERVIÇOS").FontSize(14).Bold();

                    coluna.Item().Row(linha =>
                    {
                        linha.Spacing(12);

                        linha.RelativeItem().Element(caixa => MontarParte(caixa, "CONTRATADA", new[]
                        {
                            configuracoes.NomeEscola,
                            configuracoes.DocumentoEscola,
                            $"Representada por {configuracoes.NomeResponsavelLegal}"
                        }));

                        linha.RelativeItem().Element(caixa => MontarParte(caixa, "CONTRATANTE", new[]
                        {
                            $"{aluno.ResponsavelNome} (responsável pelo aluno)",
                            $"CPF: {CpfValidador.Formatar(aluno.ResponsavelCpf)}",
                            $"Telefone: {aluno.Telefone}",
                            $"Endereço: {aluno.Endereco}"
                        }));
                    });

                    coluna.Item().Row(linha =>
                    {
                        linha.Spacing(8);
                        linha.RelativeItem().Element(caixa => MontarIndicador(caixa, "Parcelas", contrato.NumParcelas.ToString()));
                        linha.RelativeItem().Element(caixa => MontarIndicador(caixa, "Valor da parcela", valorDaParcela));
                        linha.RelativeItem().Element(caixa => MontarIndicador(caixa, "Vencimento", $"Todo dia {configuracoes.DiaVencimento}"));
                        linha.RelativeItem().Element(caixa => MontarIndicador(caixa, "Total", valorTotal));
                    });

                    MontarClausula(coluna, 1, "DO OBJETO",
                        $"A CONTRATADA prestará ao aluno {aluno.NomeCompleto} aulas de futsal na categoria {nomeCategoria}, na turma \"{nomeTurma}\".");

                    MontarClausula(coluna, 2, "DA VIGÊNCIA",
                        $"O presente contrato vigora de {contrato.DataInicio:dd/MM/yyyy} até {contrato.DataFim:dd/MM/yyyy}.");

                    MontarClausula(coluna, 3, "DO VALOR E DO PAGAMENTO",
                        $"O CONTRATANTE pagará {contrato.NumParcelas} parcelas mensais de {valorDaParcela}, " +
                        $"totalizando {valorTotal}, com vencimento todo dia {configuracoes.DiaVencimento} de cada mês.");

                    MontarClausula(coluna, 4, "DO CANCELAMENTO",
                        "O CONTRATANTE poderá cancelar o contrato a qualquer momento, mediante aviso por escrito à CONTRATADA com antecedência mínima de 30 dias. " +
                        "As parcelas vencidas até a data do cancelamento, inclusive as que vencerem durante o aviso prévio, permanecem devidas, e não haverá devolução de parcelas já pagas. " +
                        "O atraso no pagamento por mais de 60 dias autoriza a CONTRATADA a suspender a participação do aluno nas aulas até a regularização.");

                    MontarClausula(coluna, 5, "DAS AULAS E DA REPOSIÇÃO",
                        "Caso o aluno não possa comparecer a uma aula, o CONTRATANTE deverá avisar a CONTRATADA com antecedência mínima de 24 horas. " +
                        "A reposição é concedida em turma da mesma categoria do aluno e deve ser realizada dentro do mesmo mês da aula perdida.");

                    MontarClausula(coluna, 6, "DAS COMUNICAÇÕES",
                        "O CONTRATANTE autoriza a CONTRATADA a enviar avisos de cobrança por WhatsApp e e-mail, a primeira vez 10 dias antes do vencimento " +
                        "e novamente 10 dias após o vencimento, caso a parcela permaneça em aberto. Os dados pessoais do CONTRATANTE e do aluno serão usados " +
                        "apenas para a prestação dos serviços e para a cobrança, nos termos da Lei Geral de Proteção de Dados (Lei nº 13.709/2018).");

                    MontarClausula(coluna, 7, "DO FORO",
                        "Fica eleito o foro da comarca de Maringá, Estado do Paraná, para dirimir quaisquer dúvidas decorrentes deste contrato, " +
                        "com renúncia a qualquer outro, por mais privilegiado que seja.");

                    coluna.Item().PaddingTop(10).AlignRight().Text($"Emitido em {dataDeEmissao}.").FontColor(CorSecundaria);

                    coluna.Item().PaddingTop(30).Row(linha =>
                    {
                        linha.Spacing(30);
                        linha.RelativeItem().Element(assinatura => MontarAssinatura(assinatura, configuracoes.NomeResponsavelLegal, "CONTRATADA"));
                        linha.RelativeItem().Element(assinatura => MontarAssinatura(assinatura, aluno.ResponsavelNome, "CONTRATANTE"));
                    });
                });

                pagina.Footer().AlignCenter().DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(CorSecundaria)).Text(texto =>
                {
                    texto.Span($"{configuracoes.NomeEscola}  •  Contrato nº {numeroDoContrato}  •  Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void MontarCabecalho(IContainer cabecalho, ConfiguracoesContrato configuracoes, string numeroDoContrato)
    {
        cabecalho.BorderBottom(2).BorderColor(CorDaMarca).PaddingBottom(10).Row(linha =>
        {
            linha.Spacing(12);

            if (LogoDaEscola != null)
            {
                linha.ConstantItem(58).Height(66).Image(LogoDaEscola).FitArea();
            }

            linha.RelativeItem().AlignMiddle().Column(coluna =>
            {
                coluna.Item().Text(configuracoes.NomeEscola.ToUpper(PtBr)).FontSize(20).Bold().FontColor(CorDaMarca);
                coluna.Item().Text("Escolinha de futsal").FontSize(9).FontColor(CorSecundaria);
            });

            linha.ConstantItem(130).AlignMiddle().Column(coluna =>
            {
                coluna.Item().AlignRight().Text("CONTRATO Nº").FontSize(8).FontColor(CorSecundaria);
                coluna.Item().AlignRight().Text(numeroDoContrato).FontSize(14).Bold();
            });
        });
    }

    private static void MontarParte(IContainer caixa, string titulo, string[] linhas)
    {
        caixa.Background(CorDoFundo).Padding(10).Column(coluna =>
        {
            coluna.Spacing(2);
            coluna.Item().Text(titulo).FontSize(8).Bold().FontColor(CorDaMarca);
            coluna.Item().Text(linhas[0]).Bold();

            foreach (var linha in linhas.Skip(1))
            {
                coluna.Item().Text(linha);
            }
        });
    }

    private static void MontarIndicador(IContainer caixa, string rotulo, string valor)
    {
        caixa.Background(CorDoFundo).Padding(8).Column(coluna =>
        {
            coluna.Item().Text(rotulo).FontSize(8).FontColor(CorSecundaria);
            coluna.Item().Text(valor).FontSize(12).Bold();
        });
    }

    private static void MontarClausula(ColumnDescriptor coluna, int numero, string titulo, string texto)
    {
        coluna.Item().Column(clausula =>
        {
            clausula.Spacing(3);
            clausula.Item().Text($"CLÁUSULA {numero}ª — {titulo}").FontSize(10).Bold().FontColor(CorDaMarca);
            clausula.Item().Text(texto).Justify();
        });
    }

    private static void MontarAssinatura(IContainer assinatura, string nome, string papel)
    {
        assinatura.Column(coluna =>
        {
            coluna.Item().AlignCenter().Text("______________________________");
            coluna.Item().AlignCenter().Text(nome).Bold();
            coluna.Item().AlignCenter().Text(papel).FontSize(8).FontColor(CorSecundaria);
        });
    }
}