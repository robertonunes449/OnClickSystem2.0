namespace OnClickSystem.Domain.Entities
{
    // Classe auxiliar para o Carrinho (não vai para o banco de dados)
    public class CarrinhoItem
    {
        public int KitId { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; } = 1;
        public decimal Subtotal => Preco * Quantidade;
    }
}