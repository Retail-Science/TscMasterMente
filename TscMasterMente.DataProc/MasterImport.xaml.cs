using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TscMasterMente.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using static System.Net.WebRequestMethods;
using ABI.Windows.Media.Protection.PlayReady;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Shapes;
using ABI.Windows.Web.Syndication;
using System.Diagnostics;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MasterImport : Window
    {
        public MasterImport()
        {
            this.InitializeComponent();
            this.Activated += MasterImport_Activated;
        }

        #region イベント

        /// <summary>
        /// Activetedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// 【備考】
        /// WinUI3はLoadイベントがないため、Activatedイベントを使用して初期化処理を行う。
        /// </remarks>
        private void MasterImport_Activated(object sender, WindowActivatedEventArgs args)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterTextDir");

            //アプリアイコンを設定
            WindowParts.SetAppIcon(this);
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 560);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= MasterImport_Activated;
        }

        /// <summary>
        /// 閉じるボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// フォルダ選択ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnFolder_Click(object sender, RoutedEventArgs e)
        {
            //フォルダ選択ダイアログの表示
            var wPath = await IoParts.PickFolderAsync(this);
            if (wPath != null)
            {
                TxtMasterPath.Text = wPath;
            }
        }

        /// <summary>
        /// 実行ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wSucceed = false;
            var wProgressWindow = new ProgressWindow();
            string wExecMaster = "";
            try
            {
                //ウィンドウの無効化
                WindowParts.SetAllChildEnabled(MainContent, false);

                var clsSql = new SqliteParts();

                #region チェック処理

                //txtMasterPathのチェック
                var wDirPath = TxtMasterPath.Text;
                if (string.IsNullOrEmpty(wDirPath))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "取込先フォルダが設定されていません。");
                    await wDialog.ShowAsync();

                    return;
                }
                else if (!Directory.Exists(wDirPath))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "取込先フォルダが存在しません。");
                    await wDialog.ShowAsync();
                    return;
                }                

                //マスタファイルの取得
                var wMasters = clsSql.GetSelectResult<Masters>("SELECT * FROM Masters");
                var wFiles = await IoParts.GetMasterFiles(wDirPath);

                if (wFiles.Count > 0)
                {

                    //マスタファイル名の取得
                    var lqFiles = from x in wFiles
                                  join y in wMasters on x.DisplayName + x.FileType.ToLower() equals y.MasterFileName
                                  where new FileInfo(x.Path).Length > 0
                                  select x;

                    wFiles = lqFiles.ToList();

                }

                if (wFiles.Count == 0)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "マスタファイルが存在しません。");
                    await wDialog.ShowAsync();
                    return;
                }
                #endregion

                #region 取込確認

                int wMasterCount = 0;
                var wMsg = "以下のマスタファイルが見つかりました。これらのマスタを取込みますか？";
                wMsg += Environment.NewLine;
                wMsg += "【参照先パス】";
                wMsg += Environment.NewLine;
                wMsg += wDirPath;
                wMsg += Environment.NewLine;
                wMsg += "【マスタファイル】";
                foreach (var iFile in wFiles)
                {
                    //Mastersからファイル名検索
                    var lqMaster = wMasters.Where(x => x.MasterFileName == iFile.DisplayName + iFile.FileType.ToLower()).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wMsg += Environment.NewLine;
                        wMsg += $"・{lqMaster.MasterName} ({lqMaster.MasterFileName})";
                        wMasterCount++;
                    }
                }

                var wDialogMsg = MessageParts.ShowMessageYesNo(this, "確認", wMsg);
                var wDialogResult = await wDialogMsg.ShowAsync();
                if (wDialogResult == ContentDialogResult.Secondary)
                {
                    return;
                }

                #endregion

                #region マスタファイルの取込

                //プログレスウィンドウの表示                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("マスタ取込中...", wMasterCount);

                clsSql.UpdateAppInfo("MasterTextDir", wDirPath);
                using (var conn = clsSql.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var wAlertList = new List<string>();
                            foreach (var iFile in wFiles)
                            {
                                #region ファイル名からマスタ情報を取得

                                //Mastersからファイル名検索
                                int wMasterCode = 0;
                                string wErrFileName = "";
                                var lqMaster = wMasters.Where(x => x.MasterFileName == iFile.DisplayName + iFile.FileType.ToLower()).FirstOrDefault();
                                if (lqMaster != null)
                                {
                                    wMasterCode = lqMaster.MasterCode;
                                    wExecMaster = lqMaster.MasterName;
                                    wErrFileName = lqMaster.ErrFileName;

                                    if (await wProgressWindow.SetProgress(wExecMaster) == false)
                                    {
                                        var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                                        await wDialog.ShowAsync();
                                        return;
                                    }
                                }

                                #endregion

                                #region マスタ取込処理

                                switch (wMasterCode)
                                {
                                    case (int)SqliteParts.EnumMasterInfo.MakerMaster:
                                        //メーカーマスタの取込
                                        if (!ImportMkMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SyoMaster:
                                        //商品マスタの取込
                                        if (!ImportSyoMaser(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.BunMaster:
                                        //細分類マスタの取込
                                        if (!ImportBunMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.ZokMaster:
                                        //属性マスタの取込
                                        if (!ImportZokMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SuiMaster:
                                        //水準マスタの取込
                                        if (!ImportSuiMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SyoZokSei:
                                        //商品属性マスタの取込
                                        if (!ImportSyoZokSei(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.PopMaster:
                                        //POPマスタの取込
                                        if (!ImportPopMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.PBunMaster:
                                        //POP分類マスタの取込
                                        if (!ImportPBunMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.TenpoMaster:
                                        //店舗マスタの取込
                                        if (!ImportTenpoMaster(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.GonJyu:
                                        //ゴンドラ什器の取込
                                        if (!ImportGonJyu(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    case (int)SqliteParts.EnumMasterInfo.SlfJyu:
                                        //棚段什器の取込
                                        if (!ImportSlfJyu(clsSql, conn, iFile.Path, wErrFileName))
                                        {
                                            //エラーデータあり
                                            wAlertList.Add(wErrFileName);
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                #endregion
                            }
                            tran.Commit();

                            #region メッセージ処理

                            //メッセージ
                            if (wAlertList.Count == 0)
                            {
                                wSucceed = true;
                            }
                            else
                            {
                                string alertMsg = "データの取込は実行しましたが、取込に失敗したデータがありました。";
                                alertMsg += Environment.NewLine;
                                alertMsg += "エラーリストとして出力しましたので、内容をご確認ください。";
                                alertMsg += Environment.NewLine;
                                alertMsg += "【出力先パス】";
                                alertMsg += Environment.NewLine;
                                alertMsg += wDirPath;
                                alertMsg += Environment.NewLine;
                                alertMsg += "【エラーファイルリスト】";
                                foreach (var iAlert in wAlertList)
                                {
                                    alertMsg += Environment.NewLine;
                                    alertMsg += $"・{iAlert}";
                                }
                                var wMsgDialog = MessageParts.ShowMessageOkOnly(this, "完了", alertMsg);
                                await wMsgDialog.ShowAsync();

                            }
                            #endregion

                        }
                        catch (Exception)
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                //エラーメッセージの表示
                var errMsg = $"{wExecMaster}取込中にエラーが発生しました。";
                errMsg += "\r\n";
                errMsg += "【詳細】";
                errMsg += "\r\n";
                errMsg += ex.Message;

                var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", errMsg);
                await wDialog.ShowAsync();

                wSucceed = false;
            }
            finally
            {
                //プログレスウィンドウのクローズ
                wProgressWindow.Close();
                wProgressWindow = null;

                //ウィンドウの有効化
                WindowParts.SetAllChildEnabled(MainContent, true);

                if (wSucceed)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "完了", "マスタの取込処理が完了しました。");
                    await wDialog.ShowAsync();
                }
            }
        }


        #endregion

        #region メソッド

        #region マスタ取込時の共通メソッド

        /// <summary>
        /// 読込マスタファイルからマスタエラーファイルのパスを取得
        /// </summary>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private string GetMasterErrFilePath(string argErrFileName)
        {
            return System.IO.Path.Combine(TxtMasterPath.Text, argErrFileName);
        }

        /// <summary>
        /// CsvHelperで読込時にエラーになったデータをエラーファイルに出力
        /// </summary>
        /// <param name="argPath">出力先エラーファイルパス</param>
        /// <param name="argData">エラーデータ</param>
        private void WriteCsvHelperConvertErrData(string argPath, List<string> argData)
        {
            using (StreamWriter wWrite = new StreamWriter(argPath, true, Encoding.GetEncoding("shift_jis")))
            {
                foreach (var iData in argData)
                {
                    wWrite.WriteLine(iData);
                }
            }
        }

        #endregion

        #region マスタ取込時の個別メソッド

        /// <summary>
        /// メーカーマスタの取込
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportMkMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<MkMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<MkMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック


                    //メーカーコードチェック
                    if (string.IsNullOrEmpty(iData.MkCode) || iData.MkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //メーカー名チェック
                    if (!string.IsNullOrEmpty(iData.MkName) && iData.MkName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //メーカー名カナチェック
                    if (!string.IsNullOrEmpty(iData.MkName_Kana) && iData.MkName_Kana.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@MkCode", iData.MkCode),
                        new SqliteParameter("@MkName", iData.MkName),
                        new SqliteParameter("@MkName_Kana", iData.MkName_Kana)
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO MkMaster (MkCode, MkName, MkName_Kana) VALUES (@MkCode , @MkName, @MkName_Kana)";
                        tSql += " ON CONFLICT(MkCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO MkMaster (MkCode, MkName, MkName_Kana) VALUES (@MkCode , @MkName, @MkName_Kana)";
                        tSql += $" ON CONFLICT(MkCode) DO UPDATE SET MkName = @MkName , MkName_Kana = @MkName_Kana ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM MkMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO MkMaster (MkCode, MkName, MkName_Kana) VALUES (@MkCode , @MkName, @MkName_Kana)";
                        tSql += $" ON CONFLICT(MkCode) DO UPDATE SET MkName = @MkName , MkName_Kana = @MkName_Kana ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<MkMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// 商品マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportSyoMaser(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;
            var clsCsv = new CsvHelperParts();
            const int MAX_WDH = 9999;
            const int MIN_WDH = 0;

            var dtFileData = clsCsv.ReadMasterFile<SyoMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<SyoMasterEntity>();
            if (dtMaster.Count() > 0)
            {
                string wExecDate = DateTime.Now.ToString("yyyyMMdd");
                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック

                    //Janコードチェック
                    if (string.IsNullOrEmpty(iData.JanCode) || iData.JanCode.Length > 16)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //メーカコードチェック
                    if (!string.IsNullOrEmpty(iData.MkCode) && iData.MkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //商品コードチェック
                    if (!string.IsNullOrEmpty(iData.StoCode) && iData.StoCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //商品名カナチェック
                    if (!string.IsNullOrEmpty(iData.TanName) && iData.TanName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //商品名チェック
                    if (!string.IsNullOrEmpty(iData.ItemName) && iData.ItemName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //幅チェック
                    if (iData.W == null)
                    {
                        iData.W = 100;
                    }
                    else if (iData.W < MIN_WDH || iData.W > MAX_WDH)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //奥行チェック
                    if (iData.D == null)
                    {
                        iData.D = 100;
                    }
                    else if (iData.D < MIN_WDH || iData.D > MAX_WDH)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //高さチェック
                    if (iData.H == null)
                    {
                        iData.H = 100;
                    }
                    else if (iData.H < MIN_WDH || iData.H > MAX_WDH)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //入数チェック
                    if (iData.PCase == null)
                    {
                        iData.PCase = 0;
                    }
                    else if (iData.PCase < 0 || iData.PCase > 99999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //分類コードチェック
                    if (!string.IsNullOrEmpty(iData.BunCode) && iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //売価チェック
                    if (iData.Price == null)
                    {
                        iData.Price = 0;
                    }
                    else if (iData.Price < 0 || iData.Price > 9999999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //原価チェック
                    if (iData.Cost == null)
                    {
                        iData.Cost = 0;
                    }
                    else if (iData.Cost < 0 || iData.Cost > 9999999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //登録日チェック
                    if (!string.IsNullOrEmpty(iData.Attr3) && iData.Attr3.Length > 8)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //更新日チェック
                    if (!string.IsNullOrEmpty(iData.Attr5) && iData.Attr5.Length > 8)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@JanCode", iData.JanCode),
                        new SqliteParameter("@MkCode", iData.MkCode),
                        new SqliteParameter("@StoCode", iData.StoCode),
                        new SqliteParameter("@TanName", iData.TanName),
                        new SqliteParameter("@ItemName", iData.ItemName),
                        new SqliteParameter("@W", iData.W),
                        new SqliteParameter("@D", iData.D),
                        new SqliteParameter("@H", iData.H),
                        new SqliteParameter("@PCase", iData.PCase),
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@Price", iData.Price),
                        new SqliteParameter("@Cost", iData.Cost),
                        new SqliteParameter("@Attr3", iData.Attr3),
                        new SqliteParameter("@Attr5", iData.Attr5)
                    };
                    foreach (var iParam in wParams)
                    {
                        switch (iParam.ParameterName)
                        {
                            case "@Attr3":
                            case "@Attr5":
                                if (argSqlCls.IsNullData(iParam.Value))
                                {
                                    iParam.Value = wExecDate;
                                }
                                break;
                            default:
                                if (argSqlCls.IsNullData(iParam.Value))
                                {
                                    iParam.Value = DBNull.Value;
                                }
                                break;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO SyoMaster ";
                        tSql += $"(JanCode, MkCode, StoCode, TanName, ItemName, W, H, D, PCase, BunCode, Price, Cost, Attr3,  Attr5) ";
                        tSql += $" VALUES ";
                        tSql += $"(@JanCode, @MkCode, @StoCode, @TanName, @ItemName, @W, @H, @D, @PCase, @BunCode, @Price, @Cost, @Attr3, @Attr5) ";
                        tSql += $" ON CONFLICT(JanCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO SyoMaster ";
                        tSql += $" (JanCode, MkCode, StoCode, TanName, ItemName, W, H, D, PCase, BunCode, Price, Cost, Attr3,  Attr5) ";
                        tSql += $" VALUES ";
                        tSql += $" (@JanCode, @MkCode, @StoCode, @TanName, @ItemName, @W, @H, @D, @PCase, @BunCode, @Price, @Cost, @Attr3, @Attr5) ";
                        tSql += $" ON CONFLICT(JanCode) DO UPDATE ";
                        tSql += $" SET MkCode = @MkCode, StoCode = @StoCode, TanName = @TanName, ItemName = @ItemName, ";
                        tSql += $" W = @W , H = @H , D = @D , PCase = @PCase , BunCode = @BunCode , Price = @Price , Cost = @Cost , Attr3 = @Attr3 , Attr5 = @Attr5 ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SyoMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SyoMaster ";
                        tSql += $" (JanCode, MkCode, StoCode, TanName, ItemName, W, H, D, PCase, BunCode, Price, Cost, Attr3,  Attr5) ";
                        tSql += $" VALUES ";
                        tSql += $" (@JanCode, @MkCode, @StoCode, @TanName, @ItemName, @W, @H, @D, @PCase, @BunCode, @Price, @Cost, @Attr3, @Attr5) ";
                        tSql += $" ON CONFLICT(JanCode) DO UPDATE ";
                        tSql += $" SET MkCode = @MkCode, StoCode = @StoCode, TanName = @TanName, ItemName = @ItemName, ";
                        tSql += $" W = @W , H = @H , D = @D , PCase = @PCase , BunCode = @BunCode , Price = @Price , Cost = @Cost , Attr3 = @Attr3 , Attr5 = @Attr5 ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region エラー処理
            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SyoMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;

        }

        /// <summary>
        /// 細分類マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportBunMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<BunMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<BunMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック


                    //細分類コードチェック
                    if (string.IsNullOrEmpty(iData.BunCode) || iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //細分類名チェック
                    if (!string.IsNullOrEmpty(iData.BunName) && iData.BunName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@BunName", iData.BunName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO BunMaster (BunCode, BunName) VALUES (@BunCode , @BunName) ";
                        tSql += " ON CONFLICT(BunCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO BunMaster (BunCode, BunName) VALUES (@BunCode , @BunName) ";
                        tSql += $" ON CONFLICT(BunCode) DO UPDATE SET BunName = @BunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM BunMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO BunMaster (BunCode, BunName) VALUES (@BunCode , @BunName) ";
                        tSql += $" ON CONFLICT(BunCode) DO UPDATE SET BunName = @BunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<BunMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// 属性マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportZokMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<ZokMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<ZokMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック


                    //細分類コードチェック
                    if (!string.IsNullOrEmpty(iData.BunCode) && iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //属性コードチェック
                    if (string.IsNullOrEmpty(iData.ZkCode) || iData.ZkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //属性名チェック
                    if (!string.IsNullOrEmpty(iData.ZkName) && iData.ZkName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@ZkCode", iData.ZkCode),
                        new SqliteParameter("@ZkName", iData.ZkName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO ZokMaster (BunCode, ZkCode, ZkName) VALUES (@BunCode , @ZkCode, @ZkName) ";
                        tSql += " ON CONFLICT(ZkCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO ZokMaster (BunCode, ZkCode, ZkName) VALUES (@BunCode , @ZkCode, @ZkName) ";
                        tSql += $" ON CONFLICT(ZkCode) DO UPDATE SET ZkName = @ZkName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM ZokMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO ZokMaster (BunCode, ZkCode, ZkName) VALUES (@BunCode , @ZkCode, @ZkName) ";
                        tSql += $" ON CONFLICT(ZkCode) DO UPDATE SET ZkName = @ZkName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<ZokMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// 水準マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportSuiMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<SuiMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<SuiMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック


                    //細分類コードチェック
                    if (!string.IsNullOrEmpty(iData.BunCode) && iData.BunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //属性コードチェック
                    if (string.IsNullOrEmpty(iData.ZkCode) || iData.ZkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //水準コードチェック
                    if (string.IsNullOrEmpty(iData.SuiCode) || iData.SuiCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //水準名チェック
                    if (!string.IsNullOrEmpty(iData.SuiName) && iData.SuiName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@BunCode", iData.BunCode),
                        new SqliteParameter("@ZkCode", iData.ZkCode),
                        new SqliteParameter("@SuiCode", iData.SuiCode),
                        new SqliteParameter("@SuiName", iData.SuiName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO SuiMaster (BunCode, ZkCode, SuiCode, SuiName) VALUES (@BunCode , @ZkCode, @SuiCode, @SuiName) ";
                        tSql += " ON CONFLICT(ZkCode, SuiCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO SuiMaster (BunCode, ZkCode, SuiCode, SuiName) VALUES (@BunCode , @ZkCode, @SuiCode, @SuiName) ";
                        tSql += $" ON CONFLICT(ZkCode, SuiCode) DO UPDATE SET SuiName = @SuiName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SuiMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SuiMaster (BunCode, ZkCode, SuiCode, SuiName) VALUES (@BunCode , @ZkCode, @SuiCode, @SuiName) ";
                        tSql += $" ON CONFLICT(ZkCode, SuiCode) DO UPDATE SET SuiName = @SuiName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SuiMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// 商品属性マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportSyoZokSei(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<SyoZokSeiEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<SyoZokSeiEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック

                    //JanCodeチェック
                    if (string.IsNullOrEmpty(iData.JanCode) || iData.JanCode.Length > 16)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //属性コードチェック
                    if (string.IsNullOrEmpty(iData.ZkCode) || iData.ZkCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //水準コードチェック
                    if (string.IsNullOrEmpty(iData.SuiCode) || iData.SuiCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@JanCode", iData.JanCode),
                        new SqliteParameter("@ZkCode", iData.ZkCode),
                        new SqliteParameter("@SuiCode", iData.SuiCode),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true || RbMerge.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO SyoZokSei (JanCode, ZkCode, SuiCode) VALUES (@JanCode , @ZkCode, @SuiCode) ";
                        tSql += " ON CONFLICT(JanCode, ZkCode, SuiCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SyoZokSei", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SyoZokSei (JanCode, ZkCode, SuiCode) VALUES (@JanCode , @ZkCode, @SuiCode) ";
                        tSql += " ON CONFLICT(JanCode, ZkCode, SuiCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SyoZokSeiEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// POPマスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportPopMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<PopMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;

            var wErrList = new List<PopMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック

                    //PopCodeチェック
                    if (string.IsNullOrEmpty(iData.PopCode) || iData.PopCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //PopNameチェック
                    if (!string.IsNullOrEmpty(iData.PopName) && iData.PopName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //Wチェック
                    if (iData.W == null)
                    {
                        iData.W = 100;
                    }
                    else if (iData.W < 0 || iData.W > 9999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //Hチェック
                    if (iData.H == null)
                    {
                        iData.H = 100;
                    }
                    else if (iData.H < 0 || iData.H > 9999)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //PBunCodeチェック
                    if (!string.IsNullOrEmpty(iData.PBunCode) && iData.PBunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@PopCode", iData.PopCode),
                        new SqliteParameter("@PopName", iData.PopName),
                        new SqliteParameter("@W", iData.W),
                        new SqliteParameter("@H", iData.H),
                        new SqliteParameter("@PBunCode", iData.PBunCode),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO PopMaster (PopCode, PopName, W, H, PBunCode) VALUES (@PopCode , @PopName, @W, @H, @PBunCode) ";
                        tSql += " ON CONFLICT(PopCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO PopMaster (PopCode, PopName, W, H, PBunCode) VALUES (@PopCode , @PopName, @W, @H, @PBunCode) ";
                        tSql += $" ON CONFLICT(PopCode) DO UPDATE SET PopName = @PopName, W = @W, H = @H, PBunCode = @PBunCode";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM PopMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO PopMaster (PopCode, PopName, W, H, PBunCode) VALUES (@PopCode , @PopName, @W, @H, @PBunCode) ";
                        tSql += $" ON CONFLICT(PopCode) DO UPDATE SET PopName = @PopName, W = @W, H = @H, PBunCode = @PBunCode";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }

            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<PopMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// Pop分類マスタ
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportPBunMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<PBunMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<PBunMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック


                    //PBunCodeチェック
                    if (string.IsNullOrEmpty(iData.PBunCode) || iData.PBunCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //PBunNameチェック
                    if (!string.IsNullOrEmpty(iData.PBunName) && iData.PBunName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@PBunCode", iData.PBunCode),
                        new SqliteParameter("@PBunName", iData.PBunName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO PBunMaster (PBunCode, PBunName) VALUES (@PBunCode , @PBunName) ";
                        tSql += " ON CONFLICT(PBunCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新
                        var tSql = $"INSERT INTO PBunMaster (PBunCode, PBunName) VALUES (@PBunCode , @PBunName) ";
                        tSql += $" ON CONFLICT(PBunCode) DO UPDATE SET PBunName = @PBunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM PBunMaster", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO PBunMaster (PBunCode, PBunName) VALUES (@PBunCode , @PBunName) ";
                        tSql += $" ON CONFLICT(PBunCode) DO UPDATE SET PBunName = @PBunName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<PBunMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// 店舗マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportTenpoMaster(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<TenpoMasterEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<TenpoMasterEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック

                    //TenpoNumberチェック
                    if (string.IsNullOrEmpty(iData.TenpoNumber) || iData.TenpoNumber.Length > 10)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //TenpoCodeチェック
                    if (string.IsNullOrEmpty(iData.TenpoCode) || iData.TenpoCode.Length > 10)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //TenpoTanNameチェック
                    if (!string.IsNullOrEmpty(iData.TenpoTanName) && iData.TenpoTanName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //TenpoNameチェック
                    if (!string.IsNullOrEmpty(iData.TenpoName) && iData.TenpoName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //CompCodeチェック
                    if (!string.IsNullOrEmpty(iData.CompCode) && iData.CompCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //CompNameチェック
                    if (!string.IsNullOrEmpty(iData.CompName) && iData.CompName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //AreaCodeチェック
                    if (!string.IsNullOrEmpty(iData.AreaCode) && iData.AreaCode.Length > 20)
                    {
                        wErrList.Add(iData);
                        continue;
                    }
                    //AreaNameチェック
                    if (!string.IsNullOrEmpty(iData.AreaName) && iData.AreaName.Length > 255)
                    {
                        wErrList.Add(iData);
                        continue;
                    }



                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@TenpoNumber", iData.TenpoNumber),
                        new SqliteParameter("@TenpoCode", iData.TenpoCode),
                        new SqliteParameter("@TenpoTanName", iData.TenpoTanName),
                        new SqliteParameter("@TenpoName", iData.TenpoName),
                        new SqliteParameter("@CompCode", iData.CompCode),
                        new SqliteParameter("@CompName", iData.CompName),
                        new SqliteParameter("@AreaCode", iData.AreaCode),
                        new SqliteParameter("@AreaName", iData.AreaName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true)
                    {
                        //未登録のみ追加                        

                        //店舗マスタ
                        var tSql = $"INSERT INTO TenpoMaster (TenpoNumber, TenpoCode, TenpoTanName, TenpoName, CompCode, CompName, AreaCode, AreaName) ";
                        tSql += " VALUES (@TenpoNumber, @TenpoCode, @TenpoTanName, @TenpoName, @CompCode, @CompName, @AreaCode, @AreaName) ";
                        tSql += " ON CONFLICT(TenpoNumber) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //エリアマスタ
                        tSql = $"INSERT INTO AreaMaster (AreaCode, AreaName) ";
                        tSql += " VALUES (@AreaCode, @AreaName) ";
                        tSql += " ON CONFLICT(AreaCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //法人マスタ
                        tSql = $"INSERT INTO CompMaster (CompCode, CompName) ";
                        tSql += " VALUES (@CompCode, @CompName) ";
                        tSql += " ON CONFLICT(CompCode) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else if (RbMerge.IsChecked == true)
                    {
                        //追加更新

                        //店舗マスタ
                        var tSql = $"INSERT INTO TenpoMaster (TenpoNumber, TenpoCode, TenpoTanName, TenpoName, CompCode, CompName, AreaCode, AreaName) ";
                        tSql += " VALUES (@TenpoNumber, @TenpoCode, @TenpoTanName, @TenpoName, @CompCode, @CompName, @AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(TenpoNumber) DO UPDATE SET ";
                        tSql += " TenpoCode = @TenpoCode, TenpoTanName = @TenpoTanName, TenpoName = @TenpoName ";
                        tSql += " ,CompCode = @CompCode, CompName = @CompName, AreaCode = @AreaCode, AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //エリアマスタ
                        tSql = $"INSERT INTO AreaMaster (AreaCode, AreaName) ";
                        tSql += " VALUES (@AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(AreaCode) DO UPDATE SET AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //法人マスタ
                        tSql = $"INSERT INTO CompMaster (CompCode, CompName) ";
                        tSql += " VALUES (@CompCode, @CompName) ";
                        tSql += $" ON CONFLICT(CompCode) DO UPDATE SET CompName = @CompName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM TenpoMaster", null);
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM AreaMaster", null);
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM CompMaster", null);
                            wDeleted = true;
                        }

                        //店舗マスタ
                        var tSql = $"INSERT INTO TenpoMaster (TenpoNumber, TenpoCode, TenpoTanName, TenpoName, CompCode, CompName, AreaCode, AreaName) ";
                        tSql += " VALUES (@TenpoNumber, @TenpoCode, @TenpoTanName, @TenpoName, @CompCode, @CompName, @AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(TenpoNumber) DO UPDATE SET ";
                        tSql += " TenpoCode = @TenpoCode, TenpoTanName = @TenpoTanName, TenpoName = @TenpoName ";
                        tSql += " ,CompCode = @CompCode, CompName = @CompName, AreaCode = @AreaCode, AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //エリアマスタ
                        tSql = $"INSERT INTO AreaMaster (AreaCode, AreaName) ";
                        tSql += " VALUES (@AreaCode, @AreaName) ";
                        tSql += $" ON CONFLICT(AreaCode) DO UPDATE SET AreaName = @AreaName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);

                        //法人マスタ
                        tSql = $"INSERT INTO CompMaster (CompCode, CompName) ";
                        tSql += " VALUES (@CompCode, @CompName) ";
                        tSql += $" ON CONFLICT(CompCode) DO UPDATE SET CompName = @CompName ";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<TenpoMasterEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            #region 店舗番号の振り直し
            var dtTenpo = argSqlCls.GetSelectResult<TenpoMaster>(argConn, "SELECT * FROM TenpoMaster ORDER BY TenpoCode");
            var wTenpoId = 0;
            foreach (var iData in dtTenpo)
            {
                wTenpoId++;
                var tSql = $"UPDATE TenpoMaster SET TenpoNumber = '{wTenpoId.ToString("000")}' WHERE TenpoId = '{iData.TenpoCode}'";
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// ゴンドラ什器マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportGonJyu(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<GonJyuEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<GonJyuEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック

                    //GonJyuNameチェック
                    if (string.IsNullOrEmpty(iData.GonJyuName) || iData.GonJyuName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@GonJyuName", iData.GonJyuName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true || RbMerge.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO GonJyu (GonJyuName) VALUES (@GonJyuName) ";
                        tSql += " ON CONFLICT(GonJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM GonJyu", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO GonJyu (GonJyuName) VALUES (@GonJyuName) ";
                        tSql += " ON CONFLICT(GonJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<GonJyuEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        /// <summary>
        /// 棚段什器マスタの取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argPath">マスタファイルパス</param>
        /// <param name="argErrFileName">エラーファイル名</param>
        /// <returns></returns>
        private bool ImportSlfJyu(SqliteParts argSqlCls, SqliteConnection argConn, string argPath, string argErrFileName)
        {
            bool wResult = true;

            var clsCsv = new CsvHelperParts();
            var dtFileData = clsCsv.ReadMasterFile<SlfJyuEntity>(argPath);
            var dtMaster = dtFileData.retSuceedData;


            var wErrList = new List<SlfJyuEntity>();
            if (dtMaster.Count() > 0)
            {

                bool wDeleted = false;
                foreach (var iData in dtMaster)
                {
                    #region チェック

                    //SlfJyuNameチェック
                    if (string.IsNullOrEmpty(iData.SlfJyuName) || iData.SlfJyuName.Length > 50)
                    {
                        wErrList.Add(iData);
                        continue;
                    }

                    #endregion

                    #region パラメタ設定

                    var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@SlfJyuName", iData.SlfJyuName),
                    };
                    foreach (var iParam in wParams)
                    {
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DBNull.Value;
                        }
                    }

                    #endregion

                    #region SQL実行

                    if (RbInsertOnly.IsChecked == true || RbMerge.IsChecked == true)
                    {
                        //未登録のみ追加
                        var tSql = $"INSERT INTO SlfJyu (SlfJyuName) VALUES (@SlfJyuName) ";
                        tSql += " ON CONFLICT(SlfJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }
                    else
                    {
                        //削除→追加処理
                        if (wDeleted == false)
                        {
                            argSqlCls.ExecuteSql(argConn, "DELETE FROM SlfJyu", null);
                            wDeleted = true;
                        }
                        var tSql = $"INSERT INTO SlfJyu (SlfJyuName) VALUES (@SlfJyuName) ";
                        tSql += " ON CONFLICT(SlfJyuName) DO NOTHING";
                        argSqlCls.ExecuteSql(argConn, tSql, wParams);
                    }

                    #endregion
                }
            }


            #region エラー処理

            var errFilePath = GetMasterErrFilePath(argErrFileName);
            if (wErrList.Count() > 0)
            {
                clsCsv.WriteMasterFile<SlfJyuEntity>(errFilePath, wErrList);
                wResult = false;
            }

            if (dtFileData.retErrData.Count() > 0)
            {
                WriteCsvHelperConvertErrData(errFilePath, dtFileData.retErrData);
                wResult = false;
            }
            #endregion

            return wResult;
        }

        #endregion

        #endregion

    }
}
