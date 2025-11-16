using Microsoft.UI.Xaml;
using patrimonioDB.Features.CriarSetor; // Importa o Service e a Exceção
using System;
using System.Diagnostics; // Para Debug.WriteLine

namespace patrimonioDB.Features.CriarSetor
{
    /// <summary>
    /// Code-behind para a tela CriarSetorView.xaml.
    /// </summary>
    public sealed partial class CriarSetorView : Microsoft.UI.Xaml.Controls.Page
    {
        // Instancia a camada de serviço
        private readonly CriarSetorService _setorService;

        public CriarSetorView()
        {
            this.InitializeComponent();
            // Cria uma única instância do serviço que será usada por esta página
            _setorService = new CriarSetorService();
        }

        /// <summary>
        /// Chamado quando o botão 'Criar Setor' é clicado.
        /// </summary>
        private async void CriarSetor_Click(object sender, RoutedEventArgs e)
        {
            // 1. Resetar as mensagens de feedback
            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            // 2. Ativar o estado de "Carregando"
            // Isso mostra o anel de progresso e desabilita o botão
            // para evitar cliques duplicados.
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            CriarSetorButton.IsEnabled = false;

            try
            {
                // 3. Obter o valor da UI
                string nomeDoSetor = SetorTextBox.Text;

                // 4. Chamar a camada de serviço (que contém a lógica de negócio)
                // O 'await' faz com que a UI não trave e espere a 
                // operação no banco de dados terminar.
                await _setorService.CriarSetorAsync(nomeDoSetor);

                // 5. Se chegou aqui, deu tudo certo
                MensagemSucesso.Text = $"Setor '{nomeDoSetor}' criado com sucesso!";
                MensagemSucesso.Visibility = Visibility.Visible;
                SetorTextBox.Text = ""; // Limpa a caixa de texto
            }
            catch (ValidacaoSetorException vex)
            {
                // 6. Capturar erros de negócio (ex: "Setor já existe")
                // Estes são erros "esperados" e seguros para mostrar ao usuário.
                MensagemErro.Text = vex.Message;
                MensagemErro.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                // 7. Capturar erros inesperados (ex: banco de dados offline)
                MensagemErro.Text = "Ocorreu um erro inesperado. Tente novamente.";
                MensagemErro.Visibility = Visibility.Visible;
                Debug.WriteLine($"[CRIAR SETOR] Erro: {ex.Message}");
            }
            finally
            {
                // 8. Bloco 'finally' é executado SEMPRE (dando certo ou errado)
                // Usado para garantir que o estado de "Carregando" seja desativado
                // e o usuário possa tentar novamente.
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                CriarSetorButton.IsEnabled = true;
            }
        }
    }
}