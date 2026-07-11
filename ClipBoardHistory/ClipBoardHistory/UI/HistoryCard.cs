using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ClipBoardHistory.Core.Models;

namespace ClipBoardHistory.UI.Controls
{
    // Designer.cs なしで動作する完結型クラス
    public class HistoryCard : UserControl
    {
        private readonly Label _lblContent;
        private readonly Label _lblTime;
        private readonly PictureBox _pictureBox;

        // 保持するデータ
        public HistoryItem Item { get; private set; } = new();

        public HistoryCard()
        {
            _lblContent = new Label();
            _lblTime = new Label();
            _pictureBox = new PictureBox();
            DoubleBuffered = true; // ちらつき防止
            SetupUI();
        }

        // UIの構築（Reactのrenderのようなもの）
        private void SetupUI()
        {
            // --- カード自体のスタイル ---
            this.Size = new Size(260, 110);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(5);
            this.Cursor = Cursors.Hand; // ホバー時に指アイコン

            _pictureBox.Dock = DockStyle.Left;
            _pictureBox.Width = 96;
            _pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            _pictureBox.BackColor = Color.FromArgb(245, 245, 245);
            _pictureBox.Visible = false;

            // --- 時間表示ラベル ---
            _lblTime.Dock = DockStyle.Bottom;
            _lblTime.Height = 20;
            _lblTime.ForeColor = Color.Gray;
            _lblTime.Font = new Font("Yu Gothic UI", 8);
            _lblTime.TextAlign = ContentAlignment.MiddleRight;
            _lblTime.BackColor = Color.Transparent;

            // --- 内容表示ラベル ---
            _lblContent.Dock = DockStyle.Fill;
            _lblContent.Padding = new Padding(5);
            _lblContent.Font = new Font("Yu Gothic UI", 10);
            _lblContent.AutoEllipsis = true;
            _lblContent.BackColor = Color.Transparent;
            _lblContent.Text = "Loading...";

            this.Controls.Add(_lblContent);
            this.Controls.Add(_lblTime);
            this.Controls.Add(_pictureBox);

            // クリックイベントの伝播（ラベルをクリックしてもカードがクリックされたことにする）
            _lblContent.Click += (s, e) => this.InvokeOnClick(this, e);
            _lblTime.Click += (s, e) => this.InvokeOnClick(this, e);
            _pictureBox.Click += (s, e) => this.InvokeOnClick(this, e);
        }

        // データセット用メソッド
        public void SetData(HistoryItem item)
        {
            this.Item = item;
            _lblTime.Text = item.CreatedAt.ToString("HH:mm");

            _pictureBox.Image?.Dispose();
            _pictureBox.Image = null;
            _pictureBox.Visible = false;

            if (item.Type == ClipboardItemType.Image &&
                !string.IsNullOrWhiteSpace(item.ImagePath) &&
                File.Exists(item.ImagePath))
            {
                _pictureBox.Image = CreateThumbnail(item.ImagePath);
                _pictureBox.Visible = _pictureBox.Image != null;
                _lblContent.Text = "Image";
                return;
            }

            _lblContent.Text = item.Content;
        }

        private static Image? CreateThumbnail(string imagePath)
        {
            try
            {
                using var stream = File.OpenRead(imagePath);
                using var source = Image.FromStream(stream);
                return new Bitmap(source, new Size(88, 88));
            }
            catch
            {
                return null;
            }
        }

        // メモリ解放処理（UserControlの標準的なDisposeパターン）
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pictureBox.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}