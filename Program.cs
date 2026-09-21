using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace VideoPlayer
{
    public class VideoItem
    {
        public string FilePath { get; set; } = "";
        public int IntroStart { get; set; } = 0;
        public int IntroEnd { get; set; } = 0;

        public string Title
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Core.Initialize();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        // ---------------------------------------------------------
        // LIBVLC
        // ---------------------------------------------------------

        private LibVLC _libVLC = null!;
        private MediaPlayer _mediaPlayer = null!;
        private VideoView _videoView = null!;

        private Media? _currentMedia;

        // ---------------------------------------------------------
        // VIDEO
        // ---------------------------------------------------------

        private readonly List<VideoItem> _videos = new();

        private int _currentIndex = -1;

        // ---------------------------------------------------------
        // UI
        // ---------------------------------------------------------

        private Panel _videoContainer = null!;
        private Panel _topBar = null!;
        private Panel _bottomBar = null!;
        private Panel _sideBar = null!;

        private Label _titleLabel = null!;

        private Button _playPauseButton = null!;
        private Button _previousButton = null!;
        private Button _nextButton = null!;
        private Button _muteButton = null!;
        private Button _fullscreenButton = null!;

        private TrackBar _progressBar = null!;
        private TrackBar _volumeBar = null!;

        private Label _currentTimeLabel = null!;
        private Label _durationLabel = null!;

        private Button _skipIntroButton = null!;

        private Panel _nextEpisodePanel = null!;
        private Label _nextEpisodeTitle = null!;
        private Label _nextEpisodeCountdown = null!;
        private Button _watchNowButton = null!;

        private FlowLayoutPanel _videoList = null!;

        private Button _addButton = null!;
        private Button _deleteButton = null!;
        private Button _upButton = null!;
        private Button _downButton = null!;

        // ---------------------------------------------------------
        // TIMERS
        // ---------------------------------------------------------

        private System.Windows.Forms.Timer _updateTimer = null!;
        private System.Windows.Forms.Timer _mouseTimer = null!;

        private int _mouseIdleSeconds = 0;

        // ---------------------------------------------------------
        // FULLSCREEN
        // ---------------------------------------------------------

        private bool _isFullscreen = true;

        private FormBorderStyle _oldBorderStyle;
        private FormWindowState _oldWindowState;
        private Rectangle _oldBounds;

        // ---------------------------------------------------------
        // COLORS
        // ---------------------------------------------------------

        private readonly Color BackgroundColor =
            Color.FromArgb(10, 10, 12);

        private readonly Color PanelColor =
            Color.FromArgb(20, 20, 24);

        private readonly Color ButtonColor =
            Color.FromArgb(31, 31, 37);

        private readonly Color ButtonHoverColor =
            Color.FromArgb(45, 45, 52);

        private readonly Color AccentColor =
            Color.FromArgb(220, 35, 45);

        private readonly Color TextColor =
            Color.White;

        private readonly Color SecondaryTextColor =
            Color.FromArgb(170, 170, 175);

        // ---------------------------------------------------------
        // CONSTRUCTOR
        // ---------------------------------------------------------

        public MainForm()
        {
            Text = "OLCAY KILIÇ TV";

            BackColor = BackgroundColor;
            ForeColor = TextColor;

            MinimumSize = new Size(1000, 600);

            KeyPreview = true;

            InitializeVLC();
            InitializeUI();
            InitializeTimers();

            KeyDown += MainForm_KeyDown;
            MouseMove += MainForm_MouseMove;

            StartFullscreen();
        }

        // =========================================================
        // VLC
        // =========================================================

        private void InitializeVLC()
        {
            _libVLC = new LibVLC(
                "--no-video-title-show",
                "--no-osd"
            );

            _mediaPlayer = new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            _mediaPlayer.Playing += MediaPlayer_Playing;
            _mediaPlayer.Paused += MediaPlayer_Paused;
        }

        // =========================================================
        // UI
        // =========================================================

        private void InitializeUI()
        {
            // -----------------------------------------------------
            // VIDEO CONTAINER
            // -----------------------------------------------------

            _videoContainer = new Panel();

            _videoContainer.Dock = DockStyle.Fill;
            _videoContainer.BackColor = Color.Black;

            Controls.Add(_videoContainer);

            // -----------------------------------------------------
            // VIDEO
            // -----------------------------------------------------

            _videoView = new VideoView();

            _videoView.Dock = DockStyle.Fill;
            _videoView.MediaPlayer = _mediaPlayer;
            _videoView.BackColor = Color.Black;

            _videoContainer.Controls.Add(_videoView);

            // -----------------------------------------------------
            // TOP BAR
            // -----------------------------------------------------

            CreateTopBar();

            // -----------------------------------------------------
            // BOTTOM BAR
            // -----------------------------------------------------

            CreateBottomBar();

            // -----------------------------------------------------
            // SIDE BAR
            // -----------------------------------------------------

            CreateSideBar();

            // -----------------------------------------------------
            // INTRO SKIP
            // -----------------------------------------------------

            CreateSkipIntroButton();

            // -----------------------------------------------------
            // NEXT EPISODE
            // -----------------------------------------------------

            CreateNextEpisodePanel();

            // -----------------------------------------------------
            // Z ORDER
            // -----------------------------------------------------

            _topBar.BringToFront();
            _bottomBar.BringToFront();
            _sideBar.BringToFront();
            _skipIntroButton.BringToFront();
            _nextEpisodePanel.BringToFront();
        }

        // =========================================================
        // TOP BAR
        // =========================================================

        private void CreateTopBar()
        {
            _topBar = new Panel();

            _topBar.Dock = DockStyle.Top;
            _topBar.Height = 70;
            _topBar.BackColor = Color.FromArgb(235, 10, 10, 12);

            _videoContainer.Controls.Add(_topBar);

            _titleLabel = new Label();

            _titleLabel.Text = "OLCAY KILIÇ TV";
            _titleLabel.ForeColor = Color.White;
            _titleLabel.Font = new Font(
                "Segoe UI",
                15,
                FontStyle.Bold
            );

            _titleLabel.AutoSize = true;
            _titleLabel.Location = new Point(25, 21);

            _topBar.Controls.Add(_titleLabel);

            Label liveLabel = new Label();

            liveLabel.Text = "● OYNATILIYOR";
            liveLabel.ForeColor = AccentColor;
            liveLabel.Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold
            );

            liveLabel.AutoSize = true;
            liveLabel.Location = new Point(220, 25);

            _topBar.Controls.Add(liveLabel);
        }

        // =========================================================
        // BOTTOM BAR
        // =========================================================

        private void CreateBottomBar()
        {
            _bottomBar = new Panel();

            _bottomBar.Dock = DockStyle.Bottom;
            _bottomBar.Height = 105;
            _bottomBar.BackColor = Color.FromArgb(245, 8, 8, 10);

            _videoContainer.Controls.Add(_bottomBar);

            // -----------------------------------------------------
            // PROGRESS
            // -----------------------------------------------------

            _progressBar = new TrackBar();

            _progressBar.Minimum = 0;
            _progressBar.Maximum = 1000;
            _progressBar.Value = 0;

            _progressBar.TickStyle = TickStyle.None;

            _progressBar.Height = 25;

            _progressBar.Dock = DockStyle.Top;

            _progressBar.Margin = new Padding(15, 0, 15, 0);

            _progressBar.Scroll += ProgressBar_Scroll;

            _bottomBar.Controls.Add(_progressBar);

            // -----------------------------------------------------
            // CURRENT TIME
            // -----------------------------------------------------

            _currentTimeLabel = new Label();

            _currentTimeLabel.Text = "00:00";

            _currentTimeLabel.ForeColor = Color.White;

            _currentTimeLabel.Font =
                new Font("Segoe UI", 9);

            _currentTimeLabel.AutoSize = true;

            _currentTimeLabel.Location =
                new Point(18, 30);

            _bottomBar.Controls.Add(_currentTimeLabel);

            // -----------------------------------------------------
            // DURATION
            // -----------------------------------------------------

            _durationLabel = new Label();

            _durationLabel.Text = "00:00";

            _durationLabel.ForeColor =
                Color.FromArgb(180, 180, 185);

            _durationLabel.Font =
                new Font("Segoe UI", 9);

            _durationLabel.AutoSize = true;

            _durationLabel.Location =
                new Point(70, 30);

            _bottomBar.Controls.Add(_durationLabel);

            // -----------------------------------------------------
            // PREVIOUS
            // -----------------------------------------------------

            _previousButton =
                CreatePlayerButton("⏮", 125);

            _previousButton.Click +=
                delegate
                {
                    PlayPreviousVideo();
                };

            // -----------------------------------------------------
            // PLAY
            // -----------------------------------------------------

            _playPauseButton =
                CreatePlayerButton("▶", 175);

            _playPauseButton.Width = 55;
            _playPauseButton.Height = 38;

            _playPauseButton.Click +=
                delegate
                {
                    TogglePlayPause();
                };

            // -----------------------------------------------------
            // NEXT
            // -----------------------------------------------------

            _nextButton =
                CreatePlayerButton("⏭", 240);

            _nextButton.Click +=
                delegate
                {
                    PlayNextVideo();
                };

            // -----------------------------------------------------
            // VOLUME
            // -----------------------------------------------------

            _muteButton =
                CreatePlayerButton("🔊", 300);

            _muteButton.Click +=
                delegate
                {
                    ToggleMute();
                };

            // -----------------------------------------------------
            // VOLUME SLIDER
            // -----------------------------------------------------

            _volumeBar = new TrackBar();

            _volumeBar.Minimum = 0;
            _volumeBar.Maximum = 100;

            _volumeBar.Value = 80;

            _volumeBar.TickStyle =
                TickStyle.None;

            _volumeBar.Width = 100;
            _volumeBar.Height = 30;

            _volumeBar.Location =
                new Point(350, 33);

            _volumeBar.Scroll +=
                delegate
                {
                    _mediaPlayer.Volume =
                        _volumeBar.Value;
                };

            _bottomBar.Controls.Add(_volumeBar);

            _mediaPlayer.Volume = 80;

            // -----------------------------------------------------
            // FULLSCREEN
            // -----------------------------------------------------

            _fullscreenButton =
                CreatePlayerButton("⛶", 470);

            _fullscreenButton.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Top;

            _fullscreenButton.Click +=
                delegate
                {
                    ToggleFullscreen();
                };
        }

        private Button CreatePlayerButton(
            string text,
            int x)
        {
            Button button = new Button();

            button.Text = text;

            button.Location =
                new Point(x, 28);

            button.Size =
                new Size(45, 38);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 0;

            button.BackColor =
                ButtonColor;

            button.ForeColor =
                Color.White;

            button.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            button.Cursor =
                Cursors.Hand;

            button.MouseEnter +=
                delegate
                {
                    button.BackColor =
                        ButtonHoverColor;
                };

            button.MouseLeave +=
                delegate
                {
                    button.BackColor =
                        ButtonColor;
                };

            _bottomBar.Controls.Add(button);

            return button;
        }

        // =========================================================
        // SIDE BAR
        // =========================================================

        private void CreateSideBar()
        {
            _sideBar = new Panel();

            _sideBar.Name = "SideBar";

            _sideBar.Dock = DockStyle.Right;

            _sideBar.Width = 380;

            _sideBar.BackColor =
                Color.FromArgb(18, 18, 22);

            Controls.Add(_sideBar);

            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------

            Label header = new Label();

            header.Text = "BÖLÜMLER";

            header.ForeColor = Color.White;

            header.Font =
                new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold);

            header.Dock =
                DockStyle.Top;

            header.Height = 55;

            header.Padding =
                new Padding(20, 15, 0, 0);

            _sideBar.Controls.Add(header);

            // -----------------------------------------------------
            // VIDEO LIST
            // -----------------------------------------------------

            _videoList =
                new FlowLayoutPanel();

            _videoList.Name =
                "VideoList";

            _videoList.Dock =
                DockStyle.Fill;

            _videoList.FlowDirection =
                FlowDirection.TopDown;

            _videoList.WrapContents =
                false;

            _videoList.AutoScroll =
                true;

            _videoList.Padding =
                new Padding(10);

            _videoList.BackColor =
                Color.FromArgb(18, 18, 22);

            _sideBar.Controls.Add(
                _videoList);

            // -----------------------------------------------------
            // BOTTOM BUTTONS
            // -----------------------------------------------------

            Panel buttons =
                new Panel();

            buttons.Dock =
                DockStyle.Bottom;

            buttons.Height = 125;

            buttons.BackColor =
                Color.FromArgb(18, 18, 22);

            _sideBar.Controls.Add(buttons);

            _addButton =
                CreateSideButton(
                    "+ VİDEO EKLE",
                    10,
                    10,
                    350,
                    35);

            _addButton.Click +=
                AddButton_Click;

            buttons.Controls.Add(
                _addButton);

            _deleteButton =
                CreateSideButton(
                    "SİL",
                    10,
                    55,
                    100,
                    35);

            _deleteButton.Click +=
                DeleteButton_Click;

            buttons.Controls.Add(
                _deleteButton);

            _upButton =
                CreateSideButton(
                    "▲",
                    120,
                    55,
                    100,
                    35);

            _upButton.Click +=
                UpButton_Click;

            buttons.Controls.Add(
                _upButton);

            _downButton =
                CreateSideButton(
                    "▼",
                    230,
                    55,
                    100,
                    35);

            _downButton.Click +=
                DownButton_Click;

            buttons.Controls.Add(
                _downButton);

            _sideBar.BringToFront();
        }

        private Button CreateSideButton(
            string text,
            int x,
            int y,
            int width,
            int height)
        {
            Button button =
                new Button();

            button.Text = text;

            button.Location =
                new Point(x, y);

            button.Size =
                new Size(width, height);

            button.FlatStyle =
                FlatStyle.Flat;

            button.FlatAppearance.BorderSize =
                0;

            button.BackColor =
                ButtonColor;

            button.ForeColor =
                Color.White;

            button.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            button.Cursor =
                Cursors.Hand;

            return button;
        }

        // =========================================================
        // VIDEO SLOTS
        // =========================================================

        private void RefreshVideoSlots()
        {
            if (_videoList == null)
                return;

            _videoList.Controls.Clear();

            for (
                int i = 0;
                i < _videos.Count;
                i++)
            {
                CreateVideoSlot(
                    _videoList,
                    i);
            }
        }

        private void CreateVideoSlot(
            FlowLayoutPanel list,
            int index)
        {
            VideoItem item =
                _videos[index];

            Panel slot =
                new Panel();

            slot.Width =
                345;

            slot.Height =
                145;

            slot.BackColor =
                Color.FromArgb(27, 27, 32);

            slot.Margin =
                new Padding(0, 0, 0, 10);

            // -----------------------------------------------------
            // EPISODE NUMBER
            // -----------------------------------------------------

            Label number =
                new Label();

            number.Text =
                (index + 1).ToString();

            number.ForeColor =
                AccentColor;

            number.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            number.Location =
                new Point(12, 12);

            number.AutoSize =
                true;

            slot.Controls.Add(number);

            // -----------------------------------------------------
            // TITLE
            // -----------------------------------------------------

            Label title =
                new Label();

            title.Text =
                item.Title;

            title.ForeColor =
                Color.White;

            title.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            title.Location =
                new Point(45, 12);

            title.Size =
                new Size(280, 22);

            title.AutoEllipsis =
                true;

            slot.Controls.Add(title);

            // -----------------------------------------------------
            // INTRO START
            // -----------------------------------------------------

            Label startLabel =
                new Label();

            startLabel.Text =
                "Intro Başlangıç";

            startLabel.ForeColor =
                SecondaryTextColor;

            startLabel.Location =
                new Point(12, 48);

            startLabel.AutoSize =
                true;

            slot.Controls.Add(startLabel);

            TextBox startBox =
                new TextBox();

            startBox.Text =
                FormatSeconds(
                    item.IntroStart);

            startBox.Location =
                new Point(140, 45);

            startBox.Size =
                new Size(85, 23);

            slot.Controls.Add(startBox);

            // -----------------------------------------------------
            // INTRO END
            // -----------------------------------------------------

            Label endLabel =
                new Label();

            endLabel.Text =
                "Intro Bitiş";

            endLabel.ForeColor =
                SecondaryTextColor;

            endLabel.Location =
                new Point(12, 82);

            endLabel.AutoSize =
                true;

            slot.Controls.Add(endLabel);

            TextBox endBox =
                new TextBox();

            endBox.Text =
                FormatSeconds(
                    item.IntroEnd);

            endBox.Location =
                new Point(140, 79);

            endBox.Size =
                new Size(85, 23);

            slot.Controls.Add(endBox);

            // -----------------------------------------------------
            // SAVE
            // -----------------------------------------------------

            Button save =
                new Button();

            save.Text =
                "KAYDET";

            save.Location =
                new Point(235, 45);

            save.Size =
                new Size(95, 57);

            save.FlatStyle =
                FlatStyle.Flat;

            save.FlatAppearance.BorderSize =
                0;

            save.BackColor =
                ButtonColor;

            save.ForeColor =
                Color.White;

            save.Font =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            save.Click +=
                delegate
                {
                    item.IntroStart =
                        ParseTime(
                            startBox.Text);

                    item.IntroEnd =
                        ParseTime(
                            endBox.Text);

                    MessageBox.Show(
                        "Intro ayarları kaydedildi.",
                        "OLCAY KILIÇ TV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                };

            slot.Controls.Add(save);

            // -----------------------------------------------------
            // PLAY
            // -----------------------------------------------------

            Button play =
                new Button();

            play.Text =
                "▶ OYNAT";

            play.Location =
                new Point(12, 112);

            play.Size =
                new Size(318, 28);

            play.FlatStyle =
                FlatStyle.Flat;

            play.FlatAppearance.BorderSize =
                0;

            play.BackColor =
                ButtonColor;

            play.ForeColor =
                Color.White;

            play.Font =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            play.Click +=
                delegate
                {
                    PlayVideo(index);
                };

            slot.Controls.Add(play);

            list.Controls.Add(slot);
        }

        // =========================================================
        // TIME
        // =========================================================

        private string FormatSeconds(
            int seconds)
        {
            if (seconds < 0)
                seconds = 0;

            TimeSpan time =
                TimeSpan.FromSeconds(seconds);

            if (time.Hours > 0)
            {
                return time.ToString(
                    @"hh\:mm\:ss");
            }

            return time.ToString(
                @"mm\:ss");
        }

        private int ParseTime(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Trim();

            if (int.TryParse(
                value,
                out int seconds))
            {
                return Math.Max(
                    0,
                    seconds);
            }

            string[] parts =
                value.Split(':');

            try
            {
                if (parts.Length == 2)
                {
                    int minutes =
                        int.Parse(parts[0]);

                    int sec =
                        int.Parse(parts[1]);

                    return
                        Math.Max(
                            0,
                            minutes * 60 + sec);
                }

                if (parts.Length == 3)
                {
                    int hours =
                        int.Parse(parts[0]);

                    int minutes =
                        int.Parse(parts[1]);

                    int sec =
                        int.Parse(parts[2]);

                    return
                        Math.Max(
                            0,
                            hours * 3600 +
                            minutes * 60 +
                            sec);
                }
            }
            catch
            {
            }

            return 0;
        }

        // =========================================================
        // PLAY VIDEO
        // =========================================================

        private void PlayVideo(
            int index)
        {
            if (index < 0 ||
                index >= _videos.Count)
                return;

            VideoItem item =
                _videos[index];

            if (!File.Exists(
                item.FilePath))
            {
                MessageBox.Show(
                    "Video bulunamadı:\n\n" +
                    item.FilePath,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _currentIndex =
                index;

            try
            {
                _mediaPlayer.Stop();

                if (_currentMedia != null)
                {
                    _currentMedia.Dispose();
                    _currentMedia = null;
                }

                _currentMedia =
                    new Media(
                        _libVLC,
                        new Uri(
                            item.FilePath));

                _mediaPlayer.Play(
                    _currentMedia);

                _titleLabel.Text =
                    item.Title;

                _playPauseButton.Text =
                    "❚❚";

                _progressBar.Value = 0;

                _currentTimeLabel.Text =
                    "00:00";

                _durationLabel.Text =
                    "00:00";

                HideNextEpisodeNotice();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Video oynatma hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // PLAY / PAUSE
        // =========================================================

        private void TogglePlayPause()
        {
            if (_mediaPlayer == null)
                return;

            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();

                _playPauseButton.Text =
                    "▶";
            }
            else
            {
                _mediaPlayer.Play();

                _playPauseButton.Text =
                    "❚❚";
            }
        }

        // =========================================================
        // NEXT
        // =========================================================

        private void PlayNextVideo()
        {
            if (_videos.Count == 0)
                return;

            int next =
                _currentIndex + 1;

            if (next >= _videos.Count)
                next = 0;

            PlayVideo(next);
        }

        // =========================================================
        // PREVIOUS
        // =========================================================

        private void PlayPreviousVideo()
        {
            if (_videos.Count == 0)
                return;

            int previous =
                _currentIndex - 1;

            if (previous < 0)
                previous =
                    _videos.Count - 1;

            PlayVideo(previous);
        }

        // =========================================================
        // END REACHED
        // =========================================================

        private void MediaPlayer_EndReached(
            object? sender,
            EventArgs e)
        {
            BeginInvoke(
                new Action(
                    delegate
                    {
                        PlayNextVideo();
                    }));
        }

        private void MediaPlayer_Playing(
            object? sender,
            EventArgs e)
        {
            BeginInvoke(
                new Action(
                    delegate
                    {
                        _playPauseButton.Text =
                            "❚❚";
                    }));
        }

        private void MediaPlayer_Paused(
            object? sender,
            EventArgs e)
        {
            BeginInvoke(
                new Action(
                    delegate
                    {
                        _playPauseButton.Text =
                            "▶";
                    }));
        }

        // =========================================================
        // TIMERS
        // =========================================================

        private void InitializeTimers()
        {
            _updateTimer =
                new System.Windows.Forms.Timer();

            _updateTimer.Interval =
                250;

            _updateTimer.Tick +=
                UpdateTimer_Tick;

            _updateTimer.Start();

            _mouseTimer =
                new System.Windows.Forms.Timer();

            _mouseTimer.Interval =
                1000;

            _mouseTimer.Tick +=
                MouseTimer_Tick;

            _mouseTimer.Start();
        }

        // =========================================================
        // UPDATE PLAYER
        // =========================================================

        private void UpdateTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (_mediaPlayer == null)
                return;

            if (!_mediaPlayer.IsPlaying)
                return;

            long time =
                _mediaPlayer.Time;

            long length =
                _mediaPlayer.Length;

            if (length > 0)
            {
                double position =
                    (double)time /
                    length;

                int value =
                    (int)(
                        position * 1000);

                value =
                    Math.Max(
                        0,
                        Math.Min(
                            1000,
                            value));

                if (!_progressBar.Focused)
                {
                    _progressBar.Value =
                        value;
                }

                _currentTimeLabel.Text =
                    FormatMilliseconds(time);

                _durationLabel.Text =
                    FormatMilliseconds(length);

                CheckIntro(
                    time);

                CheckNextEpisode(
                    time,
                    length);
            }
        }

        private string FormatMilliseconds(
            long milliseconds)
        {
            if (milliseconds < 0)
                milliseconds = 0;

            TimeSpan t =
                TimeSpan.FromMilliseconds(
                    milliseconds);

            if (t.Hours > 0)
            {
                return t.ToString(
                    @"hh\:mm\:ss");
            }

            return t.ToString(
                @"mm\:ss");
        }

        // =========================================================
        // PROGRESS
        // =========================================================

        private void ProgressBar_Scroll(
            object? sender,
            EventArgs e)
        {
            if (_mediaPlayer == null)
                return;

            long length =
                _mediaPlayer.Length;

            if (length <= 0)
                return;

            double position =
                _progressBar.Value /
                1000.0;

            _mediaPlayer.Time =
                (long)(
                    length * position);
        }

        // =========================================================
        // INTRO
        // =========================================================

        private void CreateSkipIntroButton()
        {
            _skipIntroButton =
                new Button();

            _skipIntroButton.Text =
                "⏩  INTRO'YU ATLA";

            _skipIntroButton.Size =
                new Size(180, 45);

            _skipIntroButton.BackColor =
                Color.FromArgb(
                    210,
                    20,
                    20,
                    24);

            _skipIntroButton.ForeColor =
                Color.White;

            _skipIntroButton.FlatStyle =
                FlatStyle.Flat;

            _skipIntroButton.FlatAppearance.BorderColor =
                AccentColor;

            _skipIntroButton.FlatAppearance.BorderSize =
                1;

            _skipIntroButton.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            _skipIntroButton.Cursor =
                Cursors.Hand;

            _skipIntroButton.Visible =
                false;

            _skipIntroButton.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Bottom;

            _skipIntroButton.Location =
                new Point(
                    ClientSize.Width - 215,
                    ClientSize.Height - 170);

            _skipIntroButton.Click +=
                SkipIntroButton_Click;

            Controls.Add(
                _skipIntroButton);

            _skipIntroButton.BringToFront();
        }

        private void CheckIntro(
            long time)
        {
            if (_currentIndex < 0 ||
                _currentIndex >= _videos.Count)
                return;

            VideoItem item =
                _videos[_currentIndex];

            long currentSeconds =
                time / 1000;

            if (item.IntroEnd > item.IntroStart &&
                currentSeconds >= item.IntroStart &&
                currentSeconds < item.IntroEnd)
            {
                _skipIntroButton.Visible =
                    true;
            }
            else
            {
                _skipIntroButton.Visible =
                    false;
            }
        }

        private void SkipIntroButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex < 0 ||
                _currentIndex >= _videos.Count)
                return;

            VideoItem item =
                _videos[_currentIndex];

            if (item.IntroEnd <= 0)
                return;

            _mediaPlayer.Time =
                item.IntroEnd * 1000L;

            _skipIntroButton.Visible =
                false;
        }

        // =========================================================
        // NEXT EPISODE
        // =========================================================

        private void CreateNextEpisodePanel()
        {
            _nextEpisodePanel =
                new Panel();

            _nextEpisodePanel.Size =
                new Size(330, 145);

            _nextEpisodePanel.BackColor =
                Color.FromArgb(
                    235,
                    18,
                    18,
                    22);

            _nextEpisodePanel.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Bottom;

            _nextEpisodePanel.Visible =
                false;

            Controls.Add(
                _nextEpisodePanel);

            Label header =
                new Label();

            header.Text =
                "SONRAKİ BÖLÜM";

            header.ForeColor =
                AccentColor;

            header.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            header.Location =
                new Point(20, 12);

            header.AutoSize =
                true;

            _nextEpisodePanel.Controls.Add(
                header);

            _nextEpisodeTitle =
                new Label();

            _nextEpisodeTitle.Text =
                "Sonraki bölüm";

            _nextEpisodeTitle.ForeColor =
                Color.White;

            _nextEpisodeTitle.Font =
                new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold);

            _nextEpisodeTitle.Location =
                new Point(20, 38);

            _nextEpisodeTitle.Size =
                new Size(290, 30);

            _nextEpisodePanel.Controls.Add(
                _nextEpisodeTitle);

            _nextEpisodeCountdown =
                new Label();

            _nextEpisodeCountdown.Text =
                "10";

            _nextEpisodeCountdown.ForeColor =
                Color.White;

            _nextEpisodeCountdown.Font =
                new Font(
                    "Segoe UI",
                    16,
                    FontStyle.Bold);

            _nextEpisodeCountdown.Location =
                new Point(20, 70);

            _nextEpisodeCountdown.AutoSize =
                true;

            _nextEpisodePanel.Controls.Add(
                _nextEpisodeCountdown);

            _watchNowButton =
                new Button();

            _watchNowButton.Text =
                "ŞİMDİ İZLE  ▶";

            _watchNowButton.Location =
                new Point(155, 72);

            _watchNowButton.Size =
                new Size(145, 42);

            _watchNowButton.FlatStyle =
                FlatStyle.Flat;

            _watchNowButton.FlatAppearance.BorderSize =
                0;

            _watchNowButton.BackColor =
                AccentColor;

            _watchNowButton.ForeColor =
                Color.White;

            _watchNowButton.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            _watchNowButton.Click +=
                delegate
                {
                    PlayNextVideo();
                };

            _nextEpisodePanel.Controls.Add(
                _watchNowButton);

            _nextEpisodePanel.BringToFront();
        }

        private void CheckNextEpisode(
            long time,
            long length)
        {
            if (_currentIndex < 0 ||
                _currentIndex >= _videos.Count)
                return;

            long remaining =
                length - time;

            if (remaining <= 10000 &&
                remaining > 0 &&
                _videos.Count > 1)
            {
                ShowNextEpisodeNotice(
                    (int)Math.Ceiling(
                        remaining / 1000.0));
            }
            else
            {
                HideNextEpisodeNotice();
            }
        }

        private void ShowNextEpisodeNotice(
            int seconds)
        {
            int next =
                _currentIndex + 1;

            if (next >= _videos.Count)
                next = 0;

            _nextEpisodeTitle.Text =
                _videos[next].Title;

            _nextEpisodeCountdown.Text =
                "⏭ " +
                seconds +
                " saniye sonra başlıyor";

            _nextEpisodePanel.Visible =
                true;

            _nextEpisodePanel.BringToFront();
        }

        private void HideNextEpisodeNotice()
        {
            if (_nextEpisodePanel != null)
            {
                _nextEpisodePanel.Visible =
                    false;
            }
        }

        // =========================================================
        // VOLUME
        // =========================================================

        private void ToggleMute()
        {
            if (_mediaPlayer.Mute)
            {
                _mediaPlayer.Mute =
                    false;

                _muteButton.Text =
                    "🔊";
            }
            else
            {
                _mediaPlayer.Mute =
                    true;

                _muteButton.Text =
                    "🔇";
            }
        }

        // =========================================================
        // ADD VIDEO
        // =========================================================

        private void AddButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_videos.Count >= 20)
            {
                MessageBox.Show(
                    "En fazla 20 video ekleyebilirsin.",
                    "OLCAY KILIÇ TV");

                return;
            }

            OpenFileDialog dialog =
                new OpenFileDialog();

            dialog.Title =
                "Videoları Seç";

            dialog.Filter =
                "Video Dosyaları|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm|Tüm Dosyalar|*.*";

            dialog.Multiselect =
                true;

            if (dialog.ShowDialog() !=
                DialogResult.OK)
                return;

            foreach (
                string file in
                dialog.FileNames)
            {
                if (_videos.Count >= 20)
                    break;

                _videos.Add(
                    new VideoItem
                    {
                        FilePath = file,
                        IntroStart = 0,
                        IntroEnd = 0
                    });
            }

            RefreshVideoSlots();
        }

        // =========================================================
        // DELETE
        // =========================================================

        private void DeleteButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex < 0 ||
                _currentIndex >= _videos.Count)
                return;

            _videos.RemoveAt(
                _currentIndex);

            _currentIndex = -1;

            _mediaPlayer.Stop();

            RefreshVideoSlots();
        }

        // =========================================================
        // UP
        // =========================================================

        private void UpButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex <= 0)
                return;

            VideoItem temp =
                _videos[_currentIndex];

            _videos[_currentIndex] =
                _videos[_currentIndex - 1];

            _videos[_currentIndex - 1] =
                temp;

            _currentIndex--;

            RefreshVideoSlots();
        }

        // =========================================================
        // DOWN
        // =========================================================

        private void DownButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex < 0 ||
                _currentIndex >=
                _videos.Count - 1)
                return;

            VideoItem temp =
                _videos[_currentIndex];

            _videos[_currentIndex] =
                _videos[_currentIndex + 1];

            _videos[_currentIndex + 1] =
                temp;

            _currentIndex++;

            RefreshVideoSlots();
        }

        // =========================================================
        // FULLSCREEN
        // =========================================================

        private void StartFullscreen()
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

            TopMost = false;

            _isFullscreen = true;

            _sideBar.Visible =
                false;

            _fullscreenButton.Text =
                "⛶";

            UpdateOverlayPositions();
        }

        private void EnterFullscreen()
        {
            if (_isFullscreen)
                return;

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

            _isFullscreen = true;

            _sideBar.Visible =
                false;

            _bottomBar.Visible =
                true;

            _topBar.Visible =
                true;

            UpdateOverlayPositions();
        }

        private void ExitFullscreen()
        {
            if (!_isFullscreen)
                return;

            FormBorderStyle =
                FormBorderStyle.Sizable;

            WindowState =
                FormWindowState.Normal;

            Bounds =
                _oldBounds;

            _isFullscreen = false;

            _sideBar.Visible =
                true;

            _topBar.Visible =
                true;

            _bottomBar.Visible =
                true;

            UpdateOverlayPositions();

            ShowMouseControls();
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
                ExitFullscreen();
            else
                EnterFullscreen();
        }

        private void UpdateOverlayPositions()
        {
            if (_skipIntroButton == null)
                return;

            _skipIntroButton.Location =
                new Point(
                    ClientSize.Width -
                    _skipIntroButton.Width -
                    25,
                    ClientSize.Height -
                    175);

            if (_nextEpisodePanel != null)
            {
                _nextEpisodePanel.Location =
                    new Point(
                        ClientSize.Width -
                        _nextEpisodePanel.Width -
                        25,
                        ClientSize.Height -
                        330);
            }
        }

        // =========================================================
        // MOUSE
        // =========================================================

        private void MainForm_MouseMove(
            object? sender,
            MouseEventArgs e)
        {
            ShowMouseControls();
        }

        private void MouseTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (!_isFullscreen)
                return;

            _mouseIdleSeconds++;

            if (_mouseIdleSeconds >= 3)
            {
                HideMouseControls();
            }
        }

        private void ShowMouseControls()
        {
            _mouseIdleSeconds = 0;

            if (!_isFullscreen)
                return;

            _bottomBar.Visible =
                true;

            _topBar.Visible =
                true;

            Cursor =
                Cursors.Default;

            _bottomBar.BringToFront();
            _topBar.BringToFront();

            if (_skipIntroButton.Visible)
                _skipIntroButton.BringToFront();

            if (_nextEpisodePanel.Visible)
                _nextEpisodePanel.BringToFront();
        }

        private void HideMouseControls()
        {
            if (!_isFullscreen)
                return;

            _bottomBar.Visible =
                false;

            _topBar.Visible =
                false;

            Cursor =
                Cursors.None;
        }

        // =========================================================
        // KEYBOARD
        // =========================================================

        private void MainForm_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            // ESC
            if (e.KeyCode ==
                Keys.Escape)
            {
                if (_isFullscreen)
                {
                    ExitFullscreen();
                }

                return;
            }

            // F11
            if (e.KeyCode ==
                Keys.F11)
            {
                ToggleFullscreen();
                return;
            }

            // SPACE
            if (e.KeyCode ==
                Keys.Space)
            {
                TogglePlayPause();
                e.SuppressKeyPress = true;
                return;
            }

            // LEFT
            if (e.KeyCode ==
                Keys.Left)
            {
                if (_mediaPlayer.Time > 10000)
                    _mediaPlayer.Time -= 10000;

                return;
            }

            // RIGHT
            if (e.KeyCode ==
                Keys.Right)
            {
                _mediaPlayer.Time +=
                    10000;

                return;
            }
        }

        // =========================================================
        // RESIZE
        // =========================================================

        protected override void OnResize(
            EventArgs e)
        {
            base.OnResize(e);

            UpdateOverlayPositions();
        }

        // =========================================================
        // CLOSE
        // =========================================================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            try
            {
                _updateTimer?.Stop();
                _mouseTimer?.Stop();

                _mediaPlayer?.Stop();

                _currentMedia?.Dispose();

                _mediaPlayer?.Dispose();

                _libVLC?.Dispose();
            }
            catch
            {
            }

            base.OnFormClosed(e);
        }
    }
}
