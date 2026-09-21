using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace VideoPlayer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Core.Initialize();

            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private LibVLC _libVLC = null!;
        private MediaPlayer _mediaPlayer = null!;

        private VideoView _videoView = null!;

        private Panel _topBar = null!;
        private Panel _sideBar = null!;
        private Panel _bottomBar = null!;

        private Label _titleLabel = null!;
        private Label _timeLabel = null!;
        private Label _volumeLabel = null!;

        private Button _playButton = null!;
        private Button _prevButton = null!;
        private Button _nextButton = null!;
        private Button _fullscreenButton = null!;
        private Button _muteButton = null!;

        private TrackBar _progressBar = null!;
        private TrackBar _volumeBar = null!;

        private ListBox _playlist = null!;

        private Button _addButton = null!;
        private Button _removeButton = null!;
        private Button _upButton = null!;
        private Button _downButton = null!;

        private Label _emptyLabel = null!;

        private Panel _notificationPanel = null!;
        private Label _notificationLabel = null!;

        // ÖNEMLİ:
        // Timer artık açıkça Windows Forms Timer.
        private System.Windows.Forms.Timer _updateTimer = null!;
        private System.Windows.Forms.Timer _mouseTimer = null!;

        private List<string> _videos =
            new List<string>();

        private int _currentIndex = -1;

        private bool _isFullscreen = false;

        private bool _controlsHidden = false;

        private bool _noticeShown = false;

        private FormBorderStyle _oldBorderStyle;

        private FormWindowState _oldWindowState;

        private Rectangle _oldBounds;

        private Point _lastMousePosition;


        public MainForm()
        {
            Text = "OLCAY KILIÇ VIDEO PLAYER";

            Width = 1400;

            Height = 850;

            MinimumSize =
                new Size(1000, 650);

            StartPosition =
                FormStartPosition.CenterScreen;

            BackColor =
                Color.FromArgb(10, 10, 12);

            KeyPreview = true;

            _libVLC = new LibVLC();

            _mediaPlayer =
                new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached +=
                MediaPlayer_EndReached;

            BuildInterface();

            _updateTimer =
                new System.Windows.Forms.Timer();

            _updateTimer.Interval = 250;

            _updateTimer.Tick +=
                UpdateTimer_Tick;

            _updateTimer.Start();

            _mouseTimer =
                new System.Windows.Forms.Timer();

            _mouseTimer.Interval = 1000;

            _mouseTimer.Tick +=
                MouseTimer_Tick;

            _mouseTimer.Start();

            MouseMove +=
                MainForm_MouseMove;

            KeyDown +=
                MainForm_KeyDown;

            Shown +=
                MainForm_Shown;
        }


        // =========================================================
        // FORM AÇILDI
        // =========================================================

        private void MainForm_Shown(
            object? sender,
            EventArgs e)
        {
            CenterNotification();

            _lastMousePosition =
                Cursor.Position;
        }


        // =========================================================
        // ARAYÜZ
        // =========================================================

        private void BuildInterface()
        {
            // VIDEO
            _videoView =
                new VideoView();

            _videoView.Dock =
                DockStyle.Fill;

            _videoView.BackColor =
                Color.Black;

            _videoView.MediaPlayer =
                _mediaPlayer;

            Controls.Add(
                _videoView);


            // ÜST BAR
            _topBar =
                new Panel();

            _topBar.Dock =
                DockStyle.Top;

            _topBar.Height =
                65;

            _topBar.BackColor =
                Color.FromArgb(
                    18,
                    18,
                    22);

            Controls.Add(
                _topBar);


            _titleLabel =
                new Label();

            _titleLabel.Text =
                "OLCAY KILIÇ VIDEO PLAYER";

            _titleLabel.ForeColor =
                Color.White;

            _titleLabel.Font =
                new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold);

            _titleLabel.AutoSize =
                true;

            _titleLabel.Location =
                new Point(22, 20);

            _topBar.Controls.Add(
                _titleLabel);


            // SAĞ PANEL
            _sideBar =
                new Panel();

            _sideBar.Dock =
                DockStyle.Right;

            _sideBar.Width =
                300;

            _sideBar.BackColor =
                Color.FromArgb(
                    20,
                    20,
                    24);

            Controls.Add(
                _sideBar);


            // PLAYLIST
            _playlist =
                new ListBox();

            _playlist.Dock =
                DockStyle.Fill;

            _playlist.BackColor =
                Color.FromArgb(
                    25,
                    25,
                    29);

            _playlist.ForeColor =
                Color.White;

            _playlist.BorderStyle =
                BorderStyle.None;

            _playlist.Font =
                new Font(
                    "Segoe UI",
                    11);

            _playlist.ItemHeight =
                36;

            _playlist.SelectedIndexChanged +=
                Playlist_SelectedIndexChanged;

            _sideBar.Controls.Add(
                _playlist);


            // PLAYLIST BUTONLARI
            Panel playlistButtons =
                new Panel();

            playlistButtons.Dock =
                DockStyle.Bottom;

            playlistButtons.Height =
                150;

            playlistButtons.BackColor =
                Color.FromArgb(
                    18,
                    18,
                    22);

            _sideBar.Controls.Add(
                playlistButtons);


            _addButton =
                CreateButton("+ VİDEO EKLE");

            _addButton.Location =
                new Point(15, 12);

            _addButton.Width =
                270;

            _addButton.Click +=
                AddButton_Click;

            playlistButtons.Controls.Add(
                _addButton);


            _removeButton =
                CreateButton("VİDEOYU SİL");

            _removeButton.Location =
                new Point(15, 52);

            _removeButton.Width =
                130;

            _removeButton.Click +=
                RemoveButton_Click;

            playlistButtons.Controls.Add(
                _removeButton);


            _upButton =
                CreateButton("▲");

            _upButton.Location =
                new Point(155, 52);

            _upButton.Width =
                60;

            _upButton.Click +=
                UpButton_Click;

            playlistButtons.Controls.Add(
                _upButton);


            _downButton =
                CreateButton("▼");

            _downButton.Location =
                new Point(225, 52);

            _downButton.Width =
                60;

            _downButton.Click +=
                DownButton_Click;

            playlistButtons.Controls.Add(
                _downButton);


            // BOŞ EKRAN YAZISI
            _emptyLabel =
                new Label();

            _emptyLabel.Text =
                "VIDEO EKLE\n\n" +
                "En fazla 20 video ekleyebilirsin.";

            _emptyLabel.ForeColor =
                Color.FromArgb(
                    150,
                    150,
                    155);

            _emptyLabel.BackColor =
                Color.Transparent;

            _emptyLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            _emptyLabel.Dock =
                DockStyle.Fill;

            _emptyLabel.Font =
                new Font(
                    "Segoe UI",
                    14,
                    FontStyle.Bold);

            _videoView.Controls.Add(
                _emptyLabel);

            _emptyLabel.BringToFront();


            // ALT BAR
            _bottomBar =
                new Panel();

            _bottomBar.Dock =
                DockStyle.Bottom;

            _bottomBar.Height =
                105;

            _bottomBar.BackColor =
                Color.FromArgb(
                    18,
                    18,
                    22);

            Controls.Add(
                _bottomBar);


            // PROGRESS
            _progressBar =
                new TrackBar();

            _progressBar.Minimum =
                0;

            _progressBar.Maximum =
                1000;

            _progressBar.Value =
                0;

            _progressBar.TickStyle =
                TickStyle.None;

            _progressBar.Location =
                new Point(20, 5);

            _progressBar.Width =
                ClientSize.Width - 360;

            _progressBar.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Top;

            _progressBar.MouseDown +=
                ProgressBar_MouseDown;

            _bottomBar.Controls.Add(
                _progressBar);


            // SÜRE
            _timeLabel =
                new Label();

            _timeLabel.Text =
                "00:00 / 00:00";

            _timeLabel.ForeColor =
                Color.LightGray;

            _timeLabel.AutoSize =
                true;

            _timeLabel.Location =
                new Point(20, 35);

            _timeLabel.Font =
                new Font(
                    "Segoe UI",
                    10);

            _bottomBar.Controls.Add(
                _timeLabel);


            // ÖNCEKİ
            _prevButton =
                CreateButton("⏮");

            _prevButton.Location =
                new Point(190, 28);

            _prevButton.Width =
                50;

            _prevButton.Click +=
                PrevButton_Click;

            _bottomBar.Controls.Add(
                _prevButton);


            // PLAY
            _playButton =
                CreateButton("▶");

            _playButton.Location =
                new Point(250, 28);

            _playButton.Width =
                60;

            _playButton.Click +=
                PlayButton_Click;

            _bottomBar.Controls.Add(
                _playButton);


            // SONRAKİ
            _nextButton =
                CreateButton("⏭");

            _nextButton.Location =
                new Point(320, 28);

            _nextButton.Width =
                50;

            _nextButton.Click +=
                NextButton_Click;

            _bottomBar.Controls.Add(
                _nextButton);


            // MUTE
            _muteButton =
                CreateButton("🔊");

            _muteButton.Location =
                new Point(400, 28);

            _muteButton.Width =
                55;

            _muteButton.Click +=
                MuteButton_Click;

            _bottomBar.Controls.Add(
                _muteButton);


            // SES
            _volumeBar =
                new TrackBar();

            _volumeBar.Minimum =
                0;

            _volumeBar.Maximum =
                100;

            _volumeBar.Value =
                100;

            _volumeBar.TickStyle =
                TickStyle.None;

            _volumeBar.Location =
                new Point(455, 28);

            _volumeBar.Width =
                110;

            _volumeBar.ValueChanged +=
                VolumeBar_ValueChanged;

            _bottomBar.Controls.Add(
                _volumeBar);


            _volumeLabel =
                new Label();

            _volumeLabel.Text =
                "100%";

            _volumeLabel.ForeColor =
                Color.LightGray;

            _volumeLabel.AutoSize =
                true;

            _volumeLabel.Location =
                new Point(570, 36);

            _bottomBar.Controls.Add(
                _volumeLabel);


            // FULLSCREEN
            _fullscreenButton =
                CreateButton(
                    "⛶ TAM EKRAN");

            _fullscreenButton.Width =
                130;

            _fullscreenButton.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            _fullscreenButton.Location =
                new Point(
                    ClientSize.Width - 160,
                    28);

            _fullscreenButton.Click +=
                FullscreenButton_Click;

            _bottomBar.Controls.Add(
                _fullscreenButton);


            // =====================================================
            // 40 SANİYE UYARISI
            // =====================================================

            _notificationPanel =
                new Panel();

            _notificationPanel.Width =
                500;

            _notificationPanel.Height =
                135;

            _notificationPanel.BackColor =
                Color.FromArgb(
                    225,
                    10,
                    10,
                    13);

            _notificationPanel.Visible =
                false;

            Controls.Add(
                _notificationPanel);


            _notificationLabel =
                new Label();

            _notificationLabel.Dock =
                DockStyle.Fill;

            _notificationLabel.Text =
                "İYİ SEYİRLER OLCAY KILIÇ ❤️\n\n" +
                "Sonraki bölüme geçiliyor...";

            _notificationLabel.ForeColor =
                Color.White;

            _notificationLabel.BackColor =
                Color.Transparent;

            _notificationLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            _notificationLabel.Font =
                new Font(
                    "Segoe UI",
                    17,
                    FontStyle.Bold);

            _notificationPanel.Controls.Add(
                _notificationLabel);

            _notificationPanel.BringToFront();

            CenterNotification();

            Resize +=
                MainForm_Resize;
        }


        // =========================================================
        // BUTTON
        // =========================================================

        private Button CreateButton(
            string text)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.ForeColor =
                Color.White;

            button.BackColor =
                Color.FromArgb(
                    35,
                    35,
                    40);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            button.Height =
                34;

            button.Cursor =
                Cursors.Hand;

            return button;
        }


        // =========================================================
        // BİLDİRİM ORTALA
        // =========================================================

        private void CenterNotification()
        {
            if (_notificationPanel == null)
                return;

            _notificationPanel.Left =
                (ClientSize.Width -
                 _notificationPanel.Width) / 2;

            _notificationPanel.Top =
                (ClientSize.Height -
                 _notificationPanel.Height) / 2;
        }


        private void MainForm_Resize(
            object? sender,
            EventArgs e)
        {
            CenterNotification();
        }


        // =========================================================
        // VİDEO EKLE
        // =========================================================

        private void AddButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_videos.Count >= 20)
            {
                MessageBox.Show(
                    "En fazla 20 video ekleyebilirsin.",
                    "Video Limiti",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using OpenFileDialog dialog =
                new OpenFileDialog();

            dialog.Title =
                "Video Seç";

            dialog.Filter =
                "Video Dosyaları|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm|Tüm Dosyalar|*.*";

            dialog.Multiselect =
                true;

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

            foreach (string file
                in dialog.FileNames)
            {
                if (_videos.Count >= 20)
                    break;

                if (!_videos.Contains(file))
                {
                    _videos.Add(file);

                    _playlist.Items.Add(
                        $"{_videos.Count}.  " +
                        Path.GetFileName(file));
                }
            }

            _emptyLabel.Visible =
                _videos.Count == 0;

            if (_currentIndex == -1 &&
                _videos.Count > 0)
            {
                PlayVideo(0);
            }
        }


        // =========================================================
        // SİL
        // =========================================================

        private void RemoveButton_Click(
            object? sender,
            EventArgs e)
        {
            int index =
                _playlist.SelectedIndex;

            if (index < 0 ||
                index >= _videos.Count)
            {
                return;
            }

            _videos.RemoveAt(index);

            RefreshPlaylist();

            if (_videos.Count == 0)
            {
                _currentIndex = -1;

                _mediaPlayer.Stop();

                _titleLabel.Text =
                    "OLCAY KILIÇ VIDEO PLAYER";

                _emptyLabel.Visible =
                    true;

                return;
            }

            if (_currentIndex >=
                _videos.Count)
            {
                _currentIndex =
                    _videos.Count - 1;
            }

            PlayVideo(
                _currentIndex);
        }


        // =========================================================
        // YUKARI
        // =========================================================

        private void UpButton_Click(
            object? sender,
            EventArgs e)
        {
            int index =
                _playlist.SelectedIndex;

            if (index <= 0)
                return;

            string temp =
                _videos[index];

            _videos[index] =
                _videos[index - 1];

            _videos[index - 1] =
                temp;

            RefreshPlaylist();

            _playlist.SelectedIndex =
                index - 1;
        }


        // =========================================================
        // AŞAĞI
        // =========================================================

        private void DownButton_Click(
            object? sender,
            EventArgs e)
        {
            int index =
                _playlist.SelectedIndex;

            if (index < 0 ||
                index >= _videos.Count - 1)
            {
                return;
            }

            string temp =
                _videos[index];

            _videos[index] =
                _videos[index + 1];

            _videos[index + 1] =
                temp;

            RefreshPlaylist();

            _playlist.SelectedIndex =
                index + 1;
        }


        // =========================================================
        // PLAYLIST YENİLE
        // =========================================================

        private void RefreshPlaylist()
        {
            _playlist.Items.Clear();

            for (int i = 0;
                 i < _videos.Count;
                 i++)
            {
                _playlist.Items.Add(
                    $"{i + 1}.  " +
                    Path.GetFileName(
                        _videos[i]));
            }
        }


        // =========================================================
        // PLAYLIST SEÇ
        // =========================================================

        private void Playlist_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            int index =
                _playlist.SelectedIndex;

            if (index < 0)
                return;

            if (index != _currentIndex)
            {
                PlayVideo(index);
            }
        }


        // =========================================================
        // VİDEO OYNAT
        // =========================================================

        private void PlayVideo(
            int index)
        {
            if (index < 0 ||
                index >= _videos.Count)
            {
                return;
            }

            string file =
                _videos[index];

            if (!File.Exists(file))
            {
                MessageBox.Show(
                    "Video dosyası bulunamadı:\n\n" +
                    file);

                return;
            }

            _currentIndex =
                index;

            // YENİ VİDEO BAŞLADIĞINDA
            // 40 saniyelik uyarıyı sıfırla.
            _noticeShown =
                false;

            _notificationPanel.Visible =
                false;

            try
            {
                _mediaPlayer.Stop();

                using Media media =
                    new Media(
                        _libVLC,
                        new Uri(file));

                _mediaPlayer.Play(
                    media);

                _titleLabel.Text =
                    Path.GetFileName(file);

                _playlist.SelectedIndex =
                    index;

                _emptyLabel.Visible =
                    false;

                _playButton.Text =
                    "⏸";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Video açılırken hata oluştu:\n\n" +
                    ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        // =========================================================
        // PLAY / PAUSE
        // =========================================================

        private void PlayButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex == -1)
            {
                if (_videos.Count > 0)
                {
                    PlayVideo(0);
                }

                return;
            }

            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();

                _playButton.Text =
                    "▶";
            }
            else
            {
                _mediaPlayer.Play();

                _playButton.Text =
                    "⏸";
            }
        }


        // =========================================================
        // ÖNCEKİ
        // =========================================================

        private void PrevButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_videos.Count == 0)
                return;

            int index =
                _currentIndex - 1;

            if (index < 0)
                index = 0;

            PlayVideo(index);
        }


        // =========================================================
        // SONRAKİ
        // =========================================================

        private void NextButton_Click(
            object? sender,
            EventArgs e)
        {
            PlayNextVideo();
        }


        // =========================================================
        // VIDEO BİTTİ
        // =========================================================

        private void MediaPlayer_EndReached(
            object? sender,
            EventArgs e)
        {
            BeginInvoke(
                new Action(() =>
                {
                    PlayNextVideo();
                }));
        }


        // =========================================================
        // SONRAKİ VİDEO
        // =========================================================

        private void PlayNextVideo()
        {
            if (_videos.Count == 0)
                return;

            int next =
                _currentIndex + 1;

            if (next >= _videos.Count)
            {
                next = 0;
            }

            PlayVideo(next);
        }


        // =========================================================
        // TIMER
        // =========================================================

        private void UpdateTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (_mediaPlayer == null)
                return;

            long time =
                _mediaPlayer.Time;

            long length =
                _mediaPlayer.Length;

            if (length <= 0)
                return;

            double percent =
                (double)time /
                length;

            if (percent < 0)
                percent = 0;

            if (percent > 1)
                percent = 1;

            int value =
                (int)(
                    percent *
                    _progressBar.Maximum);

            if (value >=
                _progressBar.Minimum &&
                value <=
                _progressBar.Maximum)
            {
                _progressBar.Value =
                    value;
            }

            _timeLabel.Text =
                FormatTime(time) +
                " / " +
                FormatTime(length);


            // =====================================================
            // SON 40 SANİYE
            // =====================================================

            long remaining =
                length - time;

            if (remaining <= 40000 &&
                remaining > 0 &&
                !_noticeShown)
            {
                _noticeShown =
                    true;

                _notificationLabel.Text =
                    "İYİ SEYİRLER OLCAY KILIÇ ❤️\n\n" +
                    "Sonraki bölüme geçiliyor...";

                _notificationPanel.Visible =
                    true;

                CenterNotification();

                _notificationPanel.BringToFront();
            }

            _playButton.Text =
                _mediaPlayer.IsPlaying
                    ? "⏸"
                    : "▶";
        }


        // =========================================================
        // SÜRE
        // =========================================================

        private string FormatTime(
            long milliseconds)
        {
            if (milliseconds < 0)
                milliseconds = 0;

            TimeSpan time =
                TimeSpan.FromMilliseconds(
                    milliseconds);

            if (time.TotalHours >= 1)
            {
                return time.ToString(
                    @"hh\:mm\:ss");
            }

            return time.ToString(
                @"mm\:ss");
        }


        // =========================================================
        // SEEK
        // =========================================================

        private void ProgressBar_MouseDown(
            object? sender,
            MouseEventArgs e)
        {
            if (_mediaPlayer.Length <= 0)
                return;

            TrackBar bar =
                (TrackBar)sender!;

            double percent =
                (double)e.X /
                bar.Width;

            if (percent < 0)
                percent = 0;

            if (percent > 1)
                percent = 1;

            long position =
                (long)(
                    _mediaPlayer.Length *
                    percent);

            _mediaPlayer.Time =
                position;
        }


        // =========================================================
        // SES
        // =========================================================

        private void VolumeBar_ValueChanged(
            object? sender,
            EventArgs e)
        {
            int volume =
                _volumeBar.Value;

            _mediaPlayer.Volume =
                volume;

            _volumeLabel.Text =
                volume + "%";

            _muteButton.Text =
                volume == 0
                    ? "🔇"
                    : "🔊";
        }


        // =========================================================
        // MUTE
        // =========================================================

        private void MuteButton_Click(
            object? sender,
            EventArgs e)
        {
            _mediaPlayer.Mute =
                !_mediaPlayer.Mute;

            _muteButton.Text =
                _mediaPlayer.Mute
                    ? "🔇"
                    : "🔊";
        }


        // =========================================================
        // FULLSCREEN
        // =========================================================

        private void FullscreenButton_Click(
            object? sender,
            EventArgs e)
        {
            ToggleFullscreen();
        }


        private void ToggleFullscreen()
        {
            if (!_isFullscreen)
            {
                _oldBorderStyle =
                    FormBorderStyle;

                _oldWindowState =
                    WindowState;

                _oldBounds =
                    Bounds;

                _isFullscreen =
                    true;

                FormBorderStyle =
                    FormBorderStyle.None;

                WindowState =
                    FormWindowState.Maximized;

                _topBar.Visible =
                    false;

                _sideBar.Visible =
                    false;

                _bottomBar.Visible =
                    false;

                Cursor.Hide();

                _controlsHidden =
                    true;

                if (_notificationPanel.Visible)
                {
                    _notificationPanel.BringToFront();

                    CenterNotification();
                }
            }
            else
            {
                _isFullscreen =
                    false;

                Cursor.Show();

                _controlsHidden =
                    false;

                FormBorderStyle =
                    _oldBorderStyle;

                WindowState =
                    _oldWindowState;

                Bounds =
                    _oldBounds;

                _topBar.Visible =
                    true;

                _sideBar.Visible =
                    true;

                _bottomBar.Visible =
                    true;

                if (_notificationPanel.Visible)
                {
                    _notificationPanel.BringToFront();
                }
            }
        }


        // =========================================================
        // KLAVYE
        // =========================================================

        private void MainForm_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode == Keys.Escape &&
                _isFullscreen)
            {
                ToggleFullscreen();

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                PlayButton_Click(
                    null,
                    EventArgs.Empty);

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode == Keys.Right)
            {
                if (_mediaPlayer.Length > 0)
                {
                    _mediaPlayer.Time +=
                        10000;
                }

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                if (_mediaPlayer.Length > 0)
                {
                    _mediaPlayer.Time -=
                        10000;
                }

                e.SuppressKeyPress =
                    true;
            }
        }


        // =========================================================
        // MOUSE
        // =========================================================

        private void MainForm_MouseMove(
            object? sender,
            MouseEventArgs e)
        {
            if (!_isFullscreen)
                return;

            Cursor.Show();

            _controlsHidden =
                false;

            _lastMousePosition =
                Cursor.Position;

            if (!_notificationPanel.Visible)
            {
                _bottomBar.Visible =
                    true;
            }
        }


        private void MouseTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (!_isFullscreen)
                return;

            Point current =
                Cursor.Position;

            if (current !=
                _lastMousePosition)
            {
                _lastMousePosition =
                    current;

                Cursor.Show();

                _controlsHidden =
                    false;

                if (!_notificationPanel.Visible)
                {
                    _bottomBar.Visible =
                        true;
                }

                return;
            }

            if (_notificationPanel.Visible)
            {
                Cursor.Hide();

                _bottomBar.Visible =
                    false;

                _controlsHidden =
                    true;

                return;
            }

            Cursor.Hide();

            _bottomBar.Visible =
                false;

            _controlsHidden =
                true;
        }


        // =========================================================
        // KAPAT
        // =========================================================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            try
            {
                _mediaPlayer.Stop();

                _mediaPlayer.Dispose();

                _libVLC.Dispose();
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }
    }
}
