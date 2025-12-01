using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace patrimonioDB.Features.PdfFuncionario
{
    public sealed partial class GerarPdfFuncionariosView : Page
    {
        private readonly FuncionarioRepository _repository;
        private readonly PdfService _pdfService;
        private List<FuncionarioDetalhado> _funcionarios;

        public GerarPdfFuncionariosView()
        {
            this.InitializeComponent();
            _repository = new FuncionarioRepository();
            _pdfService = new PdfService();
        }

        /// <summary>
        /// ✅ Botão Voltar - Retorna para o AdminDashboard
        /// </summary>
        private void VoltarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        /// <summary>
        /// Carrega dados dos funcionários da view
        /// </summary>
        private async void CarregarButton_Click(object sender, RoutedEventArgs e)
        {
            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            CarregarButton.IsEnabled = false;

            try
            {
                // Buscar dados da view
                _funcionarios = await _repository.ListarFuncionariosDetalhadosAsync();

                // Atualizar UI
                TotalFuncionariosText.Text = _funcionarios.Count.ToString();
                GerarPdfButton.IsEnabled = _funcionarios.Count > 0;

                MostrarSucesso($"✓ {_funcionarios.Count} funcionário(s) carregado(s) com sucesso!");
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao carregar dados: {ex.Message}");
                Debug.WriteLine($"ERRO: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                CarregarButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Gera o PDF com os dados carregados
        /// </summary>
        private async void GerarPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (_funcionarios == null || _funcionarios.Count == 0)
            {
                MostrarErro("Nenhum funcionário carregado. Clique em 'Carregar Dados' primeiro.");
                return;
            }

            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            GerarPdfButton.IsEnabled = false;

            try
            {
                // Definir nome do arquivo
                string nomeArquivo = $"funcionarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                bool porSetor = RadioPorSetor.IsChecked == true;

                // ✅ NOVO: Salvar no banco de dados
                int documentoId = await _pdfService.GerarESalvarPdfAsync(_funcionarios, nomeArquivo, porSetor);

                MostrarSucesso($"✓ PDF gerado e salvo no banco com sucesso!\n📄 ID do documento: {documentoId}");
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao gerar PDF: {ex.Message}");
                Debug.WriteLine($"ERRO: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                GerarPdfButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Abre a pasta de Downloads
        /// </summary>
        private void AbrirPastaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pastaDownloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = pastaDownloads,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao abrir pasta: {ex.Message}");
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
