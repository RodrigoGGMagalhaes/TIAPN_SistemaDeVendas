// Controllers/EntradasController.cs
using ArtesanoPimentel.Data;
using ArtesanoPimentel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtesanoPimentel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntradasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EntradasController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var list = await _context.EntradasEstoque
                .Include(e => e.Produto)
                .Include(e => e.Fornecedor)
                .ToListAsync();

            var entradas = list.Select(e => new
            {
                e.Id,
                Produto = e.Produto != null ? e.Produto.Nome : "Desconhecido",
                Fornecedor = e.Fornecedor != null ? e.Fornecedor.Nome : "Desconhecido",
                e.Quantidade,
                // Valor pago = preço unitário atual × quantidade entrada
                ValorPago = e.Produto != null ? e.Produto.Preco * e.Quantidade : 0,
                PrecoUnitario = e.Produto != null ? e.Produto.Preco : 0,
                Data = e.DataEntrada.ToString("dd/MM/yyyy HH:mm")
            }).ToList();

            return Ok(entradas);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarEntrada(EntradaEstoque entrada)
        {
            var produto = await _context.Produtos.FindAsync(entrada.ProdutoId);
            if (produto == null) return NotFound(new { mensagem = "Produto não encontrado" });

            entrada.DataEntrada = DateTime.Now;
            _context.EntradasEstoque.Add(entrada);

            produto.QuantidadeEstoque += entrada.Quantidade;

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Entrada registrada e estoque atualizado!" });
        }
    }
}
