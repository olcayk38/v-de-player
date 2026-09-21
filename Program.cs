using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
        // =========================================================
        // WINDOWS CURSOR
        // =========================================================

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        private bool _cursorHidden = false;

        // =========================================================
        // VLC
        // =========================================================

        private LibVLC _libVLC = null!;

        private MediaPlayer _mediaPlayer = null!;

        private VideoView _videoView = null!;

        private Media? _currentMedia;

        // =========================================================
        // VIDEOS
        // =========================================================

        private readonly List<VideoItem> _videos =
            new List<VideoItem>();

        private int _currentIndex = -1;

        // =========================================================
        // MAIN UI
        // =========================================================

        private Panel _videoContainer = null!;

        private Panel _topBar = null!;

        private Panel _bottomBar = null!;

        private Panel _sideBar = null!;

        private FlowLayoutPanel _videoList = null!;

        // =========================================================
        // TOP
        // =========================================================

        private Label _titleLabel = null!;

        private Label _liveLabel = null!;

        // =========================================================
        // PLAYER CONTROLS
        // =========================================================

        private Button _previousButton = null!;

        private Button _playPauseButton = null!;

        private Button _nextButton = null!;

        private Button _muteButton = null!;

        private Button _fullscreenButton = null!;

        private TrackBar _progressBar = null!;

        private TrackBar _volumeBar = null!;

        private Label _currentTimeLabel = null!;

        private Label _durationLabel = null!;

        // =========================================================
        // INTRO
        // =========================================================

        private Button _skipIntroButton = null!;

        // =========================================================
        // NEXT EPISODE
        // =========================================================

        private Panel _nextEpisodePanel = null!;

        private Label _nextEpisodeTitle = null!;

        private Label _nextEpisodeCountdown = null!;

        private Button _watchNowButton = null!;

        // =========================================================
        // SIDE BUTTONS
        // =========================================================

        private Button _addButton = null!;

        private Button _deleteButton = null!;

        private Button _upButton = null!;

        private Button _downButton = null!;

        // =========================================================
        // TIMERS
        // =========================================================

        private System.Windows.Forms.Timer _updateTimer = null!;

        private System.Windows.Forms.Timer _mouseTimer = null!;

        private int _mouseIdleSeconds = 0;

        // =========================================================
        // FULLSCREEN
        // =========================================================

        private bool _isFullscreen = false;

        private FormBorderStyle _oldBorderStyle;

        private FormWindowState _oldWindowState;

        private Rectangle _oldBounds;

        // =========================================================
        // COLORS
        // =========================================================

        private readonly Color BackgroundColor =
            Color.FromArgb(8, 8, 10);

        private readonly Color PanelColor =
            Color.FromArgb(18, 18, 22);

        private readonly Color CardColor =
            Color.FromArgb(28, 28, 34);

        private readonly Color ButtonColor =
            Color.FromArgb(35, 35, 42);

        private readonly Color ButtonHoverColor =
            Color.FromArgb(52, 52, 60);

        private readonly Color AccentColor =
            Color.FromArgb(220, 30, 42);

        private readonly Color TextColor =
            Color.White;

        private readonly Color SecondaryTextColor =
            Color.FromArgb(165, 165, 172);

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MainForm()
        {
            Text = "OLCAY KILIÇ TV";

            BackColor = BackgroundColor;

            ForeColor = TextColor;

            MinimumSize =
                new Size(1000, 600);

            KeyPreview = true;

            StartPosition =
                FormStartPosition.CenterScreen;

            InitializeVLC();

            InitializeUI();

            InitializeTimers();

            KeyDown += MainForm_KeyDown;

            MouseMove += MainForm_MouseMove;

            Resize += MainForm_Resize;

            StartFullscreen();
        }

        // =========================================================
        // VLC
        // =========================================================

        private void InitializeVLC()
        {
            _libVLC =
                new LibVLC(
                    "--no-video-title-show",
                    "--no-osd");

            _mediaPlayer =
                new MediaPlayer(_libVLC);

            _mediaPlayer.EndReached +=
                MediaPlayer_EndReached;

            _mediaPlayer.Playing +=
                MediaPlayer_Playing;

            _mediaPlayer.Paused +=
                MediaPlayer_Paused;
        }

        // =========================================================
        // UI
        // =========================================================

        private void InitializeUI()
        {
            // -----------------------------------------------------
            // VIDEO AREA
            // -----------------------------------------------------

            _videoContainer =
                new Panel();

            _videoContainer.Dock =
                DockStyle.Fill;

            _videoContainer.BackColor =
                Color.Black;

            Controls.Add(
                _videoContainer);

            // -----------------------------------------------------
            // VIDEO
            // -----------------------------------------------------

            _videoView =
                new VideoView();

            _videoView.Dock =
                DockStyle.Fill;

            _videoView.BackColor =
                Color.Black;

            _videoView.MediaPlayer =
                _mediaPlayer;

            _videoContainer.Controls.Add(
                _videoView);

            // -----------------------------------------------------
            // TOP
            // -----------------------------------------------------

            CreateTopBar();

            // -----------------------------------------------------
            // BOTTOM
            // -----------------------------------------------------

            CreateBottomBar();

            // -----------------------------------------------------
            // SIDEBAR
            // -----------------------------------------------------

            CreateSideBar();

            // -----------------------------------------------------
            // INTRO
            // -----------------------------------------------------

            CreateSkipIntroButton();

            // -----------------------------------------------------
            // NEXT
            // -----------------------------------------------------

            CreateNextEpisodePanel();

            BringAllOverlaysToFront();
        }

        // =========================================================
        // TOP BAR
        // =========================================================

        private void CreateTopBar()
        {
            _topBar =
                new Panel();

            _topBar.Height =
                70;

            _topBar.Dock =
                DockStyle.Top;

            _topBar.BackColor =
                Color.FromArgb(
                    235,
                    8,
                    8,
                    10);

            _videoContainer.Controls.Add(
                _topBar);

            _titleLabel =
                new Label();

            _titleLabel.Text =
                "OLCAY KILIÇ TV";

            _titleLabel.ForeColor =
                Color.White;

            _titleLabel.Font =
                new Font(
                    "Segoe UI",
                    16,
                    FontStyle.Bold);

            _titleLabel.AutoSize =
                true;

            _titleLabel.Location =
                new Point(25, 20);

            _topBar.Controls.Add(
                _titleLabel);

            _liveLabel =
                new Label();

            _liveLabel.Text =
                "● OYNATILIYOR";

            _liveLabel.ForeColor =
                AccentColor;

            _liveLabel.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            _liveLabel.AutoSize =
                true;

            _liveLabel.Location =
                new Point(
                    220,
                    25);

            _topBar.Controls.Add(
                _liveLabel);
        }

        // =========================================================
        // BOTTOM BAR
        // =========================================================

        private void CreateBottomBar()
        {
            _bottomBar =
                new Panel();

            _bottomBar.Height =
                110;

            _bottomBar.Dock =
                DockStyle.Bottom;

            _bottomBar.BackColor =
                Color.FromArgb(
                    245,
                    8,
                    8,
                    10);

            _videoContainer.Controls.Add(
                _bottomBar);

            // -----------------------------------------------------
            // PROGRESS
            // -----------------------------------------------------

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

            _progressBar.Dock =
                DockStyle.Top;

            _progressBar.Height =
                30;

            _progressBar.Margin =
                new Padding(15);

            _progressBar.Scroll +=
                ProgressBar_Scroll;

            _bottomBar.Controls.Add(
                _progressBar);

            // -----------------------------------------------------
            // CURRENT TIME
            // -----------------------------------------------------

            _currentTimeLabel =
                new Label();

            _currentTimeLabel.Text =
                "00:00";

            _currentTimeLabel.ForeColor =
                Color.White;

            _currentTimeLabel.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            _currentTimeLabel.AutoSize =
                true;

            _currentTimeLabel.Location =
                new Point(
                    20,
                    32);

            _bottomBar.Controls.Add(
                _currentTimeLabel);

            // -----------------------------------------------------
            // SLASH
            // -----------------------------------------------------

            Label slash =
                new Label();

            slash.Text =
                "/";

            slash.ForeColor =
                SecondaryTextColor;

            slash.AutoSize =
                true;

            slash.Location =
                new Point(
                    68,
                    32);

            _bottomBar.Controls.Add(
                slash);

            // -----------------------------------------------------
            // DURATION
            // -----------------------------------------------------

            _durationLabel =
                new Label();

            _durationLabel.Text =
                "00:00";

            _durationLabel.ForeColor =
                SecondaryTextColor;

            _durationLabel.Font =
                new Font(
                    "Segoe UI",
                    9);

            _durationLabel.AutoSize =
                true;

            _durationLabel.Location =
                new Point(
                    82,
                    32);

            _bottomBar.Controls.Add(
                _durationLabel);

            // -----------------------------------------------------
            // PREVIOUS
            // -----------------------------------------------------

            _previousButton =
                CreatePlayerButton(
                    "⏮",
                    135);

            _previousButton.Click +=
                delegate
                {
                    PlayPreviousVideo();
                };

            // -----------------------------------------------------
            // PLAY
            // -----------------------------------------------------

            _playPauseButton =
                CreatePlayerButton(
                    "▶",
                    190);

            _playPauseButton.Size =
                new Size(
                    55,
                    40);

            _playPauseButton.Click +=
                delegate
                {
                    TogglePlayPause();
                };

            // -----------------------------------------------------
            // NEXT
            // -----------------------------------------------------

            _nextButton =
                CreatePlayerButton(
                    "⏭",
                    255);

            _nextButton.Click +=
                delegate
                {
                    PlayNextVideo();
                };

            // -----------------------------------------------------
            // MUTE
            // -----------------------------------------------------

            _muteButton =
                CreatePlayerButton(
                    "🔊",
                    315);

            _muteButton.Click +=
                delegate
                {
                    ToggleMute();
                };

            // -----------------------------------------------------
            // VOLUME
            // -----------------------------------------------------

            _volumeBar =
                new TrackBar();

            _volumeBar.Minimum =
                0;

            _volumeBar.Maximum =
                100;

            _volumeBar.Value =
                80;

            _volumeBar.TickStyle =
                TickStyle.None;

            _volumeBar.Size =
                new Size(
                    110,
                    35);

            _volumeBar.Location =
                new Point(
                    365,
                    31);

            _volumeBar.Scroll +=
                delegate
                {
                    _mediaPlayer.Volume =
                        _volumeBar.Value;
                };

            _bottomBar.Controls.Add(
                _volumeBar);

            _mediaPlayer.Volume =
                80;

            // -----------------------------------------------------
            // FULLSCREEN
            // -----------------------------------------------------

            _fullscreenButton =
                CreatePlayerButton(
                    "⛶",
                    490);

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
            Button button =
                new Button();

            button.Text =
                text;

            button.Location =
                new Point(
                    x,
                    30);

            button.Size =
                new Size(
                    48,
                    40);

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

            _bottomBar.Controls.Add(
                button);

            return button;
        }

        // =========================================================
        // SIDEBAR
        // =========================================================

        private void CreateSideBar()
        {
            _sideBar =
                new Panel();

            _sideBar.Name =
                "SideBar";

            _sideBar.Dock =
                DockStyle.Right;

            _sideBar.Width =
                380;

            _sideBar.BackColor =
                Color.FromArgb(
                    17,
                    17,
                    21);

            Controls.Add(
                _sideBar);

            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------

            Label header =
                new Label();

            header.Text =
                "BÖLÜMLER";

            header.ForeColor =
                Color.White;

            header.Font =
                new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold);

            header.Dock =
                DockStyle.Top;

            header.Height =
                55;

            header.Padding =
                new Padding(
                    20,
                    15,
                    0,
                    0);

            _sideBar.Controls.Add(
                header);

            // -----------------------------------------------------
            // BUTTON AREA
            // IMPORTANT: ADD FIRST SO FILL LIST DOES NOT COVER IT
            // -----------------------------------------------------

            Panel buttonPanel =
                new Panel();

            buttonPanel.Dock =
                DockStyle.Bottom;

            buttonPanel.Height =
                125;

            buttonPanel.BackColor =
                Color.FromArgb(
                    17,
                    17,
                    21);

            _sideBar.Controls.Add(
                buttonPanel);

            // -----------------------------------------------------
            // ADD
            // -----------------------------------------------------

            _addButton =
                CreateSideButton(
                    "+ VİDEO EKLE",
                    10,
                    10,
                    350,
                    35);

            _addButton.Click +=
                AddButton_Click;

            buttonPanel.Controls.Add(
                _addButton);

            // -----------------------------------------------------
            // DELETE
            // -----------------------------------------------------

            _deleteButton =
                CreateSideButton(
                    "SİL",
                    10,
                    55,
                    100,
                    35);

            _deleteButton.Click +=
                DeleteButton_Click;

            buttonPanel.Controls.Add(
                _deleteButton);

            // -----------------------------------------------------
            // UP
            // -----------------------------------------------------

            _upButton =
                CreateSideButton(
                    "▲",
                    120,
                    55,
                    100,
                    35);

            _upButton.Click +=
                UpButton_Click;

            buttonPanel.Controls.Add(
                _upButton);

            // -----------------------------------------------------
            // DOWN
            // -----------------------------------------------------

            _downButton =
                CreateSideButton(
                    "▼",
                    230,
                    55,
                    100,
                    35);

            _downButton.Click +=
                DownButton_Click;

            buttonPanel.Controls.Add(
                _downButton);

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
                new Padding(
                    10);

            _videoList.BackColor =
                Color.FromArgb(
                    17,
                    17,
                    21);

            _sideBar.Controls.Add(
                _videoList);

            RefreshVideoSlots();
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

            button.Text =
                text;

            button.Location =
                new Point(
                    x,
                    y);

            button.Size =
                new Size(
                    width,
                    height);

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

            return button;
        }

        // =========================================================
        // VIDEO SLOTS
        // =========================================================

        private void RefreshVideoSlots()
        {
            if (_videoList == null)
                return;

            _videoList.SuspendLayout();

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

            _videoList.ResumeLayout();
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
                155;

            slot.Margin =
                new Padding(
                    0,
                    0,
                    0,
                    10);

            slot.BackColor =
                CardColor;

            // -----------------------------------------------------
            // NUMBER
            // -----------------------------------------------------

            Label number =
                new Label();

            number.Text =
                (index + 1).ToString(
                    "00");

            number.ForeColor =
                AccentColor;

            number.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            number.Location =
                new Point(
                    12,
                    12);

            number.AutoSize =
                true;

            slot.Controls.Add(
                number);

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
                new Point(
                    48,
                    11);

            title.Size =
                new Size(
                    280,
                    25);

            title.AutoEllipsis =
                true;

            slot.Controls.Add(
                title);

            // -----------------------------------------------------
            // INTRO START LABEL
            // -----------------------------------------------------

            Label startLabel =
                new Label();

            startLabel.Text =
                "Intro Başlangıç";

            startLabel.ForeColor =
                SecondaryTextColor;

            startLabel.Font =
                new Font(
                    "Segoe UI",
                    8);

            startLabel.Location =
                new Point(
                    12,
                    48);

            startLabel.AutoSize =
                true;

            slot.Controls.Add(
                startLabel);

            // -----------------------------------------------------
            // START BOX
            // -----------------------------------------------------

            TextBox startBox =
                new TextBox();

            startBox.Text =
                FormatSeconds(
                    item.IntroStart);

            startBox.Location =
                new Point(
                    140,
                    45);

            startBox.Size =
                new Size(
                    82,
                    23);

            slot.Controls.Add(
                startBox);

            // -----------------------------------------------------
            // END LABEL
            // -----------------------------------------------------

            Label endLabel =
                new Label();

            endLabel.Text =
                "Intro Bitiş";

            endLabel.ForeColor =
                SecondaryTextColor;

            endLabel.Font =
                new Font(
                    "Segoe UI",
                    8);

            endLabel.Location =
                new Point(
                    12,
                    80);

            endLabel.AutoSize =
                true;

            slot.Controls.Add(
                endLabel);

            // -----------------------------------------------------
            // END BOX
            // -----------------------------------------------------

            TextBox endBox =
                new TextBox();

            endBox.Text =
                FormatSeconds(
                    item.IntroEnd);

            endBox.Location =
                new Point(
                    140,
                    77);

            endBox.Size =
                new Size(
                    82,
                    23);

            slot.Controls.Add(
                endBox);

            // -----------------------------------------------------
            // SAVE
            // -----------------------------------------------------

            Button save =
                new Button();

            save.Text =
                "KAYDET";

            save.Location =
                new Point(
                    232,
                    45);

            save.Size =
                new Size(
                    98,
                    55);

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

            save.Cursor =
                Cursors.Hand;

            save.Click +=
                delegate
                {
                    item.IntroStart =
                        ParseTime(
                            startBox.Text);

                    item.IntroEnd =
                        ParseTime(
                            endBox.Text);

                    save.Text =
                        "✓ KAYDEDİLDİ";

                    System.Windows.Forms.Timer resetTimer =
                        new System.Windows.Forms.Timer();

                    resetTimer.Interval =
                        1200;

                    resetTimer.Tick +=
                        delegate
                        {
                            save.Text =
                                "KAYDET";

                            resetTimer.Stop();

                            resetTimer.Dispose();
                        };

                    resetTimer.Start();
                };

            slot.Controls.Add(
                save);

            // -----------------------------------------------------
            // PLAY
            // -----------------------------------------------------

            Button play =
                new Button();

            play.Text =
                "▶  OYNAT";

            play.Location =
                new Point(
                    12,
                    112);

            play.Size =
                new Size(
                    318,
                    30);

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

            play.Cursor =
                Cursors.Hand;

            play.Click +=
                delegate
                {
                    PlayVideo(index);
                };

            slot.Controls.Add(
                play);

            list.Controls.Add(
                slot);
        }

        // =========================================================
        // TIME PARSE
        // =========================================================

        private string FormatSeconds(
            int seconds)
        {
            if (seconds < 0)
                seconds = 0;

            TimeSpan time =
                TimeSpan.FromSeconds(
                    seconds);

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
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return 0;
            }

            value =
                value.Trim();

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

                    return Math.Max(
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

                    return Math.Max(
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
            {
                return;
            }

            VideoItem item =
                _videos[index];

            if (!File.Exists(
                item.FilePath))
            {
                MessageBox.Show(
                    "Video bulunamadı:\n\n" +
                    item.FilePath,
                    "OLCAY KILIÇ TV",
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

                _progressBar.Value =
                    0;

                _currentTimeLabel.Text =
                    "00:00";

                _durationLabel.Text =
                    "00:00";

                _skipIntroButton.Visible =
                    false;

                HideNextEpisodeNotice();

                BringAllOverlaysToFront();
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
        // PLAY PAUSE
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
        // VLC EVENTS
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
        // PLAYER UPDATE
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

            double position =
                (double)time /
                length;

            int value =
                (int)(
                    position *
                    1000);

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
                FormatMilliseconds(
                    time);

            _durationLabel.Text =
                FormatMilliseconds(
                    length);

            CheckIntro(time);

            CheckNextEpisode(
                time,
                length);
        }

        private string FormatMilliseconds(
            long milliseconds)
        {
            if (milliseconds < 0)
                milliseconds = 0;

            TimeSpan time =
                TimeSpan.FromMilliseconds(
                    milliseconds);

            if (time.Hours > 0)
            {
                return time.ToString(
                    @"hh\:mm\:ss");
            }

            return time.ToString(
                @"mm\:ss");
        }

        // =========================================================
        // PROGRESS
        // =========================================================

        private void ProgressBar_Scroll(
            object? sender,
            EventArgs e)
        {
            long length =
                _mediaPlayer.Length;

            if (length <= 0)
                return;

            double position =
                _progressBar.Value /
                1000.0;

            _mediaPlayer.Time =
                (long)(
                    length *
                    position);
        }

        // =========================================================
        // INTRO BUTTON
        // =========================================================

        private void CreateSkipIntroButton()
        {
            _skipIntroButton =
                new Button();

            _skipIntroButton.Text =
                "⏩  INTRO'YU ATLA";

            _skipIntroButton.Size =
                new Size(
                    185,
                    45);

            _skipIntroButton.BackColor =
                Color.FromArgb(
                    225,
                    20,
                    20,
                    25);

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
            {
                _skipIntroButton.Visible =
                    false;

                return;
            }

            VideoItem item =
                _videos[_currentIndex];

            long currentSeconds =
                time / 1000;

            if (
                item.IntroEnd >
                item.IntroStart &&
                currentSeconds >=
                item.IntroStart &&
                currentSeconds <
                item.IntroEnd)
            {
                _skipIntroButton.Visible =
                    true;

                _skipIntroButton.BringToFront();
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
            {
                return;
            }

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
        // NEXT EPISODE PANEL
        // =========================================================

        private void CreateNextEpisodePanel()
        {
            _nextEpisodePanel =
                new Panel();

            _nextEpisodePanel.Size =
                new Size(
                    350,
                    150);

            _nextEpisodePanel.BackColor =
                Color.FromArgb(
                    242,
                    18,
                    18,
                    22);

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
                new Point(
                    20,
                    12);

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
                new Point(
                    20,
                    40);

            _nextEpisodeTitle.Size =
                new Size(
                    310,
                    25);

            _nextEpisodeTitle.AutoEllipsis =
                true;

            _nextEpisodePanel.Controls.Add(
                _nextEpisodeTitle);

            _nextEpisodeCountdown =
                new Label();

            _nextEpisodeCountdown.Text =
                "⏭ 10 saniye sonra başlıyor";

            _nextEpisodeCountdown.ForeColor =
                SecondaryTextColor;

            _nextEpisodeCountdown.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            _nextEpisodeCountdown.Location =
                new Point(
                    20,
                    72);

            _nextEpisodeCountdown.AutoSize =
                true;

            _nextEpisodePanel.Controls.Add(
                _nextEpisodeCountdown);

            _watchNowButton =
                new Button();

            _watchNowButton.Text =
                "ŞİMDİ İZLE  ▶";

            _watchNowButton.Location =
                new Point(
                    20,
                    102);

            _watchNowButton.Size =
                new Size(
                    310,
                    35);

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

            _watchNowButton.Cursor =
                Cursors.Hand;

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
            {
                return;
            }

            if (_videos.Count <= 1)
            {
                HideNextEpisodeNotice();
                return;
            }

            long remaining =
                length - time;

            if (
                remaining <= 10000 &&
                remaining > 0)
            {
                int seconds =
                    (int)Math.Ceiling(
                        remaining /
                        1000.0);

                ShowNextEpisodeNotice(
                    seconds);
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
            _mediaPlayer.Mute =
                !_mediaPlayer.Mute;

            if (_mediaPlayer.Mute)
            {
                _muteButton.Text =
                    "🔇";
            }
            else
            {
                _muteButton.Text =
                    "🔊";
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
                    "OLCAY KILIÇ TV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using OpenFileDialog dialog =
                new OpenFileDialog();

            dialog.Title =
                "Videoları Seç";

            dialog.Filter =
                "Video Dosyaları|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm|Tüm Dosyalar|*.*";

            dialog.Multiselect =
                true;

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

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
            {
                MessageBox.Show(
                    "Önce bir bölüm oynat.",
                    "OLCAY KILIÇ TV");

                return;
            }

            _mediaPlayer.Stop();

            _videos.RemoveAt(
                _currentIndex);

            _currentIndex = -1;

            _titleLabel.Text =
                "OLCAY KILIÇ TV";

            RefreshVideoSlots();
        }

        // =========================================================
        // MOVE UP
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
        // MOVE DOWN
        // =========================================================

        private void DownButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex < 0 ||
                _currentIndex >=
                _videos.Count - 1)
            {
                return;
            }

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

            _isFullscreen =
                true;

            _sideBar.Visible =
                false;

            _topBar.Visible =
                true;

            _bottomBar.Visible =
                true;

            ShowMouseControls();

            UpdateOverlayPositions();

            BringAllOverlaysToFront();
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

            _isFullscreen =
                true;

            _sideBar.Visible =
                false;

            _topBar.Visible =
                true;

            _bottomBar.Visible =
                true;

            ShowMouseControls();

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

            _isFullscreen =
                false;

            _sideBar.Visible =
                true;

            _topBar.Visible =
                true;

            _bottomBar.Visible =
                true;

            ShowMouseCursor();

            UpdateOverlayPositions();

            BringAllOverlaysToFront();
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
            {
                ExitFullscreen();
            }
            else
            {
                EnterFullscreen();
            }
        }

        // =========================================================
        // OVERLAY POSITIONS
        // =========================================================

        private void UpdateOverlayPositions()
        {
            if (_skipIntroButton != null)
            {
                _skipIntroButton.Location =
                    new Point(
                        Math.Max(
                            10,
                            ClientSize.Width -
                            _skipIntroButton.Width -
                            30),
                        Math.Max(
                            10,
                            ClientSize.Height -
                            180));
            }

            if (_nextEpisodePanel != null)
            {
                _nextEpisodePanel.Location =
                    new Point(
                        Math.Max(
                            10,
                            ClientSize.Width -
                            _nextEpisodePanel.Width -
                            30),
                        Math.Max(
                            10,
                            ClientSize.Height -
                            340));
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

        // =========================================================
        // SHOW CURSOR
        // =========================================================

        private void ShowMouseCursor()
        {
            if (!_cursorHidden)
                return;

            while (
                ShowCursor(true) < 0)
            {
            }

            _cursorHidden =
                false;
        }

        // =========================================================
        // HIDE CURSOR
        // =========================================================

        private void HideMouseCursor()
        {
            if (_cursorHidden)
                return;

            while (
                ShowCursor(false) >= 0)
            {
            }

            _cursorHidden =
                true;
        }

        // =========================================================
        // SHOW CONTROLS
        // =========================================================

        private void ShowMouseControls()
        {
            _mouseIdleSeconds =
                0;

            ShowMouseCursor();

            if (!_isFullscreen)
                return;

            _topBar.Visible =
                true;

            _bottomBar.Visible =
                true;

            _topBar.BringToFront();

            _bottomBar.BringToFront();

            if (
                _skipIntroButton != null &&
                _skipIntroButton.Visible)
            {
                _skipIntroButton.BringToFront();
            }

            if (
                _nextEpisodePanel != null &&
                _nextEpisodePanel.Visible)
            {
                _nextEpisodePanel.BringToFront();
            }
        }

        // =========================================================
        // HIDE CONTROLS
        // =========================================================

        private void HideMouseControls()
        {
            if (!_isFullscreen)
                return;

            _topBar.Visible =
                false;

            _bottomBar.Visible =
                false;

            HideMouseCursor();

            if (
                _skipIntroButton != null &&
                _skipIntroButton.Visible)
            {
                _skipIntroButton.BringToFront();
            }

            if (
                _nextEpisodePanel != null &&
                _nextEpisodePanel.Visible)
            {
                _nextEpisodePanel.BringToFront();
            }
        }

        // =========================================================
        // BRING OVERLAYS
        // =========================================================

        private void BringAllOverlaysToFront()
        {
            if (_topBar != null)
                _topBar.BringToFront();

            if (_bottomBar != null)
                _bottomBar.BringToFront();

            if (
                _skipIntroButton != null &&
                _skipIntroButton.Visible)
            {
                _skipIntroButton.BringToFront();
            }

            if (
                _nextEpisodePanel != null &&
                _nextEpisodePanel.Visible)
            {
                _nextEpisodePanel.BringToFront();
            }

            if (_sideBar != null)
                _sideBar.BringToFront();
        }

        // =========================================================
        // KEYBOARD
        // =========================================================

        private void MainForm_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            // -----------------------------------------------------
            // ESC
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.Escape)
            {
                if (_isFullscreen)
                {
                    ExitFullscreen();
                }

                return;
            }

            // -----------------------------------------------------
            // F11
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.F11)
            {
                ToggleFullscreen();

                e.SuppressKeyPress =
                    true;

                return;
            }

            // -----------------------------------------------------
            // SPACE
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.Space)
            {
                TogglePlayPause();

                e.SuppressKeyPress =
                    true;

                return;
            }

            // -----------------------------------------------------
            // LEFT
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.Left)
            {
                if (_mediaPlayer.Time > 10000)
                {
                    _mediaPlayer.Time -=
                        10000;
                }
                else
                {
                    _mediaPlayer.Time =
                        0;
                }

                return;
            }

            // -----------------------------------------------------
            // RIGHT
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.Right)
            {
                long newTime =
                    _mediaPlayer.Time +
                    10000;

                if (
                    _mediaPlayer.Length > 0 &&
                    newTime >
                    _mediaPlayer.Length)
                {
                    newTime =
                        _mediaPlayer.Length;
                }

                _mediaPlayer.Time =
                    newTime;

                return;
            }

            // -----------------------------------------------------
            // M
            // -----------------------------------------------------

            if (e.KeyCode ==
                Keys.M)
            {
                ToggleMute();

                return;
            }
        }

        // =========================================================
        // RESIZE
        // =========================================================

        private void MainForm_Resize(
            object? sender,
            EventArgs e)
        {
            UpdateOverlayPositions();

            if (_bottomBar != null)
            {
                PositionFullscreenButton();
            }
        }

        private void PositionFullscreenButton()
        {
            if (_fullscreenButton == null ||
                _bottomBar == null)
            {
                return;
            }

            _fullscreenButton.Left =
                _bottomBar.ClientSize.Width -
                _fullscreenButton.Width -
                20;
        }

        // =========================================================
        // FORM CLOSE
        // =========================================================

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            try
            {
                if (_cursorHidden)
                {
                    ShowMouseCursor();
                }

                _updateTimer?.Stop();

                _mouseTimer?.Stop();

                _mediaPlayer?.Stop();

                if (_currentMedia != null)
                {
                    _currentMedia.Dispose();

                    _currentMedia = null;
                }

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
