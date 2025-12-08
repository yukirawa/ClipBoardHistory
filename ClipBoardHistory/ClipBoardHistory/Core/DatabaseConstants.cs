using System;
using System.IO;

namespace ClipBoardHistory.Core;

public static class DatabaseConstants
{
    // アプリのデータ保存フォルダ (%LocalAppData%/ClipBoardHistoryV2)
    public static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ClipBoardHistoryV2"
    );

    // 画像キャッシュフォルダ
    public static readonly string ImageCacheFolder = Path.Combine(AppDataFolder, "Cache");

    // データベースファイルのパス
    public static readonly string DbFilePath = Path.Combine(AppDataFolder, "history.db");

    // 接続文字列
    public static readonly string ConnectionString = $"Data Source={DbFilePath};Version=3;";

    // テーブル名
    public const string TableName = "History";
}