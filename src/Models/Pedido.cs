namespace ArtesanoPimentel.Models;

public class Pedido
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public int ClienteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
    /// <summary>Preço de venda unitário definido no momento da venda (pode diferir do preço cadastrado)</summary>
    public decimal PrecoVenda { get; set; }
    public DateTime Data { get; set; }

    // Navegação
    public Produto? Produto { get; set; }
    public Cliente? Cliente { get; set; }
}
