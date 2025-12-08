using System;
using System.Windows.Forms;
using ClipBoardHistory.Core.Models;
using ClipBoardHistory.Core.Services;

namespace ClipBoardHistory
{
    public partial class Form1 : Form
    {
        // Core機能のインスタンス
        private readonly ClipboardMonitor _monitor;
        private readonly DatabaseManager _dbManager;

        public Form1()
        {
            // ここで Designer.cs の InitializeComponent を呼び出します
            InitializeComponent();

            // 1. データベースの準備
            _dbManager = new DatabaseManager();
            try
            {
                _dbManager.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"DB初期化エラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 2. 監視機能の準備
            _monitor = new ClipboardMonitor();
            _monitor.ClipboardChanged += Monitor_ClipboardChanged;

            // アプリ終了時の処理登録
            this.FormClosing += (s, e) => _monitor.Dispose();
        }

        /// <summary>
        /// フォームが表示されたタイミングで監視を開始
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _monitor.Start();
            System.Diagnostics.Debug.WriteLine("=== 監視を開始しました ===");
        }

        /// <summary>
        /// クリップボードに変更があったときに呼ばれるイベントハンドラ
        /// </summary>
        private void Monitor_ClipboardChanged(object? sender, EventArgs e)
        {
            // 監視スレッドから呼ばれるため、UIスレッドで処理するようにInvokeする
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ProcessClipboardData()));
            }
            else
            {
                ProcessClipboardData();
            }
        }

        /// <summary>
        /// 実際にクリップボードの中身を取り出してDBに保存する処理
        /// </summary>
        private void ProcessClipboardData()
        {
            try
            {
                // --- テキストデータの処理 ---
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    if (string.IsNullOrEmpty(text)) return;

                    // 保存用データの作成
                    var item = new HistoryItem
                    {
                        Content = text,
                        Type = ClipboardItemType.Text,
                        CreatedAt = DateTime.Now,
                        SearchIndex = text
                    };

                    // DBへ保存
                    _dbManager.Save(item);

                    // デバッグ出力
                    System.Diagnostics.Debug.WriteLine($"[保存完了] テキスト: {text.Substring(0, Math.Min(text.Length, 20))}...");
                }
                // --- 画像データの処理 (TODO) ---
                else if (Clipboard.ContainsImage())
                {
                    System.Diagnostics.Debug.WriteLine("[検知] 画像がコピーされました（保存処理は未実装）");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"読み取りエラー: {ex.Message}");
            }
        }
    }
}