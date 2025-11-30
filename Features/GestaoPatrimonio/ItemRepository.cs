using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using patrimonioDB.Classes;
using patrimonioDB.Shared.Database;

namespace patrimonioDB.Features.GestaoPatrimonio
{
    /// <summary>
    /// Repositório responsável por todas as operações de banco de dados relacionadas a itens
    /// Contém apenas queries SQL puras, sem lógica de negócio
    /// </summary>
    public class ItemRepository
    {
        /// <summary>
        /// Adiciona um novo item ao banco de dados
        /// O trigger atualiza automaticamente o num_itens do setor
        /// </summary>
        public async Task AdicionarItemAsync(Item item)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = @"INSERT INTO item 
                    (nome, setor_id, data_compra, funcionario_responsavel_id, quantidade, descricao, valor_unitario) 
                    VALUES 
                    (@nome, @setorId, @dataCompra, @funcionarioId, @quantidade, @descricao, @valorUnitario)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    // Adicionar todos os parâmetros para evitar SQL Injection
                    command.Parameters.AddWithValue("@nome", item.Nome);
                    command.Parameters.AddWithValue("@setorId", item.Setor_Id);
                    command.Parameters.AddWithValue("@dataCompra", item.DataCompra);
                    command.Parameters.AddWithValue("@funcionarioId", item.FuncionarioResponsavel_Id);
                    command.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    command.Parameters.AddWithValue("@descricao", item.Descricao ?? string.Empty);
                    command.Parameters.AddWithValue("@valorUnitario", item.ValorUnitario);

                    // ExecuteNonQuery para INSERT/UPDATE/DELETE
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Lista todos os itens com informações do setor via JOIN
        /// </summary>
        /// <param name="incluirRemovidos">Se true, inclui itens com data_remocao preenchida</param>
        public async Task<List<Item>> ListarItensAsync(bool incluirRemovidos = false)
        {
            var itens = new List<Item>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                // Query com JOIN para pegar o nome do setor
                string sql = @"SELECT 
                    i.id, i.nome, i.setor_id, i.data_compra, i.data_remocao,
                    i.funcionario_responsavel_id, i.quantidade, i.descricao, i.valor_unitario,
                    s.nome as nome_setor
                FROM item i 
                INNER JOIN setor s ON i.setor_id = s.id";

                // Filtrar itens removidos se necessário
                if (!incluirRemovidos)
                {
                    sql += " WHERE i.data_remocao IS NULL";
                }

                // Ordenar por nome
                sql += " ORDER BY i.nome";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new Item
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Setor_Id = reader.GetInt32(2),
                            DataCompra = reader.GetDateTime(3),
                            // IsDBNull verifica se o campo é NULL antes de ler
                            DataRemocao = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            FuncionarioResponsavel_Id = reader.GetInt32(5),
                            Quantidade = reader.GetInt32(6),
                            Descricao = reader.GetString(7),
                            ValorUnitario = reader.GetDouble(8),
                        };

                        itens.Add(item);
                    }
                }
            }

            return itens;
        }

        /// <summary>
        /// Busca um item específico por ID
        /// Retorna null se não encontrar
        /// </summary>
        public async Task<Item?> BuscarPorIdAsync(int id)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = @"SELECT 
                    i.id, i.nome, i.setor_id, i.data_compra, i.data_remocao,
                    i.funcionario_responsavel_id, i.quantidade, i.descricao, i.valor_unitario,
                    s.nome as nome_setor
                FROM item i 
                INNER JOIN setor s ON i.setor_id = s.id
                WHERE i.id = @id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Item
                            {
                                Id = reader.GetInt32(0),
                                Nome = reader.GetString(1),
                                Setor_Id = reader.GetInt32(2),
                                DataCompra = reader.GetDateTime(3),
                                DataRemocao = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                FuncionarioResponsavel_Id = reader.GetInt32(5),
                                Quantidade = reader.GetInt32(6),
                                Descricao = reader.GetString(7),
                                ValorUnitario = reader.GetDouble(8),
                            };
                        }

                        return null; // Não encontrou
                    }
                }
            }
        }

        /// <summary>
        /// Move um item para outro setor
        /// O trigger atualiza automaticamente o num_itens dos setores envolvidos
        /// </summary>
        public async Task MoverItemAsync(int itemId, int novoSetorId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "UPDATE item SET setor_id = @novoSetorId WHERE id = @itemId";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@novoSetorId", novoSetorId);
                    command.Parameters.AddWithValue("@itemId", itemId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Remove logicamente um item preenchendo a data_remocao
        /// O trigger atualiza automaticamente o num_itens do setor
        /// </summary>
        public async Task RemoverItemAsync(int itemId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                // CURRENT_DATE é a data atual do PostgreSQL
                string sql = "UPDATE item SET data_remocao = CURRENT_DATE WHERE id = @itemId";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Lista todos os setores disponíveis (para popular ComboBox)
        /// </summary>
        public async Task<List<Setor>> ListarSetoresAsync()
        {
            var setores = new List<Setor>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "SELECT id, nome, num_itens FROM setor ORDER BY nome";

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var setor = new Setor
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            num_itens = reader.GetInt32(2)
                        };

                        setores.Add(setor);
                    }
                }
            }

            return setores;
        }

        /// <summary>
        /// Verifica se um setor existe
        /// </summary>
        public async Task<bool> SetorExisteAsync(int setorId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "SELECT COUNT(1) FROM setor WHERE id = @setorId";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@setorId", setorId);

                    var result = await command.ExecuteScalarAsync();
                    long count = (result is long) ? (long)result : 0;

                    return count > 0;
                }
            }
        }
        /// <summary>
        /// Busca o nome de um setor pelo ID
        /// </summary>
        public async Task<string> BuscarNomeSetorAsync(int setorId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "SELECT nome FROM setor WHERE id = @setorId";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@setorId", setorId);

                    var result = await command.ExecuteScalarAsync();
                    return result?.ToString() ?? "Setor desconhecido";
                }
            }
        }
    }
}