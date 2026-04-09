using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    [Table("SolicitacoesSaque")]
    public class SolicitacaoSaque
    {
        [Key]
        public int ID { get; set; }

        // --- AQUI ESTÁ A CORREÇÃO MÁGICA ---
        // Dizemos ao sistema para usar "ID_Usuario" em vez de tentar adivinhar
        [Column("ID_Usuario")]
        [ForeignKey("Usuario")]
        public int ID_Usuario { get; set; }

        public virtual Usuario? Usuario { get; set; }
        // ------------------------------------

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public string? ChavePix { get; set; }
        public string? TipoChave { get; set; }
        public string? Status { get; set; } // Pendente, Pago

        public DateTime DataSolicitacao { get; set; } = DateTime.Now;
        public DateTime? DataPagamento { get; set; }
    }
}