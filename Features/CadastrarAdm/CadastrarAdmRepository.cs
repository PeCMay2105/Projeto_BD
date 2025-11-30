using Npgsql;
using patrimonioDB.Shared.Database;
using System.Threading.Tasks;
using System;
using System.Data;
using patrimonioDB.Classes;

namespace patrimonioDB.Features.CadastrarAdm
{
    public class CadastrarAdmRepository
    {
        public async Task CadastrarAdministradorAsync(string nome, string cpf, string login, string senha, DateTime nascimento, double salario)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                // Verifica conexão antes de abrir
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Inicia a transação
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. INSERT na tabela Pessoa
                        // Estou assumindo que aqui as colunas são padrão (nome, cpf, etc)
                        // Se der erro aqui também, troque para minúsculo (ex: nome, cpf, login)
                        string sqlPessoa = @"
                            INSERT INTO Pessoa (nome, cpf, login, senha, nascimento)
                            VALUES (@Nome, @Cpf, @Login, @Senha, @Nascimento)
                            RETURNING id;
                        ";

                        int idPessoa;

                        using (var command = new NpgsqlCommand(sqlPessoa, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Nome", nome);
                            command.Parameters.AddWithValue("@Cpf", cpf);
                            command.Parameters.AddWithValue("@Login", login);
                            command.Parameters.AddWithValue("@Senha", senha);
                            command.Parameters.AddWithValue("@Nascimento", nascimento);

                            var result = await command.ExecuteScalarAsync();
                            idPessoa = Convert.ToInt32(result);
                        }

                        // 2. INSERT na tabela Administrador (CORRIGIDO AQUI)
                        // Alterado de IDPessoa para id_pessoa
                        // Alterado de SALARIO para salario (por garantia)
                        string sqlAdm = @"
                            INSERT INTO Administrador (id_pessoa, salario)
                            VALUES (@IDPessoa, @Salario);
                        ";

                        using (var command = new NpgsqlCommand(sqlAdm, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IDPessoa", idPessoa);
                            command.Parameters.AddWithValue("@Salario", salario);

                            await command.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        internal async Task<bool> CpfExisteAsync(string cpf)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Ajustado para id ou count(*) e minúsculo por padrão
                string sql = "SELECT COUNT(1) FROM Pessoa WHERE cpf = @Cpf";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Cpf", cpf);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        internal async Task<bool> LoginExisteAsync(string login)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                string sql = "SELECT COUNT(1) FROM Pessoa WHERE login = @Login";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        internal async Task CriarAdministradorAsync(Administrador admin)
        {
            await CadastrarAdministradorAsync(
                admin.Nome,
                admin.CPF,
                admin.Login,
                admin.Senha,
                admin.Nascimento,
                admin.Salario
            );
        }
    }
}