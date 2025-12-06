using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace myfarstAPP
{
    public partial class Form1 : Form
    {
        // --- 低レイヤー: Windows API 定義 ---
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private const int WM_CLIPBOARDUPDATE = 0x031D;
        // ------------------------------------

        private readonly string _historyFilePath;
        private bool _isExiting = false;
        private const int MaxHistoryCount = 50; // 最適化: 履歴の最大保持数

        public Form1()
        {
            InitializeComponent();
            _historyFilePath = Path.Combine(Application.StartupPath, "history.yaml");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Windowsに「クリップボードが変わったら教えて」と登録
            AddClipboardFormatListener(this.Handle);

            LoadHistory();
            UpdateStatus();

            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Text = "ClipBoardHistory v1.1";
            notifyIcon1.Visible = true;
        }

        // --- 低レイヤー: OSからのメッセージ処理 ---
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                // 変更通知が来たら処理を行う
                OnClipboardUpdate();
            }
            base.WndProc(ref m);
        }

        private void OnClipboardUpdate()
        {
            try
            {
                // クリップボードにテキストが含まれているか確認
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();

                    // 空白や、直前の履歴と同じ内容は無視（自己ループ防止＆最適化）
                    if (string.IsNullOrWhiteSpace(text)) return;
                    if (listBox1.Items.Count > 0 && listBox1.Items[0].ToString() == text) return;

                    AddHistoryItem(text);
                }
            }
            catch (ExternalException)
            {
                // 他のアプリがクリップボードをロックしている場合は無視する（クラッシュ防止）
            }
            catch (Exception ex)
            {
                // その他の予期せぬエラーはログに出すか無視
                System.Diagnostics.Debug.WriteLine($"Clipboard Error: {ex.Message}");
            }
        }

        // 履歴追加ロジック（UI操作の最適化）
        private void AddHistoryItem(string text)
        {
            listBox1.BeginUpdate(); // 描画を一時停止して高速化

            // すでにリストにある場合は削除して先頭に持ってくる（順序更新）
            if (listBox1.Items.Contains(text))
            {
                listBox1.Items.Remove(text);
            }

            // 先頭に追加
            listBox1.Items.Insert(0, text);

            // 最大件数を超えたら古いものを削除（メモリ節約）
            while (listBox1.Items.Count > MaxHistoryCount)
            {
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            }

            listBox1.EndUpdate(); // 描画再開
        }

        // --- UIイベントハンドラ ---

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CopySelectedItemToClipboard();
        }

        private void CopySelectedItemToClipboard()
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedText = listBox1.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    try
                    {
                        Clipboard.SetText(selectedText);
                        // ここでSetTextすると再度WndProcが呼ばれるが、
                        // OnClipboardUpdate内の「直前と同じなら無視」で弾かれるため安全
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("クリップボードへのコピーに失敗しました。\n" + ex.Message);
                    }
                }
            }
        }

        // 終了・保存処理
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_isExiting)
            {
                e.Cancel = true;
                this.Hide(); // 最小化
            }
            else
            {
                // アプリ終了時
                RemoveClipboardFormatListener(this.Handle); // 監視解除
                SaveHistory();
            }
            base.OnFormClosing(e);
        }

        private void toolStripMenuItem_Show_Click(object sender, EventArgs e)
        {
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Activate();
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            _isExiting = true;
            Application.Exit();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("履歴をすべてクリアしますか？", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                listBox1.Items.Clear();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveHistory();
            MessageBox.Show("履歴を保存しました。", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("保存された履歴を読み込みますか？\n現在のリストは上書きされます。", "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LoadHistory();
            }
        }

        // --- ファイル操作 (YAML) ---

        private void SaveHistory()
        {
            try
            {
                var history = new List<string>();
                foreach (var item in listBox1.Items)
                {
                    history.Add(item.ToString());
                }

                var serializer = new SerializerBuilder().Build();
                var yaml = serializer.Serialize(history);

                File.WriteAllText(_historyFilePath, yaml);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存エラー: {ex.Message}", "エラー");
            }
        }

        private void LoadHistory()
        {
            if (!File.Exists(_historyFilePath)) return;

            try
            {
                var yaml = File.ReadAllText(_historyFilePath);
                var deserializer = new DeserializerBuilder().Build();
                var history = deserializer.Deserialize<List<string>>(yaml);

                listBox1.BeginUpdate();
                listBox1.Items.Clear();
                if (history != null)
                {
                    // 読み込んだデータが多すぎる場合は最新のMaxHistoryCount件だけにする
                    int start = 0;
                    if (history.Count > MaxHistoryCount)
                    {
                        start = history.Count - MaxHistoryCount;
                    }

                    for (int i = start; i < history.Count; i++)
                    {
                        listBox1.Items.Add(history[i]);
                    }
                }
                listBox1.EndUpdate();
            }
            catch (Exception ex)
            {
                // 起動時のエラーはユーザーを驚かせないようログ出力程度にするか、控えめに
                System.Diagnostics.Debug.WriteLine($"読み込みエラー: {ex.Message}");
            }
        }

        private void UpdateStatus()
        {
            // 将来的にステータスバーなどを実装する場合に使用
        }
    }
}