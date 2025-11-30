using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace patrimonioDB.Features.AdminDashboard
{
    public sealed partial class AdminDashboardView : Page
    {
        public AdminDashboardView()
        {
            this.InitializeComponent();
        }

    /// <summary>
/// Define o nome do administrador logado
        /// </summary>
        public void SetAdministradorNome(string nome)
        {
            // NomeAdminTextBlock.Text = $"Bem-vindo, {nome}!";
        }

        /// <summary>
    /// Logout - Retorna para a tela Home
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
      Frame.Navigate(typeof(patrimonioDB.Features.Home.HomeView));
        }

        /// <summary>
        /// Navega para a tela de Cadastrar Funcionário
   /// </summary>
        private void CadastrarFuncionarioButton_Click(object sender, RoutedEventArgs e)
 {
        // TODO: Criar tela de cadastro
            var dialog = new ContentDialog
      {
     Title = "Em Desenvolvimento",
                Content = "A tela de cadastro de funcionários será implementada em breve.",
            CloseButtonText = "OK",
     XamlRoot = this.XamlRoot
   };
            _ = dialog.ShowAsync();
        }

  /// <summary>
        /// Navega para a tela de Editar Funcionário
      /// </summary>
        private void EditarFuncionarioButton_Click(object sender, RoutedEventArgs e)
     {
  // TODO: Criar tela de edição
      var dialog = new ContentDialog
            {
   Title = "Em Desenvolvimento",
 Content = "A tela de edição de funcionários será implementada em breve.",
  CloseButtonText = "OK",
  XamlRoot = this.XamlRoot
      };
    _ = dialog.ShowAsync();
      }

        /// <summary>
        /// Navega para a tela de Remover Funcionário
     /// </summary>
        private void RemoverFuncionarioButton_Click(object sender, RoutedEventArgs e)
        {
    // TODO: Criar tela de remoção
     var dialog = new ContentDialog
      {
       Title = "Em Desenvolvimento",
                Content = "A tela de remoção de funcionários será implementada em breve.",
 CloseButtonText = "OK",
      XamlRoot = this.XamlRoot
    };
        _ = dialog.ShowAsync();
      }

    /// <summary>
    /// Navega para a Gestão de Patrimônio
        /// </summary>
        private void GestaoPatrimonioButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(patrimonioDB.Features.GestaoPatrimonio.GestaoPatrimonioView));
        }

        /// <summary>
        /// Navega para a Gestão de Setores
        /// </summary>
    private void GestaoSetoresButton_Click(object sender, RoutedEventArgs e)
        {
         Frame.Navigate(typeof(patrimonioDB.Features.CriarSetor.CriarSetorView));
        }
    }
}
