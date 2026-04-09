using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        public int ID { get; set; }
        public int? ID_Usuario { get; set; }
        public int? ID_Cliente { get; set; }
        public int ID_Kit { get; set; }
        public DateTime DataPedido { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }

        [ForeignKey("ID_Usuario")]
        public virtual Usuario? Usuario { get; set; }
        [ForeignKey("ID_Cliente")]
        public virtual Cliente? Cliente { get; set; }
        [ForeignKey("ID_Kit")]
        public virtual Kit? Kit { get; set; }
    }
}