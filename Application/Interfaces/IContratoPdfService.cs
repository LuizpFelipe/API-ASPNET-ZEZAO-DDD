using Application.Configuration;
using Domain.Entities;

namespace Application.Interfaces;

public interface IContratoPdfService
{
    byte[] Gerar(Contrato contrato, Aluno aluno, string nomeCategoria, string nomeTurma, ConfiguracoesContrato configuracoes);
}
