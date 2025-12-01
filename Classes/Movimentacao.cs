using System;

namespace patrimonioDB.Classes
{
    public class Movimentacao
    {
    public int Id { get; set; }
  public int Id_Item { get; set; }
 public int Id_Setor_Origem { get; set; }
        public int Id_Setor_Destino { get; set; }
        public int Quantidade { get; set; }
        public DateTime Data { get; set; }
        public int Id_Funcionario { get; set; }

        // Propriedades para exibição
        public string? NomeItem { get; set; }
        public string? NomeSetorOrigem { get; set; }
        public string? NomeSetorDestino { get; set; }
        public string? NomeFuncionario { get; set; }
    }
}
