using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
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
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExcelItemMasterImport : Window
    {
        #region コンストラクタ

        public ExcelItemMasterImport()
        {
            this.InitializeComponent();

            this.Activated += ExcelItemMasterImport_Activated;
        }

        #endregion

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// 【備考】
        /// WinUI3はLoadイベントがないため、Activatedイベントを使用して初期化処理を行う。
        /// </remarks>
        private async void ExcelItemMasterImport_Activated(object sender, WindowActivatedEventArgs e)
        {
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 600);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            //マスタファイルパス設定
            var clsSql = new SqliteParts();
            var dirPath = clsSql.GetAppInfo("MasterExcelDir");
            var xlFiles = await IoParts.GetFilesAsync(dirPath, false, new List<string>() { ".xlsx" });
            if (xlFiles.Count > 0)
            {
                var lqXls = xlFiles.OrderByDescending(x => x.DateCreated).FirstOrDefault();
                dirPath = Path.Combine(dirPath, lqXls.Name);
            }
            TxtMasterPath.Text = dirPath;

            this.Activated -= ExcelItemMasterImport_Activated;
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
        /// ファイル選択ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            //ファイルを開くダイアログ
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.XlsxFiles);
            if (wPath != null)
            {
                TxtMasterPath.Text = wPath;
            }
        }

        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wProgressWindow = new ProgressWindow();
            try
            {
                //ウィンドウの無効化
                WindowParts.SetAllChildEnabled(MainContent, false);

                #region チェック処理

                if (string.IsNullOrEmpty(TxtMasterPath.Text))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "ファイルを選択してください。");
                    await tMsgDialog.ShowAsync();
                    return;
                }
                else if (!System.IO.File.Exists(TxtMasterPath.Text))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "ファイルが存在しません。");
                    await tMsgDialog.ShowAsync();
                    return;
                }
                else if (!IsEnableExcelFile(TxtMasterPath.Text))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "Excelファイルが実行中です。終了後、再実行願います。");
                    await tMsgDialog.ShowAsync();
                    return;
                }

                #endregion

                #region Excel取込処理
                var wSql = new SqliteParts();
                const int BEGIN_EXCEL_ROW = 5;

                using (var wConn = wSql.GetConnection())
                using (var xlBook = new ClosedXML.Excel.XLWorkbook(TxtMasterPath.Text))
                {
                    wConn.Open();
                    using (var tran = wConn.BeginTransaction())
                    {
                        try
                        {
                            var xlSheet = xlBook.Worksheet("一括登録シート（名称変更不可）");
                            var wMaxRowCnt = xlSheet.RowsUsed().Count();
                            var wMaxColCnt = xlSheet.ColumnsUsed().Count();

                            //プログレスウィンドウの表示                               
                            wProgressWindow.Activate();
                            await wProgressWindow.SetInit("Excelマスタファイル出力中...", wMaxRowCnt - BEGIN_EXCEL_ROW);

                            if (RbDelInsert.IsChecked == true)
                            {
                                //削除→追加
                                if(ChkSyoZokImpFlg.IsChecked == true)
                                {
                                    wSql.ExecuteSql(wConn, "DELETE FROM SyoZokSei", null);
                                }
                                wSql.ExecuteSql(wConn, "DELETE FROM SyoZokSei", null);
                            }

                            for (int iRow = BEGIN_EXCEL_ROW; iRow < wMaxRowCnt; iRow++)
                            {
                                //進捗状況
                                if (await wProgressWindow.SetProgress($"{iRow}行目を処理中...") == false)
                                {
                                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                                    await wDialog.ShowAsync();
                                    return;
                                }

                                if (!ImportSyoMaser(wSql, wConn, xlSheet, iRow))
                                {
                                    var errMsg = $"商品マスタの取込に失敗しました。行番号：{iRow}";
                                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", errMsg);
                                    await tMsgDialog.ShowAsync();
                                }

                                if (ChkSyoZokImpFlg.IsChecked == true && !ImportSyoZokSei(wSql, wConn, xlSheet, iRow))
                                {
                                    var errMsg = $"商品属性マスタの取込に失敗しました。行番号：{iRow}";
                                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", errMsg);
                                    await tMsgDialog.ShowAsync();
                                }
                            }
                            tran.Commit();
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
                var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", ex.Message);
                await tMsgDialog.ShowAsync();
            }
            finally
            {
                //プログレスウィンドウのクローズ
                wProgressWindow.Close();
                wProgressWindow = null;

                //ウィンドウの有効化
                WindowParts.SetAllChildEnabled(MainContent, true);
            }
        }

        #endregion

        #region メソッド

        /// <summary>
        /// Excelが使用可能かチェック
        /// </summary>
        /// <param name="argPath">Excelファイルパス</param>
        /// <returns></returns>
        private bool IsEnableExcelFile(string argPath)
        {
            try
            {
                using (FileStream stream = File.Open(argPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // IOExceptionが発生した場合、ファイルは開いているか、アクセスできない状態です。
                return false;
            }

            // 例外が発生しなかった場合、ファイルは開いていないと見なします。
            return true;
        }

        /// <summary>
        /// 商品マスタ取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argSheet">取込先シート</param>
        /// <param name="argRow">処理行</param>
        /// <returns></returns>
        private bool ImportSyoMaser(SqliteParts argSqlCls, SqliteConnection argConn, IXLWorksheet argSheet, int argRow)
        {
            const int MAX_WDH = 9999;
            const int MIN_WDH = 0;

            #region チェック及びデータ取得

            //JanCode
            var wJanCode = argSheet.Cell(argRow, 1).GetString();
            if (string.IsNullOrEmpty(wJanCode) || wJanCode.Length > 16)
            {
                return false;
            }

            //メーカコード
            var wMkCode = argSheet.Cell(argRow, 2).GetString();
            if (!string.IsNullOrEmpty(wMkCode) && wMkCode.Length > 20)
            {
                return false;
            }

            //商品コード
            var wStoCode = argSheet.Cell(argRow, 4).GetString();
            if (!string.IsNullOrEmpty(wStoCode) && wStoCode.Length > 20)
            {
                return false;
            }

            //商品名カナ
            var wTanName = argSheet.Cell(argRow, 5).GetString();
            if (!string.IsNullOrEmpty(wTanName) && wTanName.Length > 255)
            {
                return false;
            }

            //商品名
            var wItemName = argSheet.Cell(argRow, 6).GetString();
            if (!string.IsNullOrEmpty(wItemName) && wItemName.Length > 255)
            {
                return false;
            }

            //幅
            var chkW = argSheet.Cell(argRow, 7).GetString();
            int wW = 0;
            if (string.IsNullOrEmpty(chkW))
            {
                wW = 100;
            }
            else if (!int.TryParse(chkW, out wW))
            {
                return false;
            }
            else if (wW < MIN_WDH || wW > MAX_WDH)
            {
                return false;
            }

            //奥行
            var chkD = argSheet.Cell(argRow, 8).GetString();
            int wD = 0;
            if (string.IsNullOrEmpty(chkD))
            {
                wD = 100;
            }
            else if (!int.TryParse(chkD, out wD))
            {
                return false;
            }
            else if (wW < MIN_WDH || wW > MAX_WDH)
            {
                return false;
            }

            //高さ
            var chkH = argSheet.Cell(argRow, 9).GetString();
            int wH = 0;
            if (string.IsNullOrEmpty(chkH))
            {
                wH = 100;
            }
            else if (!int.TryParse(chkH, out wH))
            {
                return false;
            }
            else if (wW < MIN_WDH || wW > MAX_WDH)
            {
                return false;
            }

            //入数
            var chkPCase = argSheet.Cell(argRow, 10).GetString();
            int wPCase = 0;
            if (string.IsNullOrEmpty(chkPCase))
            {
                wPCase = 0;
            }
            else if (!int.TryParse(chkPCase, out wPCase))
            {
                return false;
            }
            else if (wPCase < 0 || wPCase > 99999)
            {
                return false;
            }

            //分類コード
            var wBunCode = argSheet.Cell(argRow, 11).GetString();
            if (!string.IsNullOrEmpty(wBunCode) && wBunCode.Length > 20)
            {
                return false;
            }

            //売価
            var chkPrice = argSheet.Cell(argRow, 13).GetString();
            double wPrice = 0;
            if (string.IsNullOrEmpty(chkPrice))
            {
                wPrice = 0;
            }
            else if (!double.TryParse(chkPrice, out wPrice))
            {
                return false;
            }
            else if (wPrice < 0 || wPrice > 9999999)
            {
                return false;
            }

            //原価
            var chkCost = argSheet.Cell(argRow, 14).GetString();
            double wCost = 0;
            if (string.IsNullOrEmpty(chkCost))
            {
                wCost = 0;
            }
            else if (!double.TryParse(chkCost, out wCost))
            {
                return false;
            }
            else if (wPrice < 0 || wPrice > 9999999)
            {
                return false;
            }

            //登録日
            var wAttr3 = argSheet.Cell(argRow, 15).GetString();
            if (!string.IsNullOrEmpty(wAttr3) && wAttr3.Length > 8)
            {
                return false;
            }

            //廃止日
            var wAttr4 = argSheet.Cell(argRow, 16).GetString();
            if (!string.IsNullOrEmpty(wAttr4) && wAttr4.Length > 8)
            {
                return false;
            }

            //更新日
            var wAttr5 = DateTime.Now.ToString("yyyyMMdd");

            #endregion

            #region パラメタ設定

            var wParams = new List<SqliteParameter>
                    {
                        new SqliteParameter("@JanCode", wJanCode),
                        new SqliteParameter("@MkCode", wMkCode),
                        new SqliteParameter("@StoCode", wStoCode),
                        new SqliteParameter("@TanName", wTanName),
                        new SqliteParameter("@ItemName", wItemName),
                        new SqliteParameter("@W", wW),
                        new SqliteParameter("@D", wD),
                        new SqliteParameter("@H", wH),
                        new SqliteParameter("@PCase", wPCase),
                        new SqliteParameter("@BunCode", wBunCode),
                        new SqliteParameter("@Price", wPrice),
                        new SqliteParameter("@Cost", wCost),
                        new SqliteParameter("@Attr3", wAttr3),
                        new SqliteParameter("@Attr4", wAttr4),
                        new SqliteParameter("@Attr5", wAttr5)
                    };
            foreach (var iParam in wParams)
            {
                switch (iParam.ParameterName)
                {
                    case "@Attr3":
                    case "@Attr4":
                        if (argSqlCls.IsNullData(iParam.Value))
                        {
                            iParam.Value = DateTime.Now.ToString("yyyyMMdd");
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
            else
            {
                //削除→追加処理の場合は既存データ削除済み
                //追加更新、削除→追加処理は同一処理
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

            return true;
        }

        /// <summary>
        /// 商品属性マスタ取込処理
        /// </summary>
        /// <param name="argSqlCls">SqlitePartsクラス</param>
        /// <param name="argConn">コネクション</param>
        /// <param name="argSheet">取込先シート</param>
        /// <param name="argRow">処理行</param>
        /// <returns></returns>
        private bool ImportSyoZokSei(SqliteParts argSqlCls, SqliteConnection argConn, IXLWorksheet argSheet, int argRow)
        {
            //JanCodeチェック
            var wJanCode = argSheet.Cell(argRow, 1).GetString();
            if (string.IsNullOrEmpty(wJanCode) || wJanCode.Length > 16)
            {
                return false;
            }

            var dtSyoZk = new List<SyoZokSei>();
            for (int iCol = 17; iCol < argSheet.ColumnsUsed().Count(); iCol++)
            {
                var tmpSyoZk = new SyoZokSei();

                //JanCode設定
                tmpSyoZk.JanCode = wJanCode;

                //属性コードチェック
                var wZkCode = argSheet.Cell(3, iCol).GetString();
                if (string.IsNullOrEmpty(wZkCode) || wZkCode.Length > 20)
                {
                    return false;
                }

                //水準コードチェック
                var wSuiCode = argSheet.Cell(argRow, iCol).GetString();
                if (string.IsNullOrEmpty(wSuiCode) || wSuiCode.Length > 20)
                {
                    return false;
                }

                tmpSyoZk.JanCode = wJanCode;
                tmpSyoZk.ZkCode = wZkCode;
                tmpSyoZk.SuiCode = wSuiCode;
                dtSyoZk.Add(tmpSyoZk);
            }

            #region 登録処理            
            foreach (var iData in dtSyoZk)
            {
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

                //削除→追加処理の場合は既存データ削除済み
                //全ての更新条件で同一処理
                var tSql = $"INSERT INTO SyoZokSei (JanCode, ZkCode, SuiCode) VALUES (@JanCode , @ZkCode, @SuiCode) ";
                tSql += " ON CONFLICT(JanCode, ZkCode, SuiCode) DO NOTHING";
                argSqlCls.ExecuteSql(argConn, tSql, wParams);

                #endregion
            }

            #endregion

            return true;
        }



        #endregion
    }
}
