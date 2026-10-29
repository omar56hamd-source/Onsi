using SampLauncher.Models;
using SampLauncher.Services;

namespace SampLauncher;

public class MainForm : Form
{
    readonly Settings _s = Settings.Load();

    readonly TextBox _nick = new(), _ip = new(), _port = new(), _pass = new(), _gta = new();
    readonly ListBox _favs = new();
    readonly Label _status = new();
    readonly Button _play = new();

    static readonly Color Accent = Color.FromArgb(255, 128, 64);
    static readonly Color Panel = Color.FromArgb(200, 20, 12, 36);

    public MainForm()
    {
        Text = "SA-MP Launcher";
        ClientSize = new Size(900, 560);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f);
        DoubleBuffered = true;

        var assets = Path.Combine(AppContext.BaseDirectory, "Assets");
        try { BackgroundImage = Image.FromFile(Path.Combine(assets, "bg.png")); } catch { BackColor = Color.FromArgb(30, 14, 44); }
        try { Icon = new Icon(Path.Combine(assets, "icon.ico")); } catch { }

        var logo = new PictureBox { Location = new Point(30, 20), Size = new Size(420, 110), BackColor = Color.Transparent, SizeMode = PictureBoxSizeMode.Zoom };
        try { logo.Image = Image.FromFile(Path.Combine(assets, "logo.png")); } catch { }
        Controls.Add(logo);

        // ---- left panel: connection ----
        var left = new Panel { Location = new Point(30, 150), Size = new Size(420, 370), BackColor = Panel };
        Controls.Add(left);

        int y = 15;
        AddField(left, "Nickname", _nick, ref y);
        AddField(left, "Server IP", _ip, ref y);
        AddField(left, "Port", _port, ref y);
        AddField(left, "Password (optional)", _pass, ref y);
        _pass.UseSystemPasswordChar = true;
        AddField(left, "GTA San Andreas folder", _gta, ref y);

        var browse = StyledButton("Browse...", new Rectangle(310, y - 32, 90, 28), Color.FromArgb(70, 60, 100));
        _gta.Width = 285;
        browse.Click += (_, _) => BrowseGta();
        left.Controls.Add(browse);

        _play.Text = "PLAY";
        _play.SetBounds(20, y + 10, 380, 48);
        StyleButton(_play, Accent);
        _play.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        _play.Click += (_, _) => Play();
        left.Controls.Add(_play);

        // ---- right panel: favorites ----
        var right = new Panel { Location = new Point(470, 150), Size = new Size(400, 370), BackColor = Panel };
        Controls.Add(right);

        right.Controls.Add(new Label { Text = "Favorite servers", ForeColor = Color.White, Font = new Font("Segoe UI", 12f, FontStyle.Bold), Location = new Point(15, 12), AutoSize = true });

        _favs.SetBounds(15, 45, 370, 190);
        _favs.BackColor = Color.FromArgb(30, 22, 52);
        _favs.ForeColor = Color.White;
        _favs.BorderStyle = BorderStyle.None;
        _favs.SelectedIndexChanged += async (_, _) => await OnFavSelected();
        right.Controls.Add(_favs);

        var add = StyledButton("Add current", new Rectangle(15, 245, 120, 32), Color.FromArgb(70, 60, 100));
        var del = StyledButton("Remove", new Rectangle(145, 245, 100, 32), Color.FromArgb(120, 40, 60));
        var refresh = StyledButton("Query", new Rectangle(255, 245, 130, 32), Color.FromArgb(40, 110, 90));
        add.Click += (_, _) => AddFavorite();
        del.Click += (_, _) => RemoveFavorite();
        refresh.Click += async (_, _) => await QueryCurrent();
        right.Controls.AddRange(new Control[] { add, del, refresh });

        _status.SetBounds(15, 290, 370, 70);
        _status.ForeColor = Color.FromArgb(255, 214, 140);
        _status.Text = "Select a server or press Query.";
        right.Controls.Add(_status);

        // load saved values
        _nick.Text = _s.Nickname;
        _ip.Text = _s.Ip;
        _port.Text = _s.Port.ToString();
        _pass.Text = _s.Password;
        _gta.Text = !string.IsNullOrEmpty(_s.GtaPath) ? _s.GtaPath : (SampService.FindGtaFolder() ?? "");
        RefreshFavs();

        FormClosing += (_, _) => { Collect(); _s.Save(); };
    }

    // ---------- UI helpers ----------
    void AddField(Panel p, string label, TextBox box, ref int y)
    {
        p.Controls.Add(new Label { Text = label, ForeColor = Color.White, Location = new Point(20, y), AutoSize = true });
        box.SetBounds(20, y + 22, 380, 28);
        box.BackColor = Color.FromArgb(30, 22, 52);
        box.ForeColor = Color.White;
        box.BorderStyle = BorderStyle.FixedSingle;
        p.Controls.Add(box);
        y += 62;
    }

    Button StyledButton(string text, Rectangle r, Color c)
    {
        var b = new Button { Text = text, Bounds = r };
        StyleButton(b, c);
        return b;
    }

    static void StyleButton(Button b, Color c)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = c;
        b.ForeColor = Color.White;
        b.Cursor = Cursors.Hand;
    }

    // ---------- logic ----------
    void Collect()
    {
        _s.Nickname = _nick.Text.Trim();
        _s.Ip = _ip.Text.Trim();
        _s.Port = int.TryParse(_port.Text, out var p) ? p : 7777;
        _s.Password = _pass.Text;
        _s.GtaPath = _gta.Text.Trim();
    }

    void BrowseGta()
    {
        using var dlg = new FolderBrowserDialog { Description = "Select your GTA San Andreas folder" };
        if (dlg.ShowDialog() == DialogResult.OK) _gta.Text = dlg.SelectedPath;
    }

    void Play()
    {
        Collect();
        var err = SampService.Launch(_s.GtaPath, _s.Nickname, _s.Ip, _s.Port, _s.Password);
        if (err != null) MessageBox.Show(err, "Launch error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        else _s.Save();
    }

    void RefreshFavs()
    {
        _favs.Items.Clear();
        foreach (var f in _s.Favorites) _favs.Items.Add(f);
    }

    void AddFavorite()
    {
        Collect();
        var name = _status.Text.StartsWith("Name: ") ? _status.Text.Split('\n')[0][6..] : $"{_s.Ip}:{_s.Port}";
        _s.Favorites.Add(new ServerEntry { Name = name, Ip = _s.Ip, Port = _s.Port });
        RefreshFavs();
    }

    void RemoveFavorite()
    {
        if (_favs.SelectedIndex < 0) return;
        _s.Favorites.RemoveAt(_favs.SelectedIndex);
        RefreshFavs();
    }

    async Task OnFavSelected()
    {
        if (_favs.SelectedItem is not ServerEntry f) return;
        _ip.Text = f.Ip;
        _port.Text = f.Port.ToString();
        await QueryCurrent();
    }

    async Task QueryCurrent()
    {
        Collect();
        _status.Text = "Querying...";
        var info = await ServerQuery.GetInfoAsync(_s.Ip, _s.Port);
        _status.Text = info == null
            ? "Server offline or not responding."
            : $"Name: {info.Hostname}\nPlayers: {info.Players}/{info.MaxPlayers}   Ping: {info.PingMs} ms\nMode: {info.Gamemode}   Lang: {info.Language}" +
              (info.Passworded ? "\n🔒 Password required" : "");
    }
}
