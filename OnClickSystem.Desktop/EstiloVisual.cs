using System;
using System.Drawing;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    public static class EstiloVisual
    {
        // Cores Oficiais do seu Layout
        public static Color CorFundoGrid = Color.FromArgb(24, 24, 24);
        public static Color CorLinhaGrid = Color.FromArgb(30, 30, 30);
        public static Color CorDourado = Color.FromArgb(212, 175, 55);
        public static Color CorTextoAcentuado = Color.FromArgb(212, 175, 55);
        public static Color CorSelecao = Color.FromArgb(45, 45, 45);
        public static Color CorDivisoria = Color.FromArgb(45, 45, 45);

        /// <summary>
        /// Aplica o tema Dark & Gold a qualquer DataGridView
        /// </summary>
        public static void AplicarTemaPremium(DataGridView grid)
        {
            grid.BackgroundColor = CorFundoGrid;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = CorDivisoria;
            grid.EnableHeadersVisualStyles = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;

            // Cabeçalho Dourado
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = CorDourado;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 45;

            // Linhas escuras
            grid.DefaultCellStyle.BackColor = CorLinhaGrid;
            grid.DefaultCellStyle.ForeColor = Color.Goldenrod;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grid.RowTemplate.Height = 45;

            // Seleção
            grid.DefaultCellStyle.SelectionBackColor = CorSelecao;
            grid.DefaultCellStyle.SelectionForeColor = CorTextoAcentuado;

            // Ativa o desenho automático dos Badges para a coluna de Status
            grid.CellPainting += Grid_CellPainting;
        }

        private static void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            // Só desenha badge se a coluna se chamar "Status" ou "Situação"
            if (e.RowIndex >= 0 && (grid.Columns[e.ColumnIndex].Name == "Status" || grid.Columns[e.ColumnIndex].Name == "Situação"))
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string valor = e.Value?.ToString() ?? "";

                // Cores dinâmicas (Ativo=Verde, Inativo/Erro=Vermelho, Pendente=Laranja)
                Color bgColor = Color.FromArgb(60, 60, 60); // Padrão
                Color txtColor = Color.White;

                if (valor.Contains("Ativo") || valor.Contains("Sucesso") || valor.Contains("Pago"))
                {
                    bgColor = Color.FromArgb(20, 83, 45);
                    txtColor = Color.FromArgb(74, 222, 128);
                }
                else if (valor.Contains("Inativo") || valor.Contains("Erro") || valor.Contains("Bloqueado"))
                {
                    bgColor = Color.FromArgb(127, 29, 29);
                    txtColor = Color.FromArgb(248, 113, 113);
                }

                Rectangle rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 10, e.CellBounds.Width - 16, e.CellBounds.Height - 20);

                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int r = 12;
                    path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                    path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                    path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(bgColor))
                        e.Graphics.FillPath(brush, path);
                }

                TextRenderer.DrawText(e.Graphics, valor, new Font("Segoe UI", 8.5F, FontStyle.Bold), rect, txtColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}