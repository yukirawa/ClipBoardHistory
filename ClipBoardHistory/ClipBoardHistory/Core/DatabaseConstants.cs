using System;
using System.IO;

namespace ClipBoardHistory.Core
{
    /// <summary>
    /// データベースとファイルパスに関する定数を管理するクラス
    /// </summary>
    public static class DatabaseConstants
    {
        // アプリケーションIDと名前
        private const string AppName = "ClipBoardHistory";

        /// <summary>SQLiteデータベースファイルのフルパスを取得します。</summary>
        public static string DatabasePath
        {
            get
            {
                // AppData\Local\<AppName> フォルダを取得
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appDirectory = Path.Combine(localAppData, AppName);

                // ディレクトリが存在しない場合は作成
                if (!Directory.Exists(appDirectory))
                {
                    Directory.CreateDirectory(appDirectory);
                }

                // データベースファイルのパスを結合
                return Path.Combine(appDirectory, "history.sqlite");
            }
        }

        /// <summary>画像や大容量テキストデータなどを保存するキャッシュディレクトリのパスを取得します。</summary>
        public static string DataCacheDirectory
        {
            get
            {
                string path = Path.Combine(Path.GetDirectoryName(DatabasePath)!, "Cache");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }
    }
}