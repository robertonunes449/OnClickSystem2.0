using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace OnClickSystem.Desktop
{
    public static class Toast
    {
        // 1. Mensagem Verde (Tudo correu bem)
        public static void Sucesso(string mensagem)
        {
            Mostrar(mensagem, Color.FromArgb(74, 222, 128));
        }

        // 2. Mensagem Vermelha (Algo deu errado)
        public static void Erro(string mensagem)
        {
            Mostrar(mensagem, Color.FromArgb(248, 113, 113));
        }

        // 3. Mensagem Dourada (Avisos ou Alertas)
        public static void Aviso(string mensagem)
        {
            Mostrar(mensagem, Color.FromArgb(212, 175, 55));
        }

        // =======================================================
        // O MOTOR SECRETO QUE DESENHA A NOTIFICAÇÃO NO AR
        // =======================================================
        private static void Mostrar(string mensagem, Color corFaixa)
        {
            // Cria a janelinha do zero
            Form alerta = new Form();
            alerta.FormBorderStyle = FormBorderStyle.None;
            alerta.BackColor = Color.FromArgb(40, 40, 40); // Fundo cinza escuro
            alerta.Size = new Size(320, 60);
            alerta.TopMost = true; // Fica sempre por cima de tudo
            alerta.ShowInTaskbar = false; // Esconde da barra do Windows

            // Calcula a posição para aparecer no canto Inferior Direito
            var tela = Screen.PrimaryScreen.WorkingArea;
            alerta.StartPosition = FormStartPosition.Manual;
            alerta.Location = new Point(tela.Width - alerta.Width - 20, tela.Height - alerta.Height - 20);

            // Desenha a faixa colorida lateral
            Panel faixa = new Panel();
            faixa.Width = 6;
            faixa.Dock = DockStyle.Left;
            faixa.BackColor = corFaixa;
            alerta.Controls.Add(faixa);

            // Escreve o texto da mensagem
            Label lbl = new Label();
            lbl.Text = mensagem;
            lbl.ForeColor = Color.White;
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            lbl.Padding = new Padding(10, 0, 0, 0); // Dá um espaço da faixa
            alerta.Controls.Add(lbl);

            // Arredonda os cantos usando a ferramenta do Guna
            Guna2Elipse elipse = new Guna2Elipse();
            elipse.TargetControl = alerta;
            elipse.BorderRadius = 8;

            // ==================================================
            // CORREÇÃO: Nome completo do Timer para não dar erro
            // ==================================================
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000; // 3000 milissegundos = 3 segundos
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                alerta.Close(); // Fecha a mensagem sozinha!
            };

            // Mostra a mensagem e inicia o tempo
            alerta.Show();
            timer.Start();
        }
    }
}