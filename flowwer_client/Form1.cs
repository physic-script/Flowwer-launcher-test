
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;

namespace flowwer_client;

public partial class Form1 : Form, IMessageFilter
{
    private static readonly Color SakuraBack = Color.FromArgb(244, 182, 200);
    private static readonly Color SakuraDark = Color.FromArgb(226, 132, 162);
    private static readonly Color SakuraLight = Color.FromArgb(255, 236, 242);

    private bool playHover, playPressed;
    private bool modsHover, modsPressed;
    private bool gamePathHover, gamePathPressed;
    private Button? modPathBtn;

    // Mods panel animation
    private readonly System.Windows.Forms.Timer modsTimer = new() { Interval = 15 };
    private Panel? modsPanel;
    private Label? modsTitle;
    private Label? modsInfo;
    private Button? openModsFolderButton;
    private float modsPanelHeight;
    private float modsPanelTarget;
    private const int ModsPanelMaxHeight = 132;
    private const int ModsPanelClosedHeight = 0;
    private bool ModsPanelIsOpen => modsPanelTarget > 0;

    private readonly MinecraftService minecraft = new();
    private bool launching;
    private string? playStatus;
    private bool compteHover;
    private bool compteBusy;

    public Form1()
    {
        InitializeComponent();

        DoubleBuffered = true;
        ResizeRedraw = true;
        StartPosition = FormStartPosition.CenterScreen;

        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // The executable may not contain an icon.
        }

        SetupPlayButton();
        SetupHub();
        SetupAccount();
        SetupModsAndVersion();

        modsTimer.Tick += ModsTimer_Tick;
        FormClosed += (_, _) =>
        {
            modsTimer.Stop();
            modsTimer.Dispose();
            hubTimer.Stop();
            blurTimer.Stop();
            hubBlur?.Dispose();
        };
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle logo = pictureBox1.Bounds;
        float cx = logo.X + logo.Width / 2f;
        float cy = logo.Y + logo.Height / 2f;
        float halo = 62;

