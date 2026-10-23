
#nullable enable

namespace flowwer_client;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(Form1));

        play_button = new Button();
        panel1 = new Panel();
        pictureBox1 = new PictureBox();
        name_client = new TextBox();
        fleur1 = new PictureBox();
        fleur2 = new PictureBox();
        fleur3 = new PictureBox();
        fleur4 = new PictureBox();
        fleur5 = new PictureBox();
        fleur6 = new PictureBox();
        up = new Button();
        compte_button = new Button();
        versionCombo = new ComboBox();
        modsButton = new Button();

        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)fleur1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)fleur2).BeginInit();
        ((System.ComponentModel.ISupportInitialize)fleur3).BeginInit();
        ((System.ComponentModel.ISupportInitialize)fleur4).BeginInit();
        ((System.ComponentModel.ISupportInitialize)fleur5).BeginInit();
        ((System.ComponentModel.ISupportInitialize)fleur6).BeginInit();

        SuspendLayout();

        Color buttonBack = Color.FromArgb(244, 182, 200);
        Color buttonHover = Color.FromArgb(250, 194, 210);
        Color buttonPressed = Color.FromArgb(235, 158, 183);
        Color buttonText = Color.FromArgb(226, 132, 162);

        // Play button
        play_button.AutoSize = true;
        play_button.BackColor = buttonBack;
        play_button.Cursor = Cursors.Hand;
        play_button.FlatStyle = FlatStyle.Flat;
        play_button.FlatAppearance.BorderSize = 0;
        play_button.FlatAppearance.MouseOverBackColor = buttonHover;
        play_button.FlatAppearance.MouseDownBackColor = buttonPressed;
        play_button.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        play_button.ForeColor = buttonText;
        play_button.Anchor = AnchorStyles.None;
        play_button.Location = new Point(331, 219);
        play_button.Name = "play_button";
        play_button.Padding = new Padding(10);
        play_button.Size = new Size(199, 69);
        play_button.TabIndex = 0;
        play_button.Text = "play";
        play_button.UseVisualStyleBackColor = false;
        play_button.Click += play_button_Click;

        // Bottom panel
        panel1.BackColor = buttonText;
        panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        panel1.Location = new Point(0, 469);
        panel1.Name = "panel1";
        panel1.Size = new Size(884, 42);
        panel1.TabIndex = 1;

        // Logo
        pictureBox1.BackColor = Color.Transparent;
        pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
        pictureBox1.Location = new Point(46, 12);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Padding = new Padding(25);
        pictureBox1.Size = new Size(148, 142);
        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox1.TabIndex = 0;
        pictureBox1.TabStop = false;
        pictureBox1.Click += pictureBox1_Click;

        // Client name
        name_client.BackColor = buttonBack;
        name_client.BorderStyle = BorderStyle.None;
        name_client.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        name_client.ForeColor = Color.White;
        name_client.Location = new Point(210, 58);
        name_client.Name = "name_client";
        name_client.ReadOnly = true;
        name_client.Size = new Size(320, 43);
        name_client.TabIndex = 2;
        name_client.TabStop = false;
        name_client.Text = "flowwer client";
        name_client.TextChanged += textBox1_TextChanged;

        // Sakura decorations
        fleur1.BackColor = Color.Transparent;
        fleur1.Image = Properties.Resources.sakura;
        fleur1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        fleur1.Location = new Point(715, 15);
        fleur1.Name = "fleur1";
        fleur1.Size = new Size(150, 150);
        fleur1.SizeMode = PictureBoxSizeMode.Zoom;
        fleur1.TabIndex = 10;
        fleur1.TabStop = false;

        fleur2.BackColor = Color.Transparent;
        fleur2.Image = Properties.Resources.sakura;
        fleur2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        fleur2.Location = new Point(650, 290);
        fleur2.Name = "fleur2";
        fleur2.Size = new Size(81, 81);
        fleur2.SizeMode = PictureBoxSizeMode.Zoom;
        fleur2.TabIndex = 11;
        fleur2.TabStop = false;

        fleur3.BackColor = Color.Transparent;
        fleur3.Image = Properties.Resources.sakura;
        fleur3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        fleur3.Location = new Point(59, 289);
        fleur3.Name = "fleur3";
        fleur3.Size = new Size(102, 102);
        fleur3.SizeMode = PictureBoxSizeMode.Zoom;
        fleur3.TabIndex = 12;
        fleur3.TabStop = false;

        fleur4.BackColor = Color.Transparent;
        fleur4.Image = Properties.Resources.sakura;
        fleur4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        fleur4.Location = new Point(237, 387);
        fleur4.Name = "fleur4";
        fleur4.Size = new Size(47, 47);
        fleur4.SizeMode = PictureBoxSizeMode.Zoom;
        fleur4.TabIndex = 13;
        fleur4.TabStop = false;

        fleur5.BackColor = Color.Transparent;
        fleur5.Image = Properties.Resources.sakura;
        fleur5.Anchor = AnchorStyles.Top;
        fleur5.Location = new Point(579, 29);
        fleur5.Name = "fleur5";
        fleur5.Size = new Size(43, 43);
        fleur5.SizeMode = PictureBoxSizeMode.Zoom;
        fleur5.TabIndex = 14;
        fleur5.TabStop = false;

        fleur6.BackColor = Color.Transparent;
        fleur6.Image = Properties.Resources.sakura;
        fleur6.Anchor = AnchorStyles.Right;
        fleur6.Location = new Point(813, 233);
        fleur6.Name = "fleur6";
        fleur6.Size = new Size(55, 55);
        fleur6.SizeMode = PictureBoxSizeMode.Zoom;
        fleur6.TabIndex = 15;
        fleur6.TabStop = false;

        // Up button
        up.BackColor = buttonBack;
        up.Cursor = Cursors.Hand;
        up.FlatStyle = FlatStyle.Flat;
        up.FlatAppearance.BorderSize = 0;
        up.FlatAppearance.MouseOverBackColor = buttonHover;
        up.FlatAppearance.MouseDownBackColor = buttonPressed;
        up.Anchor = AnchorStyles.Bottom;
        up.Location = new Point(419, 417);
        up.Margin = new Padding(0);
        up.Name = "up";
        up.Size = new Size(46, 46);
        up.TabIndex = 16;
        up.UseVisualStyleBackColor = false;
        up.Click += up_Click;

        // Sign in
        compte_button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        compte_button.BackColor = buttonBack;
        compte_button.Cursor = Cursors.Hand;
        compte_button.FlatStyle = FlatStyle.Flat;
        compte_button.FlatAppearance.BorderSize = 0;
        compte_button.FlatAppearance.MouseOverBackColor = buttonHover;
        compte_button.FlatAppearance.MouseDownBackColor = buttonPressed;
        compte_button.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        compte_button.ForeColor = buttonText;
        compte_button.Location = new Point(654, 176);
        compte_button.Name = "compte_button";
        compte_button.Size = new Size(210, 44);
        compte_button.TabIndex = 17;
        compte_button.Text = "Sign In";
        compte_button.UseVisualStyleBackColor = false;
        compte_button.Click += compte_button_Click;

        // Minecraft version dropdown
        versionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        versionCombo.DrawMode = DrawMode.OwnerDrawFixed;
        versionCombo.FlatStyle = FlatStyle.Flat;
        versionCombo.BackColor = buttonBack;
        versionCombo.ForeColor = buttonText;
        versionCombo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        versionCombo.ItemHeight = 26;
        versionCombo.IntegralHeight = false;
        versionCombo.Location = new Point(654, 80);
        versionCombo.Name = "versionCombo";
        versionCombo.Size = new Size(210, 44);
        versionCombo.TabIndex = 18;

        // Unique handler name prevents CS0111 if Form1.cs has the old handler.
        versionCombo.DrawItem += versionCombo_DrawItem_Designer;

        // Mods
        modsButton.BackColor = buttonBack;
        modsButton.Cursor = Cursors.Hand;
        modsButton.FlatStyle = FlatStyle.Flat;
        modsButton.FlatAppearance.BorderSize = 0;
        modsButton.FlatAppearance.MouseOverBackColor = buttonHover;
        modsButton.FlatAppearance.MouseDownBackColor = buttonPressed;
        modsButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        modsButton.ForeColor = buttonText;
        modsButton.Location = new Point(654, 128);
        modsButton.Name = "modsButton";
        modsButton.Size = new Size(210, 44);
        modsButton.TabIndex = 19;
        modsButton.Text = "Mods";
        modsButton.UseVisualStyleBackColor = false;

        // Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = buttonBack;
        ClientSize = new Size(884, 511);
        MinimumSize = new Size(700, 450);

        Controls.Add(up);
        Controls.Add(compte_button);
        Controls.Add(versionCombo);
        Controls.Add(modsButton);
        Controls.Add(name_client);
        Controls.Add(pictureBox1);
        Controls.Add(panel1);
        Controls.Add(play_button);
        Controls.Add(fleur1);
        Controls.Add(fleur2);
        Controls.Add(fleur3);
        Controls.Add(fleur4);
        Controls.Add(fleur5);
        Controls.Add(fleur6);

        Name = "Form1";
        Text = "Flowwer Client";

        ApplyRoundedCorners(play_button, 18);
        ApplyRoundedCorners(modsButton, 18);
        ApplyRoundedCorners(compte_button, 18);
        ApplyRoundedCorners(versionCombo, 18);
        ApplyRoundedCorners(up, 14);

        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ((System.ComponentModel.ISupportInitialize)fleur1).EndInit();
        ((System.ComponentModel.ISupportInitialize)fleur2).EndInit();
        ((System.ComponentModel.ISupportInitialize)fleur3).EndInit();
        ((System.ComponentModel.ISupportInitialize)fleur4).EndInit();
        ((System.ComponentModel.ISupportInitialize)fleur5).EndInit();
        ((System.ComponentModel.ISupportInitialize)fleur6).EndInit();

        ResumeLayout(false);
        PerformLayout();
    }

    private void versionCombo_DrawItem_Designer(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox combo || e.Index < 0)
            return;

        Color normal = Color.FromArgb(244, 182, 200);
        Color hover = Color.FromArgb(250, 194, 210);
        Color text = Color.FromArgb(226, 132, 162);

        e.Graphics.FillRectangle(
            new SolidBrush(
                (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? hover
                    : normal),
            e.Bounds);

        TextRenderer.DrawText(
            e.Graphics,
            combo.GetItemText(combo.Items[e.Index]),
            combo.Font,
            e.Bounds,
            text,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);

        e.DrawFocusRectangle();
    }

    private void ApplyRoundedCorners(Control control, int radius)
    {
        void UpdateRegion(object? sender, EventArgs e)
        {
            if (control.Width <= 0 || control.Height <= 0 || control.IsDisposed)
                return;

            int diameter = Math.Min(
                radius * 2,
                Math.Min(control.Width, control.Height));

            using var path = new System.Drawing.Drawing2D.GraphicsPath();

            path.AddArc(0, 0, diameter, diameter, 180, 90);
            path.AddArc(control.Width - diameter, 0, diameter, diameter, 270, 90);
            path.AddArc(
                control.Width - diameter,
                control.Height - diameter,
                diameter, diameter, 0, 90);
            path.AddArc(0, control.Height - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();

            Region? oldRegion = control.Region;
            control.Region = new Region(path);
            oldRegion?.Dispose();
        }

        control.Resize += UpdateRegion;
        UpdateRegion(control, EventArgs.Empty);
    }

    #endregion

    private Button play_button;
    private Panel panel1;
    private PictureBox pictureBox1;
    private TextBox name_client;
    private PictureBox fleur1;
    private PictureBox fleur2;
    private PictureBox fleur3;
    private PictureBox fleur4;
    private PictureBox fleur5;
    private PictureBox fleur6;
    public Button up;
    private Button compte_button;
    private ComboBox versionCombo;
    private Button modsButton;
}
