using System;
using System.Windows.Forms;

namespace ClipBoardHistory;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // 高DPI設定の初期化など
        ApplicationConfiguration.Initialize();

        // メインフォームの起動
        Application.Run(new Form1());
    }
}