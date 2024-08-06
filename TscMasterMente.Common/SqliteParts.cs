using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
using Windows.Media.AppBroadcasting;

namespace TscMasterMente.Common
{

    /// <summary>
    /// Sqliteのベースクラス
    /// </summary>
    public class SqliteParts
    {
        #region 列挙型


        public enum EnumMasterInfo
        {
            /// <summary>
            /// メーカーマスタ
            /// </summary>
            MakerMaster = 1,
            /// <summary>
            /// 商品マスタ
            /// </summary>
            SyoMaster = 2,
            /// <summary>
            /// 細分類マスタ
            /// </summary>
            BunMaster = 3,
            /// <summary>
            /// 属性マスタ
            /// </summary>
            ZokMaster = 4,
            /// <summary>
            /// 水準マスタ
            /// </summary>
            SuiMaster = 5,
            /// <summary>
            /// 商品属性マスタ
            /// </summary>
            SyoZokSei = 6,
            /// <summary>
            /// POPマスタ
            /// </summary>
            PopMaster = 7,
            /// <summary>
            /// POP分類マスタ
            /// </summary>
            PBunMaster = 8,
            /// <summary>
            /// 店舗マスタ
            /// </summary>
            TenpoMaster = 9,
            /// <summary>
            /// ゴンドラ什器マスタ
            /// </summary>
            GonJyu = 10,
            /// <summary>
            /// 棚段什器マスタ
            /// </summary>
            SlfJyu = 11,
        }

        #endregion

        #region カプセル化メソッド        



        /// <summary>
        /// テーブルの存在チェック
        /// </summary>
        /// <param name="argTableName">テーブル名</param>
        /// <returns></returns>
        private bool IsExistsTable(string argTableName)
        {
            using (var wCon = ConnectDbFile())
            {
                wCon.Open();
                using (var wCmd = wCon.CreateCommand())
                {
                    wCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='" + argTableName + "'";
                    var wResult = wCmd.ExecuteScalar();
                    return Convert.ToInt32(wResult) > 0;
                }
            }
        }

        /// <summary>
        /// データベースファイルの存在チェック
        /// </summary>
        /// <returns></returns>
        private bool CheckDbFile()
        {
            return System.IO.File.Exists(GetDbFilePath());
        }

        /// <summary>
        /// データベースファイルの削除
        /// </summary>
        private void DeleteDbFile()
        {
            if (CheckDbFile())
            {
                System.IO.File.Delete(GetDbFilePath());
            }
        }

        /// <summary>
        /// データベースファイルの接続
        /// </summary>
        /// <returns></returns>
        private SqliteConnection ConnectDbFile()
        {
            return new SqliteConnection("Data Source=" + GetDbFilePath());
        }

        /// <summary>
        /// TscAppInfo.xmlの取得
        /// </summary>
        /// <param name="argKey">キー</param>
        /// <returns></returns>
        private string GetTscAppInfoConfig(string argKey)
        {
            var infApp = System.IO.Path.Combine(GetDbDirPath(), "TscAppConfig.xml");
            var wConfig = XElement.Load(infApp);
            return wConfig.Element("AppSettings").Element(argKey).Value;
        }

        /// <summary>
        /// Exe格納フォルダの取得
        /// </summary>
        /// <returns></returns>
        private string GetDbDirPath()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return System.IO.Path.GetDirectoryName(exePath);
        }

        #endregion

        #region 呼び出し用メソッド

        #region 共通

        /// <summary>
        /// データベースファイルパス
        /// </summary>
        /// <returns></returns>
        public string GetDbFilePath()
        {
            string DbFileName = GetTscAppInfoConfig("SqliteDbFileName");

            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directoryPath = System.IO.Path.GetDirectoryName(exePath);

            return System.IO.Path.Combine(directoryPath, DbFileName);
        }

        #endregion

        #region 初回起動時用

