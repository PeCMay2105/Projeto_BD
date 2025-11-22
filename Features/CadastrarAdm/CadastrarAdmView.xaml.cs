using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Features.CadastrarAdm; // Namespace do Service
using System;
using System.Diagnostics;

namespace patrimonioDB.Features.CadastrarAdm
{
    /// <summary>
    /// Code-behind da tela de cadastro de Administrador.
    /// </summary>
    public sealed partial class CadastroAdmView : Page
    {
        // Instância do serviço de cadastro
        private readonly CadastrarAdmService _admService;

        public CadastroAdmView()
        {
            this.InitializeComponent();
            // Inicializa o serviço que contém as validações
            _admService = new CadastrarAdmService();
        }

        /// <summary>
        /// Evento do botão "Voltar".
        /// Retorna para a página anterior na pilha de navegação.
        /// </summary>
        private void Voltar_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se é possível voltar no histórico de navegação
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                // Opcional: Se não houver para onde voltar, você pode definir uma navegação padrão
                // this.Frame.Navigate(typeof(TelaInicialView));
            }
        }

        /// <summary>
        /// Evento disparado pelo botão "Cadastrar" no XAML.
        /// </summary>
        private async void CadastrarAdm_Click(object sender, RoutedEventArgs e)
        {
            // 1. Resetar visualização de mensagens anteriores
            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            // 2. Ativar estado de carregamento e travar o botão
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            CadastrarAdmButton.IsEnabled = false;

            try
            {
                // 3. Capturar valores da UI
                string nome = NomeTextBox.Text;
                string cpf = CpfTextBox.Text;
                string login = LoginTextBox.Text;
                string senha = SenhaPasswordBox.Password; // Pega o texto da caixa de senha

                // Converte a data do DatePicker
                DateTime nascimento = DataNascDatePicker.Date.DateTime;

                // Define um salário padrão (já que não tem campo na tela por enquanto)
                double salario = 3500.00;

                // 4. Chamar a camada de serviço para validar e salvar
                await _admService.CadastrarAdministradorAsync(
                    nome,
                    cpf,
                    login,
                    senha,
                    nascimento,
                    salario
                );

                // 5. Sucesso: Mostrar mensagem e limpar formulário
                MensagemSucesso.Text = "Administrador cadastrado com sucesso!";
                MensagemSucesso.Visibility = Visibility.Visible;
                LimparCampos();
            }
            catch (ValidacaoAdmException vex)
            {
                // 6. Erro de Regra de Negócio (ex: CPF inválido, Login duplicado)
                MensagemErro.Text = vex.Message;
                MensagemErro.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // 7. MODIFICADO: Mostra o erro real para podermos corrigir
                MensagemErro.Text = $"Erro Técnico: {ex.Message}";
                MensagemErro.Visibility = Visibility.Visible;

                // Log detalhado no Output do Visual Studio
                Debug.WriteLine($"[ERRO CRÍTICO]: {ex}");
            }
            finally
            {
                // 8. Finalização: Destravar botão e esconder loading
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                CadastrarAdmButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Limpa os campos do formulário após um cadastro bem-sucedido.
        /// </summary>
        private void LimparCampos()
        {
            NomeTextBox.Text = string.Empty;
            CpfTextBox.Text = string.Empty;
            LoginTextBox.Text = string.Empty;
            SenhaPasswordBox.Password = string.Empty;
            DataNascDatePicker.Date = DateTimeOffset.Now;
        }
    }
}