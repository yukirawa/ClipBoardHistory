using System;
using System.Windows.Forms;
using ClipBoardHistory.Models;
using ClipBoardHistory.Services;

namespace ClipBoardHistory.UI
{
    // Form1クラスをinternalからpublicに変更 (デザイナとの連携のため)
    public partial class Form1 : Form
    {
        // 接続戦略に基づき、ClipboardServiceのインスタンスを保持
        private readonly ClipboardService _clipboardService;

        // Visual Studio デザイナー向け
        public Form1()
        {
            InitializeComponent();
        }

        // Program.cs からサービスを受け取るためのメインのコンストラクタ
        public Form1(ClipboardService service) : this()
        {
            _clipboardService = service;

            // 接続戦略に基づき、サービスイベントを購読
            _clipboardService.NewClipAdded += ClipboardService_NewClipAdded;

            // フォームが閉じるときに監視を停止するイベントを登録
            this.FormClosing += Form1_FormClosing;

            // UIがロードされたら、既存の履歴をロード
            this.Load += Form1_Load;

            this.Text = "ClipBoardHistory v2.0"; // タイトル設定
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // 初回ロード時に既存の履歴をロード
            LoadInitialHistory();
        }

        /// <summary>
        /// フォームが閉じられる前に、クリップボード監視を停止します。
        /// </summary>
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // アプリケーションの終了時に低レイヤーの処理をクリーンアップ
            _clipboardService.StopMonitoring();
            // イベント購読の解除
            _clipboardService.NewClipAdded -= ClipboardService_NewClipAdded;
        }

        /// <summary>
        /// サービスから新しいクリップが追加された通知を受け取り、UIを更新します。
        /// </summary>
        private void ClipboardService_NewClipAdded(object? sender, HistoryItem newItem)
        {
            // UIコントロールを操作するためには、Invokeが必要です（別スレッドからの呼び出しのため）
            if (this.InvokeRequired)
            {
                // UIスレッドで実行するための Invoke
                this.Invoke(new Action(() => UpdateHistoryList(newItem)));
            }
            else
            {
                UpdateHistoryList(newItem);
            }
        }

        /// <summary>
        /// 履歴リストを更新するUIロジック（UIスレッドで実行される）
        /// </summary>
        private void UpdateHistoryList(HistoryItem newItem)
        {
            // TODO: UIコンポーネント (例: ListBox) に新しいアイテムを追加する具体的なロジックを実装
            // 今後、UI/Form1.Designer.csで定義した lvHistory を操作するロジックをここに書きます。
            Console.WriteLine($"[UI Update] 新しいクリップを追加: {newItem.PreviewText}");
        }

        /// <summary>
        /// データベースからすべての履歴をロードし、UIに表示します。
        /// </summary>
        private void LoadInitialHistory()
        {
            var historyList = _clipboardService.GetAllHistory();
            // TODO: historyListを使ってUIコンポーネントを初期化するロジックを実装
            Console.WriteLine($"[UI Init] 既存の履歴 {historyList.Count} 件をロードしました。");
        }
    }
}