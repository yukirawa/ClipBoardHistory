using System;

namespace ClipBoardHistory.Models
{
    /// <summary>
    /// クリップボード履歴の一件を表すデータ構造
    /// </summary>
    public class HistoryItem
    {
        /// <summary>データベース上のユニークID (主キー)</summary>
        public int Id { get; set; }

        /// <summary>クリップボードデータの種類 (Text, Image, FileDropなど)</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>コピーが実行された日時</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>UIで表示する概要テキスト (画像やファイルの場合はファイル名など)</summary>
        public string PreviewText { get; set; } = string.Empty;

        /// <summary>データ本体が保存されているファイルパス (大容量データの場合)</summary>
        public string DataPath { get; set; } = string.Empty;

        /// <summary>コンテンツのハッシュ値 (重複チェック用)</summary>
        public string HashValue { get; set; } = string.Empty;

        /// <summary>ピン留めされているか (0=False, 1=True)</summary>
        public bool IsPinned { get; set; }

        /// <summary>データ本体の実際の値 (DBに保存せず、メモリ上でのみ使用)</summary>
        public object? Data { get; set; }
    }
}