using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;
using patrimonioDB.Shared.Database;
using patrimonioDB.Shared.Utils;
using patrimonioDB.Shared.Session;

namespace patrimonioDB.Features.Login
{
    public sealed partial class LoginView : Page
    {
        public LoginView()
        {
            this.InitializeComponent();
        }

        private void VoltarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void EntrarButton_Click(object sender, RoutedEventArgs e)
        {
            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MostrarErro("Por favor, informe o login.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SenhaPasswordBox.Password))
            {
                MostrarErro("Por favor, informe a senha.");
                return;
            }

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            EntrarButton.IsEnabled = false;

            try
            {
                var loginService = new LoginService();
                bool isAdmin = LoginAdminCheckBox.IsChecked == true;

                if (isAdmin)
                {
                    // Login como Administrador
                    var administrador = loginService.AutenticarAdministrador(EmailTextBox.Text, SenhaPasswordBox.Password);

                    if (administrador != null)
                    {
                        // *** CONFIGURAR SESSÃO ***
                        UserSession.SetAdministrador(administrador.Id, administrador.Nome);

                        MostrarSucesso($"Login de Administrador realizado com sucesso!\n\nBem-vindo, {administrador.Nome}!");

                        // Aguarda 1.5 segundos para o usuario ver a mensagem
                        await Task.Delay(1500);

                        // Navega para o Painel do Administrador e passa o nome
                        var adminDashboard = new patrimonioDB.Features.AdminDashboard.AdminDashboardView();
                        adminDashboard.SetAdministradorNome(administrador.Nome);
                        Frame.Navigate(typeof(patrimonioDB.Features.AdminDashboard.AdminDashboardView));

                        // Configura o nome apos navegar
                        if (Frame.Content is patrimonioDB.Features.AdminDashboard.AdminDashboardView adminView)
                        {
                            adminView.SetAdministradorNome(administrador.Nome);
                        }
                    }
                    else
                    {
                        MostrarErro("Login ou senha incorretos para Administrador.");
                    }
                }
                else
                {
                    // Login como Funcionario
                    var funcionario = loginService.Autenticar(EmailTextBox.Text, SenhaPasswordBox.Password);

                    if (funcionario != null)
                    {
                        // *** CONFIGURAR SESSÃO ***
                        UserSession.SetFuncionario(funcionario.Id, funcionario.Nome);

                        MostrarSucesso($"Login realizado com sucesso!\n\nBem-vindo, {funcionario.Nome}!");

                        // Aguarda 1.5 segundos para o usuario ver a mensagem
                        await Task.Delay(1500);

                        // Navega para o Painel do Funcionario
                        Frame.Navigate(typeof(patrimonioDB.Features.FuncionarioDashboard.FuncionarioDashboardView));

                        // Configura o nome apos navegar
                        if (Frame.Content is patrimonioDB.Features.FuncionarioDashboard.FuncionarioDashboardView funcView)
                        {
                            funcView.SetFuncionarioNome(funcionario.Nome);
                        }
                    }
                    else
                    {
                        MostrarErro("Login ou senha incorretos.");
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                EntrarButton.IsEnabled = true;
            }
        }

        private void MostrarErro(string mensagem)
        {
            MensagemErro.Text = mensagem;
            MensagemErro.Visibility = Visibility.Visible;
        }

        private void MostrarSucesso(string mensagem)
        {
            MensagemSucesso.Text = mensagem;
            MensagemSucesso.Visibility = Visibility.Visible;
        }
    }
}
