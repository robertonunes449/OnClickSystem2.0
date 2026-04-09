using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    [Table("LogsSistema")]
    public class LogSistema
    {
        [Key]
        public int ID { get; set; }
        public DateTime DataHora { get; set; }
        public string? UsuarioResponsavel { get; set; }
        public string Acao { get; set; }
        public string? Detalhes { get; set; }
    }
}