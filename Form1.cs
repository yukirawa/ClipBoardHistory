using System.IO;
using YamlDotNet.Serialization;

namespace myfarstAPP
{
    public partial class Form1 : Form
    {
        // WIndowsAPIのインポート
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private readonly string _historyFilePath;
        private bool _isExiting = false;
        private List<ClipboardItem> _clipboardItems = new List<ClipboardItem>();

        public Form1()
        {
            InitializeComponent();
            _historyFilePath = Path.Combine(Application.StartupPath, "history.yaml");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(this.Handle);
            LoadHistory(); // 起動時に自動で読み込む

            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Text = "ClipBoardHistory";
            notifyIcon1.Visible = true;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(clipboardText) && !listBox1.Items.Contains(clipboardText))
                    {
                        listBox1.Items.Insert(0, clipboardText);
                    }
                }
            }
        }

        // ダブルクリックイベントの処理を追加
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // ダブルクリックされた項目をクリップボードにコピー
            CopySelectedItemToClipboard();
        }

        // 選択された項目をクリップボードにコピーする処理をメソッドとして独立
        private void CopySelectedItemToClipboard()
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedText = listBox1.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    Clipboard.SetText(selectedText);
                }
                //腐ったバナナ！！！
            }
        }


        // フォームが閉じられようとするときの処理 (変更なし)
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_isExiting)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                SaveHistory(); // 完全に終了する前に保存
                RemoveClipboardFormatListener(this.Handle);
            }
            base.OnFormClosing(e);
        }

        // ★★★ UIボタンのクリックイベント ★★★

        // 「表示」メニュー
        private void toolStripMenuItem_Show_Click(object sender, EventArgs e)
        {
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Activate();
        }

        // 「終了」メニュー
        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            try
            {
                _isExiting = true;
                SaveHistory(); // 終了前に履歴を保存
                RemoveClipboardFormatListener(this.Handle); // クリップボードリスナーを解除
                notifyIcon1.Visible = false; // 通知アイコンを非表示
                notifyIcon1.Dispose(); // 通知アイコンのリソースを解放
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"アプリケーションの終了中にエラーが発生しました: {ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // 強制終了
            }
        }

        // 「履歴をクリア」ボタン
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("表示中の履歴をすべてクリアしますか？\n（ファイルは削除されません）", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                listBox1.Items.Clear();
            }
        }

        // 「履歴を保存」ボタン
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveHistory();
            MessageBox.Show("現在の履歴を history.yaml に保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        // 「履歴を読み込み」ボタン
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("history.yaml から履歴を読み込みます。\n現在のリストはクリアされますが、よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBox1.Items.Clear(); // いったんリストをクリア
                LoadHistory();      // ファイルから読み込む
            }
        }


        //YAML関連
        private void SaveHistory()
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

        private void LoadHistory()
        {
            //ネギに上下なんかねーだろよー！
            if (File.Exists(_historyFilePath))
            {
                try
                {
                    var yaml = File.ReadAllText(_historyFilePath);
                    var deserializer = new DeserializerBuilder().Build();
                    var history = deserializer.Deserialize<List<string>>(yaml);

                    if (history != null)
                    {
                        listBox1.Items.AddRange(history.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("履歴ファイルの読み込みに失敗しました。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

//こんにちは。制作者のゆきらわです。このソフトをご利用いただきありがとうございます。機能の追加要望がありましたら、DiscordやTwitter、Githubなどで要望を送って下さい。できる限りすべての要望に応えて行きたいと思います。