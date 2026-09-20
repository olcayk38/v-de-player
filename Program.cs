using System;
using System.Windows.Forms;

namespace VideoPlayer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            MessageBox.Show(
                "Video Player çalışıyor!",
                "VideoPlayer"
            );
        }
    }
}
