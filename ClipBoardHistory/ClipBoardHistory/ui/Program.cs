using System;
using System.Windows.Forms;
using ClipBoardHistory.Services;

namespace ClipBoardHistory.UI
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // アプリケーションが .NET 9.0 での推奨設定を使用するように設定
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // クリップボードサービスをインスタンス化し、監視を開始
            var clipboardService = new ClipboardService();
            clipboardService.StartMonitoring();

            // メインフォームをサービスインスタンスを渡して起動
            Application.Run(new Form1(clipboardService));
        }
    }
}