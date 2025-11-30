using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patrimonioDB.Classes
{
    public class Funcionario : Pessoa
    {
        public int Id_funcao { get; set; }
        public int SetorId { get; set; }
        public string Cargo { get; set; } = string.Empty;
        public double Salario { get; set; }
        public DateTime DataAdmissao { get; set; }
    }
}
