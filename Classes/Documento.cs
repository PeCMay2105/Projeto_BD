using System;

namespace patrimonioDB.Classes
{
    /// <summary>
    /// Representa um documento PDF armazenado no banco de dados
    /// </summary>
    public class Documento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public byte[] ArquivoPdf { get; set; } = Array.Empty<byte>();
        
        /// <summary>
        /// Tamanho do arquivo em bytes (calculado)
        /// </summary>
        public long TamanhoBytes => ArquivoPdf?.Length ?? 0;
   
        /// <summary>
        /// Tamanho formatado (ex: "1.5 MB")
        /// </summary>
        public string TamanhoFormatado
        {
         get
            {
            double bytes = TamanhoBytes;
    string[] sizes = { "B", "KB", "MB", "GB" };
    int order = 0;
         
    while (bytes >= 1024 && order < sizes.Length - 1)
           {
         order++;
     bytes = bytes / 1024;
        }
       
       return $"{bytes:0.##} {sizes[order]}";
       }
        }
    }
}
