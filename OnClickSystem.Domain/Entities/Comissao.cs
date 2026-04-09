using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    public class Comissao
    {
        public int ID { get; set; }
        public int ID_Pedido { get; set; }
        public int ID_Beneficiario { get; set; }
        public int Nivel { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }
        public DateTime DataGeracao { get; set; }
    }
}