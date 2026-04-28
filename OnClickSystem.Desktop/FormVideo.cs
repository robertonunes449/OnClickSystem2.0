using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    public partial class FormVideo : Form
    {
        // Variável que vai segurar o nosso GIF na tela
        private PictureBox caixaGif;

        public FormVideo()
        {
            InitializeComponent();

            // Liga o cronómetro à função que abre o login
            timer1.Tick += timer1_Tick;

            // 1. Define a cor de fundo do ecrã principal para Branco
            this.BackColor = Color.Black;

            CarregarAnimacaoGif();
        }

        private void CarregarAnimacaoGif()
        {
            string caminhoGif = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Videos", "logo.gif");

            caixaGif = new PictureBox();
            caixaGif.Dock = DockStyle.Fill;
            caixaGif.SizeMode = PictureBoxSizeMode.StretchImage;

            // 2. Define a cor de fundo da caixa do GIF para Branco também
            caixaGif.BackColor = Color.Black;

            if (File.Exists(caminhoGif))
            {
                caixaGif.Image = Image.FromFile(caminhoGif);
            }

            this.Controls.Add(caixaGif);

            // IMPORTANTE: Vá ao Modo de Design, clique no timer1 e certifique-se 
            // de que a propriedade "Interval" tem o tempo EXATO do seu GIF.
            // Exemplo: se o GIF dura 3,5 segundos, escreva 3500.
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // O tempo da animação acabou, paramos o cronómetro
            timer1.Stop();

            // 3. O GRANDE TRUQUE: Removemos o GIF da memória da caixa!
            // Como removemos a imagem e os fundos são brancos, 
            // a tela fica instantaneamente 100% branca e o GIF não repete.
            caixaGif.Image = null;

            // Preparamos a tela de Login
            FormLogin telaLogin = new FormLogin();
            telaLogin.StartPosition = FormStartPosition.CenterParent;

            // Abre o login "travado" POR CIMA do fundo branco
            telaLogin.ShowDialog(this);

            // Esconde o fundo branco apenas depois de entrar no sistema principal
            this.Hide();
        }
    }
}