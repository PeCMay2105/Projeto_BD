using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using patrimonioDB.Classes;
using patrimonioDB.Features.CriarSetor;
using System;
using System.Linq;

namespace patrimonioDB.Features.GestaoFuncionarios
{
    public sealed partial class EditarFuncionarioView : Page
 {
        private readonly FuncionarioService _service;
     private readonly CriarSetorService _setorService;
      private Classes.Funcionario _funcionario;  // ? Usar caminho completo

        public EditarFuncionarioView()
  {
   this.InitializeComponent();
    _service = new FuncionarioService();
   _setorService = new CriarSetorService();
  }

    /// <summary>
   /// Chamado quando a página recebe o parâmetro de navegação
        /// </summary>
  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
      base.OnNavigatedTo(e);

if (e.Parameter is Classes.Funcionario funcionario)  // ? Usar caminho completo
    {
    _funcionario = funcionario;
     
        // Carregar setores e funções primeiro
      await CarregarSetoresEFuncoesAsync();
       
// Depois preencher o formulário
   PreencherFormulario();
     }
  else
    {
   MostrarErro("Erro: Funcionário não encontrado.");
            }
      }

        private void VoltarButton_Click(object sender, RoutedEventArgs e)
    {
            if (Frame.CanGoBack)
         {
     Frame.GoBack();
       }
  }

        private async System.Threading.Tasks.Task CarregarSetoresEFuncoesAsync()
      {
    try
          {
// Carregar setores
    var setores = await _setorService.ObterSetoresDoBancoDeDadosAsync();
        SetorComboBox.ItemsSource = setores;

        // Carregar funções
        var funcoes = await _service.ListarFuncoesAsync();
           FuncaoComboBox.ItemsSource = funcoes;
   }
    catch (Exception ex)
   {
      MostrarErro($"Erro ao carregar dados: {ex.Message}");
            }
}

     private void PreencherFormulario()
     {
   if (_funcionario == null) return;

       try
          {
    // Preencher campos básicos
                NomeTextBox.Text = _funcionario.Nome;
    CpfTextBox.Text = _funcionario.CPF;
 DataNascimentoDatePicker.Date = new DateTimeOffset(_funcionario.Nascimento);
     LoginTextBox.Text = _funcionario.Login;
           SalarioNumberBox.Value = _funcionario.Salario;
DataAdmissaoDatePicker.Date = new DateTimeOffset(_funcionario.DataAdmissao);

          // Selecionar setor (procurar pelo ID na lista)
    if (SetorComboBox.ItemsSource != null)
          {
            var setores = SetorComboBox.ItemsSource as System.Collections.Generic.List<Setor>;
      if (setores != null)
     {
   var setorSelecionado = setores.FirstOrDefault(s => s.Id == _funcionario.SetorId);
if (setorSelecionado != null)
   {
SetorComboBox.SelectedItem = setorSelecionado;
 }
            }
     }

   // Selecionar função (procurar pelo ID na lista)
            if (FuncaoComboBox.ItemsSource != null)
    {
     var funcoes = FuncaoComboBox.ItemsSource as System.Collections.Generic.List<Funcao>;
    if (funcoes != null)
   {
       var funcaoSelecionada = funcoes.FirstOrDefault(f => f.Id == _funcionario.Id_funcao);
 if (funcaoSelecionada != null)
       {
      FuncaoComboBox.SelectedItem = funcaoSelecionada;
        }
     }
       }

      System.Diagnostics.Debug.WriteLine($"Formulário preenchido - Setor ID: {_funcionario.SetorId}, Função ID: {_funcionario.Id_funcao}");
    }
    catch (Exception ex)
    {
   MostrarErro($"Erro ao preencher formulário: {ex.Message}");
       System.Diagnostics.Debug.WriteLine($"ERRO PREENCHER: {ex.Message}");
  }
        }

    private async void SalvarButton_Click(object sender, RoutedEventArgs e)
        {
       MensagemErro.Visibility = Visibility.Collapsed;
        MensagemSucesso.Visibility = Visibility.Collapsed;

         // Validação básica
   if (string.IsNullOrWhiteSpace(NomeTextBox.Text))
     {
       MostrarErro("Por favor, preencha o nome.");
   return;
    }

     if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
  MostrarErro("Por favor, preencha o login.");
           return;
      }

if (SetorComboBox.SelectedItem == null)
  {
  MostrarErro("Por favor, selecione um setor.");
        return;
     }

    if (FuncaoComboBox.SelectedItem == null)
        {
        MostrarErro("Por favor, selecione uma funcao.");
   return;
        }

        LoadingRing.IsActive = true;
  LoadingRing.Visibility = Visibility.Visible;
   SalvarButton.IsEnabled = false;

 try
       {
      var setorSelecionado = (Setor)SetorComboBox.SelectedItem;
    var funcaoSelecionada = (Funcao)FuncaoComboBox.SelectedItem;

  // Se senha estiver vazia, manter a senha atual
  string senha = string.IsNullOrWhiteSpace(SenhaPasswordBox.Password) 
   ? _funcionario.Senha 
          : SenhaPasswordBox.Password;

  await _service.AtualizarFuncionarioAsync(
   _funcionario.Id,
       NomeTextBox.Text,
           _funcionario.CPF, // CPF não pode ser alterado
    DataNascimentoDatePicker.Date.DateTime,
 LoginTextBox.Text,
          senha,
              setorSelecionado.Id,
       funcaoSelecionada.Id,
     SalarioNumberBox.Value
     );

 MostrarSucesso($"Funcionario {NomeTextBox.Text} atualizado com sucesso!");

  // Aguardar 1.5 segundos e voltar
           await System.Threading.Tasks.Task.Delay(1500);
      
    if (Frame.CanGoBack)
        {
      Frame.GoBack();
}
      }
  catch (ValidacaoFuncionarioException vex)
            {
   MostrarErro(vex.Message);
            }
       catch (Exception ex)
    {
   MostrarErro($"Erro ao atualizar funcionario: {ex.Message}");
    System.Diagnostics.Debug.WriteLine($"ERRO ATUALIZAR: {ex}");
     }
            finally
 {
        LoadingRing.IsActive = false;
   LoadingRing.Visibility = Visibility.Collapsed;
       SalvarButton.IsEnabled = true;
            }
  }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
       if (Frame.CanGoBack)
         {
Frame.GoBack();
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
