using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using patrimonioDB.Classes;

namespace patrimonioDB.Features.GestaoPatrimonio
{
    /// <summary>
    /// Exceção personalizada para erros de validação de patrimônio
    /// </summary>
    public class ValidacaoItemException : Exception
    {
        public ValidacaoItemException(string message) : base(message) { }
    }

    /// <summary>
    /// Service contém toda a LÓGICA DE NEGÓCIO da gestão de patrimônio
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
        /// Cria o item e registra a compra inicial
        /// </summary>
        public async Task AdicionarItemAsync(Item item, double preco, int quantidade, int funcionarioId)
        {
            // ===== VALIDAÇÕES DE NEGÓCIO =====

            // 1. Nome não pode ser vazio
            if (string.IsNullOrWhiteSpace(item.Nome))
            {
                throw new ValidacaoItemException("O nome do item não pode estar vazio.");
            }

            // 2. Quantidade não pode ser negativa ou zero
            if (quantidade <= 0)
            {
                throw new ValidacaoItemException("A quantidade deve ser maior que zero.");
            }

            // 3. Preço não pode ser negativo
            if (preco < 0)
            {
                throw new ValidacaoItemException("O preço não pode ser negativo.");
            }

            // 4. Validar se o setor existe
            bool setorExiste = await _repository.SetorExisteAsync(item.Setor_Id);
            if (!setorExiste)
            {
                throw new ValidacaoItemException("O setor selecionado não existe.");
            }

            // ===== FIM DAS VALIDAÇÕES =====

            try
            {
                await _repository.AdicionarItemAsync(item, preco, quantidade, funcionarioId);
                Debug.WriteLine($"✓ Item '{item.Nome}' adicionado com {quantidade} unidades a R$ {preco:F2}");
            }
            catch (ValidacaoItemException)
            {
                throw; // Re-lança exceções de validação
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO ADICIONAR ITEM: {ex.Message}");
                throw new Exception("Erro ao salvar o item no banco de dados. Tente novamente.");
            }
        }

        /// <summary>
        /// Registra uma nova compra de um item existente
        /// </summary>
        public async Task RegistrarCompraAsync(int idItem, double preco, int quantidade, int funcionarioId)
        {
            // Validações
            if (quantidade <= 0)
            {
                throw new ValidacaoItemException("A quantidade deve ser maior que zero.");
            }

            if (preco < 0)
            {
                throw new ValidacaoItemException("O preço não pode ser negativo.");
            }

            // Verificar se o item existe
            var item = await _repository.BuscarPorIdAsync(idItem);
            if (item == null)
            {
                throw new ValidacaoItemException("Item não encontrado.");
            }

            try
            {
                await _repository.RegistrarCompraAsync(idItem, preco, quantidade, funcionarioId);
                Debug.WriteLine($"✓ Compra registrada: {quantidade}x {item.Nome} a R$ {preco:F2}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO REGISTRAR COMPRA: {ex.Message}");
                throw new Exception("Erro ao registrar compra. Tente novamente.");
            }
        }

        /// <summary>
        /// Registra uma venda de um item
        /// </summary>
        public async Task RegistrarVendaAsync(int idItem, double preco, int quantidade, int funcionarioId)
        {
            // Validações
            if (quantidade <= 0)
            {
                throw new ValidacaoItemException("A quantidade deve ser maior que zero.");
            }

            if (preco < 0)
            {
                throw new ValidacaoItemException("O preço não pode ser negativo.");
            }

            // Verificar se o item existe e tem estoque suficiente
            var item = await _repository.BuscarPorIdAsync(idItem);
            if (item == null)
            {
                throw new ValidacaoItemException("Item não encontrado.");
            }

            if (item.QuantidadeTotal < quantidade)
            {
                throw new ValidacaoItemException($"Estoque insuficiente. Disponível: {item.QuantidadeTotal} unidades.");
            }

            try
            {
                await _repository.RegistrarVendaAsync(idItem, preco, quantidade, funcionarioId);
                Debug.WriteLine($"✓ Venda registrada: {quantidade}x {item.Nome} a R$ {preco:F2}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO REGISTRAR VENDA: {ex.Message}");
                throw new Exception("Erro ao registrar venda. Tente novamente.");
            }
        }

