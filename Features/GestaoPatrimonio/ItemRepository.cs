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
    /// Trabalha com as tabelas: item, compra, venda, movimentacao
    /// AGORA USANDO STORED PROCEDURES (CALL statement)
    /// </summary>
    public class ItemRepository
    {
        /// <summary>
        /// Adiciona um novo item e registra a compra inicial
        /// USA STORED PROCEDURE: sp_adicionar_item_com_compra
        /// </summary>
        public async Task AdicionarItemAsync(Item item, double preco, int quantidade, int funcionarioId)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
  // Chamar a stored procedure com CALL
             string sql = @"CALL sp_adicionar_item_com_compra(
    @nome, @setorId, @preco, @quantidade, @funcionarioId, 
             @itemId, @mensagem)";

    using (var command = new NpgsqlCommand(sql, connection))
       {
    command.Parameters.AddWithValue("@nome", item.Nome);
     command.Parameters.AddWithValue("@setorId", item.Setor_Id);
          command.Parameters.AddWithValue("@preco", preco);
          command.Parameters.AddWithValue("@quantidade", quantidade);
   command.Parameters.AddWithValue("@funcionarioId", funcionarioId);
        
    // Parâmetros de saída (INOUT) - USAR TEXT
                    var paramItemId = new NpgsqlParameter("@itemId", NpgsqlTypes.NpgsqlDbType.Integer) 
                  { 
             Direction = System.Data.ParameterDirection.InputOutput,
       Value = DBNull.Value
  };
      var paramMensagem = new NpgsqlParameter("@mensagem", NpgsqlTypes.NpgsqlDbType.Text) 
  { 
           Direction = System.Data.ParameterDirection.InputOutput,
     Value = DBNull.Value
        };
   
               command.Parameters.Add(paramItemId);
                  command.Parameters.Add(paramMensagem);

    await command.ExecuteNonQueryAsync();
             
   // Ler valores de saída
      int itemId = paramItemId.Value != DBNull.Value ? (int)paramItemId.Value : 0;
     string mensagem = paramMensagem.Value?.ToString() ?? "Item adicionado";
             
      System.Diagnostics.Debug.WriteLine($"✅ {mensagem}");
          }
            }
        }

        /// <summary>
      /// Registra uma nova compra de um item existente
        /// USA STORED PROCEDURE: sp_registrar_compra
        /// </summary>
    public async Task RegistrarCompraAsync(int idItem, double preco, int quantidade, int funcionarioId)
        {
          using (var connection = DatabaseConnection.GetConnection())
            {
        string sql = @"CALL sp_registrar_compra(
         @idItem, @preco, @quantidade, @funcionarioId, @mensagem)";

      using (var command = new NpgsqlCommand(sql, connection))
     {
       command.Parameters.AddWithValue("@idItem", idItem);
          command.Parameters.AddWithValue("@preco", preco);
    command.Parameters.AddWithValue("@quantidade", quantidade);
            command.Parameters.AddWithValue("@funcionarioId", funcionarioId);
       
     var paramMensagem = new NpgsqlParameter("@mensagem", NpgsqlTypes.NpgsqlDbType.Text) 
               { 
      Direction = System.Data.ParameterDirection.InputOutput,
    Value = DBNull.Value
         };
 command.Parameters.Add(paramMensagem);

           await command.ExecuteNonQueryAsync();
        
        string mensagem = paramMensagem.Value?.ToString() ?? "Compra registrada";
            System.Diagnostics.Debug.WriteLine($"✅ {mensagem}");
  }
 }
        }

        /// <summary>
        /// Registra uma venda de um item
    /// USA STORED PROCEDURE: sp_registrar_venda
        /// </summary>
    public async Task RegistrarVendaAsync(int idItem, double preco, int quantidade, int funcionarioId)
        {
     using (var connection = DatabaseConnection.GetConnection())
{
 string sql = @"CALL sp_registrar_venda(
          @idItem, @preco, @quantidade, @funcionarioId, @mensagem)";

using (var command = new NpgsqlCommand(sql, connection))
    {
     command.Parameters.AddWithValue("@idItem", idItem);
     command.Parameters.AddWithValue("@preco", preco);
            command.Parameters.AddWithValue("@quantidade", quantidade);
  command.Parameters.AddWithValue("@funcionarioId", funcionarioId);

     var paramMensagem = new NpgsqlParameter("@mensagem", NpgsqlTypes.NpgsqlDbType.Text) 
       { 
         Direction = System.Data.ParameterDirection.InputOutput,
    Value = DBNull.Value
     };
 command.Parameters.Add(paramMensagem);

            await command.ExecuteNonQueryAsync();
      
           string mensagem = paramMensagem.Value?.ToString() ?? "Venda registrada";
    System.Diagnostics.Debug.WriteLine($"✅ {mensagem}");
            }
     }
    }

        /// <summary>
        /// Move um item para outro setor registrando a movimentação
        /// USA STORED PROCEDURE: sp_mover_item
   /// </summary>
     public async Task MoverItemAsync(int itemId, int setorOrigemId, int setorDestinoId, int quantidade, int funcionarioId)
        {
         using (var connection = DatabaseConnection.GetConnection())
          {
       string sql = @"CALL sp_mover_item(
          @itemId, @setorOrigem, @setorDestino, @quantidade, @funcionarioId, @mensagem)";

      using (var command = new NpgsqlCommand(sql, connection))
 {
command.Parameters.AddWithValue("@itemId", itemId);
              command.Parameters.AddWithValue("@setorOrigem", setorOrigemId);
           command.Parameters.AddWithValue("@setorDestino", setorDestinoId);
       command.Parameters.AddWithValue("@quantidade", quantidade);
          command.Parameters.AddWithValue("@funcionarioId", funcionarioId);
      
            var paramMensagem = new NpgsqlParameter("@mensagem", NpgsqlTypes.NpgsqlDbType.Text) 
        { 
  Direction = System.Data.ParameterDirection.InputOutput,
          Value = DBNull.Value
          };
            command.Parameters.Add(paramMensagem);

   await command.ExecuteNonQueryAsync();
             
  string mensagem = paramMensagem.Value?.ToString() ?? "Item movido";
          System.Diagnostics.Debug.WriteLine($"✅ {mensagem}");
         }
 }
        }

      /// <summary>
        /// Remove um item vendendo a quantidade especificada
        /// USA STORED PROCEDURE: sp_remover_item_completo
   /// IMPORTANTE: Item NÃO é deletado, apenas registra venda!
        /// </summary>
