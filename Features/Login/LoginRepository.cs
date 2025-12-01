using System;
using Npgsql;
using patrimonioDB.Shared.Database;
using patrimonioDB.Shared.Security;
using patrimonioDB.Classes;
using System.Diagnostics;

namespace patrimonioDB.Features.Login
{
    public class LoginRepository
    {
     /// <summary>
        /// Busca usuário por login e retorna (usuario, isAdmin)
   /// </summary>
        public (Classes.Funcionario? usuario, bool isAdmin) BuscarPorLogin(string login)
     {
       using (var connection = DatabaseConnection.GetConnection())
  {
   // ✅ PASSO 1: Tentar buscar como Funcionário
  string sqlFuncionario = @"SELECT 
      p.ID, p.NOME, p.CPF, p.NASCIMENTO, p.LOGIN, p.SENHA,
  f.ID_FUNCAO, f.ID_SETOR, f.SALARIO, f.DATA_ADMISSAO,
       fn.NOME as CARGO
     FROM Pessoa p
       INNER JOIN Funcionario f ON p.ID = f.ID_PESSOA
   LEFT JOIN Funcao fn ON f.ID_FUNCAO = fn.ID
      WHERE UPPER(p.LOGIN) = UPPER(@login)";

  using (var command = new NpgsqlCommand(sqlFuncionario, connection))
    {
  command.Parameters.AddWithValue("@login", login);

           using (var reader = command.ExecuteReader())
      {
      if (reader.Read())
      {
   var funcionario = new Classes.Funcionario
     {
         Id = reader.GetInt32(0),
         Nome = reader.GetString(1),
       CPF = reader.GetString(2),
       Nascimento = reader.GetDateTime(3),
Login = reader.GetString(4),
 Senha = reader.GetString(5),
     Id_funcao = reader.GetInt32(6),
    SetorId = reader.GetInt32(7),
    Salario = reader.GetDouble(8),
        DataAdmissao = reader.GetDateTime(9),
      Cargo = reader.IsDBNull(10) ? "Sem Cargo" : reader.GetString(10)
        };

      Debug.WriteLine($"✓ Funcionário encontrado: {funcionario.Nome} (Login: {funcionario.Login})");
   return (funcionario, false);  // ✅ false = não é admin
       }
        }
    }

      // ✅ PASSO 2: Se não encontrou como Funcionário, tentar buscar como Administrador
         string sqlAdmin = @"SELECT 
     p.ID, p.NOME, p.CPF, p.NASCIMENTO, p.LOGIN, p.SENHA,
   a.SALARIO
        FROM Pessoa p
      INNER JOIN Administrador a ON p.ID = a.ID_PESSOA
    WHERE UPPER(p.LOGIN) = UPPER(@login)";

   using (var command = new NpgsqlCommand(sqlAdmin, connection))
     {
            command.Parameters.AddWithValue("@login", login);

         using (var reader = command.ExecuteReader())
          {
           if (reader.Read())
   {
              // Retornar como Funcionario mas marcado como Admin
   var admin = new Classes.Funcionario
       {
        Id = reader.GetInt32(0),
  Nome = reader.GetString(1),
          CPF = reader.GetString(2),
        Nascimento = reader.GetDateTime(3),
 Login = reader.GetString(4),
        Senha = reader.GetString(5),
           Salario = reader.GetDouble(6),
      Cargo = "Administrador",
         Id_funcao = 0,  // Admin não tem função
       SetorId = 0,    // Admin não tem setor
  DataAdmissao = reader.GetDateTime(3) // Usar data de nascimento como aproximação
            };

       Debug.WriteLine($"✓ Administrador encontrado: {admin.Nome} (Login: {admin.Login})");
     return (admin, true);  // ✅ true = é admin
}
        }
     }
     }

            Debug.WriteLine($"✗ Nenhum usuário encontrado com login: {login}");
     return (null, false);
        }
    }
}