using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;
using patrimonioDB.Shared.Database;
using patrimonioDB.Shared.Utils;

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
                        MostrarSucesso($"Login de Administrador realizado com sucesso!\n\nBem-vindo, {administrador.Nome}!");

                        // Aguarda 1.5 segundos para o usuário ver a mensagem
                        await Task.Delay(1500);

                        // Navega para o Painel do Administrador
                        Frame.Navigate(typeof(patrimonioDB.Features.AdminDashboard.AdminDashboardView));
                    }
                    else
                    {
                        MostrarErro("Login ou senha incorretos para Administrador.");
                    }
                }
                else
                {
                    // Login como Funcionário
                    var usuario = loginService.Autenticar(EmailTextBox.Text, SenhaPasswordBox.Password);

                    if (usuario != null)
                    {
                        MostrarSucesso($"Login realizado com sucesso!\n\nBem-vindo, {usuario.Nome}!");

                        // Aguarda 1.5 segundos para o usuário ver a mensagem
                        await Task.Delay(1500);

                        // Navega para a tela Home
                        Frame.Navigate(typeof(patrimonioDB.Features.Home.HomeView));
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
