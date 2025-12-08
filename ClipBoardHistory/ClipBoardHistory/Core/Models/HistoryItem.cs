using System;

namespace ClipBoardHistory.Core.Models;

/// <summary>
/// 履歴データの種類
/// </summary>
public enum ClipboardItemType
{
    Text = 0,
    Image = 1,
    File = 2 // 将来用（ファイルコピーの履歴など）
}

/// <summary>
/// 履歴アイテム1件を表すモデル
/// </summary>
public class HistoryItem
{
    /// <summary>
    /// データベース上の主キー
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// テキストデータ（画像の場合は空、またはOCRテキストなど）
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// データの種類
    /// </summary>
    public ClipboardItemType Type { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 画像キャッシュへのフルパス（Type=Imageのときのみ使用）
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// 検索用インデックス（軽量化のため、表示用とは別に持つ場合に使用）
    /// </summary>
    public string SearchIndex { get; set; } = string.Empty;
}