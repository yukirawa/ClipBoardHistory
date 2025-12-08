using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq; // ★★★ 修正: LINQ拡張メソッド (ToArray) を使用するために追加 ★★★
using ClipBoardHistory.Core;
using ClipBoardHistory.Models;

namespace ClipBoardHistory.Services
{
    /// <summary>
    /// クリップボード監視、データ処理、データベース連携を統合するサービス層。
    /// UI層は主にこのクラスを介して低レイヤーとやり取りします。
    /// </summary>
    public class ClipboardService
    {
        private readonly ClipboardDatabase _database;
        private readonly object _lock = new object();

        // UIに新しい履歴が追加されたことを通知するイベント（低レイヤーと高レイヤーの接続点）
        public event EventHandler<HistoryItem>? NewClipAdded;

        public ClipboardService()
        {
            _database = new ClipboardDatabase();
            // クリップボード更新イベントの購読を開始
            ClipboardMonitor.ClipboardUpdate += OnClipboardUpdate;
        }

        /// <summary>
        /// クリップボードの監視を開始します。UIがアプリケーション起動時に呼び出します。
        /// </summary>
        public void StartMonitoring()
        {
            try
            {
                ClipboardMonitor.Start();
                Console.WriteLine("クリップボード監視を開始しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"監視開始エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// クリップボードの監視を停止します。UIがアプリケーション終了時に呼び出します。
        /// </summary>
        public void StopMonitoring()
        {
            ClipboardMonitor.Stop();
            // イベントの購読を解除
            ClipboardMonitor.ClipboardUpdate -= OnClipboardUpdate;
            Console.WriteLine("クリップボード監視を停止しました。");
        }

        /// <summary>
        /// データベースから全ての履歴を取得します。UIの初期表示や更新に使用されます。
        /// </summary>
        public List<HistoryItem> GetAllHistory()
        {
            return _database.GetAllHistory();
        }

        // --- プライベートなイベント処理とデータ保存ロジック ---

        /// <summary>
        /// クリップボードの更新イベントが発生したときに呼び出されます。
        /// </summary>
        private void OnClipboardUpdate(object? sender, EventArgs e)
        {
            // UIスレッドをブロックしないよう、非同期で処理を実行
            Task.Run(() => ProcessClipboardChange());
        }

        /// <summary>
        /// クリップボードの内容を読み取り、保存し、データベースに登録する主要ロジック。
        /// </summary>
        private void ProcessClipboardChange()
        {
            // 複数のイベントが同時に発生しないようにロック
            lock (_lock)
            {
                // 注: WinFormsでは、クリップボードへのアクセス（Clipboard.GetXxx）は通常STAスレッドでの実行が必要です。
                // Application.Run()が動いているメインスレッド以外からアクセスすると、COM例外が発生する可能性があります。
                // 暫定的にこのままとしますが、実運用ではメインスレッドに処理を移譲するInvokeやTryGetClipboardDataのようなラッパーが必要です。

                try
                {
                    if (Clipboard.ContainsText())
                    {
                        var text = Clipboard.GetText();
                        SaveNewItem("Text", text, text);
                    }
                    else if (Clipboard.ContainsImage())
                    {
                        var image = Clipboard.GetImage();
                        // 画像はファイルとして保存し、パスをDBに記録
                        var path = SaveImageToFile(image);
                        SaveNewItem("Image", $"画像 ({image.Width}x{image.Height})", path, image);
                    }
                    else if (Clipboard.ContainsFileDropList())
                    {
                        var files = Clipboard.GetFileDropList();
                        var text = $"ファイル ({files.Count}個): {Path.GetFileName(files[0])}";
                        // ファイルパスリストをファイルとして保存
                        var path = SaveFileDropList(files);
                        SaveNewItem("FileDrop", text, path, files);
                    }
                }
                catch (Exception ex)
                {
                    // データ取得失敗時のエラー処理
                    Console.WriteLine($"クリップボード処理エラー: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 新しい履歴アイテムをDBに保存し、UIへ通知します。
        /// </summary>
        private void SaveNewItem(string type, string previewText, string content, object? data = null)
        {
            var hash = ComputeHash(content);
            // TODO: 直前のアイテムとの重複チェックロジックをここに追加する

            var newItem = new HistoryItem
            {
                Type = type,
                PreviewText = previewText.Length > 100 ? previewText.Substring(0, 100) + "..." : previewText,
                Timestamp = DateTime.Now,
                DataPath = type == "Text" ? "" : content, // テキストはDataPathを使わないか、大容量の場合のみ使う
                HashValue = hash,
                IsPinned = false,
                Data = data // データ本体はDBには保存しないが、メモリ上のイベント通知用に含める
            };

            if (_database.AddHistory(newItem))
            {
                // UI層に新しいアイテムが追加されたことを通知
                NewClipAdded?.Invoke(this, newItem);
            }
        }

        /// <summary>
        /// 画像をキャッシュフォルダにPNG形式で保存します。
        /// </summary>
        private string SaveImageToFile(Image image)
        {
            string fileName = $"{Guid.NewGuid()}.png";
            string fullPath = Path.Combine(DatabaseConstants.DataCacheDirectory, fileName);
            image.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
            return fullPath;
        }

        /// <summary>
        /// ファイルパスのリストをテキストファイルとしてキャッシュフォルダに保存します。
        /// </summary>
        private string SaveFileDropList(System.Collections.Specialized.StringCollection files)
        {
            string fileName = $"{Guid.NewGuid()}.txt";
            string fullPath = Path.Combine(DatabaseConstants.DataCacheDirectory, fileName);
            // ★★★ 修正: ToArray()を使用するためにLINQが必要でした ★★★
            File.WriteAllLines(fullPath, files.Cast<string>().ToArray());
            return fullPath;
        }

        /// <summary>
        /// コンテンツのSHA256ハッシュ値を計算します。
        /// </summary>
        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}