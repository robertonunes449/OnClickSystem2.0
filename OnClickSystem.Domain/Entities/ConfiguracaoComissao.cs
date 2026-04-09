using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    public class ConfiguracaoComissao
    {
        [Key]
        public int Nivel { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Porcentagem { get; set; }
    }
}