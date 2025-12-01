using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Shared.Session;

namespace patrimonioDB.Features.FuncionarioDashboard
{
    public sealed partial class FuncionarioDashboardView : Page
    {
        public FuncionarioDashboardView()
        {
   this.InitializeComponent();
        }

        /// <summary>
        /// Define o nome do funcionario no cabecalho
        /// </summary>
        public void SetFuncionarioNome(string nome)
        {
        NomeFuncionarioTextBlock.Text = $"Bem-vindo, {nome}!";
        }

   /// <summary>
   /// Botao Logout - Retorna para a tela Home
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
      {
    // Limpar sessão ao fazer logout
     UserSession.Clear();
     Frame.Navigate(typeof(patrimonioDB.Features.Home.HomeView));
   }

        /// <summary>
        /// Navega para Gestao de Patrimonio
        /// </summary>
    private void GestaoPatrimonioButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(patrimonioDB.Features.GestaoPatrimonio.GestaoPatrimonioView));
  }

        /// <summary>
        /// Navega para Gestao de Setores
 /// </summary>
        private void GestaoSetoresButton_Click(object sender, RoutedEventArgs e)
        {
         Frame.Navigate(typeof(patrimonioDB.Features.CriarSetor.CriarSetorView));
        }
    }
}
