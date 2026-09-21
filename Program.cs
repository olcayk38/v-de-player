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

        private Button _playButton = null!;
        private Button _prevButton = null!;
        private Button _nextButton = null!;
        private Button _fullscreenButton = null!;
        private Button _muteButton = null!;

        private TrackBar _progressBar = null!;
        private TrackBar _volumeBar = null!;

        private Label _volumeLabel = null!;

        private Button _addButton = null!;
        private Button _removeButton = null!;
        private Button _upButton = null!;
        private Button _downButton = null!;

        private Label _emptyLabel = null!;

        private Panel _notificationPanel = null!;
        private Label _notificationTitle = null!;
        private Label _notificationEpisode = null!;
        private Label _notificationCountdown = null!;
        private Button _notificationPlayButton = null!;

        private Button _skipIntroButton = null!;

        private System.Windows.Forms.Timer _updateTimer = null!;
        private System.Windows.Forms.Timer _mouseTimer = null!;

        private List<VideoItem> _videos =
            new List<VideoItem>();

        private int _currentIndex = -1;

        private bool _isFullscreen = false;

        private bool _noticeShown = false;

        private int _nextCountdown = 10;

        private DateTime _lastMouseMove =
            DateTime.Now;

        private FormBorderStyle _oldBorderStyle;

        private FormWindowState _oldWindowState;

        private Rectangle _oldBounds;


        public MainForm()
        {
            Text =
                "OLCAY KILIÇ TV";

            Width = 1500;

            Height = 900;

            MinimumSize =
                new Size(1100, 700);

            StartPosition =
                FormStartPosition.CenterScreen;

            BackColor =
                Color.FromArgb(
                    8,
                    8,
                    10);

            KeyPreview = true;

            _libVLC =
                new LibVLC();

            _mediaPlayer =
                new MediaPlayer(
                    _libVLC);

            _mediaPlayer.EndReached +=
                MediaPlayer_EndReached;

            BuildInterface();

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

            MouseMove +=
                MainForm_MouseMove;

            KeyDown +=
                MainForm_KeyDown;

            Resize +=
                MainForm_Resize;
        }


        // =========================================================
        // ARAYÜZ
        // =========================================================

        private void BuildInterface()
        {
            // =====================================================
            // VIDEO
            // =====================================================

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


            // =====================================================
            // ÜST BAR
            // =====================================================

            _topBar =
                new Panel();

            _topBar.Dock =
                DockStyle.Top;

            _topBar.Height =
                70;

            _topBar.BackColor =
                Color.FromArgb(
                    12,
                    12,
                    15);

            Controls.Add(
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
                    18,
                    FontStyle.Bold);

            _titleLabel.AutoSize =
                true;

            _titleLabel.Location =
                new Point(
                    25,
                    20);

            _topBar.Controls.Add(
                _titleLabel);


            // =====================================================
            // SAĞ VIDEO PANELİ
            // =====================================================

            _sideBar =
                new Panel();

            _sideBar.Dock =
                DockStyle.Right;

            _sideBar.Width =
                390;

            _sideBar.BackColor =
                Color.FromArgb(
                    17,
                    17,
                    21);

            Controls.Add(
                _sideBar);


            Label listTitle =
                new Label();

            listTitle.Text =
                "BÖLÜMLER";

            listTitle.ForeColor =
                Color.White;

            listTitle.Font =
                new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Bold);

            listTitle.Dock =
                DockStyle.Top;

            listTitle.Height =
                50;

            listTitle.TextAlign =
                ContentAlignment.MiddleLeft;

            listTitle.Padding =
                new Padding(
                    15,
                    0,
                    0,
                    0);

            _sideBar.Controls.Add(
                listTitle);


            // =====================================================
            // VIDEO SLOT PANELİ
            // =====================================================

            FlowLayoutPanel videoList =
                new FlowLayoutPanel();

            videoList.Name =
                "VideoList";

            videoList.Dock =
                DockStyle.Fill;

            videoList.FlowDirection =
                FlowDirection.TopDown;

            videoList.WrapContents =
                false;

            videoList.AutoScroll =
                true;

            videoList.BackColor =
                Color.FromArgb(
                    17,
                    17,
                    21);

            _sideBar.Controls.Add(
                videoList);


            // =====================================================
            // BUTONLAR
            // =====================================================

            Panel playlistButtons =
                new Panel();

            playlistButtons.Dock =
                DockStyle.Bottom;

            playlistButtons.Height =
                105;

            playlistButtons.BackColor =
                Color.FromArgb(
                    12,
                    12,
                    15);

            _sideBar.Controls.Add(
                playlistButtons);


            _addButton =
                CreateButton(
                    "+ VİDEO EKLE");

            _addButton.Location =
                new Point(
                    15,
                    10);

            _addButton.Width =
                350;

            _addButton.Click +=
                AddButton_Click;

            playlistButtons.Controls.Add(
                _addButton);


            _removeButton =
                CreateButton(
                    "SİL");

            _removeButton.Location =
                new Point(
                    15,
                    52);

            _removeButton.Width =
                100;

            _removeButton.Click +=
                RemoveButton_Click;

            playlistButtons.Controls.Add(
                _removeButton);


            _upButton =
                CreateButton(
                    "▲");

            _upButton.Location =
                new Point(
                    125,
                    52);

            _upButton.Width =
                70;

            _upButton.Click +=
                UpButton_Click;

            playlistButtons.Controls.Add(
                _upButton);


            _downButton =
                CreateButton(
                    "▼");

            _downButton.Location =
                new Point(
                    205,
                    52);

            _downButton.Width =
                70;

            _downButton.Click +=
                DownButton_Click;

            playlistButtons.Controls.Add(
                _downButton);


            // =====================================================
            // BOŞ EKRAN
            // =====================================================

            _emptyLabel =
                new Label();

            _emptyLabel.Text =
                "VIDEO EKLE\n\n" +
                "En fazla 20 bölüm ekleyebilirsin.";

            _emptyLabel.ForeColor =
                Color.FromArgb(
                    150,
                    150,
                    155);

            _emptyLabel.BackColor =
                Color.Transparent;

            _emptyLabel.Dock =
                DockStyle.Fill;

            _emptyLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            _emptyLabel.Font =
                new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold);

            _videoView.Controls.Add(
                _emptyLabel);

            _emptyLabel.BringToFront();


            // =====================================================
            // ALT BAR
            // =====================================================

            _bottomBar =
                new Panel();

            _bottomBar.Dock =
                DockStyle.Bottom;

            _bottomBar.Height =
                105;

            _bottomBar.BackColor =
                Color.FromArgb(
                    12,
                    12,
                    15);

            Controls.Add(
                _bottomBar);


            // =====================================================
            // PROGRESS
            // =====================================================

            _progressBar =
                new TrackBar();

            _progressBar.Minimum =
                0;

            _progressBar.Maximum =
                1000;

            _progressBar.TickStyle =
                TickStyle.None;

            _progressBar.Location =
                new Point(
                    20,
                    5);

            _progressBar.Width =
                650;

            _progressBar.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Right |
                AnchorStyles.Top;

            _progressBar.MouseDown +=
                ProgressBar_MouseDown;

            _bottomBar.Controls.Add(
                _progressBar);


            // =====================================================
            // SÜRE
            // =====================================================

            _timeLabel =
                new Label();

            _timeLabel.Text =
                "00:00 / 00:00";

            _timeLabel.ForeColor =
                Color.LightGray;

            _timeLabel.AutoSize =
                true;

            _timeLabel.Location =
                new Point(
                    20,
                    38);

            _bottomBar.Controls.Add(
                _timeLabel);


            // =====================================================
            // GERİ
            // =====================================================

            _prevButton =
                CreateButton(
                    "⏮");

            _prevButton.Location =
                new Point(
                    170,
                    30);

            _prevButton.Width =
                50;

            _prevButton.Click +=
                PrevButton_Click;

            _bottomBar.Controls.Add(
                _prevButton);


            // =====================================================
            // PLAY
            // =====================================================

            _playButton =
                CreateButton(
                    "▶");

            _playButton.Location =
                new Point(
                    230,
                    30);

            _playButton.Width =
                60;

            _playButton.Click +=
                PlayButton_Click;

            _bottomBar.Controls.Add(
                _playButton);


            // =====================================================
            // İLERİ
            // =====================================================

            _nextButton =
                CreateButton(
                    "⏭");

            _nextButton.Location =
                new Point(
                    300,
                    30);

            _nextButton.Width =
                50;

            _nextButton.Click +=
                NextButton_Click;

            _bottomBar.Controls.Add(
                _nextButton);


            // =====================================================
            // SES
            // =====================================================

            _muteButton =
                CreateButton(
                    "🔊");

            _muteButton.Location =
                new Point(
                    370,
                    30);

            _muteButton.Width =
                55;

            _muteButton.Click +=
                MuteButton_Click;

            _bottomBar.Controls.Add(
                _muteButton);


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
                new Point(
                    430,
                    30);

            _volumeBar.Width =
                100;

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
                new Point(
                    535,
                    38);

            _bottomBar.Controls.Add(
                _volumeLabel);


            // =====================================================
            // FULLSCREEN
            // =====================================================

            _fullscreenButton =
                CreateButton(
                    "⛶");

            _fullscreenButton.Width =
                55;

            _fullscreenButton.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;

            _fullscreenButton.Location =
                new Point(
                    ClientSize.Width - 80,
                    30);

            _fullscreenButton.Click +=
                FullscreenButton_Click;

            _bottomBar.Controls.Add(
                _fullscreenButton);


            // =====================================================
            // INTRO ATLA
            // =====================================================

            _skipIntroButton =
                CreateButton(
                    "⏩ INTRO'YU ATLA");

            _skipIntroButton.Width =
                190;

            _skipIntroButton.Height =
                45;

            _skipIntroButton.Visible =
                false;

            _skipIntroButton.BackColor =
                Color.FromArgb(
                    210,
                    210,
                    210);

            _skipIntroButton.ForeColor =
                Color.Black;

            _skipIntroButton.Font =
                new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold);

            _skipIntroButton.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Right;

            _skipIntroButton.Location =
                new Point(
                    ClientSize.Width -
                    610,
                    ClientSize.Height -
                    170);

            _skipIntroButton.Click +=
                SkipIntroButton_Click;

            Controls.Add(
                _skipIntroButton);


            // =====================================================
            // SONRAKİ BÖLÜM PANELİ
            // =====================================================

            _notificationPanel =
                new Panel();

            _notificationPanel.Width =
                350;

            _notificationPanel.Height =
                180;

            _notificationPanel.BackColor =
                Color.FromArgb(
                    235,
                    12,
                    12,
                    15);

            _notificationPanel.Visible =
                false;

            _notificationPanel.Anchor =
                AnchorStyles.Bottom |
                AnchorStyles.Right;

            Controls.Add(
                _notificationPanel);


            _notificationTitle =
                new Label();

            _notificationTitle.Text =
                "SONRAKİ BÖLÜM";

            _notificationTitle.ForeColor =
                Color.White;

            _notificationTitle.Font =
                new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold);

            _notificationTitle.AutoSize =
                true;

            _notificationTitle.Location =
                new Point(
                    20,
                    15);

            _notificationPanel.Controls.Add(
                _notificationTitle);


            _notificationEpisode =
                new Label();

            _notificationEpisode.Text =
                "Bölüm 2";

            _notificationEpisode.ForeColor =
                Color.White;

            _notificationEpisode.Font =
                new Font(
                    "Segoe UI",
                    16,
                    FontStyle.Bold);

            _notificationEpisode.AutoSize =
                true;

            _notificationEpisode.Location =
                new Point(
                    20,
                    45);

            _notificationPanel.Controls.Add(
                _notificationEpisode);


            _notificationCountdown =
                new Label();

            _notificationCountdown.Text =
                "10";

            _notificationCountdown.ForeColor =
                Color.White;

            _notificationCountdown.Font =
                new Font(
                    "Segoe UI",
                    30,
                    FontStyle.Bold);

            _notificationCountdown.AutoSize =
                true;

            _notificationCountdown.Location =
                new Point(
                    250,
                    38);

            _notificationPanel.Controls.Add(
                _notificationCountdown);


            _notificationPlayButton =
                CreateButton(
                    "ŞİMDİ İZLE");

            _notificationPlayButton.Width =
                310;

            _notificationPlayButton.Height =
                45;

            _notificationPlayButton.Location =
                new Point(
                    20,
                    115);

            _notificationPlayButton.Click +=
                NotificationPlayButton_Click;

            _notificationPanel.Controls.Add(
                _notificationPlayButton);
        }


        // =========================================================
        // BUTON OLUŞTUR
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

            button.Cursor =
                Cursors.Hand;

            return button;
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
                    "En fazla 20 video ekleyebilirsin.");

                return;
            }

            using OpenFileDialog dialog =
                new OpenFileDialog();

            dialog.Title =
                "Videoları Seç";

            dialog.Filter =
                "Video Dosyaları|" +
                "*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.webm|" +
                "Tüm Dosyalar|*.*";

            dialog.Multiselect =
                true;

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

            foreach (
                string file
                in dialog.FileNames)
            {
                if (_videos.Count >= 20)
                    break;

                if (_videos.Exists(
                    x => x.FilePath == file))
                {
                    continue;
                }

                VideoItem item =
                    new VideoItem();

                item.FilePath =
                    file;

                item.IntroStart =
                    0;

                item.IntroEnd =
                    0;

                _videos.Add(item);
            }

            RefreshVideoSlots();

            _emptyLabel.Visible =
                _videos.Count == 0;

            if (_currentIndex == -1 &&
                _videos.Count > 0)
            {
                PlayVideo(0);
            }
        }


        // =========================================================
        // SLOT'LARI OLUŞTUR
        // =========================================================

        private void RefreshVideoSlots()
        {
            Control[] controls =
                _sideBar.Controls
                    .Find(
                        "VideoList",
                        true);

            if (controls.Length == 0)
                return;

            FlowLayoutPanel videoList =
                controls[0]
                as FlowLayoutPanel!;

            videoList.Controls.Clear();

            for (
                int i = 0;
                i < _videos.Count;
                i++)
            {
                CreateVideoSlot(
                    videoList,
                    i);
            }
        }


        // =========================================================
        // VIDEO SLOT
        // =========================================================

        private void CreateVideoSlot(
            FlowLayoutPanel parent,
            int index)
        {
            VideoItem item =
                _videos[index];

            Panel slot =
                new Panel();

            slot.Width =
                350;

            slot.Height =
                175;

            slot.BackColor =
                Color.FromArgb(
                    27,
                    27,
                    32);

            slot.Margin =
                new Padding(
                    10,
                    6,
                    10,
                    6);

            // -----------------------------------------------------
            // BÖLÜM NUMARASI
            // -----------------------------------------------------

            Label number =
                new Label();

            number.Text =
                $"{index + 1}";

            number.ForeColor =
                Color.White;

            number.Font =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold);

            number.Location =
                new Point(
                    12,
                    10);

            number.AutoSize =
                true;

            slot.Controls.Add(
                number);


            // -----------------------------------------------------
            // DOSYA ADI
            // -----------------------------------------------------

            Label name =
                new Label();

            name.Text =
                item.Title;

            name.ForeColor =
                Color.White;

            name.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            name.Location =
                new Point(
                    45,
                    12);

            name.Width =
                285;

            name.AutoEllipsis =
                true;

            slot.Controls.Add(
                name);


            // -----------------------------------------------------
            // INTRO BAŞLANGIÇ
            // -----------------------------------------------------

            Label startLabel =
                new Label();

            startLabel.Text =
                "Intro Başlangıç";

            startLabel.ForeColor =
                Color.LightGray;

            startLabel.Location =
                new Point(
                    15,
                    48);

            startLabel.AutoSize =
                true;

            slot.Controls.Add(
                startLabel);


            TextBox startBox =
                new TextBox();

            startBox.Text =
                FormatSeconds(
                    item.IntroStart);

            startBox.Location =
                new Point(
                    145,
                    44);

            startBox.Width =
                85;

            startBox.BackColor =
                Color.FromArgb(
                    40,
                    40,
                    45);

            startBox.ForeColor =
                Color.White;

            slot.Controls.Add(
                startBox);


            // -----------------------------------------------------
            // INTRO BİTİŞ
            // -----------------------------------------------------

            Label endLabel =
                new Label();

            endLabel.Text =
                "Intro Bitiş";

            endLabel.ForeColor =
                Color.LightGray;

            endLabel.Location =
                new Point(
                    15,
                    83);

            endLabel.AutoSize =
                true;

            slot.Controls.Add(
                endLabel);


            TextBox endBox =
                new TextBox();

            endBox.Text =
                FormatSeconds(
                    item.IntroEnd);

            endBox.Location =
                new Point(
                    145,
                    79);

            endBox.Width =
                85;

            endBox.BackColor =
                Color.FromArgb(
                    40,
                    40,
                    45);

            endBox.ForeColor =
                Color.White;

            slot.Controls.Add(
                endBox);


            // -----------------------------------------------------
            // KAYDET
            // -----------------------------------------------------

            Button saveButton =
                CreateButton(
                    "KAYDET");

            saveButton.Location =
                new Point(
                    240,
                    48);

            saveButton.Width =
                90;

            saveButton.Height =
                65;

            saveButton.Click +=
                (s, e) =>
                {
                    int start =
                        ParseTime(
                            startBox.Text);

                    int end =
                        ParseTime(
                            endBox.Text);

                    if (start < 0)
                        start = 0;

                    if (end < 0)
                        end = 0;

                    if (end > 0 &&
                        end <= start)
                    {
                        MessageBox.Show(
                            "Intro bitiş zamanı, " +
                            "başlangıç zamanından büyük olmalı.");

                        return;
                    }

                    item.IntroStart =
                        start;

                    item.IntroEnd =
                        end;

                    saveButton.Text =
                        "✓ KAYDEDİLDİ";

                    System.Windows.Forms.Timer timer =
                        new System.Windows.Forms.Timer();

                    timer.Interval =
                        1200;

                    timer.Tick +=
                        (ts, te) =>
                        {
                            saveButton.Text =
                                "KAYDET";

                            timer.Stop();

                            timer.Dispose();
                        };

                    timer.Start();
                };

            slot.Controls.Add(
                saveButton);


            // -----------------------------------------------------
            // OYNAT
            // -----------------------------------------------------

            Button playButton =
                CreateButton(
                    "▶ OYNAT");

            playButton.Location =
                new Point(
                    15,
                    125);

            playButton.Width =
                315;

            playButton.Height =
                35;

            playButton.Click +=
                (s, e) =>
                {
                    PlayVideo(index);
                };

            slot.Controls.Add(
                playButton);


            parent.Controls.Add(
                slot);
        }


        // =========================================================
        // ZAMAN FORMAT
        // =========================================================

        private string FormatSeconds(
            int seconds)
        {
            TimeSpan t =
                TimeSpan.FromSeconds(
                    seconds);

            if (t.TotalHours >= 1)
            {
                return t.ToString(
                    @"hh\:mm\:ss");
            }

            return t.ToString(
                @"mm\:ss");
        }


        // =========================================================
        // ZAMAN OKU
        // =========================================================

        private int ParseTime(
            string text)
        {
            text =
                text.Trim();

            if (string.IsNullOrWhiteSpace(
                text))
            {
                return 0;
            }

            if (int.TryParse(
                text,
                out int plainSeconds))
            {
                return plainSeconds;
            }

            string[] parts =
                text.Split(':');

            try
            {
                if (parts.Length == 2)
                {
                    int minutes =
                        int.Parse(parts[0]);

                    int seconds =
                        int.Parse(parts[1]);

                    return
                        minutes * 60 +
                        seconds;
                }

                if (parts.Length == 3)
                {
                    int hours =
                        int.Parse(parts[0]);

                    int minutes =
                        int.Parse(parts[1]);

                    int seconds =
                        int.Parse(parts[2]);

                    return
                        hours * 3600 +
                        minutes * 60 +
                        seconds;
                }
            }
            catch
            {
            }

            return 0;
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

            VideoItem item =
                _videos[index];

            if (!File.Exists(
                item.FilePath))
            {
                MessageBox.Show(
                    "Video bulunamadı:\n\n" +
                    item.FilePath);

                return;
            }

            _currentIndex =
                index;

            _noticeShown =
                false;

            _nextCountdown =
                10;

            _notificationPanel.Visible =
                false;

            _skipIntroButton.Visible =
                false;

            try
            {
                _mediaPlayer.Stop();

                using Media media =
                    new Media(
                        _libVLC,
                        new Uri(
                            item.FilePath));

                _mediaPlayer.Play(
                    media);

                _titleLabel.Text =
                    item.Title;

                _emptyLabel.Visible =
                    false;

                _playButton.Text =
                    "⏸";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Video açılırken hata oluştu:\n\n" +
                    ex.Message);
            }
        }


        // =========================================================
        // INTRO KONTROL
        // =========================================================

        private void CheckIntro(
            long currentTime)
        {
            if (_currentIndex < 0 ||
                _currentIndex >=
                _videos.Count)
            {
                return;
            }

            VideoItem item =
                _videos[_currentIndex];

            if (item.IntroStart <= 0 ||
                item.IntroEnd <=
                item.IntroStart)
            {
                _skipIntroButton.Visible =
                    false;

                return;
            }

            bool insideIntro =
                currentTime >=
                item.IntroStart * 1000L
                &&
                currentTime <
                item.IntroEnd * 1000L;

            if (insideIntro)
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


        // =========================================================
        // INTRO ATLA
        // =========================================================

        private void SkipIntroButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex < 0 ||
                _currentIndex >=
                _videos.Count)
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

            // PROGRESS
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


            // SÜRE
            _timeLabel.Text =
                FormatMilliseconds(
                    time)
                +
                " / "
                +
                FormatMilliseconds(
                    length);


            // INTRO
            CheckIntro(
                time);


            // =====================================================
            // SON 10 SANİYE
            // =====================================================

            long remaining =
                length - time;

            if (remaining <= 10000 &&
                remaining > 0)
            {
                if (!_noticeShown)
                {
                    ShowNextEpisodeNotice();
                }

                int countdown =
                    (int)Math.Ceiling(
                        remaining /
                        1000.0);

                if (countdown < 1)
                    countdown = 1;

                _nextCountdown =
                    countdown;

                _notificationCountdown.Text =
                    countdown.ToString();
            }


            _playButton.Text =
                _mediaPlayer.IsPlaying
                    ? "⏸"
                    : "▶";
        }


        // =========================================================
        // SONRAKİ BÖLÜM UYARISI
        // =========================================================

        private void ShowNextEpisodeNotice()
        {
            _noticeShown =
                true;

            int next =
                _currentIndex + 1;

            if (next >= _videos.Count)
            {
                next = 0;
            }

            if (_videos.Count == 1)
            {
                _notificationEpisode.Text =
                    "Video yeniden başlayacak";
            }
            else
            {
                _notificationEpisode.Text =
                    "Bölüm " +
                    (next + 1) +
                    " • " +
                    _videos[next].Title;
            }

            _notificationCountdown.Text =
                "10";

            _notificationPanel.Visible =
                true;

            _notificationPanel.BringToFront();
        }


        // =========================================================
        // ŞİMDİ İZLE
        // =========================================================

        private void NotificationPlayButton_Click(
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
                new Action(
                    () =>
                    {
                        PlayNextVideo();
                    }));
        }


        // =========================================================
        // SONRAKİ VIDEO
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
        // PLAY
        // =========================================================

        private void PlayButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentIndex == -1)
            {
                if (_videos.Count > 0)
                    PlayVideo(0);

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
        // PROGRESS
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

            _mediaPlayer.Time =
                (long)(
                    _mediaPlayer.Length *
                    percent);
        }


        // =========================================================
        // FORMAT
        // =========================================================

        private string FormatMilliseconds(
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
        // SİL
        // =========================================================

        private void RemoveButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_videos.Count == 0)
                return;

            if (_currentIndex < 0 ||
                _currentIndex >=
                _videos.Count)
            {
                return;
            }

            _videos.RemoveAt(
                _currentIndex);

            if (_videos.Count == 0)
            {
                _currentIndex = -1;

                _mediaPlayer.Stop();

                _titleLabel.Text =
                    "OLCAY KILIÇ TV";

                _emptyLabel.Visible =
                    true;

                RefreshVideoSlots();

                return;
            }

            if (_currentIndex >=
                _videos.Count)
            {
                _currentIndex =
                    _videos.Count - 1;
            }

            RefreshVideoSlots();

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
        // AŞAĞI
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

                _skipIntroButton.BringToFront();

                if (_notificationPanel.Visible)
                {
                    _notificationPanel.BringToFront();
                }
            }
            else
            {
                _isFullscreen =
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

                if (_skipIntroButton.Visible)
                {
                    _skipIntroButton.BringToFront();
                }

                if (_notificationPanel.Visible)
                {
                    _notificationPanel.BringToFront();
                }
            }
        }


        // =========================================================
        // MOUSE
        // =========================================================

        private void MainForm_MouseMove(
            object? sender,
            MouseEventArgs e)
        {
            _lastMouseMove =
                DateTime.Now;

            if (!_isFullscreen)
                return;

            _bottomBar.Visible =
                true;
        }


        private void MouseTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (!_isFullscreen)
                return;

            TimeSpan idle =
                DateTime.Now -
                _lastMouseMove;

            if (idle.TotalSeconds >= 2)
            {
                if (!_notificationPanel.Visible)
                {
                    _bottomBar.Visible =
                        false;
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
            if (e.KeyCode ==
                Keys.F11)
            {
                ToggleFullscreen();

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode ==
                Keys.Escape &&
                _isFullscreen)
            {
                ToggleFullscreen();

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode ==
                Keys.Space)
            {
                PlayButton_Click(
                    null,
                    EventArgs.Empty);

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode ==
                Keys.Right)
            {
                _mediaPlayer.Time +=
                    10000;

                e.SuppressKeyPress =
                    true;

                return;
            }

            if (e.KeyCode ==
                Keys.Left)
            {
                _mediaPlayer.Time -=
                    10000;

                e.SuppressKeyPress =
                    true;
            }
        }


        // =========================================================
        // RESIZE
        // =========================================================

        private void MainForm_Resize(
            object? sender,
            EventArgs e)
        {
            if (_skipIntroButton != null)
            {
                _skipIntroButton.Location =
                    new Point(
                        ClientSize.Width -
                        610,
                        ClientSize.Height -
                        170);
            }
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
