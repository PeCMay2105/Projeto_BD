-- ===============================================
-- STORED PROCEDURES - SISTEMA DE PATRIMÔNIO
-- ===============================================
-- Execute este script no PostgreSQL para criar
-- todas as procedures necessárias
-- ===============================================

-- PRIMEIRO: Dropar procedures antigas se existirem
DROP PROCEDURE IF EXISTS sp_adicionar_item_com_compra;
DROP PROCEDURE IF EXISTS sp_registrar_compra;
DROP PROCEDURE IF EXISTS sp_registrar_venda;
DROP PROCEDURE IF EXISTS sp_mover_item;
DROP PROCEDURE IF EXISTS sp_remover_item_completo;
DROP FUNCTION IF EXISTS fn_obter_estoque_item;

-- ================================================
-- 1. PROCEDURE: Adicionar Item com Compra Inicial
-- ================================================
CREATE OR REPLACE PROCEDURE sp_adicionar_item_com_compra(
    p_nome_item TEXT,
    p_id_setor INTEGER,
    p_preco DOUBLE PRECISION,
    p_quantidade INTEGER,
    p_id_funcionario INTEGER,
    INOUT o_item_id INTEGER DEFAULT NULL,
    INOUT o_mensagem TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Validações
    IF p_nome_item IS NULL OR TRIM(p_nome_item) = '' THEN
        RAISE EXCEPTION 'Nome do item não pode estar vazio';
    END IF;
    
    IF p_quantidade <= 0 THEN
        RAISE EXCEPTION 'Quantidade deve ser maior que zero';
    END IF;

    IF p_preco < 0 THEN
    RAISE EXCEPTION 'Preço não pode ser negativo';
  END IF;
    
    IF NOT EXISTS (SELECT 1 FROM setor WHERE id = p_id_setor) THEN
        RAISE EXCEPTION 'Setor não existe';
END IF;
    
    -- 1. Inserir item
    INSERT INTO item (nome, id_setor)
    VALUES (p_nome_item, p_id_setor)
    RETURNING id INTO o_item_id;
    
    -- 2. Registrar compra inicial
    INSERT INTO compra (id_item, preco, quantidade, data, id_funcionario)
    VALUES (o_item_id, p_preco, p_quantidade, CURRENT_TIMESTAMP, p_id_funcionario);
    
    -- Mensagem de sucesso
    o_mensagem := FORMAT('Item "%s" adicionado com sucesso (ID: %s)', p_nome_item, o_item_id);
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Erro ao adicionar item: %', SQLERRM;
END;
$$;

-- ================================================
-- 2. PROCEDURE: Registrar Compra
-- ================================================
CREATE OR REPLACE PROCEDURE sp_registrar_compra(
    p_id_item INTEGER,
 p_preco DOUBLE PRECISION,
    p_quantidade INTEGER,
    p_id_funcionario INTEGER,
    INOUT o_mensagem TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_nome_item TEXT;
BEGIN
    -- Validações
    IF p_quantidade <= 0 THEN
 RAISE EXCEPTION 'Quantidade deve ser maior que zero';
    END IF;
    
    IF p_preco < 0 THEN
        RAISE EXCEPTION 'Preço não pode ser negativo';
    END IF;
    
    -- Verificar se item existe
    SELECT nome INTO v_nome_item
    FROM item WHERE id = p_id_item;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Item não encontrado';
    END IF;
    
-- Registrar compra
    INSERT INTO compra (id_item, preco, quantidade, data, id_funcionario)
    VALUES (p_id_item, p_preco, p_quantidade, CURRENT_TIMESTAMP, p_id_funcionario);
    
  o_mensagem := FORMAT('Compra de %s unidade(s) de "%s" registrada', p_quantidade, v_nome_item);
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Erro ao registrar compra: %', SQLERRM;
END;
$$;

-- ================================================
-- 3. PROCEDURE: Registrar Venda
-- ================================================
CREATE OR REPLACE PROCEDURE sp_registrar_venda(
    p_id_item INTEGER,
    p_preco DOUBLE PRECISION,
 p_quantidade INTEGER,
    p_id_funcionario INTEGER,
    INOUT o_mensagem TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_estoque_disponivel INTEGER;
  v_nome_item TEXT;
BEGIN
  -- Validações
    IF p_quantidade <= 0 THEN
      RAISE EXCEPTION 'Quantidade deve ser maior que zero';
    END IF;
 
    IF p_preco < 0 THEN
        RAISE EXCEPTION 'Preço não pode ser negativo';
    END IF;
    
    -- Calcular estoque disponível
    SELECT 
        i.nome,
      COALESCE(SUM(c.quantidade), 0) - COALESCE(SUM(v.quantidade), 0)
    INTO v_nome_item, v_estoque_disponivel
    FROM item i
    LEFT JOIN compra c ON i.id = c.id_item
    LEFT JOIN venda v ON i.id = v.id_item
 WHERE i.id = p_id_item
    GROUP BY i.nome;
    
    IF NOT FOUND THEN
   RAISE EXCEPTION 'Item não encontrado';
    END IF;
    
    IF v_estoque_disponivel < p_quantidade THEN
  RAISE EXCEPTION 'Estoque insuficiente. Disponível: % unidades', v_estoque_disponivel;
 END IF;
    
    -- Registrar venda
    INSERT INTO venda (id_item, preco, quantidade, data, id_funcionario)
    VALUES (p_id_item, p_preco, p_quantidade, CURRENT_TIMESTAMP, p_id_funcionario);

  o_mensagem := FORMAT('Venda de %s unidade(s) de "%s" registrada', p_quantidade, v_nome_item);
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Erro ao registrar venda: %', SQLERRM;
END;
$$;

-- ================================================
-- 4. PROCEDURE: Mover Item entre Setores
-- ================================================
CREATE OR REPLACE PROCEDURE sp_mover_item(
    p_id_item INTEGER,
    p_id_setor_origem INTEGER,
    p_id_setor_destino INTEGER,
    p_quantidade INTEGER,
p_id_funcionario INTEGER,
    INOUT o_mensagem TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_setor_atual INTEGER;
    v_nome_item TEXT;
    v_nome_setor_origem TEXT;
    v_nome_setor_destino TEXT;
BEGIN
    -- Validações
    IF p_quantidade <= 0 THEN
     RAISE EXCEPTION 'Quantidade deve ser maior que zero';
    END IF;
    
    -- Verificar se item existe e buscar setor atual
    SELECT id_setor, nome INTO v_setor_atual, v_nome_item
    FROM item WHERE id = p_id_item;

  IF NOT FOUND THEN
        RAISE EXCEPTION 'Item não encontrado';
    END IF;
    
    -- Verificar se está no setor de origem correto
    IF v_setor_atual != p_id_setor_origem THEN
  RAISE EXCEPTION 'Item não está no setor de origem informado';
    END IF;
 
  -- Verificar se não é o mesmo setor
    IF p_id_setor_origem = p_id_setor_destino THEN
    RAISE EXCEPTION 'Setor de origem e destino são iguais';
    END IF;
    
  -- Verificar se setor destino existe
    SELECT nome INTO v_nome_setor_destino
    FROM setor WHERE id = p_id_setor_destino;
    
  IF NOT FOUND THEN
      RAISE EXCEPTION 'Setor de destino não existe';
    END IF;
    
    -- Buscar nome do setor origem
    SELECT nome INTO v_nome_setor_origem
    FROM setor WHERE id = p_id_setor_origem;
    
    -- 1. Atualizar setor do item
    UPDATE item 
    SET id_setor = p_id_setor_destino
    WHERE id = p_id_item;
    
    -- 2. Registrar movimentação
    INSERT INTO movimentacao (
 id_item, 
 id_setor_origem, 
        id_setor_destino, 
  quantidade, 
        data, 
        id_funcionario
    )
    VALUES (
        p_id_item,
        p_id_setor_origem,
        p_id_setor_destino,
  p_quantidade,
     CURRENT_TIMESTAMP,
  p_id_funcionario
    );
    
    o_mensagem := FORMAT(
   '%s unidade(s) de "%s" movida(s) de "%s" para "%s"',
      p_quantidade, v_nome_item, v_nome_setor_origem, v_nome_setor_destino
 );
    
EXCEPTION
    WHEN OTHERS THEN
   RAISE EXCEPTION 'Erro ao mover item: %', SQLERRM;
END;
$$;

-- ================================================
-- 5. PROCEDURE: Vender Item (Quantidade Personalizada)
-- ================================================
CREATE OR REPLACE PROCEDURE sp_remover_item_completo(
    p_id_item INTEGER,
    p_quantidade INTEGER,
    p_id_funcionario INTEGER,
 INOUT o_mensagem TEXT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_nome_item TEXT;
    v_estoque_disponivel INTEGER;
  v_preco_medio DOUBLE PRECISION;
BEGIN
    -- Buscar informações do item
    SELECT nome INTO v_nome_item
    FROM item WHERE id = p_id_item;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Item não encontrado';
    END IF;
    
  -- Validar quantidade
  IF p_quantidade <= 0 THEN
        RAISE EXCEPTION 'Quantidade deve ser maior que zero';
    END IF;
    
    -- Calcular estoque disponível e preço médio
 SELECT 
        COALESCE(SUM(c.quantidade), 0) - COALESCE(SUM(v.quantidade), 0),
        COALESCE(AVG(c.preco), 0)
 INTO v_estoque_disponivel, v_preco_medio
    FROM item i
    LEFT JOIN compra c ON i.id = c.id_item
    LEFT JOIN venda v ON i.id = v.id_item
    WHERE i.id = p_id_item
    GROUP BY i.id;
    
    -- Verificar se há estoque suficiente
    IF v_estoque_disponivel <= 0 THEN
        RAISE EXCEPTION 'Não há estoque disponível para vender';
    END IF;
    
IF v_estoque_disponivel < p_quantidade THEN
   RAISE EXCEPTION 'Estoque insuficiente. Disponível: % unidades', v_estoque_disponivel;
    END IF;
    
    -- Registrar a venda com a quantidade escolhida
    INSERT INTO venda (id_item, preco, quantidade, data, id_funcionario)
    VALUES (
      p_id_item, 
        v_preco_medio,
    p_quantidade,
        CURRENT_TIMESTAMP,
      p_id_funcionario
    );
    
    -- Mensagem corrigida: usar TO_CHAR para formatar o preço
    o_mensagem := FORMAT(
        'Vendidas %s unidade(s) de "%s" a R$ %s (Restante: %s)',
        p_quantidade,
        v_nome_item, 
     TO_CHAR(v_preco_medio, 'FM999999990.00'),  -- ? Corrigido
    v_estoque_disponivel - p_quantidade
    );
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Erro ao vender item: %', SQLERRM;
END;
$$;

-- ================================================
-- 6. FUNCTION: Obter Estoque de um Item
-- (Mantida como FUNCTION porque retorna valor)
-- ================================================
CREATE OR REPLACE FUNCTION fn_obter_estoque_item(
    p_id_item INTEGER
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_estoque INTEGER;
BEGIN
    SELECT COALESCE(SUM(c.quantidade), 0) - COALESCE(SUM(v.quantidade), 0)
    INTO v_estoque
    FROM item i
    LEFT JOIN compra c ON i.id = c.id_item
    LEFT JOIN venda v ON i.id = v.id_item
    WHERE i.id = p_id_item
    GROUP BY i.id;
    
    RETURN COALESCE(v_estoque, 0);
END;
$$;

-- ================================================
-- VERIFICAR E LISTAR PROCEDURES CRIADAS
-- ================================================
DO $$
DECLARE
    proc_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO proc_count
    FROM pg_proc p
    JOIN pg_namespace n ON p.pronamespace = n.oid
    WHERE n.nspname = 'public' 
    AND p.proname LIKE 'sp_%';
    
    RAISE NOTICE '? Script executado com sucesso!';
    RAISE NOTICE '';
    RAISE NOTICE '?? Procedures criadas: %', proc_count;
    RAISE NOTICE '';
    RAISE NOTICE 'Procedures disponíveis:';
 RAISE NOTICE '  1. sp_adicionar_item_com_compra - Adiciona item + compra inicial';
    RAISE NOTICE '  2. sp_registrar_compra - Registra nova compra';
    RAISE NOTICE '  3. sp_registrar_venda - Registra venda parcial';
    RAISE NOTICE '  4. sp_mover_item - Move item entre setores';
    RAISE NOTICE '  5. sp_remover_item_completo - VENDE TODO O ESTOQUE (não deleta!)';
    RAISE NOTICE '  6. fn_obter_estoque_item - Retorna quantidade disponível';
    RAISE NOTICE '';
    RAISE NOTICE '?? IMPORTANTE: Remover item = Vender todo estoque!';
    RAISE NOTICE '?? Item permanece no banco para histórico!';
END $$;
