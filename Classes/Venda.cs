using System;

namespace patrimonioDB.Classes
{
    public class Venda
    {
   public int Id { get; set; }
     public int Id_Item { get; set; }
        public double Preco { get; set; }
   public int Quantidade { get; set; }
        public DateTime Data { get; set; }
     public int Id_Funcionario { get; set; }

        // Propriedades calculadas
  public double ValorTotal => Preco * Quantidade;
    }
}
