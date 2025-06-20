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
            //クリップボードの更新があった場合
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                //クリックボードの内容を取得
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    //とりあえずめっせーじぼっくすで表示
                    MessageBox.Show("クリップボードがこうしんされました:\n" + clipboardText);
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
    }

}