        using var haloBrush = new SolidBrush(Color.FromArgb(170, Color.White));
        g.FillEllipse(haloBrush, cx - halo, cy - halo, halo * 2, halo * 2);
    }

    private static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();

        float r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f));
        float diameter = r * 2;

        if (diameter <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    // PLAY BUTTON
    private void SetupPlayButton()
    {
        play_button.FlatStyle = FlatStyle.Flat;
        play_button.FlatAppearance.BorderSize = 0;
        play_button.FlatAppearance.MouseOverBackColor = SakuraBack;
        play_button.FlatAppearance.MouseDownBackColor = SakuraBack;
        play_button.BackColor = SakuraBack;
        play_button.ForeColor = SakuraDark;
        play_button.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        play_button.Cursor = Cursors.Hand;
        play_button.UseVisualStyleBackColor = false;

        play_button.MouseEnter += (_, _) =>
        {
            playHover = true;
            play_button.Invalidate();
        };

        play_button.MouseLeave += (_, _) =>
        {
            playHover = false;
            playPressed = false;
            play_button.Invalidate();
        };

        play_button.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                playPressed = true;

            play_button.Invalidate();
        };

        play_button.MouseUp += (_, _) =>
        {
            playPressed = false;
            play_button.Invalidate();
        };

        play_button.Paint += play_button_Paint;
    }

    private void play_button_Paint(object? sender, PaintEventArgs e)
    {
        PaintPillButton(
            e.Graphics,
            play_button.ClientRectangle,
            play_button.Font,
            playStatus ?? play_button.Text.ToUpperInvariant(),
            playHover,
            playPressed);
    }

    // Shared Play-style appearance for the Mods and Game Path buttons.
    private void SetupPillButton(
        Button button,
        Action<bool> setHover,
        Action<bool> setPressed,
        Func<bool> getHover,
        Func<bool> getPressed)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = SakuraBack;
        button.FlatAppearance.MouseDownBackColor = SakuraBack;
        button.BackColor = SakuraBack;
        button.ForeColor = SakuraDark;
        button.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
        button.TextAlign = ContentAlignment.MiddleCenter;

        button.MouseEnter += (_, _) =>
        {
            setHover(true);
            button.Invalidate();
        };

        button.MouseLeave += (_, _) =>
        {
            setHover(false);
            setPressed(false);
            button.Invalidate();
        };

        button.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                setPressed(true);

            button.Invalidate();
        };

        button.MouseUp += (_, _) =>
        {
            setPressed(false);
            button.Invalidate();
        };

        button.Paint += (_, e) =>
        {
            PaintPillButton(
                e.Graphics,
                button.ClientRectangle,
                button.Font,
                button.Text,
                getHover(),
                getPressed());
        };

        button.Resize += (_, _) => UpdatePillRegion(button);
        UpdatePillRegion(button);
    }

    private void PaintPillButton(
        Graphics g,
        Rectangle bounds,
        Font font,
        string text,
        bool hovered,
        bool pressed)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(SakuraBack);

        var rect = new RectangleF(
            bounds.X + 3,
            bounds.Y + 2,
            Math.Max(1, bounds.Width - 7),
            Math.Max(1, bounds.Height - 7));

        if (pressed)
            rect.Offset(0, 2);

        float radius = rect.Height / 2f;

        if (!pressed)
        {
            using var shadowPath = RoundedRect(
                new RectangleF(rect.X, rect.Y + 3, rect.Width, rect.Height),
                radius);

            using var shadowBrush = new SolidBrush(Color.FromArgb(70, SakuraDark));
            g.FillPath(shadowBrush, shadowPath);
        }

        using var buttonPath = RoundedRect(rect, radius);

        using (var fillBrush = new SolidBrush(hovered ? SakuraLight : Color.White))
            g.FillPath(fillBrush, buttonPath);

        using (var borderPen = new Pen(SakuraDark, 2F))
            g.DrawPath(borderPen, buttonPath);

        TextRenderer.DrawText(
            g,
            text,
            font,
            Rectangle.Round(rect),
            SakuraDark,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPadding);
    }

    private void UpdatePillRegion(Control control)
    {
        if (control.IsDisposed || control.Width <= 0 || control.Height <= 0)
            return;

        using var path = RoundedRect(
            new RectangleF(0, 0, control.Width, control.Height),
            control.Height / 2f);

        Region? oldRegion = control.Region;
        control.Region = new Region(path);
        oldRegion?.Dispose();
    }

    // ACCOUNT BUTTON
    private void SetupAccount()
    {
        compte_button.FlatStyle = FlatStyle.Flat;
        compte_button.FlatAppearance.BorderSize = 0;
        compte_button.FlatAppearance.MouseOverBackColor = SakuraBack;
        compte_button.FlatAppearance.MouseDownBackColor = SakuraBack;
        compte_button.BackColor = SakuraBack;
        compte_button.ForeColor = SakuraDark;
        compte_button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        compte_button.Cursor = Cursors.Hand;

        compte_button.MouseEnter += (_, _) =>
        {
            compteHover = true;
            compte_button.Invalidate();
        };

        compte_button.MouseLeave += (_, _) =>
        {
            compteHover = false;
            compte_button.Invalidate();
        };

        compte_button.Paint += compte_button_Paint;
        compte_button.Resize += (_, _) => UpdateCompteRegion();
        UpdateCompteRegion();

        Shown += async (_, _) =>
        {
            compteBusy = true;
            UpdateCompteButton();

            try
            {
                await minecraft.TryAutoLoginAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Automatic login failed: " + ex.Message);
            }
            finally
            {
                compteBusy = false;
                UpdateCompteButton();
            }
        };
    }

    private void UpdateCompteRegion() => UpdatePillRegion(compte_button);

    private void UpdateCompteButton()
    {
        compte_button.Text = compteBusy
            ? "Connecting..."
            : minecraft.Session?.Username ?? "Sign In";

        compte_button.Invalidate();
    }

    // VERSIONS, MODS, GAME PATH
    private void SetupModsAndVersion()
    {
        versionCombo.Items.Clear();

        for (int minor = 1; minor <= 11; minor++)
            versionCombo.Items.Add($"1.21.{minor}");

        versionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        versionCombo.DrawMode = DrawMode.OwnerDrawFixed;
        versionCombo.FlatStyle = FlatStyle.Flat;
        versionCombo.BackColor = SakuraBack;
        versionCombo.ForeColor = SakuraDark;
        versionCombo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        versionCombo.ItemHeight = 26;
        versionCombo.IntegralHeight = false;
        versionCombo.DropDownHeight = 180;
        versionCombo.Width = 210;
        versionCombo.Location = new Point(654, 80);

        if (versionCombo.Items.Count > 0)
            versionCombo.SelectedIndex = 0;

        // The Designer attaches versionCombo_DrawItem_Designer.
        // Avoid subscribing a second DrawItem handler here.

        versionCombo.SelectedIndexChanged += (_, _) =>
        {
            if (versionCombo.SelectedItem is string version)
                minecraft.MinecraftVersion = version;
        };

        modsButton.Text = "MODS";
        modsButton.Width = 210;
        modsButton.Height = 44;
        modsButton.Location = new Point(654, 128);
        modsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        SetupPillButton(
            modsButton,
            value => modsHover = value,
            value => modsPressed = value,
            () => modsHover,
            () => modsPressed);

        modsButton.Click += (_, _) => ToggleModsPanel();

        modPathBtn = new Button
        {
            Name = "gamePathButton",
            Text = "CHANGE GAME PATH",
            Width = 210,
            Height = 44,
            Location = new Point(654, 224),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        SetupPillButton(
            modPathBtn,
            value => gamePathHover = value,
            value => gamePathPressed = value,
            () => gamePathHover,
            () => gamePathPressed);

        modPathBtn.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Flowwer game directory",
                SelectedPath = MinecraftService.GameDir,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                MinecraftService.GameDir = dialog.SelectedPath;

                MessageBox.Show(
                    this,
                    "Game directory updated:\n" + dialog.SelectedPath,
                    "Game Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        };

        Controls.Add(modPathBtn);

        SetupModsPanel();

        versionCombo.BringToFront();
        modsButton.BringToFront();
        compte_button.BringToFront();
        modPathBtn.BringToFront();
    }

    // Create a separate panel that expands underneath Game Path.
    private void SetupModsPanel()
    {
        modsPanel = new Panel
        {
            Name = "modsPanel",
            Location = new Point(654, 276),
            Size = new Size(210, ModsPanelClosedHeight),
            BackColor = SakuraBack,
            Visible = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        modsPanel.Paint += ModsPanel_Paint;

        modsTitle = new Label
        {
            Text = "YOUR MODS",
            Location = new Point(12, 9),
            Size = new Size(186, 24),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = SakuraDark,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter
        };

        modsInfo = new Label
        {
            Text = "Checking installed mods...",
            Location = new Point(10, 37),
            Size = new Size(190, 24),
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = SakuraDark,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter
        };

        openModsFolderButton = new Button
        {
            Text = "OPEN MODS FOLDER",
            Location = new Point(13, 75),
            Size = new Size(184, 36),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = SakuraBack,
            ForeColor = SakuraDark,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        openModsFolderButton.FlatAppearance.BorderSize = 0;
        openModsFolderButton.FlatAppearance.MouseOverBackColor = SakuraBack;
        openModsFolderButton.FlatAppearance.MouseDownBackColor = SakuraBack;

        openModsFolderButton.Click += (_, _) => OpenModsFolder();

        modsPanel.Controls.Add(modsTitle);
        modsPanel.Controls.Add(modsInfo);
        modsPanel.Controls.Add(openModsFolderButton);

        Controls.Add(modsPanel);
        modsPanel.BringToFront();

        modsPanel.Resize += (_, _) => modsPanel.Invalidate();

        UpdateModsInfo();
    }

    private void ToggleModsPanel()
    {
        if (modsPanel == null)
            return;

        if (ModsPanelIsOpen)
        {
            modsPanelTarget = ModsPanelClosedHeight;
        }
        else
        {
            // Close the Hub if it is open, so the panels don't compete.
            if (HubIsOpen)
            {
                hubTarget = HubClosedHeight;
                scrollTarget = 0;
                hubTimer.Start();
            }

            UpdateModsInfo();
            modsPanel.Visible = true;
            modsPanel.BringToFront();
            modsPanelTarget = ModsPanelMaxHeight;

            // Keep the primary launcher controls above the expanding panel.
            modsButton.BringToFront();
            compte_button.BringToFront();
            modPathBtn?.BringToFront();
            modsPanel.BringToFront();
        }

        modsTimer.Start();
    }

    private void ModsTimer_Tick(object? sender, EventArgs e)
    {
        // Ease towards the target height for a smooth slide animation.
        modsPanelHeight += (modsPanelTarget - modsPanelHeight) * 0.22f;

        if (Math.Abs(modsPanelTarget - modsPanelHeight) < 0.6f)
        {
            modsPanelHeight = modsPanelTarget;
            modsTimer.Stop();
        }

        if (modsPanel == null)
            return;

        int height = Math.Max(0, (int)Math.Round(modsPanelHeight));

        modsPanel.SetBounds(
            ClientSize.Width - 210 - 20,
            276,
            210,
            height);

        modsPanel.Invalidate();

        if (height == 0 && modsPanelTarget == 0)
            modsPanel.Visible = false;
    }

    private void ModsPanel_Paint(object? sender, PaintEventArgs e)
    {
        if (modsPanel == null || modsPanel.Height <= 0)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(SakuraBack);

        var rect = new RectangleF(
            2,
            2,
            Math.Max(1, modsPanel.Width - 5),
            Math.Max(1, modsPanel.Height - 5));

        using var path = RoundedRect(rect, 15);
        using var fill = new SolidBrush(Color.White);
        using var border = new Pen(SakuraDark, 2F);

        g.FillPath(fill, path);
        g.DrawPath(border, path);

        using var divider = new Pen(Color.FromArgb(120, SakuraDark), 1F);
        g.DrawLine(divider, 15, 34, modsPanel.Width - 15, 34);
    }

    private void UpdateModsInfo()
    {
        if (modsInfo == null)
            return;

        try
        {
            string modsDirectory = Path.Combine(MinecraftService.GameDir, "mods");

            if (!Directory.Exists(modsDirectory))
            {
                modsInfo.Text = "No mods folder yet";
                return;
            }

            int count = Directory.GetFiles(modsDirectory, "*.jar").Length;
            modsInfo.Text = count == 1 ? "1 mod installed" : $"{count} mods installed";
        }
        catch
        {
            modsInfo.Text = "Unable to read mods";
        }
    }

    private void OpenModsFolder()
    {
        try
        {
            string modsDirectory = Path.Combine(MinecraftService.GameDir, "mods");
            Directory.CreateDirectory(modsDirectory);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{modsDirectory}\"",
                UseShellExecute = true
            });

            UpdateModsInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Could not open the mods folder:\n" + ex.Message,
                "Mods",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    // ACCOUNT BUTTON
    private async void compte_button_Click(object? sender, EventArgs e)
    {
        if (compteBusy)
            return;

        if (!minecraft.IsLoggedIn)
        {
            DialogResult result = MessageBox.Show(
                this,
                "Does Flowwer Client have access to your Microsoft account?",
                "Microsoft Account Access",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await LoginAsync();
            return;
        }

        using var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            Font = new Font("Segoe UI", 10F)
        };

        menu.Items.Add("Open Game Folder", null, (_, _) =>
        {
            Directory.CreateDirectory(MinecraftService.GameDir);
            Process.Start("explorer.exe", MinecraftService.GameDir);
        });

        menu.Items.Add("Sign Out", null, async (_, _) =>
        {
            compteBusy = true;
            UpdateCompteButton();

            try
            {
                await minecraft.LogoutAsync();
            }
            finally
            {
                compteBusy = false;
                UpdateCompteButton();
            }
        });

        menu.Show(compte_button, new Point(0, compte_button.Height + 4));
    }

    private async Task<bool> LoginAsync()
    {
        compteBusy = true;
        UpdateCompteButton();

        try
        {
            await minecraft.LoginAsync();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Login failed:\n" + ex.Message,
                "Login",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }
        finally
        {
            compteBusy = false;
            UpdateCompteButton();
        }
    }

    private void compte_button_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(SakuraBack);

        var rect = new RectangleF(
            2, 2,
            compte_button.Width - 5,
            compte_button.Height - 5);

        using (var path = RoundedRect(rect, rect.Height / 2))
        {
            using var fill = new SolidBrush(compteHover ? SakuraLight : Color.White);
            using var border = new Pen(SakuraDark, 2);

            g.FillPath(fill, path);
            g.DrawPath(border, path);
        }

        float diameter = rect.Height - 10;
        var dot = new RectangleF(rect.X + 5, rect.Y + 5, diameter, diameter);

        using (var dotBrush = new SolidBrush(
            minecraft.IsLoggedIn ? SakuraDark : SakuraBack))
        {
            g.FillEllipse(dotBrush, dot);
        }

        string letter = "?";

        if (minecraft.IsLoggedIn &&
            !string.IsNullOrWhiteSpace(minecraft.Session?.Username))
        {
            letter = minecraft.Session.Username[..1].ToUpperInvariant();
        }

        TextRenderer.DrawText(
            g,
            letter,
            compte_button.Font,
            Rectangle.Round(dot),
            Color.White,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter);

        var textRect = new RectangleF(
            dot.Right + 6,
            rect.Y,
            rect.Right - dot.Right - 16,
            rect.Height);

        TextRenderer.DrawText(
            g,
            compte_button.Text,
            compte_button.Font,
            Rectangle.Round(textRect),
            SakuraDark,
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);
    }

    // PLAY / LAUNCH MINECRAFT
    private async void play_button_Click(object? sender, EventArgs e)
    {
        if (launching || compteBusy)
            return;

        if (!minecraft.IsLoggedIn && !await LoginAsync())
            return;

        launching = true;

        var progress = new Progress<string>(text =>
        {
            playStatus = text;
            play_button.Invalidate();
        });

        try
        {
            Process game = await minecraft.LaunchAsync(progress);

            playStatus = "In Game";
            play_button.Invalidate();

            game.EnableRaisingEvents = true;
            game.Exited += (_, _) =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                BeginInvoke(() =>
                {
                    launching = false;
                    playStatus = null;
                    play_button.Invalidate();
                });
            };
        }
        catch (Exception ex)
        {
            launching = false;
            playStatus = null;
            play_button.Invalidate();

            MessageBox.Show(
                this,
                "Failed to launch Minecraft:\n" + ex.Message,
                "Launch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // ===================== HUB =====================

    private const int HubClosedHeight = 42;
    private const float HubOpenRatio = 0.6f;
    private const int HubButtonGap = 6;
    private const int HubHeader = 42;
    private const int HubWheelStep = 60;

    private int HubOpenHeight =>
        Math.Max(200, (int)(ClientSize.Height * HubOpenRatio));

    private static readonly Color HubTint = Color.FromArgb(165, 255, 240, 245);
    private static readonly Color HubButtonFill = Color.FromArgb(255, 242, 247);

    private readonly System.Windows.Forms.Timer hubTimer = new() { Interval = 15 };
    private readonly System.Windows.Forms.Timer blurTimer = new() { Interval = 150 };

    private float hubHeight = HubClosedHeight;
    private float hubTarget = HubClosedHeight;
    private float scrollPos;
    private float scrollTarget;
    private bool hubHover;
    private Bitmap? hubBlur;

    private readonly Panel hubView = new() { BackColor = Color.Transparent };
    private readonly Panel hubItems = new() { BackColor = Color.Transparent };
    private readonly HubScrollBar hubBar = new();

    private bool HubIsOpen => hubTarget > HubClosedHeight + 1;

    private int HubContentHeight
    {
        get
        {
            int bottom = 0;

            foreach (Control control in hubItems.Controls)
                bottom = Math.Max(bottom, control.Bottom);

            return Math.Max(hubView.Height, bottom + 16);
        }
    }

    private int HubMaxScroll => Math.Max(0, HubContentHeight - hubView.Height);

    private static void EnableDoubleBuffer(Control control) =>
        typeof(Control).GetProperty(
            "DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)!
        .SetValue(control, true);

    private void SetupHub()
    {
        EnableDoubleBuffer(panel1);
        EnableDoubleBuffer(hubView);
        EnableDoubleBuffer(hubItems);

        foreach (Control control in panel1.Controls.Cast<Control>().ToList())
        {
            control.Top -= HubHeader;
            hubItems.Controls.Add(control);
        }

        hubView.Controls.Add(hubItems);
        panel1.Controls.Add(hubView);
        panel1.Controls.Add(hubBar);
        panel1.Paint += panel1_Paint;

        panel1.Controls.Add(new Label
        {
            Text = "Hub",
            ForeColor = SakuraDark,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 10)
        });

        hubBar.Scrolled += value =>
        {
            scrollPos = scrollTarget = value * HubMaxScroll;
            LayoutHub();
        };

        up.Text = "";
        up.Image = null;
        up.FlatAppearance.BorderSize = 0;
        up.FlatAppearance.MouseOverBackColor = SakuraBack;
        up.FlatAppearance.MouseDownBackColor = SakuraBack;

        using (var circle = new GraphicsPath())
        {
            circle.AddEllipse(0, 0, up.Width, up.Height);
            up.Region = new Region(circle);
        }

        up.MouseEnter += (_, _) =>
        {
            hubHover = true;
            up.Invalidate();
        };

        up.MouseLeave += (_, _) =>
        {
            hubHover = false;
            up.Invalidate();
        };

        up.Paint += up_Paint;

        Application.AddMessageFilter(this);
        FormClosed += (_, _) => Application.RemoveMessageFilter(this);

        hubTimer.Tick += hubTimer_Tick;

        Shown += (_, _) =>
        {
            CaptureBehindHub();
            LayoutHub();
        };

        Resize += (_, _) =>
        {
            if (HubIsOpen)
                hubTarget = hubHeight = HubOpenHeight;

            LayoutHub();
            blurTimer.Stop();
            blurTimer.Start();
        };

        blurTimer.Tick += (_, _) =>
        {
            blurTimer.Stop();
            CaptureBehindHub();
            panel1.Invalidate(true);
        };

        LayoutHub();
    }

    public bool PreFilterMessage(ref Message m)
    {
        const int WM_MOUSEWHEEL = 0x020A;

        if (m.Msg != WM_MOUSEWHEEL || !HubIsOpen)
            return false;

        Control? target = Control.FromHandle(m.HWnd);

        if (target == null || (target != this && target.FindForm() != this))
            return false;

        int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);

        scrollTarget = Math.Clamp(
            scrollTarget - delta / 120f * HubWheelStep,
            0,
            HubMaxScroll);

        hubTimer.Start();
        return true;
    }

    private void up_Click(object? sender, EventArgs e)
    {
        if (HubIsOpen)
        {
            hubTarget = HubClosedHeight;
            scrollTarget = 0;
        }
        else
        {
            CaptureBehindHub();
            hubTarget = HubOpenHeight;
        }

        panel1.BringToFront();
        up.BringToFront();
        hubTimer.Start();
    }

    private void hubTimer_Tick(object? sender, EventArgs e)
    {
        hubHeight += (hubTarget - hubHeight) * 0.2f;
        scrollPos += (scrollTarget - scrollPos) * 0.25f;

        bool heightDone = Math.Abs(hubTarget - hubHeight) < 0.5f;
        bool scrollDone = Math.Abs(scrollTarget - scrollPos) < 0.5f;

        if (heightDone)
            hubHeight = hubTarget;

        if (scrollDone)
            scrollPos = scrollTarget;

        if (heightDone && scrollDone)
            hubTimer.Stop();

        LayoutHub();
    }

    private void LayoutHub()
    {
        int height = (int)Math.Round(hubHeight);

        panel1.SetBounds(0, ClientSize.Height - height, ClientSize.Width, height);

        up.Location = new Point(
            (ClientSize.Width - up.Width) / 2,
            panel1.Top - up.Height - HubButtonGap);

        int barX = panel1.Width - HubScrollBar.BarWidth - 12;

        hubView.SetBounds(
            0,
            HubHeader,
            Math.Max(0, barX - 8),
            Math.Max(0, height - HubHeader));

        scrollPos = Math.Clamp(scrollPos, 0, HubMaxScroll);
        scrollTarget = Math.Clamp(scrollTarget, 0, HubMaxScroll);

        hubItems.SetBounds(
            0,
            -(int)Math.Round(scrollPos),
            hubView.Width,
            HubContentHeight);

        hubBar.SetBounds(
            barX,
            HubHeader,
            HubScrollBar.BarWidth,
            Math.Max(0, height - HubHeader - 14));

        hubBar.Visible = height > HubClosedHeight + 60;

        hubBar.ViewRatio = HubContentHeight == 0
            ? 1f
            : hubView.Height / (float)HubContentHeight;

        hubBar.Value = HubMaxScroll == 0 ? 0f : scrollPos / HubMaxScroll;

        panel1.Invalidate(true);
        up.Invalidate();
    }

    private void CaptureBehindHub()
    {
        Size size = ClientSize;

        if (size.Width <= 0 || size.Height <= 0)
            return;

        var shot = new Bitmap(size.Width, size.Height);

        using (Graphics g = Graphics.FromImage(shot))
            OnPaintBackground(new PaintEventArgs(g, ClientRectangle));

        for (int i = Controls.Count - 1; i >= 0; i--)
        {
            Control control = Controls[i];

            if (control == panel1 || control == up || !control.Visible)
                continue;

            control.DrawToBitmap(shot, control.Bounds);
        }

        hubBlur?.Dispose();
        hubBlur = Blur(Blur(shot, 10), 4);
        shot.Dispose();
    }

    private static Bitmap Blur(Bitmap source, int factor)
    {
        int width = Math.Max(1, source.Width / factor);
        int height = Math.Max(1, source.Height / factor);

        using var attrs = new System.Drawing.Imaging.ImageAttributes();
        attrs.SetWrapMode(WrapMode.TileFlipXY);

        using var small = new Bitmap(width, height);

        using (Graphics g = Graphics.FromImage(small))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImage(
                source,
                new Rectangle(0, 0, width, height),
                0, 0, source.Width, source.Height,
                GraphicsUnit.Pixel,
                attrs);
        }

        var result = new Bitmap(source.Width, source.Height);

        using (Graphics g = Graphics.FromImage(result))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(
                small,
                new Rectangle(0, 0, source.Width, source.Height),
                0, 0, width, height,
                GraphicsUnit.Pixel,
                attrs);
        }

        return result;
    }

    private void panel1_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(SakuraBack);

        if (hubBlur != null)
            g.DrawImage(hubBlur, -panel1.Left, -panel1.Top);

        using (var tint = new SolidBrush(HubTint))
            g.FillRectangle(tint, panel1.ClientRectangle);

        using var line = new Pen(Color.FromArgb(200, Color.White), 2);
        g.DrawLine(line, 0, 1, panel1.Width, 1);
    }

    private void up_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(SakuraBack);

        var circle = new RectangleF(1.5f, 1.5f, up.Width - 4, up.Height - 4);

        using (var fill = new SolidBrush(hubHover ? Color.White : HubButtonFill))
            g.FillEllipse(fill, circle);

        using (var border = new Pen(SakuraDark, 2))
            g.DrawEllipse(border, circle);

        g.TranslateTransform(up.Width / 2f - 0.5f, up.Height / 2f - 0.5f);

        float turn = Math.Clamp(
            (hubHeight - HubClosedHeight) / (HubOpenHeight - HubClosedHeight),
            0f,
            1f);

        g.RotateTransform(180f * turn);

        using var arrow = new Pen(SakuraDark, 3)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        g.DrawLines(arrow, new[]
        {
            new PointF(-8, 4),
            new PointF(0, -4),
            new PointF(8, 4)
        });
    }

    private void pictureBox1_Click(object? sender, EventArgs e) { }

    private void textBox1_TextChanged(object? sender, EventArgs e) { }
}

