using System.ComponentModel.DataAnnotations;

namespace OnClickSystem.Application.DTOs
{
    public class SolicitacaoSaqueDTO
    {
        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A chave PIX é obrigatória.")]
        public string ChavePix { get; set; }

        // AQUI ESTAVA O ERRO: Faltava esta propriedade
        [Required(ErrorMessage = "O tipo de chave é obrigatório.")]
        public string TipoChave { get; set; }
    }
}