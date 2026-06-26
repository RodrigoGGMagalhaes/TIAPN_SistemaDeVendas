using ArtesanoPimentel.Data;using ArtesanoPimentel.Models;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;
namespace ArtesanoPimentel.Controllers{
[ApiController][Route("api/[controller]")]
public class FornecedoresController:ControllerBase{
private readonly AppDbContext _context; public FornecedoresController(AppDbContext c)=>_context=c;
[HttpGet] public async Task<IActionResult> Listar()=>Ok(await _context.Fornecedores.ToListAsync());
[HttpPost] public async Task<IActionResult> Cadastrar([FromBody] Fornecedor f){_context.Fornecedores.Add(f);await _context.SaveChangesAsync();return Ok(f);}
[HttpPut("{id}")] public async Task<IActionResult> Atualizar(int id,[FromBody] Fornecedor f){var x=await _context.Fornecedores.FindAsync(id); if(x==null)return NotFound(); x.Nome=f.Nome;x.Cnpj=f.Cnpj;x.Telefone=f.Telefone; await _context.SaveChangesAsync(); return Ok(x);}
[HttpDelete("{id}")] public async Task<IActionResult> Excluir(int id){var x=await _context.Fornecedores.FindAsync(id); if(x==null)return NotFound(); _context.Fornecedores.Remove(x); await _context.SaveChangesAsync(); return NoContent();}
}}