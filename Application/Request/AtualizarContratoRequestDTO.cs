using System.Text.Json.Serialization;
using Domain.Enums;

namespace Application.Request;

public class AtualizarContratoRequestDTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StatusContrato Status { get; set; }
}
