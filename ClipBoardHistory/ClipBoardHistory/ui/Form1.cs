using System;
using System.Collections.Generic; // List用
using System.Drawing;
using System.Windows.Forms;
using ClipBoardHistory.Core.Models;
using ClipBoardHistory.Core.Services;
using ClipBoardHistory.UI.Controls; // HistoryCardを使うため

namespace ClipBoardHistory
{
    public partial class Form1 : Form
    {
        private readonly ClipboardMonitor _monitor;
        private readonly DatabaseManager _dbManager;

        // カードを並べるコンテナ
        private FlowLayoutPanel _flowLayoutPanel;

        public Form1()
        {
            InitializeComponent();
            SetupLayout(); // レイアウト構築

            // --- Coreの初期化 (前回のまま) ---
            _dbManager = new DatabaseManager();
            try { _dbManager.Initialize(); } catch { }

            _monitor = new ClipboardMonitor();
            _monitor.ClipboardChanged += Monitor_ClipboardChanged;
            this.FormClosing += (s, e) => _monitor.Dispose();
        }

        // UIの配置（Reactの render のようなもの）
        private void SetupLayout()
        {
            this.Size = new Size(900, 600); // ウィンドウサイズ

            // 1. カードを並べるエリア (FlowLayoutPanel)
            _flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, // 画面いっぱいに広げる
                AutoScroll = true,     // スクロールバーを自動表示
                BackColor = Color.FromArgb(240, 240, 240), // 背景色（薄いグレー）
                Padding = new Padding(10)
            };

            this.Controls.Add(_flowLayoutPanel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _monitor.Start();
            LoadHistory(); // 起動時に履歴を表示
        }

        // DBからデータを読み込んで表示する（Reactの useEffect -> setState 的な処理）
        private void LoadHistory()
        {
            var items = _dbManager.GetRecentItems(20); // 最新20件取得

            _flowLayoutPanel.SuspendLayout(); // 描画を一時停止（高速化）
            _flowLayoutPanel.Controls.Clear(); // 一旦クリア

            foreach (var item in items)
            {
                // コンポーネント生成
                var card = new HistoryCard();
                card.SetData(item); // propsを渡す

                // クリックイベント（詳細表示など用）
                card.Click += (s, ev) =>
                {
                    MessageBox.Show(item.Content); // とりあえずメッセージボックスで確認
                };

                _flowLayoutPanel.Controls.Add(card); // 画面に追加
            }

            _flowLayoutPanel.ResumeLayout(); // 描画再開
        }

        // クリップボード変更検知時
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
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    if (string.IsNullOrEmpty(text)) return;

                    var item = new HistoryItem
                    {
                        Content = text,
                        Type = ClipboardItemType.Text,
                        CreatedAt = DateTime.Now,
                        SearchIndex = text
                    };

                    _dbManager.Save(item);

                    // 保存したらリストを再読み込みして表示更新
                    // (Reactなら state 更新で再レンダリングされるところ)
                    LoadHistory();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}