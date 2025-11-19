using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patrimonioDB.Features.GestaoPatrimonio
{
    public class Item
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Setor_Id { get; set; }
        public DateTime DataCompra {  get; set; }
        public DateTime? DataRemocao { get; set; }
        public int FuncionarioResponsavel_Id { get; set; }
        public int Quantidade { get; set; }
        public string Descricao { get; set; }
        public double ValorUnitario { get; set; }

        public double ValorTotal => Quantidade * ValorUnitario;
        public bool EstaAtivo => DataRemocao == null;
    }
}