public async Task RemoverItemAsync(int itemId, int quantidade, int funcionarioId)
        {
    using (var connection = DatabaseConnection.GetConnection())
       {
          string sql = "CALL sp_remover_item_completo(@itemId, @quantidade, @funcionarioId, @mensagem)";

using (var command = new NpgsqlCommand(sql, connection))
                {
    command.Parameters.AddWithValue("@itemId", itemId);
         command.Parameters.AddWithValue("@quantidade", quantidade);
        command.Parameters.AddWithValue("@funcionarioId", funcionarioId);
     
     var paramMensagem = new NpgsqlParameter("@mensagem", NpgsqlTypes.NpgsqlDbType.Text) 
 { 
       Direction = System.Data.ParameterDirection.InputOutput,
 Value = DBNull.Value
             };
           command.Parameters.Add(paramMensagem);

        await command.ExecuteNonQueryAsync();
            
           string mensagem = paramMensagem.Value?.ToString() ?? "Item vendido";
   System.Diagnostics.Debug.WriteLine($"✅ {mensagem}");
                }
     }
        }

        /// <summary>
        /// Lista todos os itens com informações agregadas de compras e vendas
        /// </summary>
        public async Task<List<Item>> ListarItensAsync()
  {
 var itens = new List<Item>();

using (var connection = DatabaseConnection.GetConnection())
       {
           string sql = @"
    SELECT 
         i.id,
              i.nome,
  i.id_setor,
             s.nome as nome_setor,
   COALESCE(SUM(c.quantidade), 0) - COALESCE(SUM(v.quantidade), 0) as quantidade_total,
        COALESCE(AVG(c.preco), 0) as valor_unitario_medio,
            MAX(c.data) as ultima_compra,
   MAX(v.data) as ultima_venda
           FROM item i
  INNER JOIN setor s ON i.id_setor = s.id
         LEFT JOIN compra c ON i.id = c.id_item
        LEFT JOIN venda v ON i.id = v.id_item
   GROUP BY i.id, i.nome, i.id_setor, s.nome
     ORDER BY i.nome";

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
         NomeSetor = reader.GetString(3),
            QuantidadeTotal = reader.GetInt32(4),
              ValorUnitarioMedio = reader.GetDouble(5),
           UltimaCompra = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
              UltimaVenda = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
  };

    itens.Add(item);
            }
    }
            }

     return itens;
        }

        /// <summary>
