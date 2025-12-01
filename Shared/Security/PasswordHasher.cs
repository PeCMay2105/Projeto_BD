using System;
using System.Security.Cryptography;
using System.Text;

namespace patrimonioDB.Shared.Security
{
    /// <summary>
    /// Classe utilitária para hash de senhas usando SHA256
    /// </summary>
  public static class PasswordHasher
    {
        /// <summary>
      /// Gera um hash SHA256 da senha
        /// </summary>
    public static string HashPassword(string password)
    {
         if (string.IsNullOrEmpty(password))
  return string.Empty;

         using (var sha256 = SHA256.Create())
        {
    byte[] bytes = Encoding.UTF8.GetBytes(password);
    byte[] hash = sha256.ComputeHash(bytes);
      
             // Converter para string hexadecimal
        StringBuilder builder = new StringBuilder();
       for (int i = 0; i < hash.Length; i++)
{
     builder.Append(hash[i].ToString("x2"));
         }
         
          return builder.ToString();
    }
  }

        /// <summary>
        /// Verifica se uma senha corresponde ao hash armazenado
        /// </summary>
 public static bool VerifyPassword(string password, string hash)
   {
            string passwordHash = HashPassword(password);
          return passwordHash == hash;
        }
    }
}