        /// <summary>
        /// Move um item de um setor para outro com validações
        /// Registra a movimentação na tabela movimentacao
        /// </summary>
        public async Task MoverItemAsync(int itemId, int novoSetorId, int quantidade, int funcionarioId)
        {
            // Buscar o item
            var item = await _repository.BuscarPorIdAsync(itemId);
            if (item == null)
            {
                throw new ValidacaoItemException("Item não encontrado.");
            }

            // Verificar se não está tentando mover para o mesmo setor
            if (item.Setor_Id == novoSetorId)
            {
                string nomeSetor = await _repository.BuscarNomeSetorAsync(item.Setor_Id);
                throw new ValidacaoItemException($"O item já está no setor '{nomeSetor}'.");
            }

            // Validar se o setor destino existe
            bool setorExiste = await _repository.SetorExisteAsync(novoSetorId);
            if (!setorExiste)
            {
                throw new ValidacaoItemException("O setor de destino não existe.");
            }

            // Validar quantidade
            if (quantidade <= 0)
            {
                throw new ValidacaoItemException("A quantidade deve ser maior que zero.");
            }

            if (quantidade > item.QuantidadeTotal)
            {
                throw new ValidacaoItemException($"Quantidade inválida. Disponível: {item.QuantidadeTotal} unidades.");
            }

            try
            {
                string setorAnterior = await _repository.BuscarNomeSetorAsync(item.Setor_Id);
                string setorNovo = await _repository.BuscarNomeSetorAsync(novoSetorId);

                // Passar setorOrigemId, setorDestinoId, quantidade e funcionarioId
                await _repository.MoverItemAsync(itemId, item.Setor_Id, novoSetorId, quantidade, funcionarioId);
                Debug.WriteLine($"✓ Item '{item.Nome}' ({quantidade} unidades) movido de '{setorAnterior}' para '{setorNovo}'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO MOVER ITEM: {ex.Message}");
                throw new Exception("Erro ao mover o item. Tente novamente.");
            }
        }

        /// <summary>
        /// Remove (vende) uma quantidade específica de um item
        /// IMPORTANTE: O item NÃO é deletado do banco de dados!
        /// Apenas registra uma venda com a quantidade especificada
        /// </summary>
        public async Task RemoverItemAsync(int itemId, int quantidade, int funcionarioId)
        {
            var item = await _repository.BuscarPorIdAsync(itemId);
            if (item == null)
            {
                throw new ValidacaoItemException("Item não encontrado.");
            }

            // Validar quantidade
            if (quantidade <= 0)
            {
                throw new ValidacaoItemException("A quantidade deve ser maior que zero.");
            }

            // Verificar se há estoque suficiente
            if (item.QuantidadeTotal <= 0)
            {
                throw new ValidacaoItemException($"Não há estoque disponível de '{item.Nome}' para vender.");
            }

            if (item.QuantidadeTotal < quantidade)
            {
                throw new ValidacaoItemException($"Estoque insuficiente. Disponível: {item.QuantidadeTotal} unidades.");
            }

            try
            {
                string nomeSetor = await _repository.BuscarNomeSetorAsync(item.Setor_Id);
                await _repository.RemoverItemAsync(itemId, quantidade, funcionarioId);
                Debug.WriteLine($"✓ Vendidas {quantidade} unidade(s) de '{item.Nome}'");
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
                return await _repository.ListarItensAsync();
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

        /// <summary>
        /// Lista o histórico de compras de um item
        /// </summary>
        public async Task<List<Compra>> ListarComprasDoItemAsync(int idItem)
        {
            try
            {
                return await _repository.ListarComprasDoItemAsync(idItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO LISTAR COMPRAS: {ex.Message}");
                throw new Exception("Erro ao buscar histórico de compras.");
            }
        }

        /// <summary>
        /// Lista o histórico de vendas de um item
        /// </summary>
        public async Task<List<Venda>> ListarVendasDoItemAsync(int idItem)
        {
            try
            {
                return await _repository.ListarVendasDoItemAsync(idItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO LISTAR VENDAS: {ex.Message}");
                throw new Exception("Erro ao buscar histórico de vendas.");
            }
        }

        /// <summary>
        /// Lista o histórico de movimentações de um item
        /// </summary>
        public async Task<List<Movimentacao>> ListarMovimentacoesDoItemAsync(int idItem)
        {
            try
            {
                return await _repository.ListarMovimentacoesDoItemAsync(idItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO AO LISTAR MOVIMENTAÇÕES: {ex.Message}");
                throw new Exception("Erro ao buscar histórico de movimentações.");
            }
        }
    }
}