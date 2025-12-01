using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using patrimonioDB.Classes;
using patrimonioDB.Shared.Database;

namespace patrimonioDB.Features.PdfFuncionario
{
    /// <summary>
    /// Repository para gerenciar documentos PDF no banco de dados
    /// </summary>
    public class DocumentoRepository
    {
        /// <summary>
        /// Salva um arquivo PDF no banco de dados
        /// </summary>
        public async Task<int> SalvarPdfAsync(string nome, byte[] arquivoPdf)
        {
 using (var connection = DatabaseConnection.GetConnection())
            {
   string sql = @"
         INSERT INTO documentos (nome, arquivo_pdf)
       VALUES (@nome, @arquivo)
              RETURNING id";

  using (var command = new NpgsqlCommand(sql, connection))
                {
        command.Parameters.AddWithValue("@nome", nome);
            command.Parameters.AddWithValue("@arquivo", arquivoPdf);

          var result = await command.ExecuteScalarAsync();
      return Convert.ToInt32(result);
                }
       }
     }

        /// <summary>
  /// Salva um arquivo PDF a partir de um caminho
        /// </summary>
        public async Task<int> SalvarPdfAsync(string nome, string caminhoArquivo)
        {
            byte[] arquivoPdf = await File.ReadAllBytesAsync(caminhoArquivo);
            return await SalvarPdfAsync(nome, arquivoPdf);
        }

        /// <summary>
    /// Lista todos os documentos (sem carregar o conteúdo)
     /// </summary>
        public async Task<List<Documento>> ListarDocumentosAsync()
        {
            var documentos = new List<Documento>();

   using (var connection = DatabaseConnection.GetConnection())
            {
           string sql = @"
              SELECT id, nome, 
     LENGTH(arquivo_pdf) as tamanho
         FROM documentos
       ORDER BY id DESC";

      using (var command = new NpgsqlCommand(sql, connection))
     using (var reader = await command.ExecuteReaderAsync())
       {
          while (await reader.ReadAsync())
     {
          var doc = new Documento
   {
            Id = reader.GetInt32(0),
       Nome = reader.GetString(1),
          ArquivoPdf = new byte[reader.GetInt32(2)] // Apenas tamanho
               };

         documentos.Add(doc);
                 }
         }
    }

 return documentos;
   }

        /// <summary>
        /// Busca um documento específico (COM conteúdo)
        /// </summary>
        public async Task<Documento?> BuscarPorIdAsync(int id)
        {
            using (var connection = DatabaseConnection.GetConnection())
        {
        string sql = @"
      SELECT id, nome, arquivo_pdf
          FROM documentos
          WHERE id = @id";

using (var command = new NpgsqlCommand(sql, connection))
         {
 command.Parameters.AddWithValue("@id", id);

  using (var reader = await command.ExecuteReaderAsync())
        {
        if (await reader.ReadAsync())
           {
       return new Documento
      {
        Id = reader.GetInt32(0),
         Nome = reader.GetString(1),
  ArquivoPdf = (byte[])reader.GetValue(2)
  };
      }
}
       }
      }

       return null;
        }

      /// <summary>
        /// Deleta um documento do banco
   /// </summary>
 public async Task DeletarAsync(int id)
   {
      using (var connection = DatabaseConnection.GetConnection())
  {
    string sql = "DELETE FROM documentos WHERE id = @id";

     using (var command = new NpgsqlCommand(sql, connection))
          {
         command.Parameters.AddWithValue("@id", id);
    await command.ExecuteNonQueryAsync();
          }
   }
    }

 /// <summary>
  /// Baixa um documento e salva em arquivo
        /// </summary>
  public async Task<string> BaixarPdfAsync(int id, string pastaDestino)
        {
       var documento = await BuscarPorIdAsync(id);
      
       if (documento == null)
    throw new Exception("Documento não encontrado.");

         // Criar nome do arquivo
            string nomeArquivo = documento.Nome;
  if (!nomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                nomeArquivo += ".pdf";

  string caminhoCompleto = Path.Combine(pastaDestino, nomeArquivo);

     // Garantir que a pasta existe
      Directory.CreateDirectory(pastaDestino);

  // Salvar arquivo
            await File.WriteAllBytesAsync(caminhoCompleto, documento.ArquivoPdf);

            return caminhoCompleto;
      }
    }
}