/// Busca um item específico por ID com informações agregadas
        /// </summary>
     public async Task<Item?> BuscarPorIdAsync(int id)
    {
            using (var connection = DatabaseConnection.GetConnection())
            {
string sql = @"
           SELECT 
          i.id,
        i.nome,
           i.id_setor,
         s.nome as nome_setor,
       COALESCE(SUM(c.quantidade), 0) - COALESCE(SUM(v.quantidade), 0) as quantidade_total,
   COALESCE(AVG(c.preco), 0) as valor_unitario_medio,
MAX(c.data) as ultima_compra,
        MAX(v.data) as ultima_venda
         FROM item i
       INNER JOIN setor s ON i.id_setor = s.id
            LEFT JOIN compra c ON i.id = c.id_item
     LEFT JOIN venda v ON i.id = v.id_item
         WHERE i.id = @id
                  GROUP BY i.id, i.nome, i.id_setor, s.nome";

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
NomeSetor = reader.GetString(3),
    QuantidadeTotal = reader.GetInt32(4),
            ValorUnitarioMedio = reader.GetDouble(5),
            UltimaCompra = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
   UltimaVenda = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
          };
            }

        return null;
            }
                }
    }
        }

        /// <summary>
     /// Lista todos os setores disponíveis
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

        /// <summary>
        /// Lista o histórico de compras de um item
        /// </summary>
        public async Task<List<Compra>> ListarComprasDoItemAsync(int idItem)
   {
            var compras = new List<Compra>();

            using (var connection = DatabaseConnection.GetConnection())
          {
    string sql = @"
  SELECT id, id_item, preco, quantidade, data, id_funcionario
        FROM compra
         WHERE id_item = @idItem
     ORDER BY data DESC";

          using (var command = new NpgsqlCommand(sql, connection))
         {
         command.Parameters.AddWithValue("@idItem", idItem);

       using (var reader = await command.ExecuteReaderAsync())
     {
       while (await reader.ReadAsync())
                {
    var compra = new Compra
            {
        Id = reader.GetInt32(0),
        Id_Item = reader.GetInt32(1),
            Preco = reader.GetDouble(2),
        Quantidade = reader.GetInt32(3),
   Data = reader.GetDateTime(4),
      Id_Funcionario = reader.GetInt32(5)
     };

        compras.Add(compra);
}
}
                }
            }

       return compras;
     }

        /// <summary>
  /// Lista o histórico de vendas de um item
        /// </summary>
public async Task<List<Venda>> ListarVendasDoItemAsync(int idItem)
     {
    var vendas = new List<Venda>();

   using (var connection = DatabaseConnection.GetConnection())
   {
      string sql = @"
             SELECT id, id_item, preco, quantidade, data, id_funcionario
     FROM venda
             WHERE id_item = @idItem
       ORDER BY data DESC";

       using (var command = new NpgsqlCommand(sql, connection))
{
         command.Parameters.AddWithValue("@idItem", idItem);

       using (var reader = await command.ExecuteReaderAsync())
        {
        while (await reader.ReadAsync())
                   {
           var venda = new Venda
           {
           Id = reader.GetInt32(0),
 Id_Item = reader.GetInt32(1),
       Preco = reader.GetDouble(2),
            Quantidade = reader.GetInt32(3),
   Data = reader.GetDateTime(4),
    Id_Funcionario = reader.GetInt32(5)
                     };

        vendas.Add(venda);
         }
   }
      }
          }

    return vendas;
        }

     /// <summary>
        /// Lista o histórico de movimentações de um item
        /// </summary>
      public async Task<List<Movimentacao>> ListarMovimentacoesDoItemAsync(int idItem)
        {
            var movimentacoes = new List<Movimentacao>();

    using (var connection = DatabaseConnection.GetConnection())
    {
           string sql = @"
    SELECT 
           m.id, m.id_item, m.id_setor_origem, m.id_setor_destino, 
        m.quantidade, m.data, m.id_funcionario,
             i.nome as nome_item,
         so.nome as nome_setor_origem,
                    sd.nome as nome_setor_destino
       FROM movimentacao m
                INNER JOIN item i ON m.id_item = i.id
  INNER JOIN setor so ON m.id_setor_origem = so.id
         INNER JOIN setor sd ON m.id_setor_destino = sd.id
        WHERE m.id_item = @idItem
    ORDER BY m.data DESC";

        using (var command = new NpgsqlCommand(sql, connection))
    {
   command.Parameters.AddWithValue("@idItem", idItem);

   using (var reader = await command.ExecuteReaderAsync())
             {
 while (await reader.ReadAsync())
         {
           var movimentacao = new Movimentacao
           {
 Id = reader.GetInt32(0),
  Id_Item = reader.GetInt32(1),
               Id_Setor_Origem = reader.GetInt32(2),
     Id_Setor_Destino = reader.GetInt32(3),
       Quantidade = reader.GetInt32(4),
   Data = reader.GetDateTime(5),
    Id_Funcionario = reader.GetInt32(6),
            NomeItem = reader.GetString(7),
            NomeSetorOrigem = reader.GetString(8),
           NomeSetorDestino = reader.GetString(9)
        };

 movimentacoes.Add(movimentacao);
 }
         }
                }
 }

            return movimentacoes;
        }
    }
}