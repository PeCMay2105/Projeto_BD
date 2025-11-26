using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using patrimonioDB.Features.CriarSetor;

namespace patrimonioDB.Features.GestaoPatrimonio
{
    /// <summary>
    /// Exceção personalizada para erros de validação de patrimônio
    /// que podem ser exibidos diretamente para o usuário na UI
    /// </summary>
    public class ValidacaoItemException : Exception
    {
        public ValidacaoItemException(string message) : base(message) { }
    }

    /// <summary>
    /// Service contém toda a LÓGICA DE NEGÓCIO da gestão de patrimônio
    /// Valida regras antes de chamar o Repository
    /// </summary>
    public class ItemService
    {
        private readonly ItemRepository _repository;

        public ItemService()
        {
            _repository = new ItemRepository();
        }

        /// <summary>
        /// Adiciona um novo item ao patrimônio com validações de negócio
        /// </summary>
        public async Task AdicionarItemAsync(Item item)
        {
            // ===== VALIDAÇÕES DE NEGÓCIO =====

            // 1. Nome não pode ser vazio
            if (string.IsNullOrWhiteSpace(item.Nome))
            {
                throw new ValidacaoItemException("O nome do item não pode estar vazio.");
            }

            // 2. Quantidade não pode ser negativa
            if (item.Quantidade < 0)
            {
                throw new ValidacaoItemException("A quantidade não pode ser negativa.");
            }

            // 3. Valor unitário não pode ser negativo
            if (item.ValorUnitario < 0)
            {
                throw new ValidacaoItemException("O valor unitário não pode ser negativo.");
            }

            // 4. Descrição não pode ser vazia
            if (string.IsNullOrWhiteSpace(item.Descricao))
            {
                throw new ValidacaoItemException("A descrição do item não pode estar vazia.");
            }

            // 5. Validar se o setor existe
            bool setorExiste = await _repository.SetorExisteAsync(item.Setor_Id);
            if (!setorExiste)
            {
                throw new ValidacaoItemException("O setor selecionado não existe.");
            }

            // ===== FIM DAS VALIDAÇÕES =====

            try
            {
                // Se todas as validações passaram, salva no banco
                await _repository.AdicionarItemAsync(item);

                Debug.WriteLine($"✓ Item '{item.Nome}' adicionado ao setor {item.Setor_Id} com sucesso!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO ADICIONAR ITEM: {ex.Message}");
                throw new Exception("Erro ao salvar o item no banco de dados. Tente novamente.");
            }
        }

        /// <summary>
        /// Move um item de um setor para outro com validações
        /// </summary>
        public async Task MoverItemAsync(int itemId, int novoSetorId)
        {
            // ===== VALIDAÇÕES DE NEGÓCIO =====

            // 1. Buscar o item
            var item = await _repository.BuscarPorIdAsync(itemId);

            // 2. Verificar se o item existe
            if (item == null)
            {
                throw new ValidacaoItemException("Item não encontrado.");
            }

            // 3. Verificar se o item já foi removido
            if (item.DataRemocao != null)
            {
                throw new ValidacaoItemException($"Não é possível mover um item que foi removido em {item.DataRemocao:dd/MM/yyyy}.");
            }

            // 4. Verificar se não está tentando mover para o mesmo setor
            if (item.Setor_Id == novoSetorId)
            {
                string nomeSetor = await _repository.BuscarNomeSetorAsync(item.Setor_Id);
                throw new ValidacaoItemException($"O item já está no setor '{nomeSetor}'.");
            }
            // 5. Validar se o setor destino existe
            bool setorExiste = await _repository.SetorExisteAsync(novoSetorId);
            if (!setorExiste)
            {
                throw new ValidacaoItemException("O setor de destino não existe.");
            }

            // ===== FIM DAS VALIDAÇÕES =====

            try
            {
                // Buscar nome do setor antes de mover
                string setorAnterior = await _repository.BuscarNomeSetorAsync(item.Setor_Id);
                string setorNovo = await _repository.BuscarNomeSetorAsync(novoSetorId);

                // Move o item
                await _repository.MoverItemAsync(itemId, novoSetorId);

                Debug.WriteLine($"✓ Item '{item.Nome}' movido de '{setorAnterior}' para '{setorNovo}'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO MOVER ITEM: {ex.Message}");
                throw new Exception("Erro ao mover o item. Tente novamente.");
            }
        }

        /// <summary>
        /// Remove logicamente um item (preenche data_remocao)
        /// </summary>
        public async Task RemoverItemAsync(int itemId)
        {
            // ===== VALIDAÇÕES DE NEGÓCIO =====

            // 1. Buscar o item
            var item = await _repository.BuscarPorIdAsync(itemId);

            // 2. Verificar se o item existe
            if (item == null)
            {
                throw new ValidacaoItemException("Item não encontrado.");
            }

            // 3. Verificar se já não foi removido antes
            if (item.DataRemocao != null)
            {
                throw new ValidacaoItemException($"Este item já foi removido em {item.DataRemocao:dd/MM/yyyy}.");
            }

            // ===== FIM DAS VALIDAÇÕES =====
            try
            {
                // Buscar nome do setor antes de remover
                string nomeSetor = await _repository.BuscarNomeSetorAsync(item.Setor_Id);

                // Remove o item
                await _repository.RemoverItemAsync(itemId);

                Debug.WriteLine($"✓ Item '{item.Nome}' removido do setor '{nomeSetor}' em {DateTime.Now:dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO REMOVER ITEM: {ex.Message}");
                throw new Exception("Erro ao remover o item. Tente novamente.");
            }
        }

        /// <summary>
        /// Lista todos os itens do patrimônio
        /// </summary>
        public async Task<List<Item>> ListarItensAsync(bool incluirRemovidos = false)
        {
            try
            {
                return await _repository.ListarItensAsync(incluirRemovidos);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO LISTAR ITENS: {ex.Message}");
                throw new Exception("Erro ao buscar itens. Tente novamente.");
            }
        }

        /// <summary>
        /// Lista todos os setores para popular ComboBox
        /// </summary>
        public async Task<List<Setor>> ListarSetoresAsync()
        {
            try
            {
                return await _repository.ListarSetoresAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO LISTAR SETORES: {ex.Message}");
                throw new Exception("Erro ao buscar setores. Tente novamente.");
            }
        }

        /// <summary>
        /// Busca um item específico
        /// </summary>
        public async Task<Item?> BuscarItemAsync(int id)
        {
            try
            {
                return await _repository.BuscarPorIdAsync(id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO BUSCAR ITEM: {ex.Message}");
                throw new Exception("Erro ao buscar item. Tente novamente.");
            }
        }
    }
}