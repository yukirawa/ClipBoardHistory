using System;
using System.Collections.Generic;
using System.Data.SQLite; // NuGet: System.Data.SQLite が必要
using System.IO;
using ClipBoardHistory.Core.Models;

namespace ClipBoardHistory.Core.Services;

/// <summary>
/// SQLiteデータベースへのアクセスを一手に引き受けるクラス
/// </summary>
public class DatabaseManager
{
    /// <summary>
    /// データベースの初期化（テーブル作成）を行います。
    /// アプリ起動時に一度だけ呼び出してください。
    /// </summary>
    public void Initialize()
    {
        // 保存先フォルダがなければ作成
        var folder = Path.GetDirectoryName(DatabaseConstants.DbFilePath);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // キャッシュフォルダもついでに作成
        if (!Directory.Exists(DatabaseConstants.ImageCacheFolder))
        {
            Directory.CreateDirectory(DatabaseConstants.ImageCacheFolder);
        }

        // テーブル作成のSQL
        // 高速化のため、SearchIndexにもインデックスを貼る想定（今回は簡易実装）
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Content TEXT,
                Type INTEGER,
                CreatedAt TEXT,
                ImagePath TEXT,
                SearchIndex TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_created_at ON History(CreatedAt DESC);
        ";

        ExecuteNonQuery(createTableSql);
    }

    /// <summary>
    /// 履歴アイテムを1件保存します。
    /// </summary>
    public void Save(HistoryItem item)
    {
        const string sql = @"
            INSERT INTO History (Content, Type, CreatedAt, ImagePath, SearchIndex)
            VALUES (@Content, @Type, @CreatedAt, @ImagePath, @SearchIndex)
        ";

        var parameters = new Dictionary<string, object?>
        {
            { "@Content", item.Content },
            { "@Type", (int)item.Type },
            { "@CreatedAt", item.CreatedAt.ToString("o") }, // ISO 8601形式で保存
            { "@ImagePath", item.ImagePath },
            { "@SearchIndex", item.SearchIndex }
        };

        ExecuteNonQuery(sql, parameters);
    }

    /// <summary>
    /// 最新の履歴を取得します。
    /// </summary>
    /// <param name="limit">取得件数</param>
    public List<HistoryItem> GetRecentItems(int limit = 50)
    {
        const string sql = "SELECT * FROM History ORDER BY Id DESC LIMIT @Limit";
        var parameters = new Dictionary<string, object?> { { "@Limit", limit } };
        var list = new List<HistoryItem>();

        using var connection = new SQLiteConnection(DatabaseConstants.ConnectionString);
        connection.Open();
        using var command = new SQLiteCommand(sql, connection);
        foreach (var p in parameters) command.Parameters.AddWithValue(p.Key, p.Value);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToItem(reader));
        }

        return list;
    }

    /// <summary>
    /// 検索機能（簡易版）
    /// </summary>
    public List<HistoryItem> Search(string keyword)
    {
        // 実際はFTSなどを使うと高速ですが、まずはLIKE検索で実装
        const string sql = "SELECT * FROM History WHERE Content LIKE @Keyword ORDER BY Id DESC LIMIT 50";
        var parameters = new Dictionary<string, object?> { { "@Keyword", $"%{keyword}%" } };

        var list = new List<HistoryItem>();
        using var connection = new SQLiteConnection(DatabaseConstants.ConnectionString);
        connection.Open();
        using var command = new SQLiteCommand(sql, connection);
        foreach (var p in parameters) command.Parameters.AddWithValue(p.Key, p.Value);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReaderToItem(reader));
        }
        return list;
    }

    // 汎用的なSQL実行メソッド
    private void ExecuteNonQuery(string sql, Dictionary<string, object?>? parameters = null)
    {
        using var connection = new SQLiteConnection(DatabaseConstants.ConnectionString);
        connection.Open();
        using var command = new SQLiteCommand(sql, connection);

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            }
        }
        command.ExecuteNonQuery();
    }

    // DBの行データをオブジェクトに変換
    private HistoryItem MapReaderToItem(SQLiteDataReader reader)
    {
        return new HistoryItem
        {
            Id = Convert.ToInt64(reader["Id"]),
            Content = reader["Content"].ToString() ?? string.Empty,
            Type = (ClipboardItemType)Convert.ToInt32(reader["Type"]),
            CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString() ?? DateTime.Now.ToString()),
            ImagePath = reader["ImagePath"]?.ToString(),
            SearchIndex = reader["SearchIndex"]?.ToString() ?? string.Empty
        };
    }
}