using ArtesanoPimentel.Data;using ArtesanoPimentel.Models;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;
namespace ArtesanoPimentel.Controllers;
[ApiController][Route("api/clientes")]
public class ClientesController:ControllerBase{
private readonly AppDbContext _db; public ClientesController(AppDbContext db){_db=db;}
[HttpGet] public async Task<IActionResult> Get()=>Ok(await _db.Clientes.ToListAsync());
[HttpPost] public async Task<IActionResult> Post([FromBody] Cliente c){_db.Clientes.Add(c);await _db.SaveChangesAsync();return Ok(c);}
[HttpPut("{id}")] public async Task<IActionResult> Put(int id,[FromBody] Cliente c){var x=await _db.Clientes.FindAsync(id); if(x==null)return NotFound(); x.Nome=c.Nome;x.Telefone=c.Telefone;x.Email=c.Email;x.Contato=c.Contato;x.Status=c.Status; await _db.SaveChangesAsync(); return Ok(x);}
[HttpDelete("{id}")] public async Task<IActionResult> Delete(int id){var x=await _db.Clientes.FindAsync(id); if(x==null)return NotFound(); _db.Clientes.Remove(x); await _db.SaveChangesAsync(); return NoContent();}
}