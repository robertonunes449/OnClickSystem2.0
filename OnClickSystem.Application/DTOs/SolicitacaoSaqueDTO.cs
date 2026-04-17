using System.ComponentModel.DataAnnotations;

public class SolicitacaoSaqueDTO
{
    [Required]
    public decimal Valor { get; set; }

    [Required]
    public string ChavePix { get; set; }
}