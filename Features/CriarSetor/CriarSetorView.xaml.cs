using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics; // Para Debug.WriteLine

namespace patrimonioDB.Features.CriarSetor
{
    /// <summary>
    /// Code-behind para a tela CriarSetorView.xaml.
    /// </summary>
    public sealed partial class CriarSetorView : Microsoft.UI.Xaml.Controls.Page
    {
        private readonly CriarSetorService _setorService;

        public ObservableCollection<Setor> SetoresLista { get; set; } = new ObservableCollection<Setor>();
        private Setor _setorEmEdicao;

        public CriarSetorView()
        {
            this.InitializeComponent();

            // 1. CORREÇÃO CRÍTICA: Inicializar o serviço ANTES de usar
            _setorService = new CriarSetorService();

            ListaSetores.ItemsSource = SetoresLista;

            // Carrega os dados
            CarregarSetores();
        }

        private void VoltarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        // 2. CORREÇÃO: Mudei para 'async void' para não travar a tela
        private async void CarregarSetores()
        {
            try
            {
                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;

                // Limpa a lista antes de buscar
                SetoresLista.Clear();

                // 3. CORREÇÃO: Uso de 'await' em vez de travar a thread
                List<Setor> setores = await _setorService.ObterSetoresDoBancoDeDadosAsync();

                if (setores != null)
                {
                    foreach (var setor in setores)
                    {
                        SetoresLista.Add(setor);
                    }
                }
            }
            catch (Exception ex)
            {
                MensagemErro.Text = "Erro ao carregar setores: " + ex.Message;
                MensagemErro.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private async void CriarSetor_Click(object sender, RoutedEventArgs e)
        {
            MensagemErro.Visibility = Visibility.Collapsed;
            MensagemSucesso.Visibility = Visibility.Collapsed;

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            CriarSetorButton.IsEnabled = false;

            try
            {
                string nomeDoSetor = SetorTextBox.Text;

                await _setorService.CriarSetorAsync(nomeDoSetor);

                MensagemSucesso.Text = $"Setor '{nomeDoSetor}' criado com sucesso!";
                MensagemSucesso.Visibility = Visibility.Visible;

                SetorTextBox.Text = string.Empty;

                // Recarrega a lista
                CarregarSetores();
            }
            catch (ValidacaoSetorException vex)
            {
                MensagemErro.Text = vex.Message;
                MensagemErro.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MensagemErro.Text = "Ocorreu um erro inesperado.";
                MensagemErro.Visibility = Visibility.Visible;
                Debug.WriteLine($"[CRIAR SETOR] Erro: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                CriarSetorButton.IsEnabled = true;
            }
        }

        private void ListaSetores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verifica se realmente tem algo selecionado
            if (ListaSetores.SelectedItem is Setor setorSelecionado)
            {
                // Guarda o objeto selecionado na variável global
                _setorEmEdicao = setorSelecionado;

                // Preenche o campo de texto
                SetorTextBox.Text = setorSelecionado.Nome;

                // Troca a visibilidade dos botões (Esconde "Criar", Mostra "Salvar/Cancelar")
                CriarSetorButton.Visibility = Visibility.Collapsed;
                BotoesEdicao.Visibility = Visibility.Visible;

                // Limpa mensagens anteriores
                MensagemSucesso.Visibility = Visibility.Collapsed;
                MensagemErro.Visibility = Visibility.Collapsed;
            }
        }

        private void SalvarEdicao_Click(object sender, RoutedEventArgs e)
        {
            if (_setorEmEdicao == null) return;

            string novoNome = SetorTextBox.Text.Trim();

            try
            {
                // --- SEU CÓDIGO DE UPDATE AQUI ---
                _setorService.atualizarSetor(_setorEmEdicao.Id, novoNome);

                // Atualiza visualmente (opcional se recarregar tudo depois)
                _setorEmEdicao.Nome = novoNome;

                MensagemSucesso.Text = "Setor atualizado!";
                MensagemSucesso.Visibility = Visibility.Visible;

                // Reseta a tela para o estado inicial
                ResetarFormulario();
                CarregarSetores(); // Recarrega do banco para garantir sincronia
            }
            catch (Exception ex)
            {
                MensagemErro.Text = "Erro ao atualizar: " + ex.Message;
                MensagemErro.Visibility = Visibility.Visible;
            }
        }

        // 4. Cancelar a Edição
        private void CancelarEdicao_Click(object sender, RoutedEventArgs e)
        {
            ResetarFormulario();
        }

        // Função auxiliar para voltar ao estado "Criar"
        private void ResetarFormulario()
        {
            SetorTextBox.Text = string.Empty;
            _setorEmEdicao = null;
            ListaSetores.SelectedItem = null; // Tira a seleção visual da lista

            // Volta os botões ao normal
            BotoesEdicao.Visibility = Visibility.Collapsed;
            CriarSetorButton.Visibility = Visibility.Visible;

            MensagemErro.Visibility = Visibility.Collapsed;
        }
    
        private async void DeletarSetor_Click(object sender, RoutedEventArgs e)
        {
            if (_setorEmEdicao == null) return;

            // Opcional: Aqui você poderia colocar um ContentDialog para confirmar a exclusão
            // Mas faremos direto para manter simples por enquanto.

            try
            {
                LoadingRing.IsActive = true;
                LoadingRing.Visibility = Visibility.Visible;
                BotoesEdicao.Visibility = Visibility.Collapsed; // Esconde botões para evitar clique duplo

                // Chama o serviço
                await _setorService.DeletarSetorAsync(_setorEmEdicao.Id);

                MensagemSucesso.Text = "Setor excluído com sucesso!";
                MensagemSucesso.Visibility = Visibility.Visible;

                // Limpa a tela
                ResetarFormulario();
                CarregarSetores();
            }
            catch (ValidacaoSetorException vex)
            {
                MensagemErro.Text = vex.Message;
                MensagemErro.Visibility = Visibility.Visible;
                BotoesEdicao.Visibility = Visibility.Visible; // Devolve os botões caso dê erro
            }
            catch (Exception ex)
            {
                MensagemErro.Text = "Erro ao excluir: " + ex.Message;
                MensagemErro.Visibility = Visibility.Visible;
                BotoesEdicao.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        } 
    }

}