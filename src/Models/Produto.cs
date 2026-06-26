// Models/Produto.cs
namespace ArtesanoPimentel.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; } // Controle de estoque direto aqui
    }
}