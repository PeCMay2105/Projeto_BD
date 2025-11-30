using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using patrimonioDB.Classes;
using patrimonioDB.Shared.Database;

namespace patrimonioDB.Features.GestaoFuncionarios
{
    public class FuncionarioRepository
  {
        /// <summary>
    /// Cadastra um novo funcionário no banco de dados
 /// </summary>
   public async Task CadastrarFuncionarioAsync(Pessoa pessoa, int setorId, int funcaoId, double salario, DateTime dataAdmissao)
        {
    using (var connection = DatabaseConnection.GetConnection())
  {
       using (var transaction = await connection.BeginTransactionAsync())
           {
 try
  {
// 1. INSERT na tabela Pessoa
     string sqlPessoa = @"
INSERT INTO pessoa (nome, cpf, login, senha, nascimento)
    VALUES (@Nome, @Cpf, @Login, @Senha, @Nascimento)
RETURNING id";

    int idPessoa;
         using (var command = new NpgsqlCommand(sqlPessoa, connection, transaction))
       {
    command.Parameters.AddWithValue("@Nome", pessoa.Nome);
      command.Parameters.AddWithValue("@Cpf", pessoa.CPF);
       command.Parameters.AddWithValue("@Login", pessoa.Login);
    command.Parameters.AddWithValue("@Senha", pessoa.Senha);
      command.Parameters.AddWithValue("@Nascimento", pessoa.Nascimento);

  var result = await command.ExecuteScalarAsync();
  idPessoa = Convert.ToInt32(result);
   }

   // 2. INSERT na tabela Funcionario
    string sqlFuncionario = @"
INSERT INTO funcionario (id_pessoa, id_setor, id_funcao, salario, data_admissao)
  VALUES (@IdPessoa, @SetorId, @FuncaoId, @Salario, @DataAdmissao)";

   using (var command = new NpgsqlCommand(sqlFuncionario, connection, transaction))
      {
     command.Parameters.AddWithValue("@IdPessoa", idPessoa);
  command.Parameters.AddWithValue("@SetorId", setorId);
   command.Parameters.AddWithValue("@FuncaoId", funcaoId);
 command.Parameters.AddWithValue("@Salario", salario);
command.Parameters.AddWithValue("@DataAdmissao", dataAdmissao);

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

        /// <summary>
      /// Verifica se um CPF já existe no banco
  /// </summary>
public async Task<bool> CpfExisteAsync(string cpf)
   {
 try
         {
    using (var connection = DatabaseConnection.GetConnection())
    {
     string sql = "SELECT COUNT(1) FROM pessoa WHERE cpf = @Cpf";

     using (var command = new NpgsqlCommand(sql, connection))
      {
      command.Parameters.AddWithValue("@Cpf", cpf);
     var count = Convert.ToInt32(await command.ExecuteScalarAsync());
    return count > 0;
         }
  }
  }
      catch
      {
 return false;
    }
  }

        /// <summary>
      /// Verifica se um login já existe no banco
     /// </summary>
        public async Task<bool> LoginExisteAsync(string login)
        {
try
  {
  using (var connection = DatabaseConnection.GetConnection())
        {
   string sql = "SELECT COUNT(1) FROM pessoa WHERE login = @Login";

 using (var command = new NpgsqlCommand(sql, connection))
    {
   command.Parameters.AddWithValue("@Login", login);
      var count = Convert.ToInt32(await command.ExecuteScalarAsync());
 return count > 0;
      }
 }
       }
  catch
 {
       return false;
  }
}

/// <summary>
     /// Lista todas as funções disponíveis
    /// </summary>
        public async Task<List<Funcao>> ListarFuncoesAsync()
    {
         var funcoes = new List<Funcao>();

 try
   {
      using (var connection = DatabaseConnection.GetConnection())
     {
    string sql = "SELECT id, nome, descricao FROM funcao ORDER BY nome";

 using (var command = new NpgsqlCommand(sql, connection))
     using (var reader = await command.ExecuteReaderAsync())
   {
       while (await reader.ReadAsync())
    {
      var funcao = new Funcao
      {
       Id = reader.GetInt32(0),
       Nome = reader.GetString(1),
      Descricao = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
 };

     funcoes.Add(funcao);
          }
 }
     }
            }
   catch (Exception ex)
      {
     System.Diagnostics.Debug.WriteLine($"[ERRO LISTAR FUNÇÕES] {ex.Message}");
}

  return funcoes;
    }

     /// <summary>
  /// Lista todos os funcionários
    /// </summary>
        public async Task<List<Funcionario>> ListarFuncionariosAsync()
    {
    var funcionarios = new List<Funcionario>();

      try
 {
     using (var connection = DatabaseConnection.GetConnection())
   {
  string sql = @"
       SELECT p.id, p.nome, p.cpf, p.login, p.senha, p.nascimento,
   f.id_setor, f.id_funcao, f.salario, f.data_admissao, func.nome as nome_funcao
   FROM funcionario f
       INNER JOIN pessoa p ON f.id_pessoa = p.id
       LEFT JOIN funcao func ON f.id_funcao = func.id
    ORDER BY p.nome";

     using (var command = new NpgsqlCommand(sql, connection))
     using (var reader = await command.ExecuteReaderAsync())
       {
    while (await reader.ReadAsync())
    {
  var funcionario = new Funcionario
  {
   Id = reader.GetInt32(0),
         Nome = reader.GetString(1),
  CPF = reader.GetString(2),
  Login = reader.GetString(3),
 Senha = reader.GetString(4),
       Nascimento = reader.GetDateTime(5),
     SetorId = reader.GetInt32(6),
 Id_funcao = reader.GetInt32(7),
    Salario = reader.GetDouble(8),
        DataAdmissao = reader.GetDateTime(9),
  Cargo = reader.IsDBNull(10) ? "Sem funcao" : reader.GetString(10)
         };

   funcionarios.Add(funcionario);
         }
 }
  }
      }
  catch (Exception ex)
{
   System.Diagnostics.Debug.WriteLine($"[ERRO LISTAR FUNCIONÁRIOS] {ex.Message}");
    }

   return funcionarios;
   }

     /// <summary>
  /// Busca um funcionário por ID
  /// </summary>
  public async Task<Funcionario?> BuscarPorIdAsync(int id)
        {
  try
   {
   using (var connection = DatabaseConnection.GetConnection())
 {
  string sql = @"
  SELECT p.id, p.nome, p.cpf, p.login, p.senha, p.nascimento,
          f.id_setor, f.id_funcao, f.salario, f.data_admissao, func.nome as nome_funcao
    FROM funcionario f
   INNER JOIN pessoa p ON f.id_pessoa = p.id
 LEFT JOIN funcao func ON f.id_funcao = func.id
    WHERE p.id = @Id";

 using (var command = new NpgsqlCommand(sql, connection))
{
     command.Parameters.AddWithValue("@Id", id);

  using (var reader = await command.ExecuteReaderAsync())
   {
 if (await reader.ReadAsync())
       {
     return new Funcionario
  {
    Id = reader.GetInt32(0),
        Nome = reader.GetString(1),
     CPF = reader.GetString(2),
    Login = reader.GetString(3),
   Senha = reader.GetString(4),
      Nascimento = reader.GetDateTime(5),
    SetorId = reader.GetInt32(6),
   Id_funcao = reader.GetInt32(7),
   Salario = reader.GetDouble(8),
     DataAdmissao = reader.GetDateTime(9),
     Cargo = reader.IsDBNull(10) ? "Sem funcao" : reader.GetString(10)
       };
       }

  return null;
    }
 }
 }
       }
  catch (Exception ex)
  {
      System.Diagnostics.Debug.WriteLine($"[ERRO BUSCAR FUNCIONÁRIO] {ex.Message}");
  return null;
    }
        }

/// <summary>
 /// Atualiza dados de um funcionário
    /// </summary>
  public async Task AtualizarFuncionarioAsync(int id, Pessoa pessoa, int setorId, int funcaoId, double salario)
     {
using (var connection = DatabaseConnection.GetConnection())
   {
   using (var transaction = await connection.BeginTransactionAsync())
        {
         try
       {
 // 1. UPDATE na tabela Pessoa
   string sqlPessoa = @"
   UPDATE pessoa 
      SET nome = @Nome, cpf = @Cpf, login = @Login, 
      senha = @Senha, nascimento = @Nascimento
        WHERE id = @Id";

       using (var command = new NpgsqlCommand(sqlPessoa, connection, transaction))
        {
  command.Parameters.AddWithValue("@Id", id);
           command.Parameters.AddWithValue("@Nome", pessoa.Nome);
   command.Parameters.AddWithValue("@Cpf", pessoa.CPF);
   command.Parameters.AddWithValue("@Login", pessoa.Login);
    command.Parameters.AddWithValue("@Senha", pessoa.Senha);
   command.Parameters.AddWithValue("@Nascimento", pessoa.Nascimento);

       await command.ExecuteNonQueryAsync();
      }

  // 2. UPDATE na tabela Funcionario
   string sqlFuncionario = @"
          UPDATE funcionario 
 SET id_setor = @SetorId, id_funcao = @FuncaoId, salario = @Salario
      WHERE id_pessoa = @IdPessoa";

       using (var command = new NpgsqlCommand(sqlFuncionario, connection, transaction))
  {
       command.Parameters.AddWithValue("@IdPessoa", id);
     command.Parameters.AddWithValue("@SetorId", setorId);
        command.Parameters.AddWithValue("@FuncaoId", funcaoId);
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

 /// <summary>
    /// Remove um funcionário do banco de dados
  /// </summary>
     public async Task RemoverFuncionarioAsync(int id)
  {
   using (var connection = DatabaseConnection.GetConnection())
    {
    using (var transaction = await connection.BeginTransactionAsync())
  {
try
  {
        // 1. DELETE da tabela Funcionario
          string sqlFuncionario = "DELETE FROM funcionario WHERE id_pessoa = @Id";

 using (var command = new NpgsqlCommand(sqlFuncionario, connection, transaction))
     {
   command.Parameters.AddWithValue("@Id", id);
     await command.ExecuteNonQueryAsync();
      }

  // 2. DELETE da tabela Pessoa
          string sqlPessoa = "DELETE FROM pessoa WHERE id = @Id";

using (var command = new NpgsqlCommand(sqlPessoa, connection, transaction))
  {
      command.Parameters.AddWithValue("@Id", id);
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
    }
}
