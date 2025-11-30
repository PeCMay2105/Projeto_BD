using Npgsql;
using patrimonioDB.Classes;
using patrimonioDB.Shared.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace patrimonioDB.Features.CriarSetor
{
    public class CriarSetorRepository
    {
        /// <summary>
        /// Adiciona um novo setor ao banco de dados de forma assíncrona.
        /// </summary>
        public async Task AdicionarSetorAsync(string nome)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "INSERT INTO Setor (NOME, NUM_ITENS) VALUES (@Nome, 0)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Nome", nome);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeletarSetorBDAsync(int id)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "DELETE FROM Setor WHERE ID = @id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Setor>> buscarSetores()
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "SELECT ID, NOME, NUM_ITENS FROM Setor";
                var setores = new List<Setor>();
                using (var command = new NpgsqlCommand(sql, connection))
                {
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
        }

        public async Task AtualizarSetorBD(int id, string novoNome)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "UPDATE Setor SET NOME = @novoNome WHERE Setor.ID = @id";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@novoNome", novoNome);
                    command.Parameters.AddWithValue("@id", id);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Verifica de forma assíncrona se um setor com o nome especificado já existe.
        /// A verificação é case-insensitive (ignora maiúsculas/minúsculas).
        /// </summary>
        public async Task<bool> SetorJaExisteAsync(string nome)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = "SELECT COUNT(1) FROM Setor WHERE UPPER(NOME) = UPPER(@Nome)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Nome", nome);

                    // ExecuteScalarAsync é usado para obter um único valor (o resultado do COUNT)
                    var result = await command.ExecuteScalarAsync();

                    // O Npgsql retorna o COUNT como um 'long' (Int64)
                    long count = (result is long) ? (long)result : 0;

                    return count > 0;
                }
            }
        }
    }
}