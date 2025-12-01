using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
using patrimonioDB.Shared.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace patrimonioDB.Features.GestaoPatrimonio
{
  public sealed partial class GestaoPatrimonioView : Page
    {
  private readonly ItemService _itemService;
private List<Item> _todosItens;
   private List<Setor> _todosSetores;

        public GestaoPatrimonioView()
{
 this.InitializeComponent();
     _itemService = new ItemService();

 // Carregar dados iniciais
      this.Loaded += GestaoPatrimonioView_Loaded;
        }

    private void VoltarButton_Click(object sender, RoutedEventArgs e)
   {
  if (Frame.CanGoBack)
  {
  Frame.GoBack();
       }
  }

 private void LogoutButton_Click(object sender, RoutedEventArgs e)
      {
        // Limpar sessão ao fazer logout
UserSession.Clear();
   Frame.Navigate(typeof(patrimonioDB.Features.Home.HomeView));
    }

  private async void GestaoPatrimonioView_Loaded(object sender, RoutedEventArgs e)
 {
      try
 {
     // Carregar setores para os ComboBox
    _todosSetores = await _itemService.ListarSetoresAsync();
SetorComboBox.ItemsSource = _todosSetores;
NovoSetorComboBox.ItemsSource = _todosSetores;

       // Carregar itens ativos
    await CarregarItensAtivosAsync();

// Definir data padrao como hoje
DataCompraDatePicker.Date = DateTimeOffset.Now;
  }
  catch (Exception ex)
{
     Debug.WriteLine($"Erro ao carregar dados iniciais: {ex.Message}");
       }
 }

        private async System.Threading.Tasks.Task CarregarItensAtivosAsync()
   {
 _todosItens = await _itemService.ListarItensAsync();
     ItensMoverListView.ItemsSource = _todosItens;
    ItensRemoverListView.ItemsSource = _todosItens;
   }

    // ========== ABA 1: ADICIONAR ITEM ==========

        private async void AdicionarButton_Click(object sender, RoutedEventArgs e)
   {
MensagemErroAdicionar.Visibility = Visibility.Collapsed;
       MensagemSucessoAdicionar.Visibility = Visibility.Collapsed;

    LoadingRingAdicionar.IsActive = true;
LoadingRingAdicionar.Visibility = Visibility.Visible;
AdicionarButton.IsEnabled = false;

 try
   {
// Verificar se há usuário logado
if (!UserSession.IsLoggedIn)
   {
  MostrarErroAdicionar("Erro: Nenhum usuário está logado.");
        return;
     }

// Validações básicas
 if (string.IsNullOrWhiteSpace(NomeTextBox.Text))
{
  MostrarErroAdicionar("Por favor, preencha o nome do item.");
    return;
}

  if (SetorComboBox.SelectedItem == null)
   {
   MostrarErroAdicionar("Por favor, selecione um setor.");
    return;
       }

     // Criar objeto Item
        var novoItem = new Item
   {
  Nome = NomeTextBox.Text,
  Setor_Id = ((Setor)SetorComboBox.SelectedItem).Id
     };

// Pegar valores de preço e quantidade
    double preco = ValorNumberBox.Value;
     int quantidade = (int)QuantidadeNumberBox.Value;
      
        // *** USAR USUÁRIO DA SESSÃO ***
     int funcionarioId = UserSession.GetUsuarioLogadoId();

    // Chamar o service (cria item E registra compra)
await _itemService.AdicionarItemAsync(novoItem, preco, quantidade, funcionarioId);

   MostrarSucessoAdicionar($"Item '{novoItem.Nome}' adicionado com {quantidade} unidades a R$ {preco:F2}!");

 LimparFormularioAdicionar();
await CarregarItensAtivosAsync();
   }
 catch (ValidacaoItemException vex)
    {
    MostrarErroAdicionar(vex.Message);
     }
        catch (Exception ex)
    {
  MostrarErroAdicionar($"Erro: {ex.Message}");
     Debug.WriteLine($"ERRO: {ex.Message}");
  }
    finally
      {
  LoadingRingAdicionar.IsActive = false;
       LoadingRingAdicionar.Visibility = Visibility.Collapsed;
        AdicionarButton.IsEnabled = true;
  }
        }

        private void LimparFormularioAdicionar()
 {
     NomeTextBox.Text = "";
    SetorComboBox.SelectedIndex = -1;
QuantidadeNumberBox.Value = 1;
  ValorNumberBox.Value = 0;
  DataCompraDatePicker.Date = DateTimeOffset.Now;
 }

   private void MostrarErroAdicionar(string mensagem)
{
  MensagemErroAdicionar.Text = mensagem;
      MensagemErroAdicionar.Visibility = Visibility.Visible;
 }

     private void MostrarSucessoAdicionar(string mensagem)
        {
MensagemSucessoAdicionar.Text = mensagem;
  MensagemSucessoAdicionar.Visibility = Visibility.Visible;
}

   // ========== ABA 2: MOVER ITEM ==========

     private void ItensMoverListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
MoverButton.IsEnabled = ItensMoverListView.SelectedItem != null;
MensagemErroMover.Visibility = Visibility.Collapsed;
MensagemSucessoMover.Visibility = Visibility.Collapsed;
      }

  private void BuscarMover_TextChanged(object sender, TextChangedEventArgs e)
    {
            string busca = BuscarMoverTextBox.Text.ToLower();

   if (string.IsNullOrWhiteSpace(busca))
{
     ItensMoverListView.ItemsSource = _todosItens;
   }
      else
            {
        var itensFiltrados = _todosItens
.Where(i => i.Nome.ToLower().Contains(busca) ||
  (i.NomeSetor != null && i.NomeSetor.ToLower().Contains(busca)))
 .ToList();

  ItensMoverListView.ItemsSource = itensFiltrados;
        }
   }

private async void MoverButton_Click(object sender, RoutedEventArgs e)
  {
  MensagemErroMover.Visibility = Visibility.Collapsed;
  MensagemSucessoMover.Visibility = Visibility.Collapsed;

     if (ItensMoverListView.SelectedItem == null)
 {
 MostrarErroMover("Selecione um item para mover.");
  return;
}

     if (NovoSetorComboBox.SelectedItem == null)
   {
    MostrarErroMover("Selecione o setor de destino.");
    return;
       }

LoadingRingMover.IsActive = true;
 LoadingRingMover.Visibility = Visibility.Visible;
 MoverButton.IsEnabled = false;

try
{
// Verificar se há usuário logado
  if (!UserSession.IsLoggedIn)
       {
     MostrarErroMover("Erro: Nenhum usuário está logado.");
      return;
}

     var itemSelecionado = (Item)ItensMoverListView.SelectedItem;
    var setorDestino = (Setor)NovoSetorComboBox.SelectedItem;
       int quantidade = (int)QuantidadeMoverNumberBox.Value;
  int funcionarioId = UserSession.GetUsuarioLogadoId();

  // Passar quantidade e funcionarioId para o service
    await _itemService.MoverItemAsync(itemSelecionado.Id, setorDestino.Id, quantidade, funcionarioId);

 MostrarSucessoMover($"{quantidade} unidade(s) de '{itemSelecionado.Nome}' movida(s) para '{setorDestino.Nome}' com sucesso!");

       await CarregarItensAtivosAsync();

ItensMoverListView.SelectedItem = null;
   NovoSetorComboBox.SelectedIndex = -1;
      QuantidadeMoverNumberBox.Value = 1;
 }
       catch (ValidacaoItemException vex)
 {
     MostrarErroMover(vex.Message);
 }
  catch (Exception ex)
 {
     MostrarErroMover($"Erro: {ex.Message}");
    Debug.WriteLine($"ERRO: {ex.Message}");
  }
  finally
      {
     LoadingRingMover.IsActive = false;
LoadingRingMover.Visibility = Visibility.Collapsed;
   MoverButton.IsEnabled = ItensMoverListView.SelectedItem != null;
       }
  }

        private void MostrarErroMover(string mensagem)
 {
  MensagemErroMover.Text = mensagem;
MensagemErroMover.Visibility = Visibility.Visible;
   }

    private void MostrarSucessoMover(string mensagem)
        {
  MensagemSucessoMover.Text = mensagem;
  MensagemSucessoMover.Visibility = Visibility.Visible;
 }

 // ========== ABA 3: REMOVER ITEM ==========

    private void ItensRemoverListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
   RemoverButton.IsEnabled = ItensRemoverListView.SelectedItem != null;
MensagemErroRemover.Visibility = Visibility.Collapsed;
MensagemSucessoRemover.Visibility = Visibility.Collapsed;
        }

 private void BuscarRemover_TextChanged(object sender, TextChangedEventArgs e)
        {
   string busca = BuscarRemoverTextBox.Text.ToLower();

 if (string.IsNullOrWhiteSpace(busca))
   {
 ItensRemoverListView.ItemsSource = _todosItens;
  }
      else
      {
var itensFiltrados = _todosItens
  .Where(i => i.Nome.ToLower().Contains(busca) ||
      i.NomeSetor.ToLower().Contains(busca))
   .ToList();

    ItensRemoverListView.ItemsSource = itensFiltrados;
        }
     }

 private async void RemoverButton_Click(object sender, RoutedEventArgs e)
  {
   MensagemErroRemover.Visibility = Visibility.Collapsed;
MensagemSucessoRemover.Visibility = Visibility.Collapsed;

       if (ItensRemoverListView.SelectedItem == null)
       {
   MostrarErroRemover("Selecione um item para remover.");
   return;
   }

   // Verificar se há usuário logado
 if (!UserSession.IsLoggedIn)
{
            MostrarErroRemover("Erro: Nenhum usuário está logado.");
     return;
        }

   var itemSelecionado = (Item)ItensRemoverListView.SelectedItem;
        int quantidade = (int)QuantidadeVenderNumberBox.Value;

   // Validar quantidade
        if (quantidade <= 0)
   {
     MostrarErroRemover("A quantidade deve ser maior que zero.");
        return;
   }

        if (quantidade > itemSelecionado.QuantidadeTotal)
   {
     MostrarErroRemover($"Quantidade inválida. Disponível: {itemSelecionado.QuantidadeTotal} unidades.");
 return;
      }

 LoadingRingRemover.IsActive = true;
  LoadingRingRemover.Visibility = Visibility.Visible;
    RemoverButton.IsEnabled = false;

 try
{
     int funcionarioId = UserSession.GetUsuarioLogadoId();
    
            await _itemService.RemoverItemAsync(itemSelecionado.Id, quantidade, funcionarioId);

int restante = itemSelecionado.QuantidadeTotal - quantidade;
       MostrarSucessoRemover($"{quantidade} unidade(s) de '{itemSelecionado.Nome}' vendida(s) com sucesso! Restante: {restante}");

  await CarregarItensAtivosAsync();
   ItensRemoverListView.SelectedItem = null;
     QuantidadeVenderNumberBox.Value = 1;
   }
catch (ValidacaoItemException vex)
        {
     MostrarErroRemover(vex.Message);
}
   catch (Exception ex)
  {
     MostrarErroRemover($"Erro: {ex.Message}");
 Debug.WriteLine($"ERRO: {ex.Message}");
     }
    finally
      {
    LoadingRingRemover.IsActive = false;
   LoadingRingRemover.Visibility = Visibility.Collapsed;
RemoverButton.IsEnabled = ItensRemoverListView.SelectedItem != null;
 }
     }

        private void MostrarErroRemover(string mensagem)
 {
  MensagemErroRemover.Text = mensagem;
MensagemErroRemover.Visibility = Visibility.Visible;
 }

   private void MostrarSucessoRemover(string mensagem)
  {
        MensagemSucessoRemover.Text = mensagem;
  MensagemSucessoRemover.Visibility = Visibility.Visible;
        }

        // ========== ABA 4: RECOMPRAR ITEM ==========

     private void ItensRecompraListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
   RecomprarButton.IsEnabled = ItensRecompraListView.SelectedItem != null;
   MensagemErroRecompra.Visibility = Visibility.Collapsed;
      MensagemSucessoRecompra.Visibility = Visibility.Collapsed;
   }

  private void BuscarRecompra_TextChanged(object sender, TextChangedEventArgs e)
        {
     string busca = BuscarRecompraTextBox.Text.ToLower();

     if (string.IsNullOrWhiteSpace(busca))
  {
     ItensRecompraListView.ItemsSource = _todosItens;
   }
 else
{
    var itensFiltrados = _todosItens
     .Where(i => i.Nome.ToLower().Contains(busca) ||
     (i.NomeSetor != null && i.NomeSetor.ToLower().Contains(busca)))
 .ToList();

       ItensRecompraListView.ItemsSource = itensFiltrados;
     }
      }

  private async void RecomprarButton_Click(object sender, RoutedEventArgs e)
     {
MensagemErroRecompra.Visibility = Visibility.Collapsed;
        MensagemSucessoRecompra.Visibility = Visibility.Collapsed;

   if (ItensRecompraListView.SelectedItem == null)
 {
       MostrarErroRecompra("Selecione um item para recomprar.");
    return;
      }

      // Verificar se há usuário logado
  if (!UserSession.IsLoggedIn)
        {
   MostrarErroRecompra("Erro: Nenhum usuário está logado.");
       return;
  }

      var itemSelecionado = (Item)ItensRecompraListView.SelectedItem;
   int quantidade = (int)QuantidadeRecompraNumberBox.Value;
       double preco = PrecoRecompraNumberBox.Value;

    // Validações
    if (quantidade <= 0)
  {
      MostrarErroRecompra("A quantidade deve ser maior que zero.");
  return;
    }

        if (preco <= 0)
   {
   MostrarErroRecompra("O preço deve ser maior que zero.");
 return;
   }

    LoadingRingRecompra.IsActive = true;
   LoadingRingRecompra.Visibility = Visibility.Visible;
     RecomprarButton.IsEnabled = false;

       try
  {
         int funcionarioId = UserSession.GetUsuarioLogadoId();

 // Registrar nova compra do item existente
       await _itemService.RegistrarCompraAsync(itemSelecionado.Id, preco, quantidade, funcionarioId);

     MostrarSucessoRecompra($"Recompra registrada! {quantidade} unidade(s) de '{itemSelecionado.Nome}' adicionadas ao estoque.");

     await CarregarItensAtivosAsync();
  ItensRecompraListView.SelectedItem = null;
       QuantidadeRecompraNumberBox.Value = 1;
    PrecoRecompraNumberBox.Value = 0;
 }
    catch (ValidacaoItemException vex)
  {
       MostrarErroRecompra(vex.Message);
   }
        catch (Exception ex)
      {
MostrarErroRecompra($"Erro: {ex.Message}");
    Debug.WriteLine($"ERRO: {ex.Message}");
        }
     finally
     {
       LoadingRingRecompra.IsActive = false;
  LoadingRingRecompra.Visibility = Visibility.Collapsed;
            RecomprarButton.IsEnabled = ItensRecompraListView.SelectedItem != null;
   }
  }

        private void MostrarErroRecompra(string mensagem)
{
   MensagemErroRecompra.Text = mensagem;
 MensagemErroRecompra.Visibility = Visibility.Visible;
     }

        private void MostrarSucessoRecompra(string mensagem)
   {
 MensagemSucessoRecompra.Text = mensagem;
     MensagemSucessoRecompra.Visibility = Visibility.Visible;
    }
    }
}