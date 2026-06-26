using ArtesanoPimentel.Data;using ArtesanoPimentel.Models;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;
namespace ArtesanoPimentel.Controllers{
[ApiController][Route("api/[controller]")]
public class ProdutosController:ControllerBase{
private readonly AppDbContext _context; public ProdutosController(AppDbContext c)=>_context=c;
[HttpGet] public async Task<IActionResult> Listar()=>Ok(await _context.Produtos.ToListAsync());
[HttpPost] public async Task<IActionResult> Cadastrar([FromBody] Produto p){_context.Produtos.Add(p);await _context.SaveChangesAsync();return Ok(p);}
[HttpPut("{id}")] public async Task<IActionResult> Atualizar(int id,[FromBody] Produto p){var x=await _context.Produtos.FindAsync(id); if(x==null)return NotFound(); x.Nome=p.Nome;x.Preco=p.Preco;x.QuantidadeEstoque=p.QuantidadeEstoque; await _context.SaveChangesAsync(); return Ok(x);}
[HttpDelete("{id}")] public async Task<IActionResult> Excluir(int id){var x=await _context.Produtos.FindAsync(id); if(x==null)return NotFound(); _context.Produtos.Remove(x); await _context.SaveChangesAsync(); return NoContent();}
}}