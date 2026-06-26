using ArtesanoPimentel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtesanoPimentel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) { _context = context; }

        [HttpGet("dados")]
        public async Task<IActionResult> GetDadosDashboard()
        {
            // Cards
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);
            var entradasHoje = await _context.EntradasEstoque
                .Where(e => e.DataEntrada >= hoje && e.DataEntrada < amanha)
                .SumAsync(e => (int?)e.Quantidade) ?? 0;

            var totalEstoqueItens = await _context.Produtos.SumAsync(p => (int?)p.QuantidadeEstoque) ?? 0;
            var totalFornecedores = await _context.Fornecedores.CountAsync();

            var ultimaEntrada = await _context.EntradasEstoque
                .OrderByDescending(e => e.DataEntrada)
                .FirstOrDefaultAsync();
            string ultimaEntradaData = ultimaEntrada != null
                ? ultimaEntrada.DataEntrada.ToString("dd/MM/yyyy 'às' HH:mm")
                : "--/--/----";

            // Gráfico de Movimentação de Entradas (últimos 7 dias)
            var dataLimite = DateTime.Today.AddDays(-6);
            var entradasRecentes = await _context.EntradasEstoque
                .Where(e => e.DataEntrada >= dataLimite)
                .ToListAsync();

            var movimentacaoSemana = new int[7]; // 0=Seg ... 6=Dom
            foreach (var e in entradasRecentes)
            {
                int diaWeb = ((int)e.DataEntrada.DayOfWeek + 6) % 7;
                movimentacaoSemana[diaWeb] += e.Quantidade;
            }

            // Gráfico de Rosca – Status do Estoque por QUANTIDADE DE ITENS
            // "Normal" = produto com estoque > 15 unidades
            // "Baixo"  = produto com estoque entre 1 e 15 unidades
            // "Em falta" = produto com estoque = 0
            var todosProdutos = await _context.Produtos.ToListAsync();

            int qtdEmEstoque = todosProdutos.Where(p => p.QuantidadeEstoque > 15).Sum(p => p.QuantidadeEstoque);
            int qtdBaixo     = todosProdutos.Where(p => p.QuantidadeEstoque > 0 && p.QuantidadeEstoque <= 15).Sum(p => p.QuantidadeEstoque);
            int qtdEmFalta   = todosProdutos.Count(p => p.QuantidadeEstoque == 0); // nº de produtos zerados

            // Se não há produtos, retornar zeros para evitar gráfico vazio inválido
            bool semDados = todosProdutos.Count == 0;

            return Ok(new {
                cards = new {
                    entradasHoje,
                    totalEstoque = totalEstoqueItens,
                    totalFornecedores,
                    ultimaEntrada = ultimaEntradaData
                },
                graficoEntradas = movimentacaoSemana,
                statusEstoque = new {
                    emEstoque = qtdEmEstoque,
                    reservado  = qtdBaixo,
                    emFalta    = qtdEmFalta,
                    semDados
                }
            });
        }
    }
}
