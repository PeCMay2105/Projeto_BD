-- 1. Limpeza inicial  

 DROP TABLE IF EXISTS public.demissao CASCADE;  

DROP TABLE IF EXISTS public.contratacao CASCADE;  

DROP TABLE IF EXISTS public.movimentacao CASCADE;  

DROP TABLE IF EXISTS public.venda CASCADE; DROP TABLE IF EXISTS public.compra CASCADE; 

 DROP TABLE IF EXISTS public.item CASCADE;  

DROP TABLE IF EXISTS public.documentos CASCADE;  

DROP TABLE IF EXISTS public.administrador CASCADE;  

DROP TABLE IF EXISTS public.funcionario CASCADE;  

DROP TABLE IF EXISTS public.funcao CASCADE;  

DROP TABLE IF EXISTS public.setor CASCADE;  

DROP TABLE IF EXISTS public.pessoa CASCADE; 

 

-- 2. Tabelas Base  
CREATE TABLE IF NOT EXISTS public.pessoa ( id SERIAL PRIMARY KEY, cpf VARCHAR(11) NOT NULL UNIQUE, nome VARCHAR(100) NOT NULL, nascimento TIMESTAMP WITHOUT TIME ZONE NOT NULL, login VARCHAR(50) NOT NULL UNIQUE, senha VARCHAR(255) NOT NULL ); 
CREATE TABLE IF NOT EXISTS public.setor ( id SERIAL PRIMARY KEY, nome VARCHAR(255) NOT NULL, num_itens INTEGER DEFAULT 0 ); 
CREATE TABLE IF NOT EXISTS public.funcao ( id SERIAL PRIMARY KEY, nome VARCHAR(100) NOT NULL, descricao VARCHAR(255) NOT NULL ); 
CREATE TABLE IF NOT EXISTS public.documentos ( id SERIAL PRIMARY KEY, nome VARCHAR(255), arquivo_pdf BYTEA ); 

 

-- 3. Tabelas Intermediárias (Dependem das tabelas base)
CREATE TABLE IF NOT EXISTS public.item ( id SERIAL PRIMARY KEY, nome VARCHAR(255) NOT NULL, id_setor INTEGER, CONSTRAINT item_id_setor_fkey FOREIGN KEY (id_setor) REFERENCES public.setor (id) ON UPDATE NO ACTION ON DELETE SET NULL ); 
CREATE TABLE IF NOT EXISTS public.funcionario ( id_pessoa INTEGER NOT NULL, id_setor INTEGER, id_funcao INTEGER, salario DOUBLE PRECISION NOT NULL, data_admissao DATE, CONSTRAINT funcionario_pkey PRIMARY KEY (id_pessoa), CONSTRAINT funcionario_id_pessoa_fkey FOREIGN KEY (id_pessoa) REFERENCES public.pessoa (id) ON UPDATE NO ACTION ON DELETE CASCADE, CONSTRAINT funcionario_id_setor_fkey FOREIGN KEY (id_setor) REFERENCES public.setor (id) ON UPDATE NO ACTION ON DELETE SET NULL, CONSTRAINT funcionario_id_funcao_fkey FOREIGN KEY (id_funcao) REFERENCES public.funcao (id) ON UPDATE NO ACTION ON DELETE SET NULL ); 
CREATE INDEX IF NOT EXISTS idx_funcionario_id_funcao ON public.funcionario (id_funcao); 
CREATE TABLE IF NOT EXISTS public.administrador ( id_pessoa INTEGER NOT NULL, salario DOUBLE PRECISION NOT NULL, CONSTRAINT administrador_pkey PRIMARY KEY (id_pessoa), CONSTRAINT administrador_id_pessoa_fkey FOREIGN KEY (id_pessoa) REFERENCES public.pessoa (id) ON UPDATE NO ACTION ON DELETE CASCADE ); 

 

-- 4. Tabelas de Transação/Movimentação (Dependem de tudo acima) 
CREATE TABLE IF NOT EXISTS public.compra ( id SERIAL PRIMARY KEY, id_item INTEGER, preco DOUBLE PRECISION NOT NULL, quantidade INTEGER, data DATE NOT NULL, id_funcionario INTEGER, CONSTRAINT compra_id_funcionario_fkey FOREIGN KEY (id_funcionario) REFERENCES public.funcionario (id_pessoa) ON UPDATE NO ACTION ON DELETE SET NULL, CONSTRAINT compra_id_item_fkey FOREIGN KEY (id_item) REFERENCES public.item (id) ON UPDATE NO ACTION ON DELETE CASCADE ); 
CREATE TABLE IF NOT EXISTS public.venda ( id SERIAL PRIMARY KEY, id_item INTEGER, preco DOUBLE PRECISION NOT NULL, quantidade INTEGER, data DATE NOT NULL, id_funcionario INTEGER, CONSTRAINT venda_id_funcionario_fkey FOREIGN KEY (id_funcionario) REFERENCES public.funcionario (id_pessoa) ON UPDATE NO ACTION ON DELETE SET NULL, CONSTRAINT venda_id_item_fkey FOREIGN KEY (id_item) REFERENCES public.item (id) ON UPDATE NO ACTION ON DELETE CASCADE ); 
CREATE TABLE IF NOT EXISTS public.movimentacao ( id SERIAL PRIMARY KEY, id_item INTEGER, id_setor_origem INTEGER, id_setor_destino INTEGER, quantidade INTEGER, data DATE NOT NULL, id_funcionario INTEGER, CONSTRAINT movimentacao_id_funcionario_fkey FOREIGN KEY (id_funcionario) REFERENCES public.funcionario (id_pessoa) ON UPDATE NO ACTION ON DELETE SET NULL, CONSTRAINT movimentacao_id_item_fkey FOREIGN KEY (id_item) REFERENCES public.item (id) ON UPDATE NO ACTION ON DELETE CASCADE, CONSTRAINT movimentacao_id_setor_origem_fkey FOREIGN KEY (id_setor_origem) REFERENCES public.setor (id) ON UPDATE NO ACTION ON DELETE SET NULL, CONSTRAINT movimentacao_id_setor_destino_fkey FOREIGN KEY (id_setor_destino) REFERENCES public.setor (id) ON UPDATE NO ACTION ON DELETE SET NULL ); 
CREATE TABLE IF NOT EXISTS public.contratacao ( id SERIAL PRIMARY KEY, id_funcionario INTEGER, id_setor INTEGER, salario DOUBLE PRECISION NOT NULL, data DATE NOT NULL, CONSTRAINT contratacao_id_funcionario_fkey FOREIGN KEY (id_funcionario) REFERENCES public.funcionario (id_pessoa) ON UPDATE NO ACTION ON DELETE CASCADE, CONSTRAINT contratacao_id_setor_fkey FOREIGN KEY (id_setor) REFERENCES public.setor (id) ON UPDATE NO ACTION ON DELETE SET NULL ); 
CREATE TABLE IF NOT EXISTS public.demissao ( id SERIAL PRIMARY KEY, id_funcionario INTEGER, id_setor INTEGER, salario DOUBLE PRECISION NOT NULL, data DATE NOT NULL, CONSTRAINT demissao_id_funcionario_fkey FOREIGN KEY (id_funcionario) REFERENCES public.funcionario (id_pessoa) ON UPDATE NO ACTION ON DELETE CASCADE, CONSTRAINT demissao_id_setor_fkey FOREIGN KEY (id_setor) REFERENCES public.setor (id) ON UPDATE NO ACTION ON DELETE SET NULL ); 
