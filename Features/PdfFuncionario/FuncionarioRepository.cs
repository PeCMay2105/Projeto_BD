using Npgsql;
using patrimonioDB.Shared.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace patrimonioDB.Features.PdfFuncionario
{
    /// <summary>
    /// Repository para consultar dados de funcionários via view
    /// </summary>
    public class FuncionarioRepository
    {
        /// <summary>
        /// Busca todos os funcionários da view vw_detalhes_funcionarios
        /// </summary>
        public async Task<List<FuncionarioDetalhado>> ListarFuncionariosDetalhadosAsync()
        {
            var funcionarios = new List<FuncionarioDetalhado>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                // Query direto na VIEW (sem JOIN manual)
                string sql = @"SELECT 
                    ID_Pessoa, Nome_Funcionario, CPF, LOGIN, 
                    Cargo, Setor, Salario, ID_Setor, ID_Funcao
                FROM vw_detalhes_funcionarios
                ORDER BY Nome_Funcionario";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Pegar índices das colunas
                    int idxIdPessoa = reader.GetOrdinal("ID_Pessoa");
                    int idxNome = reader.GetOrdinal("Nome_Funcionario");
                    int idxCpf = reader.GetOrdinal("CPF");
                    int idxLogin = reader.GetOrdinal("LOGIN");
                    int idxCargo = reader.GetOrdinal("Cargo");
                    int idxSetor = reader.GetOrdinal("Setor");
                    int idxSalario = reader.GetOrdinal("Salario");
                    int idxIdSetor = reader.GetOrdinal("ID_Setor");
                    int idxIdFuncao = reader.GetOrdinal("ID_Funcao");

                    while (await reader.ReadAsync())
                    {
                        var funcionario = new FuncionarioDetalhado
                        {
                            ID_Pessoa = reader.GetInt32(idxIdPessoa),
                            Nome_Funcionario = reader.GetString(idxNome),
                            CPF = reader.GetString(idxCpf),
                            LOGIN = reader.GetString(idxLogin),
                            
                            // Campos nullable da view
                            Cargo = reader.IsDBNull(idxCargo) ? null : reader.GetString(idxCargo),
                            Setor = reader.IsDBNull(idxSetor) ? null : reader.GetString(idxSetor),
                            Salario = reader.GetDouble(idxSalario),
                            ID_Setor = reader.IsDBNull(idxIdSetor) ? null : reader.GetInt32(idxIdSetor),
                            ID_Funcao = reader.IsDBNull(idxIdFuncao) ? null : reader.GetInt32(idxIdFuncao)
                        };

                        funcionarios.Add(funcionario);
                    }
                }
            }

            return funcionarios;
        }

        /// <summary>
        /// Busca funcionários de um setor específico
        /// </summary>
        public async Task<List<FuncionarioDetalhado>> ListarPorSetorAsync(int setorId)
        {
            var funcionarios = new List<FuncionarioDetalhado>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = @"SELECT 
                    ID_Pessoa, Nome_Funcionario, CPF, LOGIN, 
                    Cargo, Setor, Salario, ID_Setor, ID_Funcao
                FROM vw_detalhes_funcionarios
                WHERE ID_Setor = @setorId
                ORDER BY Nome_Funcionario";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@setorId", setorId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int idxIdPessoa = reader.GetOrdinal("ID_Pessoa");
                        int idxNome = reader.GetOrdinal("Nome_Funcionario");
                        int idxCpf = reader.GetOrdinal("CPF");
                        int idxLogin = reader.GetOrdinal("LOGIN");
                        int idxCargo = reader.GetOrdinal("Cargo");
                        int idxSetor = reader.GetOrdinal("Setor");
                        int idxSalario = reader.GetOrdinal("Salario");
                        int idxIdSetor = reader.GetOrdinal("ID_Setor");
                        int idxIdFuncao = reader.GetOrdinal("ID_Funcao");

                        while (await reader.ReadAsync())
                        {
                            var funcionario = new FuncionarioDetalhado
                            {
                                ID_Pessoa = reader.GetInt32(idxIdPessoa),
                                Nome_Funcionario = reader.GetString(idxNome),
                                CPF = reader.GetString(idxCpf),
                                LOGIN = reader.GetString(idxLogin),
                                Cargo = reader.IsDBNull(idxCargo) ? null : reader.GetString(idxCargo),
                                Setor = reader.IsDBNull(idxSetor) ? null : reader.GetString(idxSetor),
                                Salario = reader.GetDouble(idxSalario),
                                ID_Setor = reader.IsDBNull(idxIdSetor) ? null : reader.GetInt32(idxIdSetor),
                                ID_Funcao = reader.IsDBNull(idxIdFuncao) ? null : reader.GetInt32(idxIdFuncao)
                            };

                            funcionarios.Add(funcionario);
                        }
                    }
                }
            }

            return funcionarios;
        }
    }
}
