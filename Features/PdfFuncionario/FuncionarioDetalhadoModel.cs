using System;

namespace patrimonioDB.Features.PdfFuncionario
{
    /// <summary>
    /// Modelo que representa os dados da view vw_detalhes_funcionarios
    /// Usado para gerar relatórios em PDF
    /// </summary>
    public class FuncionarioDetalhado
    {
        public int ID_Pessoa { get; set; }
        public string Nome_Funcionario { get; set; }
        public string CPF { get; set; }
        public string LOGIN { get; set; }
        public string Cargo { get; set; }
        public string Setor { get; set; }
        public double Salario { get; set; }
        public int? ID_Setor { get; set; }
        public int? ID_Funcao { get; set; }

        // Propriedade calculada para formatação
        public string SalarioFormatado => $"R$ {Salario:N2}";
        public string CargoOuSemCargo => string.IsNullOrEmpty(Cargo) ? "Sem cargo" : Cargo;
        public string SetorOuSemSetor => string.IsNullOrEmpty(Setor) ? "Sem setor" : Setor;
    }
}
