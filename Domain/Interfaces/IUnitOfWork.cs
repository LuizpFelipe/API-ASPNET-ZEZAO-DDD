namespace Domain.Interfaces;

public interface IUnitOfWork
{
    Task ExecutarEmTransacaoAsync(Func<Task> acao);
}
