using System;
using System.Collections.Generic;
using System.Text;

using System;

public class ProfessorResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public bool Status { get; set; }
    public object Data { get; set; } = string.Empty; // Pode ser tipado para uma classe interna com Id, Nome, Telefone, NomeUsuario
}