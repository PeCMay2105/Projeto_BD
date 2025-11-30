using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
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

        /// <summary>
        /// Chamado quando a página é carregada
        /// Carrega setores e itens iniciais
        /// </summary>
        private async void GestaoPatrimonioView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Carregar setores para os ComboBox
                _todosSetores = await _itemService.ListarSetoresAsync();
                SetorComboBox.ItemsSource = _todosSetores;
                NovoSetorComboBox.ItemsSource = _todosSetores;

                // Carregar itens ativos para as abas de Mover e Remover
                await CarregarItensAtivosAsync();

                // Definir data padrão como hoje
                DataCompraDatePicker.Date = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar dados iniciais: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega todos os itens ativos (não removidos)
        /// </summary>
        private async System.Threading.Tasks.Task CarregarItensAtivosAsync()
        {
            _todosItens = await _itemService.ListarItensAsync(incluirRemovidos: false);
            ItensMoverListView.ItemsSource = _todosItens;
            ItensRemoverListView.ItemsSource = _todosItens;
        }

        // ========== ABA 1: ADICIONAR ITEM ==========

        /// <summary>
        /// Botão Adicionar Item clicado
        /// </summary>
        private async void AdicionarButton_Click(object sender, RoutedEventArgs e)
        {
            // Limpar mensagens anteriores
            MensagemErroAdicionar.Visibility = Visibility.Collapsed;
            MensagemSucessoAdicionar.Visibility = Visibility.Collapsed;

            // Mostrar loading
            LoadingRingAdicionar.IsActive = true;
            LoadingRingAdicionar.Visibility = Visibility.Visible;
            AdicionarButton.IsEnabled = false;

            try
            {
                // Validações básicas de UI
                if (SetorComboBox.SelectedValue == null)
                {
                    MostrarErroAdicionar("Por favor, selecione um setor.");
                    return;
                }

                // Criar objeto Item com os dados do formulário
                var novoItem = new Item
                {
                    Nome = NomeTextBox.Text,
                    Descricao = DescricaoTextBox.Text,
                    Setor_Id = (int)SetorComboBox.SelectedValue,
                    Quantidade = (int)QuantidadeNumberBox.Value,
                    ValorUnitario = ValorNumberBox.Value,
                    DataCompra = DataCompraDatePicker.Date.DateTime,
                    FuncionarioResponsavel_Id = (int)FuncionarioNumberBox.Value
                };

                // Chamar o service (que faz as validações de negócio)
                await _itemService.AdicionarItemAsync(novoItem);

                // Sucesso!
                MostrarSucessoAdicionar($"Item '{novoItem.Nome}' adicionado com sucesso!");

                // Limpar formulário
                LimparFormularioAdicionar();

                // Recarregar listas das outras abas
                await CarregarItensAtivosAsync();
            }
            catch (ValidacaoItemException vex)
            {
                // Erros de validação de negócio
                MostrarErroAdicionar(vex.Message);
            }
            catch (Exception ex)
            {
                // Erros inesperados
                MostrarErroAdicionar("Ocorreu um erro inesperado. Tente novamente.");
                Debug.WriteLine($"ERRO: {ex.Message}");
            }
            finally
            {
                // Desativar loading
                LoadingRingAdicionar.IsActive = false;
                LoadingRingAdicionar.Visibility = Visibility.Collapsed;
                AdicionarButton.IsEnabled = true;
            }
        }

        private void LimparFormularioAdicionar()
        {
            NomeTextBox.Text = "";
            DescricaoTextBox.Text = "";
            SetorComboBox.SelectedIndex = -1;
            QuantidadeNumberBox.Value = 1;
            ValorNumberBox.Value = 0;
            DataCompraDatePicker.Date = DateTimeOffset.Now;
            FuncionarioNumberBox.Value = 1;
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

        /// <summary>
        /// Quando um item é selecionado para mover
        /// </summary>
        private void ItensMoverListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Habilitar botão apenas se algo foi selecionado
            MoverButton.IsEnabled = ItensMoverListView.SelectedItem != null;

            // Limpar mensagens
            MensagemErroMover.Visibility = Visibility.Collapsed;
            MensagemSucessoMover.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Busca de itens na aba Mover
        /// </summary>
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

        /// <summary>
        /// Botão Mover Item clicado
        /// </summary>
        private async void MoverButton_Click(object sender, RoutedEventArgs e)
        {
            MensagemErroMover.Visibility = Visibility.Collapsed;
            MensagemSucessoMover.Visibility = Visibility.Collapsed;

            // Validações de UI
            if (ItensMoverListView.SelectedItem == null)
            {
                MostrarErroMover("Selecione um item para mover.");
                return;
            }

            if (NovoSetorComboBox.SelectedValue == null)
            {
                MostrarErroMover("Selecione o setor de destino.");
                return;
            }

            LoadingRingMover.IsActive = true;
            LoadingRingMover.Visibility = Visibility.Visible;
            MoverButton.IsEnabled = false;

            try
            {
                var itemSelecionado = (Item)ItensMoverListView.SelectedItem;
                int novoSetorId = (int)NovoSetorComboBox.SelectedValue;

                // Chamar o service
                await _itemService.MoverItemAsync(itemSelecionado.Id, novoSetorId);

                // Sucesso!
                var novoSetor = _todosSetores.FirstOrDefault(s => s.Id == novoSetorId);
                MostrarSucessoMover($"Item '{itemSelecionado.Nome}' movido para '{novoSetor?.Nome}' com sucesso!");

                // Recarregar listas
                await CarregarItensAtivosAsync();

                // Limpar seleções
                ItensMoverListView.SelectedItem = null;
                NovoSetorComboBox.SelectedIndex = -1;
            }
            catch (ValidacaoItemException vex)
            {
                MostrarErroMover(vex.Message);
            }
            catch (Exception ex)
            {
                MostrarErroMover("Ocorreu um erro inesperado. Tente novamente.");
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

        /// <summary>
        /// Quando um item é selecionado para remover
        /// </summary>
        private void ItensRemoverListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoverButton.IsEnabled = ItensRemoverListView.SelectedItem != null;

            MensagemErroRemover.Visibility = Visibility.Collapsed;
            MensagemSucessoRemover.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Busca de itens na aba Remover
        /// </summary>
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

        /// <summary>
        /// Botão Remover Item clicado
        /// </summary>
        private async void RemoverButton_Click(object sender, RoutedEventArgs e)
        {
            MensagemErroRemover.Visibility = Visibility.Collapsed;
            MensagemSucessoRemover.Visibility = Visibility.Collapsed;

            if (ItensRemoverListView.SelectedItem == null)
            {
                MostrarErroRemover("Selecione um item para remover.");
                return;
            }

            var itemSelecionado = (Item)ItensRemoverListView.SelectedItem;

            LoadingRingRemover.IsActive = true;
            LoadingRingRemover.Visibility = Visibility.Visible;
            RemoverButton.IsEnabled = false;

            try
            {
                await _itemService.RemoverItemAsync(itemSelecionado.Id);

                MostrarSucessoRemover($"Item '{itemSelecionado.Nome}' removido com sucesso!");

                await CarregarItensAtivosAsync();
                ItensRemoverListView.SelectedItem = null;
            }
            catch (ValidacaoItemException vex)
            {
                MostrarErroRemover(vex.Message);
            }
            catch (Exception ex)
            {
                MostrarErroRemover("Ocorreu um erro inesperado. Tente novamente.");
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
    }
}