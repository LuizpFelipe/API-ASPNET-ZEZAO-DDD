using System;
using System.Collections.Generic;
using System.Text;


namespace Application.Request;
public class CriarProfessorRequestDTO
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string NomeUsuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}