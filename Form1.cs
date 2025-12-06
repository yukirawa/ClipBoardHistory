using System.IO;
using YamlDotNet.Serialization;

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
        private readonly ISerializer _serializer = new SerializerBuilder().Build();
        private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();
        private bool _isExiting = false;
        private string _lastClipboardText = string.Empty;

        public Form1()
        {
            InitializeComponent();
            _historyFilePath = Path.Combine(Application.StartupPath, "history.yaml");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(this.Handle);
            LoadHistory();

            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Text = "ClipBoardHistory";
            notifyIcon1.Visible = true;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_CLIPBOARDUPDATE || !Clipboard.ContainsText())
                return;

            string clipboardText = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(clipboardText) && clipboardText != _lastClipboardText)
            {
                _lastClipboardText = clipboardText;
                listBox1.Items.Insert(0, clipboardText);
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
                SaveHistory();
                RemoveClipboardFormatListener(this.Handle);
            }
            base.OnFormClosing(e);
        }

        private void toolStripMenuItem_Show_Click(object sender, EventArgs e)
        {
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            try
            {
                _isExiting = true;
                SaveHistory();
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
                listBox1.Items.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveHistory();
            MessageBox.Show("現在の履歴を history.yaml に保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("history.yaml から履歴を読み込みます。\n現在のリストはクリアされますが、よろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                listBox1.Items.Clear();
                LoadHistory();
            }
        }

        private void SaveHistory()
        {
            var history = listBox1.Items.Cast<string>().ToList();
            var yaml = _serializer.Serialize(history);
            File.WriteAllText(_historyFilePath, yaml);
        }

        private void LoadHistory()
        {
            if (!File.Exists(_historyFilePath))
                return;

            try
            {
                var yaml = File.ReadAllText(_historyFilePath);
                var history = _deserializer.Deserialize<List<string>>(yaml);

                if (history?.Count > 0)
                    listBox1.Items.AddRange(history.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show("履歴ファイルの読み込みに失敗しました。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}