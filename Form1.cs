using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace myfarstAPP
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private readonly string _historyFilePath;
        private bool _isExiting = false;
        private string _lastClipboardText = string.Empty;

        // 内部で管理する履歴。UI に依存させずバックグラウンド処理を軽くする。
        private readonly List<string> _history = new List<string>();
        private readonly object _historyLock = new object();
        private const int MAX_HISTORY = 200;

        // デバウンスと非同期保存用
        private readonly System.Timers.Timer _debounceTimer;
        private readonly System.Timers.Timer _saveTimer;
        private string _pendingText;
        private volatile bool _isDirty = false;

        public Form1()
        {
            InitializeComponent();
            _historyFilePath = Path.Combine(Application.StartupPath, "history.txt");

            // デバウンス: 短時間に来る連続イベントをまとめる (300ms)
            _debounceTimer = new System.Timers.Timer(300) { AutoReset = false };
            _debounceTimer.Elapsed += DebounceTimer_Elapsed;

            // 定期保存: 変更があればまとめて保存する (60秒)
            _saveTimer = new System.Timers.Timer(60_000) { AutoReset = true };
            _saveTimer.Elapsed += SaveTimer_Elapsed;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(this.Handle);
            LoadHistory();

            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Text = "ClipBoardHistory";
            notifyIcon1.Visible = true;

            // 起動時は UI を内部履歴から構築
            RefreshListBoxFromHistory();

            // 保存タイマー開始
            _saveTimer.Start();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_CLIPBOARDUPDATE)
                return;

            // クリップボードアクセスは例外が発生することがあるため保護する
            string clipboardText;
            try
            {
                if (!Clipboard.ContainsText())
                    return;
                clipboardText = Clipboard.GetText();
            }
            catch
            {
                // 他プロセスがクリップボードをロックしている等、失敗時は何もしない
                return;
            }

            if (string.IsNullOrWhiteSpace(clipboardText) || clipboardText == _lastClipboardText)
                return;

            _lastClipboardText = clipboardText;

            // デバウンス: pending にセットしてタイマーを再起動
            _pendingText = clipboardText;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void DebounceTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var text = _pendingText;
            if (string.IsNullOrEmpty(text))
                return;

            AddToHistory(text);
        }

        private void AddToHistory(string clipboardText)
        {
            bool added = false;
            lock (_historyLock)
            {
                int existing = _history.IndexOf(clipboardText);
                if (existing == 0)
                {
                    // すでに先頭にある -> 変更なし
                }
                else
                {
                    if (existing > 0)
                        _history.RemoveAt(existing);

                    _history.Insert(0, clipboardText);

                    if (_history.Count > MAX_HISTORY)
                        _history.RemoveRange(MAX_HISTORY, _history.Count - MAX_HISTORY);

                    _isDirty = true;
                    added = true;
                }
            }

            if (added)
            {
                // UI は表示中のみ更新
                if (this.Visible && this.WindowState != FormWindowState.Minimized && listBox1 != null)
                {
                    try
                    {
                        this.Invoke((Action)(() =>
                        {
                            listBox1.BeginUpdate();
                            try
                            {
                                // 同じロジックで UI を更新
                                listBox1.Items.Insert(0, clipboardText);
                                while (listBox1.Items.Count > MAX_HISTORY)
                                    listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
                            }
                            finally
                            {
                                listBox1.EndUpdate();
                            }
                        }));
                    }
                    catch
                    {
                        // Invoke に失敗した場合は無視する（バックグラウンド時など）
                    }
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem is string selectedText && !string.IsNullOrEmpty(selectedText))
                Clipboard.SetText(selectedText);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_isExiting)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                // 強制終了時は同期保存してから終了
                SaveHistorySync();
                RemoveClipboardFormatListener(this.Handle);

                _saveTimer.Stop();
                _debounceTimer.Stop();
                _saveTimer.Dispose();
                _debounceTimer.Dispose();
            }
            base.OnFormClosing(e);
        }

        private void toolStripMenuItem_Show_Click(object sender, EventArgs e)
        {
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();

            // 表示時に UI を内部履歴で同期
            RefreshListBoxFromHistory();
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            try
            {
                _isExiting = true;
                SaveHistorySync();
                RemoveClipboardFormatListener(this.Handle);
                notifyIcon1.Visible = false;
                notifyIcon1.Dispose();
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"アプリケーションの終了中にエラーが発生しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("表示中の履歴をすべてクリアしますか？\n（ファイルは削除されません）", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                lock (_historyLock)
                {
                    _history.Clear();
                    _isDirty = true;
                }

                if (listBox1 != null)
                    listBox1.Items.Clear();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // ユーザー操作による即時保存は非同期で行う
            _ = SaveHistoryAsync();
            MessageBox.Show("現在の履歴を history.txt に保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("history.txt から履歴を読み込みます。\n現在のリストはクリアされますが、よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lock (_historyLock)
                {
                    _history.Clear();
                    _isDirty = true;
                }
                LoadHistory();
                RefreshListBoxFromHistory();
            }
        }

        private void SaveTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isDirty)
                return;

            // 非同期で保存
            _ = SaveHistoryAsync();
        }

        private async Task SaveHistoryAsync()
        {
            string[] snapshot;
            lock (_historyLock)
            {
                snapshot = _history.ToArray();
                _isDirty = false; // 保存を試みるためフラグを落とす
            }

            try
            {
                // 行単位の簡易フォーマットへ変更
                await File.WriteAllLinesAsync(_historyFilePath, snapshot).ConfigureAwait(false);
            }
            catch
            {
                // 保存失敗時はフラグを再セットして次回試行させる
                _isDirty = true;
            }
        }

        private void SaveHistorySync()
        {
            string[] snapshot;
            lock (_historyLock)
            {
                snapshot = _history.ToArray();
                _isDirty = false;
            }

            try
            {
                File.WriteAllLines(_historyFilePath, snapshot);
            }
            catch
            {
                // 最終手段として無視
            }
        }

        private void LoadHistory()
        {
            if (!File.Exists(_historyFilePath))
                return;

            try
            {
                var lines = File.ReadAllLines(_historyFilePath)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim())
                    .ToList();

                lock (_historyLock)
                {
                    _history.Clear();
                    foreach (var item in lines)
                        _history.Add(item);

                    if (_history.Count > MAX_HISTORY)
                        _history.RemoveRange(MAX_HISTORY, _history.Count - MAX_HISTORY);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("履歴ファイルの読み込みに失敗しました。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 内部履歴から ListBox を更新するユーティリティ
        private void RefreshListBoxFromHistory()
        {
            if (listBox1 == null)
                return;

            listBox1.BeginUpdate();
            try
            {
                listBox1.Items.Clear();
                lock (_historyLock)
                {
                    if (_history.Count > 0)
                        listBox1.Items.AddRange(_history.ToArray());
                }
            }
            finally
            {
                listBox1.EndUpdate();
            }
        }
    }
}