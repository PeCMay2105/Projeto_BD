namespace patrimonioDB.Shared.Session
{
    /// <summary>
    /// Classe estática para armazenar informações do usuário logado
  /// Mantém a sessão durante toda a execução do aplicativo
    /// </summary>
    public static class UserSession
    {
        public static int? UsuarioLogadoId { get; private set; }
        public static string? UsuarioLogadoNome { get; private set; }
        public static bool IsAdministrador { get; private set; }

        /// <summary>
        /// Configura a sessão para um administrador
   /// </summary>
      public static void SetAdministrador(int id, string nome)
     {
            UsuarioLogadoId = id;
            UsuarioLogadoNome = nome;
    IsAdministrador = true;
   }

        /// <summary>
     /// Configura a sessão para um funcionário
    /// </summary>
   public static void SetFuncionario(int id, string nome)
 {
  UsuarioLogadoId = id;
       UsuarioLogadoNome = nome;
            IsAdministrador = false;
        }

 /// <summary>
        /// Limpa a sessão (logout)
        /// </summary>
     public static void Clear()
        {
UsuarioLogadoId = null;
      UsuarioLogadoNome = null;
         IsAdministrador = false;
        }

        /// <summary>
      /// Verifica se há um usuário logado
        /// </summary>
public static bool IsLoggedIn => UsuarioLogadoId.HasValue;

        /// <summary>
      /// Obtém o ID do usuário logado (lança exceção se não estiver logado)
        /// </summary>
  public static int GetUsuarioLogadoId()
     {
   if (!UsuarioLogadoId.HasValue)
            {
 throw new System.InvalidOperationException("Nenhum usuário está logado.");
   }
      return UsuarioLogadoId.Value;
        }
    }
}
