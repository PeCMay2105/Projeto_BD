CREATE OR REPLACE FUNCTION fn_registrar_contratacao_auto()
RETURNS TRIGGER AS $$
BEGIN
    -- Insere na tabela Contratacao pegando os dados do novo funcionario (NEW)
    INSERT INTO public.contratacao (
        id_funcionario, 
        id_setor, 
        salario, 
        data
    ) 
    VALUES (
        NEW.id_pessoa,     -- O ID da pessoa que acabou de virar funcionário
        NEW.id_setor,      -- O setor que ele entrou
        NEW.salario,       -- O salário inicial
        -- IMPORTANTE: A sua tabela 'contratacao' obriga ter data (NOT NULL), 
        -- mas 'funcionario' aceita data nula.
        -- O COALESCE abaixo garante que se não vier data, usa a data de hoje.
        COALESCE(NEW.data_admissao, CURRENT_DATE) 
    );
    
    RETURN NEW;
END;

CREATE OR REPLACE FUNCTION fn_registrar_demissao_auto()
RETURNS TRIGGER AS $$
BEGIN
    
    INSERT INTO public.demissao (
        id_funcionario, 
        id_setor, 
        salario, 
        data
    ) 
    VALUES (
        NEW.id_pessoa,     
        NEW.id_setor,      
        NEW.salario,       
        COALESCE(NEW.data_admissao, CURRENT_DATE) 
    );
    
    RETURN NEW;
END;

-- Remove a trigger se ela já existir para evitar erro ao recriar
DROP TRIGGER IF EXISTS trg_auto_contratacao ON public.funcionario;

CREATE TRIGGER trg_auto_contratacao
AFTER INSERT ON public.funcionario
FOR EACH ROW
EXECUTE FUNCTION fn_registrar_contratacao_auto();

DROP TRIGGER IF EXISTS trg_auto_demissao ON public.funcionario;

CREATE TRIGGER trg_auto_demissao
AFTER DELETE ON public.funcionario
FOR EACH ROW
EXECUTE FUNCTION fn_registrar_demissao_auto();

$$ LANGUAGE plpgsql;
