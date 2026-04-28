namespace OnClickSystem.Desktop
{
    partial class FormAdminUsuarios
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            txtBuscar = new Guna.UI2.WinForms.Guna2TextBox();
            dgvUsuarios = new Guna.UI2.WinForms.Guna2DataGridView();
            btnBloquear = new Guna.UI2.WinForms.Guna2Button();
            btnAtivar = new Guna.UI2.WinForms.Guna2Button();
            guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(components);
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
            SuspendLayout();
            // 
            // guna2HtmlLabel1
            // 
            guna2HtmlLabel1.BackColor = Color.Transparent;
            guna2HtmlLabel1.Font = new Font("Segoe UI", 26.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            guna2HtmlLabel1.ForeColor = Color.Gold;
            guna2HtmlLabel1.Location = new Point(51, 28);
            guna2HtmlLabel1.Margin = new Padding(3, 4, 3, 4);
            guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            guna2HtmlLabel1.Size = new Size(437, 62);
            guna2HtmlLabel1.TabIndex = 0;
            guna2HtmlLabel1.Text = "Controle de Usuários";
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscar.CustomizableEdges = customizableEdges1;
            txtBuscar.DefaultText = "";
            txtBuscar.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtBuscar.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtBuscar.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtBuscar.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtBuscar.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtBuscar.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtBuscar.ForeColor = Color.Black;
            txtBuscar.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtBuscar.Location = new Point(51, 124);
            txtBuscar.Margin = new Padding(3, 5, 3, 5);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderForeColor = Color.FromArgb(45, 45, 45);
            txtBuscar.PlaceholderText = "Buscar por Nome, E-mail ou CPF...";
            txtBuscar.SelectedText = "";
            txtBuscar.ShadowDecoration.CustomizableEdges = customizableEdges2;
            txtBuscar.Size = new Size(849, 48);
            txtBuscar.TabIndex = 1;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // dgvUsuarios
            // 
            dataGridViewCellStyle1.BackColor = Color.White;
            dgvUsuarios.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUsuarios.BackgroundColor = Color.DimGray;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvUsuarios.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvUsuarios.ColumnHeadersHeight = 4;
            dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvUsuarios.DefaultCellStyle = dataGridViewCellStyle3;
            dgvUsuarios.GridColor = Color.FromArgb(231, 229, 255);
            dgvUsuarios.Location = new Point(51, 213);
            dgvUsuarios.Margin = new Padding(3, 4, 3, 4);
            dgvUsuarios.Name = "dgvUsuarios";
            dgvUsuarios.RowHeadersVisible = false;
            dgvUsuarios.RowHeadersWidth = 51;
            dgvUsuarios.RowTemplate.Height = 40;
            dgvUsuarios.Size = new Size(849, 272);
            dgvUsuarios.TabIndex = 2;
            dgvUsuarios.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvUsuarios.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgvUsuarios.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dgvUsuarios.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dgvUsuarios.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dgvUsuarios.ThemeStyle.BackColor = Color.DimGray;
            dgvUsuarios.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            dgvUsuarios.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            dgvUsuarios.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvUsuarios.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvUsuarios.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvUsuarios.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvUsuarios.ThemeStyle.HeaderStyle.Height = 4;
            dgvUsuarios.ThemeStyle.ReadOnly = false;
            dgvUsuarios.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvUsuarios.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvUsuarios.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvUsuarios.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvUsuarios.ThemeStyle.RowsStyle.Height = 40;
            dgvUsuarios.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvUsuarios.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dgvUsuarios.CellFormatting += dgvUsuarios_CellFormatting;
            // 
            // btnBloquear
            // 
            btnBloquear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBloquear.CustomizableEdges = customizableEdges3;
            btnBloquear.DisabledState.BorderColor = Color.DarkGray;
            btnBloquear.DisabledState.CustomBorderColor = Color.DarkGray;
            btnBloquear.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnBloquear.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnBloquear.FillColor = Color.FromArgb(220, 38, 38);
            btnBloquear.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBloquear.ForeColor = Color.White;
            btnBloquear.Location = new Point(694, 505);
            btnBloquear.Margin = new Padding(3, 4, 3, 4);
            btnBloquear.Name = "btnBloquear";
            btnBloquear.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnBloquear.Size = new Size(206, 60);
            btnBloquear.TabIndex = 3;
            btnBloquear.Text = "Desativar";
            btnBloquear.Click += btnBloquear_Click;
            // 
            // btnAtivar
            // 
            btnAtivar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAtivar.CustomizableEdges = customizableEdges5;
            btnAtivar.DisabledState.BorderColor = Color.DarkGray;
            btnAtivar.DisabledState.CustomBorderColor = Color.DarkGray;
            btnAtivar.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnAtivar.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnAtivar.FillColor = Color.FromArgb(22, 163, 74);
            btnAtivar.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAtivar.ForeColor = Color.White;
            btnAtivar.Location = new Point(462, 505);
            btnAtivar.Margin = new Padding(3, 4, 3, 4);
            btnAtivar.Name = "btnAtivar";
            btnAtivar.ShadowDecoration.CustomizableEdges = customizableEdges6;
            btnAtivar.Size = new Size(206, 60);
            btnAtivar.TabIndex = 3;
            btnAtivar.Text = "Ativar";
            btnAtivar.Click += btnAtivar_Click;
            // 
            // guna2BorderlessForm1
            // 
            guna2BorderlessForm1.ContainerControl = this;
            guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            guna2BorderlessForm1.TransparentWhileDrag = true;
            // 
            // FormAdminUsuarios
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(950, 600);
            Controls.Add(btnAtivar);
            Controls.Add(btnBloquear);
            Controls.Add(dgvUsuarios);
            Controls.Add(txtBuscar);
            Controls.Add(guna2HtmlLabel1);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormAdminUsuarios";
            Text = "FormAdminUsuarios";
            Load += FormAdminUsuarios_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel1;
        private Guna.UI2.WinForms.Guna2TextBox txtBuscar;
        private Guna.UI2.WinForms.Guna2DataGridView dgvUsuarios;
        private Guna.UI2.WinForms.Guna2Button btnBloquear;
        private Guna.UI2.WinForms.Guna2Button btnAtivar;
        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
    }
}