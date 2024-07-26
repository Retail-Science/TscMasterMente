using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common.SqlMaps;
using TscMasterMente.Common;
using System.Diagnostics;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Reflection.Emit;
using TscMasterMente.Common.MasterFileEntity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExcelItemMasterExport : Window
    {
        #region ListViewsデータソース
        
        /// <summary>
        /// メーカー定義リスト
        /// </summary>
        public ObservableCollection<MkMaster> MakerDef { get; } = new ObservableCollection<MkMaster>();
        /// <summary>
        /// メーカー選択リスト
        /// </summary>
        public ObservableCollection<MkMaster> MakerSel { get; } = new ObservableCollection<MkMaster>();
        /// <summary>
        /// 分類定義リスト
        /// </summary>
        public ObservableCollection<BunMaster> BunDef { get; } = new ObservableCollection<BunMaster>();
        /// <summary>
        /// 分類選択リスト
        /// </summary>
        public ObservableCollection<BunMaster> BunSel { get; } = new ObservableCollection<BunMaster>();

        #endregion

        #region コンストラクタ

        public ExcelItemMasterExport()
        {
            this.InitializeComponent();

            var clsSql = new SqliteParts();

            //メーカーマスタ取得
            var wMkMasters = clsSql.GetSelectResult<MkMaster>("SELECT * FROM MkMaster ORDER BY MkCode");
            foreach (var item in wMkMasters)
            {
                MakerDef.Add(item);
            }

            //分類マスタ取得
            var wBunMasters = clsSql.GetSelectResult<BunMaster>("SELECT * FROM BunMaster ORDER BY BunCode");
            foreach (var item in wBunMasters)
            {
                BunDef.Add(item);
            }   

            this.Activated += ExcelItemMasterExport_Activated;
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
        private void ExcelItemMasterExport_Activated(object sender, WindowActivatedEventArgs e)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterExcelDir");

            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 850);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= ExcelItemMasterExport_Activated;
        }

        /// <summary>
        /// メーカー選択ボタンクリック(右へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkSet_Click(object sender, RoutedEventArgs e)
        {
            var wMkDefined = LvMkDefined.SelectedItems;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkDefined)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, true);
        }

        /// <summary>
        /// メーカー選択ボタンクリック(左へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkUnset_Click(object sender, RoutedEventArgs e)
        {
            var wMkSelected = LvMkSelected.SelectedItems;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkSelected)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, false);

        }

        /// <summary>
        /// メーカー全選択ボタンクリック(右へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkAllSet_Click(object sender, RoutedEventArgs e)
        {
            var wMkAllDifined = LvMkDefined.Items;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkAllDifined)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, true);

        }

        /// <summary>
        /// メーカー全選択ボタンクリック(左へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMkAllUnset_Click(object sender, RoutedEventArgs e)
        {
            var wMkAllSelected = LvMkSelected.Items;
            var wMkTarget = new ObservableCollection<MkMaster>();
            foreach (MkMaster iMaker in wMkAllSelected)
            {
                wMkTarget.Add(iMaker);
            }

            MakerMoveItems(wMkTarget, false);
        }

        /// <summary>
        /// 分類選択ボタンクリック(右へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunSet_Click(object sender, RoutedEventArgs e)
        {
            var wBunDefined = LvBunDefined.SelectedItems;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunDefined)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, true);
        }

        /// <summary>
        /// 分類選択ボタンクリック(左へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunUnset_Click(object sender, RoutedEventArgs e)
        {
            var wBunSelected = LvBunSelected.SelectedItems;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunSelected)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, false);
        }

        /// <summary>
        /// 分類全選択ボタンクリック(右へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunAllSet_Click(object sender, RoutedEventArgs e)
        {
            var wBunAllDifined = LvBunDefined.Items;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunAllDifined)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, true);
        }

        /// <summary>
        /// 分類全選択ボタンクリック(左へ)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBunAllUnset_Click(object sender, RoutedEventArgs e)
        {
            var wBunAllSelected = LvBunSelected.Items;
            var wBunTarget = new ObservableCollection<BunMaster>();
            foreach (BunMaster iBun in wBunAllSelected)
            {
                wBunTarget.Add(iBun);
            }

            BunMoveItems(wBunTarget, false);
        }
        
        /// <summary>
        /// 閉じるボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// フォルダ選択ボタンクリック
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
        /// 実行ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wProgressWindow = new ProgressWindow();
            try
            {
                //ウィンドウの無効化
                WindowParts.SetAllChildEnabled(MainContent, false);

                var clsSql = new SqliteParts();

                #region チェック処理

                //テンプレートファイルが存在するかチェック
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string dirPath = System.IO.Path.GetDirectoryName(exePath);
                var orgFilePath = Path.Combine(dirPath, "Assets", "MstMnt.xlsx");
                if (!File.Exists(orgFilePath))
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "テンプレートファイルが存在しません。");
                    await tMsgDialog.ShowAsync();
                    return;
                }


                //出力先フォルダが設定されているかチェック
                var wDirPath = TxtMasterPath.Text;
                if (string.IsNullOrEmpty(wDirPath))
                {
                    //Okボタンメッセージボックスの表示
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "出力先フォルダが設定されていません。");
                    await wDialog.ShowAsync();

                    return;
                }
                else if (Directory.Exists(wDirPath) == false)
                {
                    Directory.CreateDirectory(wDirPath);
                }
                clsSql.UpdateAppInfo("MasterExcelDir", wDirPath);

                #endregion

                #region Excel出力
                const int EXCEL_MAX_ROW = 1048576;

                var dtItems = GetOutputData();
                if (dtItems.Count == 0)
                {
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", $"条件に合致するデータがありません。");
                    await tMsgDialog.ShowAsync();
                    return;
                }
                if (dtItems.Count > EXCEL_MAX_ROW)
                {
                    string tMsg = $"出力データが{EXCEL_MAX_ROW}件を超えています。";
                    tMsg += Environment.NewLine;
                    tMsg += "出力条件を変更して実行してください。";
                    var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", tMsg);
                    await tMsgDialog.ShowAsync();
                    return;
                }
                var dtZokMaster = GetZokMaster();

                //プログレスウィンドウの表示                               
                wProgressWindow.Activate();
                //ファイルコピーとExcel起動を含めて+2
                await wProgressWindow.SetInit("Excelマスタファイル出力中...", dtItems.Count + 2);

                //テンプレートのコピー
                if (await wProgressWindow.SetProgress("テンプレートファイルをコピー中...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    return;
                }
                var wFilePath = Path.Combine(wDirPath, $"MstMnt_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                System.IO.File.Copy(orgFilePath, wFilePath, true);

                //closedxmlワークシート
                using (var xlBook = new ClosedXML.Excel.XLWorkbook(wFilePath))
                {
                    var xlSheet = xlBook.Worksheet(1);

                    if(ChkZok.IsChecked == true)
                    {
                        //属性マスタの出力
                        const int ZOK_CODE_ROW = 3;
                        const int ZOK_NAME_ROW = 4;
                        var actZokCol = 16;
                        foreach (var iZok in dtZokMaster)
                        {
                            actZokCol++;

                            //属性コード
                            xlSheet.Cell(ZOK_CODE_ROW, actZokCol).Value = iZok.ZkCode;
                            //属性名
                            xlSheet.Cell(ZOK_NAME_ROW, actZokCol).Value = iZok.ZkName;
                        }
                    }

                    var actRow = 4;
                    foreach(var iItem in dtItems)
                    {
                        actRow++;

                        string wProgressMsg = $"{actRow}行目を処理中...";
                        if (await wProgressWindow.SetProgress(wProgressMsg) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        //JANコード
                        xlSheet.Cell(actRow, 1).Value = iItem.JanCode;
                        //メーカーコード
                        xlSheet.Cell(actRow, 2).Value = iItem.MkCode;
                        //メーカー名
                        xlSheet.Cell(actRow, 3).Value = iItem.MkName;
                        //商品コード
                        xlSheet.Cell(actRow, 4).Value = iItem.StoCode;
                        //商品名カナ
                        xlSheet.Cell(actRow, 5).Value = iItem.TanName;
                        //商品名
                        xlSheet.Cell(actRow, 6).Value = iItem.ItemName;
                        //幅
                        xlSheet.Cell(actRow, 7).Value = iItem.W;
                        //高さ
                        xlSheet.Cell(actRow, 8).Value = iItem.H;
                        //奥行
                        xlSheet.Cell(actRow, 9).Value = iItem.D;
                        //入数
                        xlSheet.Cell(actRow, 10).Value = iItem.PCase;
                        //分類コード
                        xlSheet.Cell(actRow, 11).Value = iItem.BunCode;
                        //分類名
                        xlSheet.Cell(actRow, 12).Value = iItem.BunName;
                        //売価
                        xlSheet.Cell(actRow, 13).Value = iItem.Price;
                        //原価
                        xlSheet.Cell(actRow, 14).Value = iItem.Cost;
                        //登録日
                        xlSheet.Cell(actRow, 15).Value = iItem.Attr3;
                        //廃止日
                        xlSheet.Cell(actRow, 16).Value = iItem.Attr4;

                        if (ChkZok.IsChecked == true)
                        {
                            var actZokCol = 16;
                            var dtSyoZokSei = GetSyoZokSei(iItem.JanCode);
                            foreach (var iZk in dtZokMaster)
                            {
                                actZokCol++;
                                var lqZok = dtSyoZokSei.Where(x => x.ZkCode == iZk.ZkCode).FirstOrDefault();
                                if (lqZok != null)
                                {
                                    xlSheet.Cell(actRow, actZokCol).Value = lqZok.SuiCode;
                                }
                            }
                        }
                    }

                    xlBook.SaveAs(wFilePath);
                }


                #endregion

                #region Excel起動

                if (await wProgressWindow.SetProgress("Excelマスタファイルを起動中...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    return;
                }
                var wInf = new ProcessStartInfo
                {
                    FileName = "EXCEL.EXE",
                    Arguments = wFilePath,
                    UseShellExecute = true
                };
                Process.Start(wInf);

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

        #region 選択処理関連

        /// <summary>
        /// メーカー選択処理
        /// </summary>
        /// <param name="argItems">移動対象メーカー</param>
        /// <param name="argIsSet">移動設定(true:定義→選択,false:選択→定義)</param>
        private void MakerMoveItems(ObservableCollection<MkMaster> argItems, bool argIsSet)
        {

            foreach (MkMaster iMaker in argItems)
            {
                if (argIsSet)
                {
                    MakerSel.Add(iMaker);
                    MakerDef.Remove(iMaker);
                }
                else
                {
                    MakerDef.Add(iMaker);
                    MakerSel.Remove(iMaker);
                }
            }
        }

        /// <summary>
        /// 分類選択処理
        /// </summary>
        /// <param name="argItems">移動対象分類</param>
        /// <param name="argIsSet">移動設定(true:定義→選択,false:選択→定義)</param>
        private void BunMoveItems(ObservableCollection<BunMaster> argItems, bool argIsSet)
        {

            foreach (BunMaster iBun in argItems)
            {
                if (argIsSet)
                {
                    BunSel.Add(iBun);
                    BunDef.Remove(iBun);
                }
                else
                {
                    BunDef.Add(iBun);
                    BunSel.Remove(iBun);
                }
            }
        }

        #endregion

        #region Excel出力関連

        /// <summary>
        /// Excel出力データ取得
        /// </summary>
        /// <returns></returns>
        private IList<SyoMasterExcel> GetOutputData()
        {
            var clsSql = new SqliteParts();
            string wSql = "";
            wSql += "SELECT s.JanCode, s.MkCode, m.MkName, s.StoCode, s.TanName, s.ItemName, ";
            wSql += " s.W, s.H, s.D, s.PCase, s.BunCode, b.BunName, s.Price, s.Cost, s.Attr3, s.Attr4";
            wSql += " FROM(BunMaster b RIGHT JOIN SyoMaster s ON b.BunCode = s.BunCode)";
            wSql += " LEFT JOIN MkMaster m ON s.MkCode = m.MkCode";
            wSql +=" WHERE 1 = 1";

            #region メーカー絞込

            string wMkCond = "";
            if (LvMkSelected.SelectedItems.Count > 0)
            {
                foreach(MkMaster iMaker in LvMkSelected.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(wMkCond)) wMkCond += " , ";
                    wMkCond += $" '{iMaker.MkCode}'";
                }
            }
            if (!string.IsNullOrEmpty(wMkCond))
            {
                wSql += $" AND s.MkCode IN ({wMkCond}) ";
            }

            #endregion

            #region 分類絞込

            string wBunCond = "";
            if (LvBunSelected.SelectedItems.Count > 0)
            {
                foreach (BunMaster iBun in LvBunSelected.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(wBunCond)) wBunCond += " , ";
                    wBunCond += $" '{iBun.BunCode}'";
                }
            }
            if (!string.IsNullOrEmpty(wBunCond))
            {
                wSql += $" AND s.BunCode IN ({wBunCond}) ";
            }
            #endregion

            wSql += " ORDER BY s.JanCode";

            var tData = clsSql.GetSelectResult<SyoMasterExcel>(wSql);

            return tData;
        }

        /// <summary>
        /// 属性マスタの取得
        /// </summary>
        /// <returns></returns>
        private IList<ZokMaster> GetZokMaster()
        {

           var clsSql = new SqliteParts();
            string wSql = "";
            wSql += "SELECT * FROM ZokMaster";
            wSql += " ORDER BY ZkCode";

            var tData = clsSql.GetSelectResult<ZokMaster>(wSql);

            return tData;
        }

        /// <summary>
        /// 商品属性の取得
        /// </summary>
        /// <param name="argJanCode"></param>
        /// <returns></returns>
        private IList<SyoZokSei> GetSyoZokSei(string argJanCode)
        {

            var clsSql = new SqliteParts();
            string wSql = "";
            wSql += "SELECT * FROM SyoZokSei";
            wSql += $" WHERE JanCode = '{argJanCode}'";

            var tData = clsSql.GetSelectResult<SyoZokSei>(wSql);

            return tData;
        }

        #endregion

        #endregion


    }
}

