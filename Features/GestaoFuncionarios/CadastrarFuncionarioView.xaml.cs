using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using patrimonioDB.Classes;
using patrimonioDB.Features.CriarSetor;
using System;

namespace patrimonioDB.Features.GestaoFuncionarios
{
    public sealed partial class CadastrarFuncionarioView : Page
    {
        private readonly FuncionarioService _service;
        private readonly CriarSetorService _setorService;

        public CadastrarFuncionarioView()
    {
    this.InitializeComponent();
          _service = new FuncionarioService();
            _setorService = new CriarSetorService();

            // Define data padrao
       DataNascimentoDatePicker.Date = DateTimeOffset.Now.AddYears(-25);
    DataAdmissaoDatePicker.Date = DateTimeOffset.Now;

            this.Loaded += CadastrarFuncionarioView_Loaded;
  }

        private async void CadastrarFuncionarioView_Loaded(object sender, RoutedEventArgs e)
     {
     await CarregarSetoresAsync();
     await CarregarFuncoesAsync();
        }

  private void VoltarButton_Click(object sender, RoutedEventArgs e)
     {
            if (Frame.CanGoBack)
            {
      Frame.GoBack();
        }
    }

    private async System.Threading.Tasks.Task CarregarSetoresAsync()
   {
    try
         {
     var setores = await _setorService.ObterSetoresDoBancoDeDadosAsync();
      SetorComboBox.ItemsSource = setores;
     }
            catch (Exception ex)
 {
    MostrarErro($"Erro ao carregar setores: {ex.Message}");
  }
        }

        private async System.Threading.Tasks.Task CarregarFuncoesAsync()
        {
         try
        {
var funcoes = await _service.ListarFuncoesAsync();
   FuncaoComboBox.ItemsSource = funcoes;
 }
        catch (Exception ex)
     {
    MostrarErro($"Erro ao carregar funcoes: {ex.Message}");
   }
        }

    private async void CadastrarButton_Click(object sender, RoutedEventArgs e)
        {
            MensagemErro.Visibility = Visibility.Collapsed;
  MensagemSucesso.Visibility = Visibility.Collapsed;

 // Validacao basica de campos vazios
         if (string.IsNullOrWhiteSpace(NomeTextBox.Text))
            {
        MostrarErro("Por favor, preencha o nome.");
                return;
         }

            if (string.IsNullOrWhiteSpace(CpfTextBox.Text))
    {
             MostrarErro("Por favor, preencha o CPF.");
         return;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
       MostrarErro("Por favor, preencha o login.");
              return;
            }

   if (string.IsNullOrWhiteSpace(SenhaPasswordBox.Password))
            {
    MostrarErro("Por favor, preencha a senha.");
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
    CadastrarButton.IsEnabled = false;

 try
      {
          // Pega o objeto Setor selecionado e extrai o Id
      var setorSelecionado = (Setor)SetorComboBox.SelectedItem;
              var funcaoSelecionada = (Funcao)FuncaoComboBox.SelectedItem;

 await _service.CadastrarFuncionarioAsync(
      NomeTextBox.Text,
      CpfTextBox.Text,
         DataNascimentoDatePicker.Date.DateTime,
         LoginTextBox.Text,
          SenhaPasswordBox.Password,
           setorSelecionado.Id,  // Pega o Id do objeto Setor
 funcaoSelecionada.Id,  // Pega o Id do objeto Funcao
          SalarioNumberBox.Value,
          DataAdmissaoDatePicker.Date.DateTime
                );

                MostrarSucesso($"Funcionario {NomeTextBox.Text} cadastrado com sucesso!");
    LimparCampos();
         }
         catch (ValidacaoFuncionarioException vex)
            {
            MostrarErro(vex.Message);
            }
        catch (Exception ex)
   {
     MostrarErro($"Erro ao cadastrar funcionario: {ex.Message}");
     }
    finally
 {
                LoadingRing.IsActive = false;
         LoadingRing.Visibility = Visibility.Collapsed;
     CadastrarButton.IsEnabled = true;
            }
        }

        private void LimparButton_Click(object sender, RoutedEventArgs e)
        {
 LimparCampos();
    }

        private void LimparCampos()
      {
        NomeTextBox.Text = string.Empty;
    CpfTextBox.Text = string.Empty;
        LoginTextBox.Text = string.Empty;
   SenhaPasswordBox.Password = string.Empty;
            SetorComboBox.SelectedIndex = -1;
    FuncaoComboBox.SelectedIndex = -1;
  SalarioNumberBox.Value = 1500;
    DataNascimentoDatePicker.Date = DateTimeOffset.Now.AddYears(-25);
        DataAdmissaoDatePicker.Date = DateTimeOffset.Now;
            MensagemErro.Visibility = Visibility.Collapsed;
  MensagemSucesso.Visibility = Visibility.Collapsed;
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
