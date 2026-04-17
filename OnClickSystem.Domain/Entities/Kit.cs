using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    [Table("Kits")]
    public class Kit
    {
        [Key]
        public int ID { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }

        // Nova propriedade para armazenar o caminho ou URL da imagem
        public string? ImagemUrl { get; set; }
    }
}