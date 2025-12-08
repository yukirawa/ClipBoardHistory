using System;
using System.Drawing;
using System.Windows.Forms;
using ClipBoardHistory.Core.Models;

namespace ClipBoardHistory.UI.Controls
{
    // Designer.cs なしで動作する完結型クラス
    public class HistoryCard : UserControl
    {
        private Label _lblContent;
        private Label _lblTime;

        // 保持するデータ
        public HistoryItem Item { get; private set; }

        public HistoryCard()
        {
            // InitializeComponent();  <-- これを削除しました（自前で描画するため不要）

            // 基本設定
            this.DoubleBuffered = true; // ちらつき防止
            SetupUI();
        }

        // UIの構築（Reactのrenderのようなもの）
        private void SetupUI()
        {
            // --- カード自体のスタイル ---
            this.Size = new Size(200, 100);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(5);
            this.Cursor = Cursors.Hand; // ホバー時に指アイコン

            // --- 時間表示ラベル ---
            _lblTime = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                ForeColor = Color.Gray,
                Font = new Font("Yu Gothic UI", 8),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent // 背景透過
            };

            // --- 内容表示ラベル ---
            _lblContent = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                Font = new Font("Yu Gothic UI", 10),
                AutoEllipsis = true, // 長文は「...」にする
                BackColor = Color.Transparent,
                Text = "Loading..." // 初期値
            };

            // コントロールの追加（追加順序に注意：Fillを先に追加すると隠れることがあるため、Dockの仕様に合わせて調整）
            // WinFormsではDockプロパティの逆順でControls.Addするのが定石ですが、
            // 今回は単純に Fill と Bottom なので、Bottomを先に追加する方が安全なケースもあります。
            // ここではシンプルに追加します。
            this.Controls.Add(_lblContent);
            this.Controls.Add(_lblTime);

            // クリックイベントの伝播（ラベルをクリックしてもカードがクリックされたことにする）
            _lblContent.Click += (s, e) => this.InvokeOnClick(this, e);
            _lblTime.Click += (s, e) => this.InvokeOnClick(this, e);
        }

        // データセット用メソッド
        public void SetData(HistoryItem item)
        {
            this.Item = item;
            _lblContent.Text = item.Content;
            _lblTime.Text = item.CreatedAt.ToString("HH:mm");

            // ツールチップ（マウスを乗せたときに全文表示）
            // ※都度newするとメモリ無駄なので、簡易的に既存のToolTipを使うか、ここでの実装は省いてもOK
        }

        // メモリ解放処理（UserControlの標準的なDisposeパターン）
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ここで使ったリソース（もしあれば）を解放
                // 今回は標準コントロールだけなので特になし
            }
            base.Dispose(disposing);
        }
    }
}