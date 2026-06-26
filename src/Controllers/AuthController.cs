using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // <--- IMPORTANTE: Adicionado para habilitar SetString e GetString
using ArtesanoPimentel.Data;
using ArtesanoPimentel.Models;

namespace ArtesanoPimentel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/cadastro
        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] Usuario model)
        {
            if (string.IsNullOrWhiteSpace(model.Nome) || 
                string.IsNullOrWhiteSpace(model.Email) || 
                string.IsNullOrWhiteSpace(model.Senha))
            {
                return BadRequest(new { mensagem = "Todos os campos são obrigatórios." });
            }

            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (emailExiste)
            {
                return BadRequest(new { mensagem = "Este e-mail já está cadastrado." });
            }

            _context.Usuarios.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Cadastro realizado com sucesso!" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Senha))
            {
                return BadRequest(new { mensagem = "E-mail e senha são obrigatórios." });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.Senha == model.Senha);

            if (usuario == null)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });
            }

            // Grava os dados do usuário na sessão local da janela desktop
            HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
            HttpContext.Session.SetString("UsuarioNome", usuario.Nome);

            return Ok(new { mensagem = "Login bem-sucedido!", nome = usuario.Nome });
        }

        // GET: api/auth/sessao
        [HttpGet("sessao")]
        public IActionResult ChecarSessao()
        {
            var id = HttpContext.Session.GetString("UsuarioId");
            var nome = HttpContext.Session.GetString("UsuarioNome");

            if (string.IsNullOrEmpty(id))
            {
                return Unauthorized(new { mensagem = "Não autenticado." });
            }

            return Ok(new { id, nome });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { mensagem = "Sessão encerrada." });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}