using System.ComponentModel.DataAnnotations;

namespace ArtesanoPimentel.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Senha { get; set; } = string.Empty;
    }
}