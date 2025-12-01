-- =====================================================
-- SCRIPT: Criar Administrador Inicial
-- =====================================================
-- Execute este script para criar o primeiro administrador
-- do sistema (login: admin, senha: admin123)
-- =====================================================

-- 1. Inserir na tabela Pessoa
INSERT INTO Pessoa (nome, cpf, login, senha, nascimento)
VALUES ('Administrador', '000.000.000-00', 'admin', 'admin123', '1990-01-01')
ON CONFLICT DO NOTHING;

-- 2. Pegar o ID do admin
DO $$
DECLARE
    v_id_pessoa INTEGER;
BEGIN
    -- Buscar ID da pessoa
    SELECT id INTO v_id_pessoa
    FROM Pessoa
    WHERE login = 'admin';
    
    -- Inserir na tabela Administrador
    INSERT INTO Administrador (id_pessoa, salario)
    VALUES (v_id_pessoa, 10000.00)
    ON CONFLICT DO NOTHING;
    
    RAISE NOTICE '? Administrador criado com sucesso!';
    RAISE NOTICE '  Login: admin';
    RAISE NOTICE '  Senha: admin123';
    RAISE NOTICE '  ID: %', v_id_pessoa;
END $$;

-- 3. Verificar se foi criado
SELECT 
    p.id,
    p.nome,
    p.login,
    p.senha,
    a.salario,
    'Administrador' as tipo
FROM Pessoa p
INNER JOIN Administrador a ON p.id = a.id_pessoa
WHERE p.login = 'admin';
