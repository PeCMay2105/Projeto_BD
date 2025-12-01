using System;

namespace patrimonioDB.Classes
{
    public class Item
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Setor_Id { get; set; }
        
        // Propriedades para exibição (não estão na tabela item)
        public string NomeSetor { get; set; } = string.Empty;
        public int QuantidadeTotal { get; set; } // Soma das quantidades das compras
        public double ValorUnitarioMedio { get; set; } // Média dos preços de compra
        public DateTime? UltimaCompra { get; set; }
        public DateTime? UltimaVenda { get; set; }

        // Propriedades calculadas
        public double ValorTotalEstoque => QuantidadeTotal * ValorUnitarioMedio;
        public bool TemEstoque => QuantidadeTotal > 0;
    }
}
