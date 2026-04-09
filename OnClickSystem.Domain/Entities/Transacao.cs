using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    [Table("Transacoes")]
    public class Transacao
    {
        [Key] public int ID { get; set; }
        public int ID_Usuario { get; set; }
        public string Descricao { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }
        public string Tipo { get; set; }
        public DateTime Data { get; set; }
    }
}