// Rounded custom scrollbar for the Hub
public class HubScrollBar : Control
{
    public const int BarWidth = 16;

    private static readonly Color Pink = Color.FromArgb(226, 132, 162);
    private static readonly Color PinkLight = Color.FromArgb(255, 236, 242);

    private float value;
    private float viewRatio = 1f;
    private bool dragging;
    private int dragOffset;
    private bool hover;

    public event Action<float>? Scrolled;

    public HubScrollBar()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);

        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Width = BarWidth;
    }

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public float Value
    {
        get => value;
        set
        {
            this.value = Math.Clamp(value, 0f, 1f);
            Invalidate();
        }
    }

    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public float ViewRatio
    {
        get => viewRatio;
        set
        {
            viewRatio = Math.Clamp(value, 0f, 1f);
            Invalidate();
        }
    }

    private int ThumbHeight => Math.Min(Height, Math.Max(40, (int)(Height * viewRatio)));
    private int Travel => Math.Max(0, Height - ThumbHeight);
    private Rectangle ThumbRect => new(0, (int)Math.Round(value * Travel), Width, ThumbHeight);

    private static GraphicsPath Pill(RectangleF rect)
    {
        float diameter = Math.Min(rect.Width, rect.Height);
        var path = new GraphicsPath();

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 180);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 0, 180);
        path.CloseFigure();

        return path;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var track = new RectangleF(3, 1, Width - 6, Height - 2);

        using (var trackPath = Pill(track))
        {
            using var trackBrush = new SolidBrush(Color.FromArgb(110, Color.White));
            using var trackPen = new Pen(Color.FromArgb(90, Pink), 1.5f);

            g.FillPath(trackBrush, trackPath);
            g.DrawPath(trackPen, trackPath);
        }

        Rectangle thumbBounds = ThumbRect;

        var thumb = new RectangleF(
            thumbBounds.X + 1,
            thumbBounds.Y + 1,
            thumbBounds.Width - 2,
            thumbBounds.Height - 4);

        using (var shadowPath = Pill(
            new RectangleF(thumb.X, thumb.Y + 2.5f, thumb.Width, thumb.Height)))
        using (var shadowBrush = new SolidBrush(Color.FromArgb(70, Pink)))
        {
            g.FillPath(shadowBrush, shadowPath);
        }

        using (var thumbPath = Pill(thumb))
        {
            using var thumbBrush = new SolidBrush(hover || dragging ? PinkLight : Color.White);
            using var thumbPen = new Pen(Pink, 2.5f);

            g.FillPath(thumbBrush, thumbPath);
            g.DrawPath(thumbPen, thumbPath);
        }

        using var highlightBrush = new SolidBrush(Color.FromArgb(150, Color.White));

        g.FillEllipse(
            highlightBrush,
            thumb.X + 4,
            thumb.Y + 5,
            Math.Max(1, thumb.Width - 8),
            Math.Min(10, Math.Max(1, thumb.Height / 4)));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
            return;

        Rectangle thumb = ThumbRect;
        dragOffset = thumb.Contains(e.Location) ? e.Y - thumb.Y : ThumbHeight / 2;
        dragging = true;
        Capture = true;
        MoveTo(e.Y);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (dragging)
            MoveTo(e.Y);

        bool over = ThumbRect.Contains(e.Location);

        if (over != hover)
        {
            hover = over;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        dragging = false;
        Capture = false;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hover = false;
        Invalidate();
    }

    private void MoveTo(int mouseY)
    {
        if (Travel == 0)
            return;

        Value = (mouseY - dragOffset) / (float)Travel;
        Scrolled?.Invoke(Value);
    }
}
