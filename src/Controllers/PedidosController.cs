using ArtesanoPimentel.Data;
using ArtesanoPimentel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtesanoPimentel.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _db;
    public PedidosController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await _db.Pedidos
            .Include(p => p.Produto)
            .Include(p => p.Cliente)
            .OrderByDescending(p => p.Data)   // Ordenação no DateTime real, não na string
            .ToListAsync();

        var pedidos = list.Select(p => new
        {
            p.Id,
            produtoNome = p.Produto != null ? p.Produto.Nome : "Desconhecido",
            produtoPreco = p.Produto != null ? p.Produto.Preco : 0,
            clienteNome = p.Cliente != null ? p.Cliente.Nome : "Consumidor Final",
            p.Quantidade,
            p.ValorTotal,
            p.PrecoVenda,
            data = p.Data.ToString("yyyy-MM-ddTHH:mm:ss")
        }).ToList();

        return Ok(pedidos);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PedidoRequest req)
    {
        var produto = await _db.Produtos.FindAsync(req.ProdutoId);
        if (produto == null)
            return BadRequest(new { mensagem = "Produto não encontrado." });
        if (produto.QuantidadeEstoque < req.Quantidade)
            return BadRequest(new { mensagem = $"Estoque insuficiente. Disponível: {produto.QuantidadeEstoque} un." });

        // Se o frontend enviou um preço de venda personalizado, usa ele; caso contrário usa o preço do produto
        decimal precoVenda = req.PrecoVenda > 0 ? req.PrecoVenda : produto.Preco;

        var pedido = new Pedido
        {
            ProdutoId = req.ProdutoId,
            ClienteId = req.ClienteId,
            Quantidade = req.Quantidade,
            PrecoVenda = precoVenda,
            ValorTotal = precoVenda * req.Quantidade,
            Data = DateTime.Now
        };

        produto.QuantidadeEstoque -= req.Quantidade;

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            mensagem = "Venda registrada com sucesso!",
            pedido.Id,
            pedido.ValorTotal,
            pedido.PrecoVenda,
            lucroUnitario = precoVenda - produto.Preco
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Pedidos.Include(x => x.Produto).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        if (p.Produto != null)
            p.Produto.QuantidadeEstoque += p.Quantidade;

        _db.Pedidos.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

/// <summary>DTO de entrada para registrar venda com preço de venda opcional</summary>
public class PedidoRequest
{
    public int ProdutoId { get; set; }
    public int ClienteId { get; set; }
    public int Quantidade { get; set; }
    /// <summary>Preço de venda unitário. Se 0 ou omitido, usa o preço cadastrado no produto.</summary>
    public decimal PrecoVenda { get; set; }
}
