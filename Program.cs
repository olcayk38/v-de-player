using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView _videoView;

        private List<string> _videos = new List<string>();
        private int _currentIndex = -1;

        private ListBox _videoList;

        private Button _addButton;
        private Button _removeButton;
        private Button _upButton;
        private Button _downButton;
        private Button _playButton;
        private Button _pauseButton;
        private Button _stopButton;
        private Button _fullscreenButton;

        private TrackBar _volumeBar;

        private Label _statusLabel;

        private bool _fullscreen = false;
        private FormBorderStyle _oldBorderStyle;
        private FormWindowState _oldWindowState;
        private Rectangle _oldBounds;

        public MainForm()
        {
            Text = "Video Player";
            Width = 1200;
            Height = 750;
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;

            BackColor = Color.FromArgb(20, 20, 20);

            _libVLC = new LibVLC();

            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached += MediaPlayer_EndReached;

            BuildInterface();

            FormClosing += MainForm_FormClosing;

            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
        }

        private void BuildInterface()
        {
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(20, 20, 20);

            Controls.Add(mainPanel);

            // =========================
            // VIDEO
            // =========================

            _videoView = new VideoView();

            _videoView.Dock = DockStyle.Fill;

            _videoView.MediaPlayer = _mediaPlayer;

            _videoView.BackColor = Color.Black;

            mainPanel.Controls.Add(_videoView);

            // =========================
            // RIGHT LIST PANEL
            // =========================

            Panel listPanel = new Panel();

            listPanel.Dock = DockStyle.Right;
            listPanel.Width = 300;
            listPanel.BackColor = Color.FromArgb(30, 30, 30);

            mainPanel.Controls.Add(listPanel);

            Label titleLabel = new Label();

            titleLabel.Text = "VİDEOLAR";

            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 45;

            titleLabel.TextAlign = ContentAlignment.MiddleCenter;

            titleLabel.ForeColor = Color.White;
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            listPanel.Controls.Add(titleLabel);

            _videoList = new ListBox();

            _videoList.Dock = DockStyle.Fill;

            _videoList.BackColor = Color.FromArgb(35, 35, 35);
            _videoList.ForeColor = Color.White;

            _videoList.BorderStyle = BorderStyle.None;

            _videoList.Font = new Font("Segoe UI", 10);

            _videoList.DoubleClick += VideoList_DoubleClick;

            listPanel.Controls.Add(_videoList);

            // =========================
            // BOTTOM CONTROL PANEL
            // =========================

            Panel bottomPanel = new Panel();

            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 110;

            bottomPanel.BackColor = Color.FromArgb(25, 25, 25);

            mainPanel.Controls.Add(bottomPanel);

            // =========================
            // BUTTONS
            // =========================

            _addButton = CreateButton("＋ Video Ekle");
            _addButton.Click += AddButton_Click;

            _removeButton = CreateButton("Sil");
            _removeButton.Click += RemoveButton_Click;

            _upButton = CreateButton("↑");
            _upButton.Click += UpButton_Click;

            _downButton = CreateButton("↓");
            _downButton.Click += DownButton_Click;

            _playButton = CreateButton("▶ Oynat");
            _playButton.Click += PlayButton_Click;

            _pauseButton = CreateButton("Ⅱ Duraklat");
            _pauseButton.Click += PauseButton_Click;

            _stopButton = CreateButton("■ Durdur");
            _stopButton.Click += StopButton_Click;

            _fullscreenButton = CreateButton("⛶ Tam Ekran");
            _fullscreenButton.Click += FullscreenButton_Click;

            FlowLayoutPanel buttonsPanel = new FlowLayoutPanel();

            buttonsPanel.Dock = DockStyle.Top;
            buttonsPanel.Height = 50;

            buttonsPanel.Padding = new Padding(10, 5, 10, 5);

            buttonsPanel.WrapContents = false;

            buttonsPanel.AutoScroll = true;

            bottomPanel.Controls.Add(buttonsPanel);

            buttonsPanel.Controls.Add(_addButton);
            buttonsPanel.Controls.Add(_removeButton);
            buttonsPanel.Controls.Add(_upButton);
            buttonsPanel.Controls.Add(_downButton);

            buttonsPanel.Controls.Add(_playButton);
            buttonsPanel.Controls.Add(_pauseButton);
            buttonsPanel.Controls.Add(_stopButton);

            buttonsPanel.Controls.Add(_fullscreenButton);

            // =========================
            // VOLUME
            // =========================

            Label volumeLabel = new Label();

            volumeLabel.Text = "Ses";

            volumeLabel.ForeColor = Color.White;

            volumeLabel.AutoSize = true;

            volumeLabel.Location = new Point(20, 70);

            bottomPanel.Controls.Add(volumeLabel);

            _volumeBar = new TrackBar();

            _volumeBar.Minimum = 0;
            _volumeBar.Maximum = 100;

            _volumeBar.Value = 100;

            _volumeBar.Width = 180;

            _volumeBar.Location = new Point(55, 62);

            _volumeBar.Scroll += VolumeBar_Scroll;

            bottomPanel.Controls.Add(_volumeBar);

            // =========================
            // STATUS
            // =========================

            _statusLabel = new Label();

            _statusLabel.Text = "Video seçin.";

            _statusLabel.ForeColor = Color.LightGray;

            _statusLabel.AutoSize = true;

            _statusLabel.Location = new Point(250, 70);

            bottomPanel.Controls.Add(_statusLabel);
        }

        private Button CreateButton(string text)
        {
            Button button = new Button();

            button.Text = text;

            button.Height = 35;

            button.AutoSize = true;

            button.FlatStyle = FlatStyle.Flat;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(80, 80, 80);

            button.BackColor =
                Color.FromArgb(45, 45, 45);

            button.ForeColor = Color.White;

            button.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);

            button.Margin =
                new Padding(4);

            return button;
        }

        // =========================
        // VIDEO EKLE
        // =========================

        private void AddButton_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Videoları seç";

            dialog.Filter =
                "Video Dosyaları|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm|Tüm Dosyalar|*.*";

            dialog.Multiselect = true;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            foreach (string file in dialog.FileNames)
            {
                if (!_videos.Contains(file))
                {
                    _videos.Add(file);

                    _videoList.Items.Add(
                        Path.GetFileName(file)
                    );
                }
            }

            if (_currentIndex == -1 && _videos.Count > 0)
            {
                _videoList.SelectedIndex = 0;
            }

            UpdateStatus();
        }

        // =========================
        // SİL
        // =========================

        private void RemoveButton_Click(object? sender, EventArgs e)
        {
            int index = _videoList.SelectedIndex;

            if (index < 0)
                return;

            bool currentlyPlaying =
                index == _currentIndex;

            if (currentlyPlaying)
            {
                _mediaPlayer.Stop();

                _currentIndex = -1;
            }

            _videos.RemoveAt(index);

            _videoList.Items.RemoveAt(index);

            if (_videoList.Items.Count > 0)
            {
                int newIndex =
                    Math.Min(index, _videoList.Items.Count - 1);

                _videoList.SelectedIndex = newIndex;
            }

            UpdateStatus();
        }

        // =========================
        // YUKARI
        // =========================

        private void UpButton_Click(object? sender, EventArgs e)
        {
            int index = _videoList.SelectedIndex;

            if (index <= 0)
                return;

            string temp = _videos[index];

            _videos[index] =
                _videos[index - 1];

            _videos[index - 1] =
                temp;

            object item =
                _videoList.Items[index];

            _videoList.Items[index] =
                _videoList.Items[index - 1];

            _videoList.Items[index - 1] =
                item;

            _videoList.SelectedIndex =
                index - 1;
        }

        // =========================
        // AŞAĞI
        // =========================

        private void DownButton_Click(object? sender, EventArgs e)
        {
            int index = _videoList.SelectedIndex;

            if (index < 0 ||
                index >= _videos.Count - 1)
                return;

            string temp = _videos[index];

            _videos[index] =
                _videos[index + 1];

            _videos[index + 1] =
                temp;

            object item =
                _videoList.Items[index];

            _videoList.Items[index] =
                _videoList.Items[index + 1];

            _videoList.Items[index + 1] =
                item;

            _videoList.SelectedIndex =
                index + 1;
        }

        // =========================
        // OYNAT
        // =========================

        private void PlayButton_Click(object? sender, EventArgs e)
        {
            int index = _videoList.SelectedIndex;

            if (index < 0)
            {
                if (_videos.Count == 0)
                {
                    MessageBox.Show(
                        "Önce video ekle.",
                        "Video Player"
                    );

                    return;
                }

                index = 0;
            }

            PlayVideo(index);
        }

        private void PlayVideo(int index)
        {
            if (index < 0 ||
                index >= _videos.Count)
                return;

            string file = _videos[index];

            if (!File.Exists(file))
            {
                MessageBox.Show(
                    "Video bulunamadı:\n\n" + file,
                    "Hata"
                );

                return;
            }

            _currentIndex = index;

            _videoList.SelectedIndex = index;

            _mediaPlayer.Stop();

            using Media media =
                new Media(_libVLC, new Uri(file));

            _mediaPlayer.Play(media);

            _mediaPlayer.Volume =
                _volumeBar.Value;

            _statusLabel.Text =
                "Oynatılıyor: " +
                Path.GetFileName(file);
        }

        // =========================
        // OTOMATİK SONRAKİ
        // =========================

        private void MediaPlayer_EndReached(
            object? sender,
            EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                PlayNextVideo();
            }));
        }

        private void PlayNextVideo()
        {
            if (_videos.Count == 0)
                return;

            int nextIndex =
                _currentIndex + 1;

            if (nextIndex >= _videos.Count)
            {
                _statusLabel.Text =
                    "Tüm videolar tamamlandı.";

                _currentIndex = -1;

                _videoList.ClearSelected();

                return;
            }

            PlayVideo(nextIndex);
        }

        // =========================
        // DURAKLAT
        // =========================

        private void PauseButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();

                _statusLabel.Text =
                    "Duraklatıldı.";
            }
        }

        // =========================
        // DURDUR
        // =========================

        private void StopButton_Click(
            object? sender,
            EventArgs e)
        {
            _mediaPlayer.Stop();

            _statusLabel.Text =
                "Durduruldu.";
        }

        // =========================
        // ÇİFT TIK
        // =========================

        private void VideoList_DoubleClick(
            object? sender,
            EventArgs e)
        {
            int index =
                _videoList.SelectedIndex;

            if (index >= 0)
            {
                PlayVideo(index);
            }
        }

        // =========================
        // SES
        // =========================

        private void VolumeBar_Scroll(
            object? sender,
            EventArgs e)
        {
            _mediaPlayer.Volume =
                _volumeBar.Value;
        }

        // =========================
        // FULLSCREEN
        // =========================

        private void FullscreenButton_Click(
            object? sender,
            EventArgs e)
        {
            ToggleFullscreen();
        }

        private void ToggleFullscreen()
        {
            if (!_fullscreen)
            {
                _oldBorderStyle =
                    FormBorderStyle;

                _oldWindowState =
                    WindowState;

                _oldBounds =
                    Bounds;

                FormBorderStyle =
                    FormBorderStyle.None;

                WindowState =
                    FormWindowState.Maximized;

                _fullscreen = true;

                _videoView.BringToFront();
            }
            else
            {
                FormBorderStyle =
                    _oldBorderStyle;

                WindowState =
                    _oldWindowState;

                Bounds =
                    _oldBounds;

                _fullscreen = false;
            }
        }

        // =========================
        // KLAVYE
        // =========================

        private void MainForm_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();

                e.Handled = true;
            }

            if (e.KeyCode == Keys.Space)
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Pause();
                }
                else
                {
                    _mediaPlayer.Play();
                }

                e.Handled = true;
            }

            if (e.KeyCode == Keys.Escape &&
                _fullscreen)
            {
                ToggleFullscreen();

                e.Handled = true;
            }
        }

        // =========================
        // DURUM
        // =========================

        private void UpdateStatus()
        {
            _statusLabel.Text =
                _videos.Count +
                " video listede.";
        }

        // =========================
        // KAPAT
        // =========================

        private void MainForm_FormClosing(
            object? sender,
            FormClosingEventArgs e)
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
        }
    }
}
