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
            //win�Ɋ�{�I�Ȏd���������t����
            base.WndProc(ref m);
            //�N���b�v�{�[�h�̍X�V���������ꍇ
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                //�N���b�N�{�[�h�̓��e���擾
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    // ���X�g�{�b�N�X�̐擪�ɁA�R�s�[���ꂽ�e�L�X�g��ǉ�����
                    // �������������e�����X�g�ɂȂ���΁A�Ƃ���������ǉ�����ƍX�ɗǂ�
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
            //�t�H�[������ʂɕ\�����ꂽ��N���b�v�{�[�ǂ̊Ď����J�n
            //AddClipboardFormatListener(this.Handle);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(this.Handle);

        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //�������X�g�̉����̍��ڂ��I�����ꂽ��i��U��X�q�j
            if (listBox1.SelectedItem != null)
            {
                //�I������Ă��鍀�ڂ̃e�L�X�g���擾
                string selectedText = listBox1.SelectedItem.ToString();
                //�e�L�X�g���{���ɑ��݂��邩�m�F
                if (!string.IsNullOrEmpty(selectedText))
                {
                    //���̃e�L�X�g���N���b�v�{�[�h�ɐݒ�
                    Clipboard.SetText(selectedText);
                }
                //���̂Ă����Ƃ��N���b�v�{�[�h�ɃR�s�[����B
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
