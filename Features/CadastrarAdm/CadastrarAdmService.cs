using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using patrimonioDB.Features.CadastrarAdm; // Para acessar o Repository

namespace patrimonioDB.Features.CadastrarAdm
{
    // Exceção personalizada usada na View
    public class ValidacaoAdmException : Exception
    {
        public ValidacaoAdmException(string message) : base(message) { }
    }

    public class CadastrarAdmService
    {
        private readonly CadastrarAdmRepository _repository;

        public CadastrarAdmService()
        {
            _repository = new CadastrarAdmRepository();
        }

        public async Task CadastrarAdministradorAsync(string nome, string cpf, string login, string senha, DateTime nascimento, double salario)
        {
            // 1. Validações Básicas
            if (string.IsNullOrWhiteSpace(nome))
                throw new ValidacaoAdmException("O nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(cpf))
                throw new ValidacaoAdmException("O CPF é obrigatório.");

            // Remove formatação do CPF se houver
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();
            if (!Regex.IsMatch(cpfLimpo, "^[0-9]{11}$"))
                throw new ValidacaoAdmException("CPF inválido. Digite apenas os 11 números.");

            if (string.IsNullOrWhiteSpace(login))
                throw new ValidacaoAdmException("O login (e-mail) é obrigatório.");

            if (!login.Contains("@") || !login.Contains("."))
                throw new ValidacaoAdmException("O login deve ser um e-mail válido.");

            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
                throw new ValidacaoAdmException("A senha deve ter pelo menos 6 caracteres.");

            // 2. Validações de Banco (Regras de Negócio)
            // Verifica se CPF já existe
            if (await _repository.CpfExisteAsync(cpfLimpo))
                throw new ValidacaoAdmException($"O CPF {cpf} já está cadastrado no sistema.");

            // Verifica se Login já existe
            if (await _repository.LoginExisteAsync(login))
                throw new ValidacaoAdmException($"O login {login} já está em uso.");

            // 3. Chama o Repository para salvar
            // Passamos o cpfLimpo para o banco
            await _repository.CadastrarAdministradorAsync(nome, cpfLimpo, login, senha, nascimento, salario);
        }
    }
}