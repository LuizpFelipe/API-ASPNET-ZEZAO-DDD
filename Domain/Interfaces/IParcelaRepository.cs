using Domain.Entities;

namespace Domain.Interfaces;

public interface IParcelaRepository
{
    Task AdicionarVariasAsync(IEnumerable<Parcela> parcelas);
}
