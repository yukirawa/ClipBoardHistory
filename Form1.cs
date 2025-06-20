using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace myfarstAPP
{
    public partial class Form1 : Form
    {
        //windows�̋@�\���g�p���邽�߂̃C���|�[�g
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        //windows���瑗���Ă��郁�b�Z�[�W���󂯎�邽�߂̃C���|�[�g
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        //windows�̃��b�Z�[�W���󂯎�邽�߂̃��\�b�h
        protected override void WndProc(ref Message m)
        {
            //�N���b�v�{�[�h�̍X�V���������ꍇ
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                //�N���b�N�{�[�h�̓��e���擾
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    //�Ƃ肠�����߂����[���ڂ������ŕ\��
                    MessageBox.Show("�N���b�v�{�[�h���������񂳂�܂���:\n" + clipboardText);
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
            //�t�H�[������ʂɕ\�����ꂽ��N���b�v�{�[�ǂ̊Ď����J�n
            //AddClipboardFormatListener(this.Handle);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(this.Handle);

        }
    }

}
