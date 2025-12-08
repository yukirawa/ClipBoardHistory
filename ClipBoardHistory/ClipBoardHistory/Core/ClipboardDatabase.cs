using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ClipBoardHistory.Models;

namespace ClipBoardHistory.Core
{
    /// <summary>
    /// SQLiteデータベースへの永続化処理を専門に行うクラス。
    /// UIやクリップボード監視ロジックから完全に分離され、保守性を高めます。
    /// </summary>
    public class ClipboardDatabase
    {
        private readonly string _connectionString;

        public ClipboardDatabase()
        {
            // 接続文字列を設定
            _connectionString = $"Data Source={DatabaseConstants.DatabasePath};Version=3;";
            InitializeDatabase();
        }

        /// <summary>
        /// データベースファイルとテーブルを初期化します。
        /// </summary>
        private void InitializeDatabase()
        {
            // データベースファイルが存在しない場合は作成される
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS History (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Type TEXT NOT NULL,
                        Timestamp TEXT NOT NULL,
                        PreviewText TEXT,
                        DataPath TEXT,
                        HashValue TEXT,
                        IsPinned INTEGER DEFAULT 0
                    );";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 新しい履歴アイテムをデータベースに保存します。
        /// </summary>
        /// <param name="item">保存する履歴アイテム</param>
        /// <returns>保存が成功したらTrue</returns>
        public bool AddHistory(HistoryItem item)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    string sql = @"
                        INSERT INTO History (Type, Timestamp, PreviewText, DataPath, HashValue, IsPinned)
                        VALUES (@Type, @Timestamp, @PreviewText, @DataPath, @HashValue, @IsPinned)";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Type", item.Type);
                        command.Parameters.AddWithValue("@Timestamp", item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@PreviewText", item.PreviewText);
                        command.Parameters.AddWithValue("@DataPath", item.DataPath);
                        command.Parameters.AddWithValue("@HashValue", item.HashValue);
                        command.Parameters.AddWithValue("@IsPinned", item.IsPinned ? 1 : 0);

                        command.ExecuteNonQuery();
                        // 挿入されたIDを取得し、アイテムに設定（UI連携のため）
                        item.Id = (int)connection.LastInsertRowId;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // エラーログ出力など
                Console.WriteLine($"DBへの追加中にエラーが発生: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 全ての履歴アイテムを取得します（UI連携用）。
        /// </summary>
        /// <returns>履歴アイテムのリスト</returns>
        public List<HistoryItem> GetAllHistory()
        {
            var history = new List<HistoryItem>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                // 新しいものが上に来るように降順で取得
                string sql = "SELECT * FROM History ORDER BY Id DESC";
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(new HistoryItem
                            {
                                Id = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                Timestamp = DateTime.Parse(reader.GetString(2)),
                                PreviewText = reader.GetString(3),
                                DataPath = reader.GetString(4),
                                HashValue = reader.GetString(5),
                                IsPinned = reader.GetInt32(6) == 1
                            });
                        }
                    }
                }
            }
            return history;
        }

        // TODO: UpdateHistory, DeleteHistory, SearchHistoryメソッドを今後追加する
    }
}