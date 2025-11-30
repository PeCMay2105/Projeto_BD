using System;
using Npgsql;
using patrimonioDB.Shared.Database;
using patrimonioDB.Classes;

namespace patrimonioDB.Features.Login
{
    public class LoginRepository
    {
        public Funcionario? BuscarPorLogin(string login)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    string sql = @"
                      SELECT p.id, p.nome, p.login, p.senha, u.data_admissao, u.setor_id, u.salario, u.cargo
                       FROM usuario u 
                          INNER JOIN pessoa p ON u.id_pessoa = p.id 
                        WHERE p.login = @login";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var funcionario = new Funcionario
                                {
                                    Id = reader.GetInt32(0),
                                    Nome = reader.GetString(1),
                                    Login = reader.GetString(2),
                                    Senha = reader.GetString(3),
                                    DataAdmissao = reader.GetDateTime(4),
                                    SetorId = reader.GetInt32(5),
                                    Salario = reader.GetDouble(6),
                                    Id_funcao = reader.GetInt32(7)
                                };
                                return funcionario;
                            }

                            return null; // não encontrou
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro para debug (opcional)
                System.Diagnostics.Debug.WriteLine($"[ERRO LOGIN FUNCIONÁRIO] {ex.Message}");

                // Retorna null em caso de erro (usuário não encontrado ou problema na query)
                return null;
            }
        }

        public Administrador? BuscarAdministradorPorLogin(string login)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    string sql = @"
                     SELECT p.id, p.nome, p.login, p.senha, a.salario
                      FROM administrador a 
                 INNER JOIN pessoa p ON a.id_pessoa = p.id 
                  WHERE p.login = @login";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var administrador = new Administrador
                                {
                                    Id = reader.GetInt32(0),
                                    Nome = reader.GetString(1),
                                    Login = reader.GetString(2),
                                    Senha = reader.GetString(3),
                                    Salario = reader.GetDouble(4)
                                };
                                return administrador;
                            }

                            return null; // não encontrou
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro para debug (opcional)
                System.Diagnostics.Debug.WriteLine($"[ERRO LOGIN ADMINISTRADOR] {ex.Message}");

                // Retorna null em caso de erro (usuário não encontrado ou problema na query)
                return null;
            }
        }
    }
}