using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace patrimonioDB.Features.CriarSetor
{
    /// <summary>
    /// Uma exceção personalizada para erros de validação da lógica de negócios
    /// que podem ser exibidos diretamente para o usuário na UI.
    /// </summary>
    public class ValidacaoSetorException : Exception
    {
        public ValidacaoSetorException(string message) : base(message) { }
    }

    public class CriarSetorService
    {
        private readonly CriarSetorRepository _repository;

        public CriarSetorService()
        {
            _repository = new CriarSetorRepository();
        }

        /// <summary>
        /// Orquestra a criação de um novo setor, aplicando regras de negócio.
        /// </summary>
        /// <param name="nomeSetor">O nome do setor a ser criado.</param>
        /// <exception cref="ValidacaoSetorException">Lançada se uma regra de negócio falhar.</exception>
        public async Task CriarSetorAsync(string nomeSetor)
        {
            // --- INÍCIO DAS REGRAS DE NEGÓCIO ---

            // 1. Validação de entrada básica
            if (string.IsNullOrWhiteSpace(nomeSetor))
            {
                throw new ValidacaoSetorException("O nome do setor não pode estar vazio.");
            }

            // 2. Validação de regra de negócio (não permitir duplicatas)
            bool jaExiste = await _repository.SetorJaExisteAsync(nomeSetor);
            if (jaExiste)
            {
                throw new ValidacaoSetorException($"O setor '{nomeSetor}' já está cadastrado.");
            }

            // --- FIM DAS REGRAS DE NEGÓCIO ---

            try
            {
                // Se todas as validações passaram, chama o repositório para salvar.
                await _repository.AdicionarSetorAsync(nomeSetor);

                Debug.WriteLine($"✓ Setor '{nomeSetor}' criado com sucesso.");
            }
            catch (Exception ex)
            {
                // Captura erros inesperados do banco de dados (ex: conexão falhou)
                Debug.WriteLine($"ERRO DE BANCO: {ex.Message}");
                // Lança uma exceção mais genérica para a UI
                throw new Exception("Ocorreu um erro ao tentar salvar no banco de dados. Tente novamente.");
            }
        }
    }
}