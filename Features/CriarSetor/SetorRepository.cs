using Npgsql;
using patrimonioDB.Shared.Database;
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
                int id = 1;
                string sql = "INSERT INTO Setor (ID, NOME, NUM_ITENS) VALUES (@Id, @Nome, 0)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Nome", nome);
                    command.Parameters.AddWithValue("@Id", 2);

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