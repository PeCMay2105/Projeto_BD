using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Npgsql;
using patrimonioDB.Shared.Database;
using Windows.System.UserProfile;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace patrimonioDB.Features.GestaoPatrimonio
{
    public class ItemRepository
    {

        public async Task AdicionarObjetoAsync(Item item)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = @"INSERT INTO item (nome,setor_id,data_compra,funcionario_responsavel_id,quantidade,descricao,valor_unitario)
                VALUES 
                (@nome,@setor_id,@data_compra,@funcionario_responsavel_id,@quantidade,@descricao,@valor_unitario)";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nome", item.Nome);
                    command.Parameters.AddWithValue("@setor_id", item.Setor_Id);
                    command.Parameters.AddWithValue("data_compra", item.DataCompra);
                    command.Parameters.AddWithValue("@funcionario_responsavel_id",item.FuncionarioResponsavel_Id);
                    command.Parameters.AddWithValue("@quantidade", item.Quantidade);
                    command.Parameters.AddWithValue("@descricao", item.Descricao);
                    command.Parameters.AddWithValue("@valor_unitario", item.ValorUnitario);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }
        public async Task<List<Item>> ListarObjetosAsync(bool incluirRemovidos = false)
        {
            var itens = new List<Item>();

            using (var connection = DatabaseConnection.GetConnection())
            {
                string sql = @"SELECT o.*, s.nome as nome_setor 
                               FROM item o 
                               INNER JOIN setor s ON o.setor_id = s.id";
                
                
                
              

                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // TODO: Mapear o reader para objeto Objeto
                        var item = new Item
                        {
                             Id = reader.GetInt32(0),
                             Nome = reader.GetString(1),
                             Setor_Id = reader.GetInt32(2),
                             DataCompra = reader.GetDateTime(3),
                             DataRemocao = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                             FuncionarioResponsavel_Id = reader.GetInt32(5),
                             Quantidade = reader.GetInt32(6),
                             Descricao = reader.GetString(7),
                             ValorUnitario = reader.GetDouble(8)

                            // ...
                        };

                        itens.Add(item);
                    }
                }
            }

            return itens;
        }

        /// <summary>
        /// Move um objeto para outro setor
        /// </summary>
        public async Task MoverObjetoAsync(int objetoId, int novoSetorId)
        {
            // TODO: Escrever UPDATE que muda apenas o setor_id
        }

        /// <summary>
        /// Remove um objeto (logicamente) preenchendo data_remocao
        /// </summary>
        public async Task RemoverObjetoAsync(int objetoId)
        {
            // TODO: Escrever UPDATE que preenche data_remocao com a data atual
        }

        /// <summary>
        /// Busca um objeto específico por ID
        /// </summary>
        public async Task<Item?> BuscarPorIdAsync(int id)
        {
            // TODO: SELECT com WHERE id = @id
            return null;
        }
    

    }
}
