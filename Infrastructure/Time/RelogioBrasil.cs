using Application.Interfaces;

namespace Infrastructure.Time;

public class RelogioBrasil : IRelogio
{
    private static readonly TimeZoneInfo FusoDoBrasil = ResolverFusoDoBrasil();

    public DateOnly HojeNoBrasil()
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoDoBrasil));
    }

    private static TimeZoneInfo ResolverFusoDoBrasil()
    {
        foreach (var identificador in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(identificador);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone("Brasilia", TimeSpan.FromHours(-3), "Brasília", "Brasília");
    }
}