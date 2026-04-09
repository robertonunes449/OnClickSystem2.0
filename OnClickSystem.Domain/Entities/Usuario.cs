using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnClickSystem.Domain.Entities
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Senha { get; set; }

        // --- NOVOS CAMPOS ADICIONADOS ---
        [StringLength(20)]
        public string? CPF { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }
        // --------------------------------

        public string Perfil { get; set; } = "Comum";
        public bool Ativo { get; set; } = false;
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Dados Financeiros e Rede
        public string? ChavePix { get; set; }
        public string? TipoChavePix { get; set; }
        public int? ID_Patrocinador { get; set; }

        // A etiqueta [NotMapped] diz ao sistema para NÃO criar isto na base de dados.
        // Serve apenas para a memória do site saber o nível da pessoa.
        [NotMapped]
        public int NivelNaRede { get; set; }

        [ForeignKey("ID_Patrocinador")]
        public virtual Usuario? Patrocinador { get; set; }

        [InverseProperty("Patrocinador")]
        public virtual ICollection<Usuario> Indicados { get; set; } = new List<Usuario>();
    }
}