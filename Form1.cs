using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace myfarstAPP
{
    public partial class Form1 : Form
    {
        //windowsの機能を使用するためのインポート
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        //windowsから送られてくるメッセージを受け取るためのインポート
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        //windowsのメッセージを受け取るためのメソッド
        protected override void WndProc(ref Message m)
        {
            //winに基本的な仕事を押し付ける
            base.WndProc(ref m);
            //クリップボードの更新があった場合
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                //クリックボードの内容を取得
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    // リストボックスの先頭に、コピーされたテキストを追加する
                    // ※もし同じ内容がリストになければ、という条件を追加すると更に良い
                    if (!listBox1.Items.Contains(clipboardText))
                    {
                        listBox1.Items.Insert(0, clipboardText);
                    }
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
            //フォームが画面に表示されたらクリップボーどの監視を開始
            //AddClipboardFormatListener(this.Handle);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(this.Handle);

        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //もしリストの何かの項目が選択されたら（空振り帽子）
            if (listBox1.SelectedItem != null)
            {
                //選択されている項目のテキストを取得
                string selectedText = listBox1.SelectedItem.ToString();
                //テキストが本当に存在するか確認
                if (!string.IsNullOrEmpty(selectedText))
                {
                    //そのテキストをクリップボードに設定
                    Clipboard.SetText(selectedText);
                }
                //そのてきすとをクリップボードにコピーする。
                Clipboard.SetText(selectedText);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Exit();
        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
        }
    }

}
