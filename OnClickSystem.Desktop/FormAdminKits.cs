using Microsoft.EntityFrameworkCore;
using OnClickSystem.Infrastructure.Data;
using OnClickSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OnClickSystem.Desktop
{
    public partial class FormAdminKits : Form
    {
        private int idKitSelecionado = 0;
        private List<Kit> _kitsOriginais = new List<Kit>();

        public FormAdminKits()
        {
            InitializeComponent();
        }

        private void FormAdminKits_Load(object sender, EventArgs e)
        {
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;
            EstiloVisual.AplicarTemaPremium(dgvKits);
            CarregarKits();
        }

        private void CarregarKits()
        {
            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    _kitsOriginais = db.Kits.OrderBy(k => k.Nome).ToList();
                    AtualizarGrid();
                }
            }
            catch (Exception)
            {
                Toast.Erro("Erro ao carregar Kits do banco de dados.");
            }
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AtualizarGrid();
        }

        private void AtualizarGrid()
        {
            if (_kitsOriginais == null) return;

            string busca = txtBuscar?.Text.ToLower() ?? "";

            var filtrados = _kitsOriginais.Where(k =>
                string.IsNullOrWhiteSpace(busca) ||
                (k.Nome != null && k.Nome.ToLower().Contains(busca)) ||
                (k.Descricao != null && k.Descricao.ToLower().Contains(busca))
            ).ToList();

            var listaParaGrid = filtrados.Select(k => new
            {
                ID = k.ID,
                Nome = k.Nome,
                Valor = $"R$ {k.Preco:N2}",
                Status = k.Ativo ? "Ativo" : "Inativo"
            }).ToList();

            dgvKits.DataSource = listaParaGrid;

            if (dgvKits.Columns.Contains("ID")) dgvKits.Columns["ID"].Width = 60;
            if (dgvKits.Columns.Contains("Nome")) dgvKits.Columns["Nome"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (dgvKits.Columns.Contains("Valor")) dgvKits.Columns["Valor"].Width = 140;
            if (dgvKits.Columns.Contains("Status")) dgvKits.Columns["Status"].Width = 120;
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtValor.Text))
            {
                Toast.Aviso("O Nome e o Valor são obrigatórios!");
                return;
            }

            string textoValor = txtValor.Text.Replace("R$", "").Trim();
            if (!decimal.TryParse(textoValor, out decimal precoKit))
            {
                Toast.Aviso("Por favor, introduza um valor numérico válido (Ex: 150,00).");
                return;
            }

            try
            {
                using (var db = ConfiguracaoBanco.ObterContexto())
                {
                    bool estaAtivo = cmbStatus.Text == "Ativo";
                    string detalhesLog = "";

                    if (idKitSelecionado == 0)
                    {
                        var novoKit = new Kit
                        {
                            Nome = txtNome.Text.Trim(),
                            Descricao = txtDescricao.Text.Trim(),
                            Preco = precoKit,
                            Ativo = estaAtivo
                        };
                        db.Kits.Add(novoKit);

                        detalhesLog = $"Novo Kit criado no sistema: '{novoKit.Nome}'. Valor definido: R$ {novoKit.Preco:N2} | Status: {(estaAtivo ? "Ativo" : "Inativo")}.";
                        Toast.Sucesso("Kit criado com sucesso!");
                    }
                    else
                    {
                        var kitExistente = db.Kits.Find(idKitSelecionado);
                        if (kitExistente != null)
                        {
                            kitExistente.Nome = txtNome.Text.Trim();
                            kitExistente.Descricao = txtDescricao.Text.Trim();
                            kitExistente.Preco = precoKit;
                            kitExistente.Ativo = estaAtivo;

                            detalhesLog = $"Kit atualizado: '{kitExistente.Nome}'. Novo valor: R$ {kitExistente.Preco:N2} | Status atualizado para: {(estaAtivo ? "Ativo" : "Inativo")}.";
                            Toast.Sucesso("Kit atualizado com sucesso!");
                        }
                    }

                    // ========================================================
                    // AUDITORIA: CRIAÇÃO OU EDIÇÃO DO KIT
                    // ========================================================
                    string acaoRealizada = idKitSelecionado == 0 ? "Criação de kits" : "Edição de kits";
                    var logKit = new LogSistema
                    {
                        DataHora = DateTime.Now,
                        UsuarioResponsavel = SessaoAtual.UsuarioLogado.Email,
                        Acao = acaoRealizada,
                        Detalhes = detalhesLog
                    };
                    db.LogsSistema.Add(logKit);

                    db.SaveChanges();
                    btnLimpar_Click(null, null);
                    CarregarKits();
                }
            }
            catch (Exception ex)
            {
                Toast.Erro("Erro ao salvar no banco de dados:\n" + ex.Message);
            }
        }

        private void dgvKits_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                idKitSelecionado = Convert.ToInt32(dgvKits.Rows[e.RowIndex].Cells["ID"].Value);

                try
                {
                    using (var db = ConfiguracaoBanco.ObterContexto())
                    {
                        var kitReal = db.Kits.Find(idKitSelecionado);

                        if (kitReal != null)
                        {
                            txtNome.Text = kitReal.Nome;
                            txtDescricao.Text = kitReal.Descricao;
                            txtValor.Text = kitReal.Preco.ToString("N2");
                            cmbStatus.Text = kitReal.Ativo ? "Ativo" : "Inativo";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Toast.Erro("Erro ao buscar dados do Kit:\n" + ex.Message);
                }
            }
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            idKitSelecionado = 0;
            txtNome.Clear();
            txtValor.Clear();
            txtDescricao.Clear();
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;
            if (txtBuscar != null) txtBuscar.Clear();
        }

        private void dgvKits_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvKits.Columns["Status"].Index)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                string status = e.Value?.ToString() ?? "";
                Color bgColor = status == "Ativo" ? Color.FromArgb(20, 83, 45) : Color.FromArgb(127, 29, 29);
                Color txtColor = status == "Ativo" ? Color.FromArgb(74, 222, 128) : Color.FromArgb(248, 113, 113);

                Rectangle rect = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 10, e.CellBounds.Width - 20, e.CellBounds.Height - 20);
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 12;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var brush = new SolidBrush(bgColor))
                        e.Graphics.FillPath(brush, path);
                }

                TextRenderer.DrawText(e.Graphics, status, new Font("Segoe UI", 9F, FontStyle.Bold), rect, txtColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}