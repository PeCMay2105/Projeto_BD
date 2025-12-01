using System;
using System.Diagnostics;
using patrimonioDB.Shared.Security;
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
        /// Autentica um usuário e retorna o funcionário correspondente + tipo de usuário
        /// </summary>
        /// <returns>Tupla com (usuario, isAdmin)</returns>
        public (Classes.Funcionario? usuario, bool isAdmin) Autenticar(string login, string senha)
        {
            // 1. Buscar o usuário pelo login (funcionário ou admin)
            var resultado = _repository.BuscarPorLogin(login);

            if (resultado.usuario == null)
            {
                Debug.WriteLine($"✗ Usuário não encontrado para login: {login}");
                return (null, false);
            }

            // 2. Verificar a senha (TEXTO PURO)
            Debug.WriteLine($"--- Autenticação de '{login}' ---");
            Debug.WriteLine($"[1] Senha no banco: '{resultado.usuario.Senha}'");
            Debug.WriteLine($"[2] Senha digitada: '{senha}'");
            Debug.WriteLine($"[3] Tipo de usuário: {(resultado.isAdmin ? "Administrador" : "Funcionário")}");

            // 3. Comparar as senhas diretamente (TEXTO PURO)
            if (resultado.usuario.Senha == senha)
            {
                Debug.WriteLine($"✓ Login bem-sucedido para '{resultado.usuario.Nome}'");
                return (resultado.usuario, resultado.isAdmin);
            }

            Debug.WriteLine($"✗ Senha incorreta para '{resultado.usuario.Nome}'");
            Debug.WriteLine($"   Esperado: '{resultado.usuario.Senha}'");
            Debug.WriteLine($"   Recebido: '{senha}'");
            return (null, false);
        }
    }
}