using ArtesanoPimentel.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtesanoPimentel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
        public DbSet<EntradaEstoque> EntradasEstoque => Set<EntradaEstoque>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
    }
}