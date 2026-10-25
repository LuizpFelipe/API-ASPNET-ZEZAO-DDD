namespace Application.Response
{
    internal class ProfessorDadosDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public Guid UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
