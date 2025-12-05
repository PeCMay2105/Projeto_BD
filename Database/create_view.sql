CREATE VIEW vw_detalhes_funcionarios AS
SELECT 
    p.ID AS ID_Pessoa,
    p.Nome AS Nome_Funcionario,
    p.CPF,
    p.LOGIN,
    f.Nome AS Cargo,
    s.Nome AS Setor,
    func.Salario,
    func.ID_Setor,
    func.ID_Funcao
FROM 
    Pessoa p
JOIN 
    Funcionario func ON p.ID = func.ID_Pessoa
LEFT JOIN 
    Setor s ON func.ID_Setor = s.ID
LEFT JOIN 
    Funcao f ON func.ID_Funcao = f.ID;
