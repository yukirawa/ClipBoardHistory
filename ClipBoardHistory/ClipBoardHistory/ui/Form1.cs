using System;
using System.Collections.Generic; // List�p
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using ClipBoardHistory.Core;
using ClipBoardHistory.Core.Models;
using ClipBoardHistory.Core.Services;
using ClipBoardHistory.UI.Controls; // HistoryCard���g������

namespace ClipBoardHistory
{
    public partial class Form1 : Form
    {
        private const int MaxVisibleItems = 20;
        private readonly ClipboardMonitor _monitor;
        private readonly DatabaseManager _dbManager;
        private string? _lastClipboardSignature;

        // �J�[�h����ׂ�R���e�i
        private FlowLayoutPanel _flowLayoutPanel = null!;

        public Form1()
        {
            InitializeComponent();
            SetupLayout(); // ���C�A�E�g�\�z

            // --- Core�̏����� (�O��̂܂�) ---
            _dbManager = new DatabaseManager();
            try { _dbManager.Initialize(); } catch { }

            _monitor = new ClipboardMonitor();
            _monitor.ClipboardChanged += Monitor_ClipboardChanged;
            this.FormClosing += (s, e) => _monitor.Dispose();
        }

        // UI�̔z�u�iReact�� render �̂悤�Ȃ��́j
        private void SetupLayout()
        {
            this.Size = new Size(900, 600); // �E�B���h�E�T�C�Y

            // 1. �J�[�h����ׂ�G���A (FlowLayoutPanel)
            _flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, // ��ʂ����ς��ɍL����
                AutoScroll = true,     // �X�N���[���o�[�������\��
                BackColor = Color.FromArgb(240, 240, 240), // �w�i�F�i�����O���[�j
                Padding = new Padding(10)
            };

            this.Controls.Add(_flowLayoutPanel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _monitor.Start();
            LoadHistory(); // �N�����ɗ�����\��
        }

        // DB����f�[�^��ǂݍ���ŕ\������iReact�� useEffect -> setState �I�ȏ����j
        private void LoadHistory()
        {
            var items = _dbManager.GetRecentItems(MaxVisibleItems); // �ŐV20���擾

            _flowLayoutPanel.SuspendLayout(); // �`����ꎞ��~�i�������j
            _flowLayoutPanel.Controls.Clear(); // ��U�N���A

            foreach (var item in items)
            {
                _flowLayoutPanel.Controls.Add(CreateHistoryCard(item)); // ��ʂɒǉ�
            }

            _flowLayoutPanel.ResumeLayout(); // �`��ĊJ
        }

        // �N���b�v�{�[�h�ύX���m��
        private void Monitor_ClipboardChanged(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ProcessClipboardData()));
            }
            else
            {
                ProcessClipboardData();
            }
        }

        private void ProcessClipboardData()
        {
            try
            {
                if (!TryCreateHistoryItem(out var item, out var signature))
                {
                    return;
                }

                if (_lastClipboardSignature == signature)
                {
                    return;
                }

                _lastClipboardSignature = signature;
                _dbManager.Save(item);
                AddHistoryCard(item, prepend: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private HistoryCard CreateHistoryCard(HistoryItem item)
        {
            var card = new HistoryCard();
            card.SetData(item);
            card.Click += (s, ev) => MessageBox.Show(item.Content);
            return card;
        }

        private void AddHistoryCard(HistoryItem item, bool prepend)
        {
            var card = CreateHistoryCard(item);

            _flowLayoutPanel.SuspendLayout();
            _flowLayoutPanel.Controls.Add(card);
            if (prepend)
            {
                _flowLayoutPanel.Controls.SetChildIndex(card, 0);
            }

            while (_flowLayoutPanel.Controls.Count > MaxVisibleItems)
            {
                var lastIndex = _flowLayoutPanel.Controls.Count - 1;
                var control = _flowLayoutPanel.Controls[lastIndex];
                _flowLayoutPanel.Controls.RemoveAt(lastIndex);
                control.Dispose();
            }
            _flowLayoutPanel.ResumeLayout();
        }

        private bool TryCreateHistoryItem(out HistoryItem item, out string signature)
        {
            item = new HistoryItem();
            signature = string.Empty;

            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }

                item = new HistoryItem
                {
                    Content = text,
                    Type = ClipboardItemType.Text,
                    CreatedAt = DateTime.Now,
                    SearchIndex = text
                };
                signature = $"text:{text}";
                return true;
            }

            if (Clipboard.ContainsImage())
            {
                using var image = Clipboard.GetImage();
                if (image == null)
                {
                    return false;
                }

                var (imagePath, hash) = SaveImageToCache(image);
                item = new HistoryItem
                {
                    Content = "[Image]",
                    Type = ClipboardItemType.Image,
                    CreatedAt = DateTime.Now,
                    ImagePath = imagePath,
                    SearchIndex = hash
                };
                signature = $"image:{hash}";
                return true;
            }

            return false;
        }

        private static (string path, string hash) SaveImageToCache(Image image)
        {
            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            var bytes = memoryStream.ToArray();
            var hash = Convert.ToHexString(SHA256.HashData(bytes));
            var filePath = Path.Combine(DatabaseConstants.ImageCacheFolder, $"{hash}.png");

            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, bytes);
            }

            return (filePath, hash);
        }
    }
}