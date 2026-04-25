using System;

namespace OnClickSystem.Application.DTOs
{
    public class SaquePendenteDTO
    {
        public int ID { get; set; }
        public string NomeUsuario { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string ChavePix { get; set; }
    }
}