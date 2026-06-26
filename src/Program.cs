using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ArtesanoPimentel.Data;
using ArtesanoPimentel.Models;

namespace ArtesanoPimentel
{
    public static class Program
    {
        [STAThread] // Garante que a interface gráfica rode na Thread correta
        public static void Main(string[] args)
        {
            // 1. Configuração do WebApplication (Motor Web / API)
            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

            // Configuração do suporte de Sessão na API local
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=artesano.db");
            });

            Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSession(); 
            app.MapControllers();

            // 2. Garante a criação do banco de dados local SQLite antes de abrir a janela
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                // Migração manual: adiciona PrecoVenda à tabela Pedidos se ainda não existir
                // (necessário para bancos criados antes desta versão)
                try
                {
                    db.Database.ExecuteSqlRaw(
                        "ALTER TABLE Pedidos ADD COLUMN PrecoVenda DECIMAL NOT NULL DEFAULT 0");
                }
                catch
                {
                    // Coluna já existe — ignorar o erro silenciosamente
                }

                // Seeder de Dados Iniciais (caso o banco esteja completamente vazio)
                if (!db.Usuarios.Any())
                {
                    db.Usuarios.Add(new Usuario
                    {
                        Nome = "Administrador",
                        Email = "admin@artesano.com",
                        Senha = "admin"
                    });
                }

                if (!db.Clientes.Any())
                {
                    db.Clientes.AddRange(new[]
                    {
                        new Cliente { Nome = "Queijaria do Bairro", Contato = "João Silva", Telefone = "(11) 98765-4321", Email = "joao@queijariabairro.com", Status = "Ativo" },
                        new Cliente { Nome = "Mercado Central", Contato = "Maria Souza", Telefone = "(11) 99999-8888", Email = "compras@mercadocentral.com", Status = "Ativo" },
                        new Cliente { Nome = "Restaurante Sabor Real", Contato = "Carlos Santos", Telefone = "(11) 97777-6666", Email = "carlos@saborreal.com", Status = "Ativo" },
                        new Cliente { Nome = "Antigo Empório", Contato = "Ana Oliveira", Telefone = "(11) 96666-5555", Email = "ana@antigoemporio.com", Status = "Inativo" }
                    });
                }

                if (!db.Fornecedores.Any())
                {
                    db.Fornecedores.AddRange(new[]
                    {
                        new Fornecedor { Nome = "Laticínios Vale Verde", Cnpj = "12.345.678/0001-90", Telefone = "(35) 3221-4455" },
                        new Fornecedor { Nome = "Doces de Minas Tradicional", Cnpj = "98.765.432/0001-21", Telefone = "(31) 3444-5555" }
                    });
                }

                if (!db.Produtos.Any())
                {
                    db.Produtos.AddRange(new[]
                    {
                        new Produto { Nome = "Queijo Minas Padrão", Preco = 32.50m, QuantidadeEstoque = 45 },
                        new Produto { Nome = "Doce de Leite Viçosa 400g", Preco = 18.90m, QuantidadeEstoque = 60 },
                        new Produto { Nome = "Queijo Coalho Premium", Preco = 28.00m, QuantidadeEstoque = 20 },
                        new Produto { Nome = "Requeijão de Corte", Preco = 22.00m, QuantidadeEstoque = 0 }
                    });
                }

                db.SaveChanges();
            }

            // 3. Inicializa o servidor Web de forma assíncrona em segundo plano
            _ = app.RunAsync("http://localhost:5500");

            // 4. Inicializa e desenha a interface gráfica do Desktop (Sem depender do Initialize())
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            System.Windows.Forms.Form janelaPrincipal = new System.Windows.Forms.Form
            {
                Text = "Controle de Vendas",
                Width = 1280,
                Height = 720,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen,
                Icon = System.Drawing.SystemIcons.Application
            };

            Microsoft.Web.WebView2.WinForms.WebView2 navegadorEmbutido = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            janelaPrincipal.Controls.Add(navegadorEmbutido);

            // Carrega a interface web no navegador embutido assim que a janela abrir
            janelaPrincipal.Load += async (sender, e) =>
            {
                await navegadorEmbutido.EnsureCoreWebView2Async(null);
                navegadorEmbutido.CoreWebView2.Navigate("http://localhost:5500/login.html");
                
                // Desabilita recursos de navegador para parecer uma aplicação desktop nativa
                navegadorEmbutido.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                navegadorEmbutido.CoreWebView2.Settings.IsZoomControlEnabled = false;
            };

            // Derruba o servidor Web quando a janela desktop for fechada
            janelaPrincipal.FormClosed += (sender, e) =>
            {
                app.StopAsync().Wait();
                System.Windows.Forms.Application.Exit();
            };

            // Inicia o loop de renderização da janela do Windows
            System.Windows.Forms.Application.Run(janelaPrincipal);
        }
    }
}