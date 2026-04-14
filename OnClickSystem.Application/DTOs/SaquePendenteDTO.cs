namespace OnClickSystem.Application.DTOs
{
    public class SaquePendenteDTO
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; }
        public decimal Valor { get; set; }
        public string DataSolicitacao { get; set; }
        public string Status { get; set; }
    }
}