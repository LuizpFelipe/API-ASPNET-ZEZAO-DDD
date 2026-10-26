using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class AvisoCobranca
    {
        public Guid Id { get; private set; }
        public Guid ParcelaId { get; private set; }
        public Parcela Parcela { get; private set; } = null!;
        public TipoAvisoCobranca Tipo { get; private set; }
        public DateTime DataEnvio { get; private set; }
        public string Destinatario { get; private set; } = string.Empty;

        protected AvisoCobranca() { }

        public AvisoCobranca(Guid parcelaId, TipoAvisoCobranca tipo, string destinatario)
        {
            Id = Guid.NewGuid();
            ParcelaId = parcelaId;
            Tipo = tipo;
            DataEnvio = DateTime.UtcNow;
            Destinatario = destinatario;
        }
    }
}