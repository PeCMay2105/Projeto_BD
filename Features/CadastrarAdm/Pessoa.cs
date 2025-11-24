using System;

namespace patrimonioDB.Features.Cadastro
{
    public class Pessoa
    {
        public int Id { get; set; }
        public string CPF { get; set; } = string.Empty;
        public DateTime Nascimento { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}