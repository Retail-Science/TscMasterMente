using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.UI.Xaml.Shapes;

namespace TscMasterMente.Common
{
    public class CsvHelperParts
    {
        public CsvHelperParts()
        {
            // エンコーディングプロバイダーを登録
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region 出力関連

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="argPath">ファイル出力パス</param>
        /// <param name="argData">出力データ</param>
        /// <param name="argIsHeader">ヘッダー表示(true:表示/false:非表示)</param>
        /// <param name="argDelimiter">データ区切り文字</param>
        /// <param name="argIsAppend">>追記モード(true:追記モード/false:上書きモード)</param>
        public void WriteFile<T>(string argPath, IEnumerable<T> argData, bool argIsHeader, string argDelimiter, bool argIsAppend)
        {
            // CsvConfigurationの設定（Shift-JISとして出力）
            var wConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.GetEncoding("shift_jis"),
                HasHeaderRecord = argIsHeader,
                Delimiter = argDelimiter
            };

            // CSVファイルへの書き込み
            using (var wWriter = new StreamWriter(argPath, argIsAppend, wConfig.Encoding))
            using (var wCsv = new CsvWriter(wWriter, wConfig))
            {
                wCsv.WriteRecords(argData);
            }
        }

        /// <summary>
        /// マスタファイル出力
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="argPath">ファイル出力パス</param>
        /// <param name="argData">出力データ</param>
        public void WriteMasterFile<T>(string argPath, IEnumerable<T> argData)
        {
            WriteFile(argPath, argData, false, "\t", false);
        }

        #endregion

        #region 読込関連

        #region Del

        ///// <summary>
        ///// ファイル読込
        ///// </summary>
        ///// <typeparam name="T">型</typeparam>
        ///// <param name="argPath">取込先パス</param>
        ///// <param name="argIsHeader">ヘッダー(true:あり/false:なし)</param>
        ///// <param name="argDelimiter">データ区切り文字</param>
        ///// <returns></returns>
        //public IEnumerable<T> ReadFile<T>(string argPath, bool argIsHeader, string argDelimiter)
        //{
        //    // CsvConfigurationの設定（Shift-JISとして出力）
        //    var wConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        //    {
        //        Encoding = Encoding.GetEncoding("shift_jis"),
        //        HasHeaderRecord = argIsHeader,
        //        Delimiter = argDelimiter
        //    };

        //    // CSVファイルの読み込み
        //    using (var wReader = new StreamReader(argPath, wConfig.Encoding))
        //    using (var wCsv = new CsvReader(wReader, wConfig))
        //    {               
        //        return wCsv.GetRecords<T>().ToList();
        //    }
        //}


        ///// <summary>
        ///// マスタファイル読込
        ///// </summary>
        ///// <typeparam name="T">から</typeparam>
        ///// <param name="argPath">取込先パス</param>
        ///// <returns></returns>
        //public IEnumerable<T> ReadMasterFile<T>(string argPath)
        //{
        //    return ReadFile<T>(argPath, false, "\t");
        //}

        #endregion

        /// <summary>
        /// マスタファイル読込
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="argPath">取込先パス</param>
        /// <returns></returns>
        public (IEnumerable<T> retSuceedData, List<string> retErrData) ReadMasterFile<T>(string argPath)
        {
            return ReadFile<T>(argPath, false, "\t");
        }

        /// <summary>
        /// ファイル読込
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="argPath">取込先パス</param>
        /// <param name="argIsHeader">ヘッダー(true:あり/false:なし)</param>
        /// <param name="argDelimiter">データ区切り文字</param>
        /// <returns></returns>
        public (IEnumerable<T> retSuceedData, List<string> retErrData) ReadFile<T>(string argPath, bool argIsHeader, string argDelimiter)
        {
            var dtSucceed=new List<T>();
            var dtErr = new List<string>();

            // CsvConfigurationの設定（Shift-JISとして出力）
            var wConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.GetEncoding("shift_jis"),
                HasHeaderRecord = argIsHeader,
                Delimiter = argDelimiter
            };

            // CSVファイルの読み込み
            using (var wReader = new StreamReader(argPath, wConfig.Encoding))
            using (var wCsv = new CsvReader(wReader, wConfig))
            {
                
                if (argIsHeader)
                {
                    wCsv.Read();
                    wCsv.ReadHeader();
                }

                while (wCsv.Read())
                {
                    try
                    {
                        dtSucceed.Add(wCsv.GetRecord<T>());
                    }
                    catch (CsvHelperException)
                    {
                        //エラーデータを取得
                        dtErr.Add(((CsvHelper.CsvParser)wCsv.Context.Parser).RawRecord);
                        continue;
                    }
                }
            }

            return (dtSucceed, dtErr);
        }

        #endregion
    }
}
