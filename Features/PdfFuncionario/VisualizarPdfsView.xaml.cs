using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace patrimonioDB.Features.PdfFuncionario
{
    public sealed partial class VisualizarPdfsView : Page
    {
 private readonly DocumentoRepository _repository;
     private List<Documento> _documentos;

  public VisualizarPdfsView()
        {
    this.InitializeComponent();
            _repository = new DocumentoRepository();
     this.Loaded += VisualizarPdfsView_Loaded;
       }

        private async void VisualizarPdfsView_Loaded(object sender, RoutedEventArgs e)
     {
       await CarregarDocumentosAsync();
 }

        private void VoltarButton_Click(object sender, RoutedEventArgs e)
        {
     if (Frame.CanGoBack)
            {
            Frame.GoBack();
     }
   }

  private async void AtualizarButton_Click(object sender, RoutedEventArgs e)
 {
      await CarregarDocumentosAsync();
 }

   private async System.Threading.Tasks.Task CarregarDocumentosAsync()
{
 MensagemErro.Visibility = Visibility.Collapsed;
 MensagemSucesso.Visibility = Visibility.Collapsed;
  LoadingRing.IsActive = true;
   LoadingRing.Visibility = Visibility.Visible;

    try
            {
    _documentos = await _repository.ListarDocumentosAsync();
     DocumentosListView.ItemsSource = _documentos;

                if (_documentos.Count == 0)
  {
     MostrarErro("Nenhum documento encontrado no banco de dados.");
    }
   else
    {
   MostrarSucesso($"? {_documentos.Count} documento(s) carregado(s).");
           }
       }
     catch (Exception ex)
    {
       MostrarErro($"Erro ao carregar documentos: {ex.Message}");
    Debug.WriteLine($"ERRO: {ex.Message}");
     }
   finally
    {
  LoadingRing.IsActive = false;
   LoadingRing.Visibility = Visibility.Collapsed;
     }
        }

        private async void BaixarButton_Click(object sender, RoutedEventArgs e)
 {
  if (sender is Button button && button.Tag is int documentoId)
    {
    LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;

     try
   {
    string pastaDownloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
      string caminhoArquivo = await _repository.BaixarPdfAsync(documentoId, pastaDownloads);

     MostrarSucesso($"? PDF baixado com sucesso!\n?? {caminhoArquivo}");

    // Abrir a pasta Downloads
            Process.Start(new ProcessStartInfo
{
 FileName = pastaDownloads,
   UseShellExecute = true
          });
  }
            catch (Exception ex)
        {
       MostrarErro($"Erro ao baixar PDF: {ex.Message}");
            Debug.WriteLine($"ERRO: {ex.Message}");
        }
       finally
       {
   LoadingRing.IsActive = false;
      LoadingRing.Visibility = Visibility.Collapsed;
          }
   }
 }

        private async void VisualizarButton_Click(object sender, RoutedEventArgs e)
     {
   if (sender is Button button && button.Tag is int documentoId)
       {
      LoadingRing.IsActive = true;
   LoadingRing.Visibility = Visibility.Visible;

          try
    {
        string pastaTempo = Path.GetTempPath();
  string caminhoArquivo = await _repository.BaixarPdfAsync(documentoId, pastaTempo);

         // Abrir o PDF
     Process.Start(new ProcessStartInfo
    {
     FileName = caminhoArquivo,
         UseShellExecute = true
   });

      MostrarSucesso("? PDF aberto para visualização!");
     }
       catch (Exception ex)
  {
  MostrarErro($"Erro ao visualizar PDF: {ex.Message}");
    Debug.WriteLine($"ERRO: {ex.Message}");
  }
  finally
       {
       LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
            }
        }
}

        private void DeletarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int documentoId)
            {
   MostrarDialogDeletar(documentoId);
        }
    }

        private async void MostrarDialogDeletar(int documentoId)
        {
   // Confirmar exclusão
         var dialog = new ContentDialog
   {
                Title = "Confirmar Exclusão",
        Content = "Tem certeza que deseja deletar este documento? Esta ação não pode ser desfeita.",
   PrimaryButtonText = "Deletar",
             CloseButtonText = "Cancelar",
         DefaultButton = ContentDialogButton.Close,
       XamlRoot = this.XamlRoot
            };

            dialog.PrimaryButtonClick += async (s, args) =>
         {
            args.Cancel = true; // Previne o fechamento imediato
             dialog.Hide();
        
      LoadingRing.IsActive = true;
       LoadingRing.Visibility = Visibility.Visible;

                try
   {
        await _repository.DeletarAsync(documentoId);
  MostrarSucesso("? Documento deletado com sucesso!");
                    await CarregarDocumentosAsync();
       }
 catch (Exception ex)
         {
     MostrarErro($"Erro ao deletar documento: {ex.Message}");
              Debug.WriteLine($"ERRO: {ex.Message}");
        }
    finally
  {
      LoadingRing.IsActive = false;
           LoadingRing.Visibility = Visibility.Collapsed;
         }
      };

       _ = dialog.ShowAsync();
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