        /// <summary>
        /// マスタテーブルの作成
        /// </summary>
        public void CretateTables()
        {
            var wDbVer = GetTscAppInfoConfig("DbVer");

            #region マスタテキスト出力先フォルダ

            var wMstTxtDirName = GetTscAppInfoConfig("MasterTextDirectoryName");
            var wMstTxtDirPath = System.IO.Path.Combine(GetDbDirPath(), wMstTxtDirName);

            // マスタテキスト出力先フォルダの作成
            if (!System.IO.Directory.Exists(wMstTxtDirPath))
            {
                System.IO.Directory.CreateDirectory(wMstTxtDirPath);
            }
            #endregion

            #region マスタExcel出力先フォルダ

            var wMstExcelDirName = GetTscAppInfoConfig("MasterExcelDirectoryName");
            var wMstExcelDirPath = System.IO.Path.Combine(GetDbDirPath(), wMstExcelDirName);

            // マスタExcel出力先フォルダの作成
            if (!System.IO.Directory.Exists(wMstExcelDirPath))
            {
                System.IO.Directory.CreateDirectory(wMstExcelDirPath);
            }

            #endregion

            if (!CheckDbFile())
            {
                string wSql = "";

                #region アプリの情報管理テーブル

                #region アプリ情報

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS AppInfo (";
                wSql += " DbVer TEXT NOT NULL PRIMARY KEY CHECK(length(DbVer) <= 10), ";
                wSql += " MasterTextDir TEXT CHECK(length(MasterTextDir) <= 1000), ";
                wSql += " MasterExcelDir TEXT CHECK(length(MasterExcelDir) <= 1000), ";
                wSql += " PlanetWriteDir TEXT CHECK(length(PlanetWriteDir) <= 1000), ";
                wSql += " TscConfigPath TEXT CHECK(length(TscConfigPath) <= 1000) ";
                wSql += " );";
                ExecuteSql(wSql);


                var wPlanetDirName = GetTscAppInfoConfig("PlanetWriteDirectoryName");
                var wPlanetDirPath = System.IO.Path.Combine(GetDbDirPath(), wPlanetDirName);
                var wTscFilePath = System.IO.Path.Combine(GetDbDirPath(), "Tana.ini");

                //アプリの設定情報を登録
                wSql = $"INSERT INTO AppInfo(DbVer, MasterTextDir, MasterExcelDir, PlanetWriteDir, TscConfigPath) ";
                wSql += $" VALUES ('{wDbVer}', '{wMstTxtDirPath}', '{wMstExcelDirPath}', '{wPlanetDirPath}', '{wTscFilePath}');";
                ExecuteSql(wSql);

                #endregion

                #region マスタファイル情報
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS Masters (";
                wSql += " MasterCode INTEGER NOT NULL PRIMARY KEY, ";
                wSql += " MasterName TEXT CHECK(length(MasterName) <= 255), ";
                wSql += " MasterFileName TEXT CHECK(length(MasterFileName) <= 255), ";
                wSql += " ErrFileName TEXT CHECK(length(ErrFileName) <= 255) ";
                wSql += " );";
                ExecuteSql(wSql);

                //マスタファイル情報を登録

                #region マスタファイル情報の登録

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.MakerMaster}, 'メーカーマスタ', 'M_ﾒｰｶｰ.txt', 'M_ﾒｰｶｰ_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.SyoMaster}, '商品マスタ', 'M_商品.txt', 'M_商品_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.BunMaster}, '細分類マスタ', 'M_細分類.txt', 'M_細分類_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.ZokMaster}, '属性マスタ', 'M_属性.txt', 'M_属性_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.SuiMaster}, '水準マスタ', 'M_水準.txt', 'M_水準_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.SyoZokSei}, '商品属性マスタ', 'M_商属.txt', 'M_商属_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.PopMaster}, 'POPマスタ', 'M_ﾊﾟﾈﾙ.txt', 'M_ﾊﾟﾈﾙ_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.PBunMaster}, 'POP分類マスタ', 'M_P分類.txt', 'M_P分類_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.TenpoMaster}, '店舗マスタ', 'M_店舗.txt', 'M_店舗_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.GonJyu}, 'ゴンドラ什器マスタ', 'M_GONJYU.txt', 'M_GONJYU_ERR.txt');";
                ExecuteSql(wSql);

                wSql = $" INSERT INTO Masters(MasterCode, MasterName, MasterFileName, ErrFileName)";
                wSql += $" VALUES ({(int)EnumMasterInfo.SlfJyu}, '棚段什器マスタ', 'M_SLFJYU.txt', 'M_SLFJYU_ERR.txt');";
                ExecuteSql(wSql);

                #endregion

                #endregion

                #endregion

                #region マスタ関連

                #region メーカマスタ

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS MkMaster (";
                wSql += " MkCode TEXT NOT NULL PRIMARY KEY CHECK(length(MkCode) <= 20), ";
                wSql += " MkName_Kana TEXT CHECK(length(MkName_Kana) <= 255), ";
                wSql += " MkName TEXT CHECK(length(MkName) <= 255) ";
                wSql += " );";
                ExecuteSql(wSql);

                #endregion

                #region 商品マスタ

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS SyoMaster( ";
                wSql += " JanCode TEXT NOT NULL PRIMARY KEY CHECK(length(JanCode) <= 16), ";
                wSql += " MkCode TEXT CHECK(length(MkCode) <= 20), ";
                wSql += " StoCode TEXT CHECK(length(StoCode) <= 20), ";
                wSql += " TanName TEXT CHECK(length(TanName) <= 255), ";
                wSql += " ItemName TEXT CHECK(length(ItemName) <= 255), ";
                wSql += " W INTEGER, ";
                wSql += " H INTEGER, ";
                wSql += " D INTEGER, ";
                wSql += " PCase INTEGER, ";
                wSql += " BunCode TEXT CHECK(length(BunCode) <= 20), ";
                wSql += " Price REAL, ";
                wSql += " Cost REAL, ";
                wSql += " Attr3 TEXT CHECK(length(Attr3) <= 8), ";
                wSql += " Attr4 TEXT CHECK(length(Attr4) <= 8), ";
                wSql += " Attr5 TEXT CHECK(length(Attr5) <= 8), ";
                wSql += " Attr6 TEXT CHECK(length(Attr6) <= 8), ";
                wSql += " Attr7 INTEGER DEFAULT 0, ";
                wSql += " Attr8 TEXT CHECK(length(Attr8) <= 8) ";
                wSql += " );";
                ExecuteSql(wSql);

                #endregion

                #region 細分類マスタ

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS BunMaster (";
                wSql += " BunCode TEXT NOT NULL PRIMARY KEY CHECK(length(BunCode) <= 20), ";
                wSql += " BunName TEXT CHECK(length(BunName) <= 255) ";
                wSql += " );";
                ExecuteSql(wSql);

                #endregion

                #region 属性マスタ

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS ZokMaster (";
                wSql += " ZkCode TEXT NOT NULL CHECK(length(ZkCode) <= 20), ";
                wSql += " ZkName TEXT CHECK(length(ZkName) <= 255), ";
                wSql += " BunCode TEXT CHECK(length(BunCode) <= 20), ";
                wSql += " PRIMARY KEY (ZkCode) ";
                wSql += " );";
                ExecuteSql(wSql);

                #endregion

                #region 水準マスタ

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS SuiMaster (";
                wSql += " ZkCode TEXT NOT NULL CHECK(length(ZkCode) <= 20), ";
                wSql += " SuiCode TEXT NOT NULL CHECK(length(SuiCode) <= 20), ";
                wSql += " SuiName TEXT CHECK(length(SuiName) <= 255), ";
                wSql += " BunCode TEXT CHECK(length(BunCode) <= 20), ";
                wSql += " PRIMARY KEY (ZkCode, SuiCode) ";
                wSql += " );";
                ExecuteSql(wSql);

                #endregion

                #region 商品属性マスタ

                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS SyoZokSei (";
                wSql += " JanCode TEXT NOT NULL CHECK(length(JanCode) <= 16), ";
                wSql += " ZkCode TEXT NOT NULL CHECK(length(ZkCode) <= 20), ";
                wSql += " SuiCode TEXT NOT NULL CHECK(length(SuiCode) <= 20), ";
                wSql += " PRIMARY KEY (JanCode, ZkCode, SuiCode) ";
                wSql += " );";
                ExecuteSql(wSql);

                #endregion

                #region POPマスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS PopMaster ( ";
                wSql += " PopCode TEXT NOT NULL PRIMARY KEY CHECK(length(PopCode) <= 20), ";
                wSql += " PopName TEXT CHECK(length(PopName) <= 255), ";
                wSql += " W INTEGER, ";
                wSql += " H INTEGER, ";
                wSql += " D INTEGER, ";
                wSql += " PBunCode TEXT CHECK(length(PBunCode) <= 20) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #region POP分類マスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS PBunMaster ( ";
                wSql += " PBunCode TEXT NOT NULL PRIMARY KEY CHECK(length(PBunCode) <= 10), ";
                wSql += " PBunName TEXT CHECK(length(PBunName) <= 20) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #region 法人マスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS CompMaster ( ";
                wSql += " CompCode TEXT NOT NULL PRIMARY KEY CHECK(length(CompCode) <= 20), ";
                wSql += " CompName TEXT CHECK(length(CompName) <= 255) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #region エリアマスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS AreaMaster ( ";
                wSql += " AreaCode TEXT NOT NULL PRIMARY KEY CHECK(length(AreaCode) <= 20), ";
                wSql += " AreaName TEXT CHECK(length(AreaName) <= 255) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #region 店舗マスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS TenpoMaster ( ";
                wSql += " TenpoId INTEGER PRIMARY KEY AUTOINCREMENT, ";
                wSql += " TenpoNumber TEXT NOT NULL CHECK(length(TenpoNumber) <= 10), ";
                wSql += " TenpoCode TEXT CHECK(length(TenpoCode) <= 10), ";
                wSql += " TenpoTanName TEXT CHECK(length(TenpoTanName) <= 50), ";
                wSql += " TenpoName TEXT CHECK(length(TenpoName) <= 50), ";
                wSql += " CompCode TEXT CHECK(length(CompCode) <= 20), ";
                wSql += " CompName TEXT CHECK(length(CompName) <= 255), ";
                wSql += " AreaCode TEXT CHECK(length(AreaCode) <= 20), ";
                wSql += " AreaName TEXT CHECK(length(AreaName) <= 255), ";
                wSql += " CONSTRAINT UniqueTenpoNumber UNIQUE (TenpoNumber) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #region ゴンドラ什器マスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS GonJyu ( ";
                wSql += " GonJyuName TEXT NOT NULL PRIMARY KEY CHECK(length(GonJyuName) <= 50) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #region 棚段什器マスタ
                wSql = "";
                wSql += " CREATE TABLE IF NOT EXISTS SlfJyu ( ";
                wSql += " SlfJyuName TEXT NOT NULL PRIMARY KEY CHECK(length(SlfJyuName) <= 50) ";
                wSql += " ); ";
                ExecuteSql(wSql);

                #endregion

                #endregion

            }
            else
            {
                string wSql = "";
                wSql = $"UPDATE AppInfo Set DbVer = '{wDbVer}';";
                ExecuteSql(wSql);
            }
        }

        #endregion

        #region AppInfo固有

        /// <summary>
        /// AppInfoの取得
        /// </summary>
        /// <param name="argKey">取得カラム</param>
        /// <returns></returns>
        public string GetAppInfo(string argKey)
        {
            string wSql = $"SELECT {argKey} FROM AppInfo;";
            using (var wCon = ConnectDbFile())
            {
                wCon.Open();
                using (var wCmd = wCon.CreateCommand())
                {
                    wCmd.CommandText = wSql;
                    return wCmd.ExecuteScalar().ToString();
                }
            }
        }

        /// <summary>
        /// AppInfoの更新
        /// </summary>
        /// <param name="argKey">設定カラム</param>
        /// <param name="argValue">設定値</param>
        public void UpdateAppInfo(string argKey, string argValue)
        {
            string wSql = $"UPDATE AppInfo SET {argKey} = '{argValue}';";
            ExecuteSql(wSql);
        }
        #endregion

        #region ユーティリティメソッド

        /// <summary>
        /// Nullデータのチェック
        /// </summary>
        /// <param name="argVal">チェックデータ</param>
        /// <returns></returns>
        public bool IsNullData(object argVal)
        {
            if (argVal != null)
            {
                string xData = argVal.ToString().Trim();
                if (!string.IsNullOrEmpty(xData))
                {
                    // 制御文字を削除
                    var yData = string.Concat(xData.Where(c => !char.IsControl(c)));
                    if (!string.IsNullOrEmpty(yData) && !string.IsNullOrEmpty(yData.Trim()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region 通常用

        /// <summary>
        /// Select文の実行
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="argSql">SQL(Select)</param>
        /// <returns></returns>
        public List<T> GetSelectResult<T>(string argSql) where T : new()
        {
            var wResult = new List<T>();

            using (var connection = ConnectDbFile())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = argSql;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T instance = new T();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                                if (propertyInfo != null && !reader.IsDBNull(i))
                                {
                                    //propertyInfo.SetValue(instance, Convert.ChangeType(reader.GetValue(i), propertyInfo.PropertyType), null);
                                    object value = reader.GetValue(i);
                                    Type propertyType = propertyInfo.PropertyType;
                                    Type nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);

                                    if (nullableUnderlyingType != null)
                                    {
                                        // プロパティが null 許容型の場合
                                        value = Convert.ChangeType(value, nullableUnderlyingType);
                                    }
                                    else
                                    {
                                        // プロパティが null 許容型でない場合
                                        value = Convert.ChangeType(value, propertyType);
                                    }

                                    propertyInfo.SetValue(instance, value, null);
                                }
                            }
                            wResult.Add(instance);
                        }
                    }
                }
            }

            return wResult;
        }


        /// <summary>
        /// SQLの実行
        /// </summary>
        /// <param name="argSql"></param>
        public int ExecuteSql(string argSql)
        {
            return ExecuteSql(argSql, null);
        }

        /// <summary>
        /// SQLの実行
        /// </summary>
        /// <param name="argSql">SQL</param>
        /// <param name="argParam">パラメータ</param>
        /// <returns></returns>
        /// <remarks>
        /// トランザクション内でのSQLの実行を想定
        /// </remarks>
        public int ExecuteSql(string argSql, List<SqliteParameter> argParam)
        {
            using (var wCon = ConnectDbFile())
            {
                wCon.Open();
                using (var wCmd = wCon.CreateCommand())
                {
                    wCmd.CommandText = argSql;
                    if (argParam != null && argParam.Count > 0)
                    {
                        foreach (var iParm in argParam)
                        {
                            wCmd.Parameters.Add(iParm);
                        }
                    }

                    return wCmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// DB最適化
        /// </summary>
        /// <param name="argDbPath"></param>
        public void ExecuteVacuum(string argDbPath)
        {
            if (string.IsNullOrEmpty(argDbPath))
            {
                return;
            }
            else if (!System.IO.File.Exists(argDbPath))
            {
                return;
            }

            using (var wCon = new SqliteConnection("Data Source=" + GetDbFilePath()))
            {
                wCon.Open();
                using (var wCmd = wCon.CreateCommand())
                {
                    wCmd.CommandText = "VACUUM;";
                    wCmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region トランザクション用

        /// <summary>
        /// コネクションの取得
        /// </summary>
        /// <returns></returns>
        public SqliteConnection GetConnection()
        {
            var wCon = ConnectDbFile();
            return wCon;
        }

        /// <summary>
        /// Select文の実行
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="argConn">コネクション</param>
        /// <param name="argSql">SQL(Select)</param>
        /// <returns></returns>
        /// <remarks>
        /// トランザクション内でのSelect文の実行を想定
        /// </remarks>
        public List<T> GetSelectResult<T>(SqliteConnection argConn, string argSql) where T : new()
        {
            var wResult = new List<T>();

            using (var command = argConn.CreateCommand())
            {
                command.CommandText = argSql;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T instance = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            PropertyInfo propertyInfo = typeof(T).GetProperty(reader.GetName(i));
                            if (propertyInfo != null && !reader.IsDBNull(i))
                            {
                                //propertyInfo.SetValue(instance, Convert.ChangeType(reader.GetValue(i), propertyInfo.PropertyType), null);
                                object value = reader.GetValue(i);
                                Type propertyType = propertyInfo.PropertyType;
                                Type nullableUnderlyingType = Nullable.GetUnderlyingType(propertyType);

                                if (nullableUnderlyingType != null)
                                {
                                    // プロパティが null 許容型の場合
                                    value = Convert.ChangeType(value, nullableUnderlyingType);
                                }
                                else
                                {
                                    // プロパティが null 許容型でない場合
                                    value = Convert.ChangeType(value, propertyType);
                                }

                                propertyInfo.SetValue(instance, value, null);
                            }
                        }
                        wResult.Add(instance);
                    }
                }
            }
            return wResult;
        }

        /// <summary>
        /// SQLの実行
        /// </summary>
        /// <param name="argConn">コネクション</param>
        /// <param name="argSql">SQL</param>
        /// <param name="argParam">パラメータ</param>
        /// <returns></returns>
        /// <remarks>
        /// トランザクション内でのSQLの実行を想定
        /// </remarks>
        public int ExecuteSql(SqliteConnection argConn, string argSql, List<SqliteParameter> argParam)
        {
            using (var wCmd = argConn.CreateCommand())
            {
                wCmd.CommandText = argSql;
                if (argParam != null && argParam.Count > 0)
                {
                    foreach (var iParm in argParam)
                    {
                        wCmd.Parameters.Add(iParm);
                    }
                }
                return wCmd.ExecuteNonQuery();
            }
        }

        #endregion

        #endregion
    }
}
