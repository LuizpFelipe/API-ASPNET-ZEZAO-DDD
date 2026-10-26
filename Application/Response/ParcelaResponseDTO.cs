using System;

namespace Application.Response
{
    public class ParcelaResponseDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}