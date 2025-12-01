using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
using patrimonioDB.Features.AdminDashboard;
using patrimonioDB.Features.FuncionarioDashboard;
using patrimonioDB.Shared.Session;

namespace patrimonioDB.Features.Login
{
    public sealed partial class LoginView : Page
    {
        private readonly LoginService _loginService;

        public LoginView()
        {
            this.InitializeComponent();
            _loginService = new LoginService();
        }

        private void VoltarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void LoginTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SenhaPasswordBox.Focus(FocusState.Programmatic);
            }
        }

        private void SenhaPasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                EntrarButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void EntrarButton_Click(object sender, RoutedEventArgs e)
        {
            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            // Validações básicas
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MostrarErro("Por favor, digite o login.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SenhaPasswordBox.Password))
            {
                MostrarErro("Por favor, digite a senha.");
                return;
            }

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            EntrarButton.IsEnabled = false;

            try
            {
                await System.Threading.Tasks.Task.Delay(500);

                string login = EmailTextBox.Text;
                string senha = SenhaPasswordBox.Password;

                Debug.WriteLine($"\n=== TENTATIVA DE LOGIN ===");
                Debug.WriteLine($"Login digitado: {login}");
                Debug.WriteLine($"Senha digitada: {senha}");

                var resultado = _loginService.Autenticar(login, senha);

                if (resultado.usuario != null)
                {
                    // Login bem-sucedido
                    UserSession.SetFuncionario(resultado.usuario.Id, resultado.usuario.Nome);

                    MostrarSucesso($"Login realizado com sucesso!\n\nBem-vindo, {resultado.usuario.Nome}!");

                    // Aguardar 1.5 segundos e navegar
                    await System.Threading.Tasks.Task.Delay(1500);

                    // ✅ CORRIGIDO: Verificar pelo tipo de usuário retornado
                    if (resultado.isAdmin)
                    {
                        Debug.WriteLine("→ Navegando para AdminDashboard");
                        Frame.Navigate(typeof(AdminDashboardView));
                    }
                    else
                    {
                        Debug.WriteLine("→ Navegando para FuncionarioDashboard");
                        Frame.Navigate(typeof(FuncionarioDashboardView));
                    }
                }
                else
                {
                    // Login falhou
                    MostrarErro("Login ou senha incorretos.\n\nVerifique suas credenciais e tente novamente.");
                    Debug.WriteLine("✗ Autenticação falhou");
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao realizar login:\n{ex.Message}");
                Debug.WriteLine($"ERRO LOGIN: {ex}");
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
            MensagemSucesso.Visibility = Visibility.Collapsed;
        }

        private void MostrarSucesso(string mensagem)
        {
            MensagemSucesso.Text = mensagem;
            MensagemSucesso.Visibility = Visibility.Visible;
            MensagemErro.Visibility = Visibility.Collapsed;
        }
    }
}
