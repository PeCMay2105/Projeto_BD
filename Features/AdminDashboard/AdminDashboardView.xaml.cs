using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Shared.Session;

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
            NomeAdminTextBlock.Text = $"Bem-vindo, {nome}!";
        }

        /// <summary>
        /// Logout - Retorna para a tela Home
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Limpar sessão ao fazer logout
            UserSession.Clear();
            Frame.Navigate(typeof(patrimonioDB.Features.Home.HomeView));
        }

        /// <summary>
        /// Navega para a tela de Cadastrar Funcionário
        /// </summary>
        private void CadastrarFuncionarioButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(patrimonioDB.Features.GestaoFuncionarios.CadastrarFuncionarioView));
        }

        /// <summary>
        /// Navega para a tela de Editar Funcionário
        /// </summary>
        private void EditarFuncionarioButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(patrimonioDB.Features.GestaoFuncionarios.GestaoFuncionariosView));
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

        /// <summary>
        /// ? NOVO: Navega para a tela de Gerar PDF de Funcionários
        /// Disponível apenas para administrador
        /// </summary>
        private void GerarPdfButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(patrimonioDB.Features.PdfFuncionario.GerarPdfFuncionariosView));
        }
    }
}
