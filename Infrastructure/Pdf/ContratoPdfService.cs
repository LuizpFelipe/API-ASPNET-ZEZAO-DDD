using System.Globalization;
using Application.Configuration;
using Application.Interfaces;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Pdf;

public class ContratoPdfService : IContratoPdfService
{
    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    public byte[] Gerar(Contrato contrato, Aluno aluno, string nomeCategoria, string nomeTurma, ConfiguracoesContrato configuracoes)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var valor = contrato.ValorParcela.ToString("C", PtBr);

        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(40);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(11));

                pagina.Content().Column(coluna =>
                {
                    coluna.Spacing(12);

                    coluna.Item().AlignCenter().Text("CONTRATO DE PRESTAÇÃO DE SERVIÇOS").FontSize(16).Bold();

                    Clausula(coluna, "PARTES:",
                        $"Escola: {configuracoes.NomeEscola}, documento {configuracoes.DocumentoEscola}, " +
                        $"representada por {configuracoes.NomeResponsavelLegal}. " +
                        $"Responsável: {aluno.ResponsavelNome}, responsável pelo aluno {aluno.NomeCompleto}.");

                    Clausula(coluna, "OBJETO:",
                        $"Aulas de futsal para o aluno {aluno.NomeCompleto}, categoria {nomeCategoria}, turma {nomeTurma}.");

                    Clausula(coluna, "VIGÊNCIA:",
                        $"De {contrato.DataInicio:dd/MM/yyyy} até {contrato.DataFim:dd/MM/yyyy}.");

                    Clausula(coluna, "VALOR:",
                        $"{contrato.NumParcelas} parcelas mensais de {valor}, com vencimento todo dia {configuracoes.DiaVencimento}.");

                    Clausula(coluna, "CONDIÇÕES DE CANCELAMENTO:", "A DEFINIR pela escola.");

                    Clausula(coluna, "FORO:", "A DEFINIR pela escola.");

                    coluna.Item().PaddingTop(40).Row(linha =>
                    {
                        linha.RelativeItem().Column(assinatura =>
                        {
                            assinatura.Item().AlignCenter().Text("______________________________");
                            assinatura.Item().AlignCenter().Text(configuracoes.NomeResponsavelLegal);
                        });

                        linha.RelativeItem().Column(assinatura =>
                        {
                            assinatura.Item().AlignCenter().Text("______________________________");
                            assinatura.Item().AlignCenter().Text(aluno.ResponsavelNome);
                        });
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void Clausula(ColumnDescriptor coluna, string titulo, string texto)
    {
        coluna.Item().Text(conteudo =>
        {
            conteudo.Span(titulo + " ").Bold();
            conteudo.Span(texto);
        });
    }
}
