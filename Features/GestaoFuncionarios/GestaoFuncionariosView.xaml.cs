using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace patrimonioDB.Features.GestaoFuncionarios
{
    public sealed partial class GestaoFuncionariosView : Page
    {
   private readonly FuncionarioService _service;
        private List<Funcionario> _todosFuncionarios;

      public GestaoFuncionariosView()
        {
       this.InitializeComponent();
            _service = new FuncionarioService();
    _todosFuncionarios = new List<Funcionario>();
 
   this.Loaded += GestaoFuncionariosView_Loaded;
 }

private async void GestaoFuncionariosView_Loaded(object sender, RoutedEventArgs e)
    {
    await CarregarFuncionariosAsync();
      }

   private void VoltarButton_Click(object sender, RoutedEventArgs e)
        {
    if (Frame.CanGoBack)
     {
    Frame.GoBack();
  }
        }

  private async System.Threading.Tasks.Task CarregarFuncionariosAsync()
        {
       try
  {
  LoadingOverlay.Visibility = Visibility.Visible;
    MensagemErro.Visibility = Visibility.Collapsed;
      MensagemSucesso.Visibility = Visibility.Collapsed;
      
    _todosFuncionarios = await _service.ListarFuncionariosAsync();
        ExibirFuncionarios(_todosFuncionarios);
        }
         catch (Exception ex)
            {
   MostrarErro($"Erro ao carregar funcionarios: {ex.Message}");
        }
    finally
        {
   LoadingOverlay.Visibility = Visibility.Collapsed;
  }
   }

        private void ExibirFuncionarios(List<Funcionario> funcionarios)
     {
   FuncionariosPanel.Children.Clear();

  if (funcionarios.Count == 0)
          {
     var mensagem = new TextBlock
   {
  Text = "Nenhum funcionario cadastrado",
           HorizontalAlignment = HorizontalAlignment.Center,
     Margin = new Thickness(0, 20, 0, 0),
   FontSize = 16,
  Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
    };
  FuncionariosPanel.Children.Add(mensagem);
    return;
    }

  foreach (var funcionario in funcionarios)
        {
      var card = CriarCardFuncionario(funcionario);
    FuncionariosPanel.Children.Add(card);
    }
        }

        private Border CriarCardFuncionario(Funcionario funcionario)
     {
      var card = new Border
    {
     Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
     BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
     BorderThickness = new Thickness(1),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
      Padding = new Thickness(15),
   Margin = new Thickness(0, 0, 0, 10)
     };

      var grid = new Grid();
  grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

  var infoStack = new StackPanel { Spacing = 5 };
 
   var nome = new TextBlock
  {
      Text = funcionario.Nome,
        FontSize = 18,
       FontWeight = Microsoft.UI.Text.FontWeights.Bold
      };
   
          var cpf = new TextBlock
      {
   Text = $"CPF: {funcionario.CPF}",
           FontSize = 14,
 Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
   };
 
   var cargo = new TextBlock
       {
    Text = $"Cargo: {funcionario.Cargo}",
    FontSize = 14
        };
   
  var salario = new TextBlock
    {
    Text = $"Salario: R$ {funcionario.Salario:N2}",
  FontSize = 14
    };

     infoStack.Children.Add(nome);
   infoStack.Children.Add(cpf);
infoStack.Children.Add(cargo);
     infoStack.Children.Add(salario);

  Grid.SetColumn(infoStack, 0);
grid.Children.Add(infoStack);

    var botoesStack = new StackPanel
         {
    Orientation = Orientation.Horizontal,
     Spacing = 10
   };

  var btnEditar = new Button
       {
        Content = "Editar",
    Tag = funcionario
     };
     btnEditar.Click += BtnEditar_Click;

       var btnExcluir = new Button
         {
       Content = "Excluir",
      Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
  Tag = funcionario
    };
       btnExcluir.Click += BtnExcluir_Click;

botoesStack.Children.Add(btnEditar);
  botoesStack.Children.Add(btnExcluir);

  Grid.SetColumn(botoesStack, 1);
         grid.Children.Add(botoesStack);

        card.Child = grid;
       return card;
        }

    private void BuscarTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
     var busca = BuscarTextBox.Text.ToLower().Trim();

if (string.IsNullOrWhiteSpace(busca))
{
       ExibirFuncionarios(_todosFuncionarios);
    }
  else
    {
       var filtrados = _todosFuncionarios
  .Where(f => f.Nome.ToLower().Contains(busca) || 
    f.CPF.Contains(busca))
     .ToList();
       
    ExibirFuncionarios(filtrados);
       }
   }

    private void CadastrarNovoButton_Click(object sender, RoutedEventArgs e)
   {
            Frame.Navigate(typeof(patrimonioDB.Features.GestaoFuncionarios.CadastrarFuncionarioView));
     }

    private void BtnEditar_Click(object sender, RoutedEventArgs e)
      {
 var funcionario = (Funcionario)((Button)sender).Tag;
    Frame.Navigate(typeof(patrimonioDB.Features.GestaoFuncionarios.EditarFuncionarioView), funcionario);
    }

      private async void BtnExcluir_Click(object sender, RoutedEventArgs e)
     {
  var funcionario = (Funcionario)((Button)sender).Tag;

            // Mostrar painel de confirmacao
    FuncionarioSelecionado.Text = $"{funcionario.Nome} (CPF: {funcionario.CPF})";
   FuncionarioParaExcluir.Tag = funcionario;
  ConfirmacaoExclusaoPanel.Visibility = Visibility.Visible;
    MensagemErro.Visibility = Visibility.Collapsed;
    MensagemSucesso.Visibility = Visibility.Collapsed;
      }

    private void CancelarExclusaoButton_Click(object sender, RoutedEventArgs e)
    {
      ConfirmacaoExclusaoPanel.Visibility = Visibility.Collapsed;
 FuncionarioParaExcluir.Tag = null;
      }

        private async void ConfirmarExclusaoButton_Click(object sender, RoutedEventArgs e)
        {
  if (FuncionarioParaExcluir.Tag is Funcionario funcionario)
       {
     try
       {
    LoadingOverlay.Visibility = Visibility.Visible;
    ConfirmacaoExclusaoPanel.Visibility = Visibility.Collapsed;
           
   await _service.RemoverFuncionarioAsync(funcionario.Id);
    
 MostrarSucesso($"Funcionario {funcionario.Nome} excluido com sucesso!");
   await CarregarFuncionariosAsync();
      }
      catch (Exception ex)
         {
      MostrarErro($"Erro ao excluir funcionario: {ex.Message}");
 }
   finally
    {
      LoadingOverlay.Visibility = Visibility.Collapsed;
       FuncionarioParaExcluir.Tag = null;
      }
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

   private void MostrarMensagem(string mensagem)
  {
  MensagemSucesso.Text = mensagem;
       MensagemSucesso.Visibility = Visibility.Visible;
  MensagemErro.Visibility = Visibility.Collapsed;
  }
    }
}
