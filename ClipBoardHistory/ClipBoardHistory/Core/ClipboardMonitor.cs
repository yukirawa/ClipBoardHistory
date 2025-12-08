using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;

namespace ClipBoardHistory.Core
{
    /// <summary>
    /// クリップボードの変更をWin32 APIで監視する静的ユーティリティクラス。
    /// MessageListenerFormと連携して、OSからのWM_CLIPBOARDUPDATEメッセージを受け取ります。
    /// </summary>
    public static class ClipboardMonitor
    {
        // 外部クラスに公開するクリップボード更新イベント
        public static event EventHandler? ClipboardUpdate;

        // MessageListenerFormのインスタンスを保持
        private static MessageListenerForm? _listenerForm;

        // Win32 APIインポート (P/Invoke)
        // クリップボードのフォーマットリスナーを現在のプロセスに関連付けられたウィンドウに追加
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        // クリップボードのフォーマットリスナーを現在のプロセスに関連付けられたウィンドウから削除
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// クリップボードの監視を開始します。
        /// </summary>
        public static void Start()
        {
            if (_listenerForm != null)
            {
                // 既に開始済み
                return;
            }

            // 新しいスレッドで非表示のフォームを作成し、メッセージループを開始
            _listenerForm = new MessageListenerForm();
            _listenerForm.FormClosed += (s, e) => _listenerForm = null;

            // AddClipboardFormatListenerは、ウィンドウが作成されてから呼び出す必要がある
            if (!AddClipboardFormatListener(_listenerForm.Handle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "AddClipboardFormatListenerの呼び出しに失敗しました。");
            }
        }

        /// <summary>
        /// クリップボードの監視を停止します。
        /// </summary>
        public static void Stop()
        {
            if (_listenerForm == null)
            {
                return;
            }

            // リスナーを削除
            RemoveClipboardFormatListener(_listenerForm.Handle);

            // フォームを閉じ、メッセージループを終了させる
            _listenerForm.Invoke(new MethodInvoker(_listenerForm.Close));
            _listenerForm = null;
        }

        /// <summary>
        /// WM_CLIPBOARDUPDATEメッセージを受け取るための、UIを持たない非表示のフォームクラス。
        /// </summary>
        private class MessageListenerForm : Form
        {
            // Win32メッセージの定数: クリップボードの内容が更新されたときに送信される
            private const int WM_CLIPBOARDUPDATE = 0x031D;

            public MessageListenerForm()
            {
                // フォームを非表示にし、タスクバーにも表示しないように設定
                this.Visible = false;
                this.ShowInTaskbar = false;
            }

            // メッセージフック処理
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    // クリップボードが更新された場合、外部にイベントを発火させる
                    ClipboardUpdate?.Invoke(null, EventArgs.Empty);
                }
                base.WndProc(ref m);
            }
        }
    }
}