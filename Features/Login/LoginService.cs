using System;
using patrimonioDB.Shared.Utils;
using patrimonioDB.Classes;

namespace patrimonioDB.Features.Login
{
    public class LoginService
    {
        private readonly LoginRepository _repository;

        public LoginService()
        {
            _repository = new LoginRepository();
        }

        /// <summary>
        /// Autentica um usuário comparando hashes de senha
        /// 
        /// ⚠️ IMPORTANTE - PROJETO DIDÁTICO:
        /// Neste projeto, as senhas estão em texto puro no banco para demonstração.
        /// O hash é feito no momento da comparação para ilustrar o conceito.
        /// 
        /// EM PRODUÇÃO: as senhas devem ser armazenadas JÁ em hash no banco,
        /// e o hash deve ser feito no momento do CADASTRO, não do login.
        /// </summary>
        public Funcionario? Autenticar(string login, string senha)
        {
            // 1. Buscar usuário pelo login
            var funcionario = _repository.BuscarPorLogin(login);

            if (funcionario == null)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Usuário '{login}' não encontrado");
                return null;
            }

            // 2. [DEMONSTRAÇÃO DIDÁTICA] Fazer hash de ambas as senhas
            string hashSenhaBanco = PasswordHasher.HashPassword(funcionario.Senha);
            string hashSenhaDigitada = PasswordHasher.HashPassword(senha);

            // 3. Log para fins didáticos (remover em produção)
            System.Diagnostics.Debug.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("║          DEMONSTRAÇÃO DE AUTENTICAÇÃO COM HASH             ║");
            System.Diagnostics.Debug.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Diagnostics.Debug.WriteLine($"[1] Senha armazenada no banco (texto): '{funcionario.Senha}'");
            System.Diagnostics.Debug.WriteLine($"[2] Hash da senha do banco: {hashSenhaBanco}");
            System.Diagnostics.Debug.WriteLine($"[3] Senha digitada pelo usuário (texto): '{senha}'");
            System.Diagnostics.Debug.WriteLine($"[4] Hash da senha digitada: {hashSenhaDigitada}");
            System.Diagnostics.Debug.WriteLine($"[5] Os hashes são iguais? {hashSenhaBanco == hashSenhaDigitada}");
            System.Diagnostics.Debug.WriteLine("────────────────────────────────────────────────────────────");

            // 4. Comparar os hashes
            if (hashSenhaBanco == hashSenhaDigitada)
            {
                System.Diagnostics.Debug.WriteLine($"✓ Login bem-sucedido para '{funcionario.Nome}'");
                return funcionario;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✗ Senha incorreta para o usuário '{login}'");
                return null;
            }
        }

        /// <summary>
        /// Autentica um administrador comparando hashes de senha
        /// </summary>
        public Administrador? AutenticarAdministrador(string login, string senha)
        {
            // 1. Buscar administrador pelo login
            var administrador = _repository.BuscarAdministradorPorLogin(login);

            if (administrador == null)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Administrador '{login}' não encontrado");
                return null;
            }

            // 2. [DEMONSTRAÇÃO DIDÁTICA] Fazer hash de ambas as senhas
            string hashSenhaBanco = PasswordHasher.HashPassword(administrador.Senha);
            string hashSenhaDigitada = PasswordHasher.HashPassword(senha);

            // 3. Log para fins didáticos (remover em produção)
            System.Diagnostics.Debug.WriteLine("╔════════════════════════════════════════════════════════════╗");
            System.Diagnostics.Debug.WriteLine("║       DEMONSTRAÇÃO DE AUTENTICAÇÃO DE ADMINISTRADOR        ║");
            System.Diagnostics.Debug.WriteLine("╚════════════════════════════════════════════════════════════╝");
            System.Diagnostics.Debug.WriteLine($"[1] Senha armazenada no banco (texto): '{administrador.Senha}'");
            System.Diagnostics.Debug.WriteLine($"[2] Hash da senha do banco: {hashSenhaBanco}");
            System.Diagnostics.Debug.WriteLine($"[3] Senha digitada pelo administrador (texto): '{senha}'");
            System.Diagnostics.Debug.WriteLine($"[4] Hash da senha digitada: {hashSenhaDigitada}");
            System.Diagnostics.Debug.WriteLine($"[5] Os hashes são iguais? {hashSenhaBanco == hashSenhaDigitada}");
            System.Diagnostics.Debug.WriteLine("────────────────────────────────────────────────────────────");

            // 4. Comparar os hashes
            if (hashSenhaBanco == hashSenhaDigitada)
            {
                System.Diagnostics.Debug.WriteLine($"✓ Login de administrador bem-sucedido para '{administrador.Nome}'");
                return administrador;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✗ Senha incorreta para o administrador '{login}'");
                return null;
            }
        }
    }
}