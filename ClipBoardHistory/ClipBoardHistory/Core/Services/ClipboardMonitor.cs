using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClipBoardHistory.Core.Services;

/// <summary>
/// クリップボードの変更を監視するクラス。
/// UIスレッドとは独立したSTAスレッドで動作し、OSからのメッセージ(WM_CLIPBOARDUPDATE)を捕捉します。
/// </summary>
public class ClipboardMonitor : IDisposable
{
    // クリップボードに変更があったときに発生するイベント
    public event EventHandler? ClipboardChanged;

    private Thread? _monitoringThread;
    private ManualResetEvent _readyEvent = new(false);
    private MonitorWindow? _window;
    private bool _isRunning;

    /// <summary>
    /// 監視を開始します。
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _readyEvent.Reset();

        // 専用のSTAスレッドを作成して監視ループを回す
        _monitoringThread = new Thread(MonitoringLoop)
        {
            IsBackground = true, // アプリ終了時に道連れで終了するように
            Name = "ClipboardMonitorThread"
        };
        _monitoringThread.SetApartmentState(ApartmentState.STA); // OLE/Clipboard操作にはSTAが必須
        _monitoringThread.Start();

        // スレッド内のウィンドウ生成とフック完了を待機
        _readyEvent.WaitOne();
    }

    /// <summary>
    /// 監視を停止し、リソースを解放します。
    /// </summary>
    public void Stop()
    {
        if (!_isRunning || _window == null) return;

        try
        {
            // 監視スレッド側のウィンドウに終了処理を依頼
            if (_window.InvokeRequired)
            {
                _window.Invoke(new MethodInvoker(() =>
                {
                    _window.StopMonitoring();
                    _window.Close();
                    Application.ExitThread(); // メッセージループを抜ける
                }));
            }
        }
        catch (Exception)
        {
            // すでにスレッドが死んでいる場合は無視
        }
        finally
        {
            _isRunning = false;
            _monitoringThread = null;
        }
    }

    public void Dispose()
    {
        Stop();
        _readyEvent.Dispose();
        GC.SuppressFinalize(this);
    }

    // バックグラウンドスレッドで実行されるメイン処理
    private void MonitoringLoop()
    {
        try
        {
            // メッセージ受信用の隠しウィンドウを作成
            _window = new MonitorWindow();

            // ウィンドウ側で検知した変更を、このクラスのイベントとして再送出
            _window.ClipboardUpdated += (s, e) => ClipboardChanged?.Invoke(this, EventArgs.Empty);

            // ハンドル作成を強制し、準備完了を通知
            var h = _window.Handle;
            _window.StartMonitoring();
            _readyEvent.Set();

            // メッセージループ開始（これをしないとイベントが来ない）
            Application.Run();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Monitor Thread Error: {ex.Message}");
        }
    }

    /// <summary>
    /// メッセージ受信専用の隠しウィンドウ（Formを継承することでInvokeなどが使いやすくなる）
    /// </summary>
    private class MonitorWindow : Form
    {
        public event EventHandler? ClipboardUpdated;

        public MonitorWindow()
        {
            // 画面には一切表示させない設定
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        // フォームを表示させないためのオーバーライド
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        public void StartMonitoring()
        {
            NativeMethods.AddClipboardFormatListener(this.Handle);
        }

        public void StopMonitoring()
        {
            NativeMethods.RemoveClipboardFormatListener(this.Handle);
        }

        // Windowsメッセージ処理
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                ClipboardUpdated?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }
    }

    /// <summary>
    /// Win32 API の定義
    /// </summary>
    private static class NativeMethods
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    }
}