using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using patrimonioDB.Classes;

namespace patrimonioDB.Features.GestaoFuncionarios
{
    /// <summary>
 /// Exceção personalizada para erros de validação de funcionário
 /// </summary>
    public class ValidacaoFuncionarioException : Exception
    {
        public ValidacaoFuncionarioException(string message) : base(message) { }
    }

    public class FuncionarioService
    {
    private readonly FuncionarioRepository _repository;

        public FuncionarioService()
   {
  _repository = new FuncionarioRepository();
  }

        /// <summary>
        /// Cadastra um novo funcionário com validações
     /// </summary>
    public async Task CadastrarFuncionarioAsync(string nome, string cpf, DateTime nascimento, 
    string login, string senha, int setorId, int funcaoId, double salario, DateTime dataAdmissao)
        {
      // ===== VALIDAÇÕES =====

     // 1. Nome não pode ser vazio
 if (string.IsNullOrWhiteSpace(nome))
    {
 throw new ValidacaoFuncionarioException("O nome não pode estar vazio.");
         }

          // 2. Validar CPF
  if (!ValidarCPF(cpf))
         {
      throw new ValidacaoFuncionarioException("CPF inválido. Use o formato: 000.000.000-00");
 }

       // 3. Verificar se CPF já existe
  if (await _repository.CpfExisteAsync(cpf))
   {
            throw new ValidacaoFuncionarioException("Este CPF já está cadastrado no sistema.");
   }

   // 4. Login não pode ser vazio
   if (string.IsNullOrWhiteSpace(login))
  {
   throw new ValidacaoFuncionarioException("O login não pode estar vazio.");
      }

     // 5. Verificar se login já existe
     if (await _repository.LoginExisteAsync(login))
            {
       throw new ValidacaoFuncionarioException("Este login já está em uso.");
  }

  // 6. Senha não pode ser vazia
  if (string.IsNullOrWhiteSpace(senha))
      {
  throw new ValidacaoFuncionarioException("A senha não pode estar vazia.");
   }

       // 7. Senha deve ter no mínimo 6 caracteres
  if (senha.Length < 6)
   {
    throw new ValidacaoFuncionarioException("A senha deve ter no mínimo 6 caracteres.");
          }

  // 8. Função não pode ser zero
      if (funcaoId <= 0)
   {
 throw new ValidacaoFuncionarioException("Por favor, selecione uma função.");
        }

  // 9. Salário deve ser positivo
          if (salario <= 0)
 {
           throw new ValidacaoFuncionarioException("O salário deve ser maior que zero.");
            }

     // 10. Data de nascimento não pode ser futura
    if (nascimento > DateTime.Now)
         {
  throw new ValidacaoFuncionarioException("A data de nascimento não pode ser futura.");
  }

            // 11. Funcionário deve ter pelo menos 16 anos
       var idade = DateTime.Now.Year - nascimento.Year;
if (nascimento > DateTime.Now.AddYears(-idade)) idade--;
     
            if (idade < 16)
   {
       throw new ValidacaoFuncionarioException("O funcionário deve ter pelo menos 16 anos.");
  }

     // ===== FIM DAS VALIDAÇÕES =====

     var pessoa = new Pessoa
          {
    Nome = nome,
    CPF = cpf,
        Nascimento = nascimento,
     Login = login,
    Senha = senha
         };

 await _repository.CadastrarFuncionarioAsync(pessoa, setorId, funcaoId, salario, dataAdmissao);
   }

        /// <summary>
    /// Lista todas as funções disponíveis
        /// </summary>
   public async Task<List<Funcao>> ListarFuncoesAsync()
        {
            return await _repository.ListarFuncoesAsync();
        }

 /// <summary>
  /// Lista todos os funcionários
    /// </summary>
public async Task<List<Funcionario>> ListarFuncionariosAsync()
   {
         return await _repository.ListarFuncionariosAsync();
        }

        /// <summary>
        /// Busca um funcionário por ID
        /// </summary>
        public async Task<Funcionario?> BuscarFuncionarioPorIdAsync(int id)
        {
 return await _repository.BuscarPorIdAsync(id);
      }

   /// <summary>
        /// Atualiza dados de um funcionário
    /// </summary>
  public async Task AtualizarFuncionarioAsync(int id, string nome, string cpf, DateTime nascimento,
          string login, string senha, int setorId, int funcaoId, double salario)
   {
       // Validações similares ao cadastro
       if (string.IsNullOrWhiteSpace(nome))
{
  throw new ValidacaoFuncionarioException("O nome não pode estar vazio.");
     }

    if (!ValidarCPF(cpf))
 {
              throw new ValidacaoFuncionarioException("CPF inválido.");
            }

            if (funcaoId <= 0)
         {
    throw new ValidacaoFuncionarioException("Por favor, selecione uma função.");
          }

            if (salario <= 0)
   {
  throw new ValidacaoFuncionarioException("O salário deve ser maior que zero.");
       }

   var pessoa = new Pessoa
    {
        Id = id,
         Nome = nome,
       CPF = cpf,
    Nascimento = nascimento,
 Login = login,
    Senha = senha
};

  await _repository.AtualizarFuncionarioAsync(id, pessoa, setorId, funcaoId, salario);
     }

    /// <summary>
    /// Remove um funcionário
        /// </summary>
        public async Task RemoverFuncionarioAsync(int id)
  {
    var funcionario = await _repository.BuscarPorIdAsync(id);

      if (funcionario == null)
 {
           throw new ValidacaoFuncionarioException("Funcionário não encontrado.");
       }

          await _repository.RemoverFuncionarioAsync(id);
 }

      /// <summary>
   /// Valida formato de CPF
    /// </summary>
  private bool ValidarCPF(string cpf)
  {
    if (string.IsNullOrWhiteSpace(cpf))
       return false;

  // Remove pontos e traços
       cpf = cpf.Replace(".", "").Replace("-", "").Trim();

    // Verifica se tem 11 dígitos
  if (cpf.Length != 11)
 return false;

   // Verifica se todos os dígitos são iguais (CPF inválido)
          if (Regex.IsMatch(cpf, @"^(\d)\1{10}$"))
return false;

   return true;
        }
    }
}
