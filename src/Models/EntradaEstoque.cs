// Models/EntradaEstoque.cs
using System;

namespace ArtesanoPimentel.Models
{
    public class EntradaEstoque
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }
        public int FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataEntrada { get; set; } = DateTime.Now;
    }
}