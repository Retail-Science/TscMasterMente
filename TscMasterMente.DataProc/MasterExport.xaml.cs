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
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using TscMasterMente.Common;
using TscMasterMente.Common.MasterFileEntity;
using TscMasterMente.Common.SqlMaps;
using Microsoft.VisualBasic;
using static TscMasterMente.Common.SqliteParts;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MasterExport : Window
    {
        public MasterExport()
        {
            this.InitializeComponent();
            this.Activated += MasterExport_Activated;
        }

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// 【備考】
        /// WinUI3はLoadイベントがないため、Activatedイベントを使用して初期化処理を行う。
        /// </remarks>
        private void MasterExport_Activated(object sender, WindowActivatedEventArgs args)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterTextDir");

            //アプリアイコンを設定
            WindowParts.SetAppIcon(this);
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 700);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= MasterExport_Activated;
            
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
        /// フォルダボタン
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

                //csvファイルの出力
                var clsCsv = new CsvHelperParts();

                //sqltscmasterインスタンス
                var clsSql = new SqliteParts();

                //マスタ情報を取得
                var wMasters = clsSql.GetSelectResult<Masters>("SELECT * FROM Masters");

                #region 実行前チェック

                //マスタ出力対象チェックが設定されているかチェック
                if (IsValidCheckedMaster() == false)
                {
                    //Okボタンメッセージボックスの表示
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "マスタ出力対象が設定されていません。");
                    await wDialog.ShowAsync();

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
                clsSql.UpdateAppInfo("MasterTextDir", wDirPath);
                #endregion

                #region 出力処理

                //プログレスウィンドウの表示                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("マスタ出力中...", GetMasterCount());

                //メーカーマスタ
                if (ChkMakerMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.MakerMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "メーカーマスタ";
                        if(await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT * FROM MkMaster ORDER BY MkCode ";
                        var tData = clsSql.GetSelectResult<MkMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<MkMasterEntity>(tPath, tData);
                    }
                }

                //商品マスタ
                if (ChkItemMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SyoMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {

                        wExecMaster = "商品マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT * FROM SyoMaster ";
                        if (ChkDisposal.IsChecked == true)
                        {
                            tSql += " WHERE (Attr4 IS NULL ";
                            tSql += " OR Attr4 = '' ";
                            tSql += $" OR Attr4 > '{DateTime.Now.ToString("yyyyMMdd")}') ";
                            //tSql += " and mkCode='JTI' ";
                        }
                        tSql += " ORDER BY JanCode ";

                        var tData = clsSql.GetSelectResult<SyoMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SyoMasterEntity>(tPath, tData);
                    }
                }

                //細分類マスタ
                if (ChkSubClassMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.BunMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "細分類マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT BunCode, BunName ";
                        tSql += " FROM BunMaster ";
                        tSql += " ORDER BY BunCode ";
                        var tData = clsSql.GetSelectResult<BunMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<BunMasterEntity>(tPath, tData);
                    }
                }

                //属性マスタ
                if (ChkAttributeMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.ZokMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "属性マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT BunCode, ZkCode, ZkName ";
                        tSql += " FROM ZokMaster ";
                        tSql += " ORDER BY ZkCode ";
                        var tData = clsSql.GetSelectResult<ZokMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<ZokMasterEntity>(tPath, tData);
                    }
                }

                //水準マスタ
                if (ChkLevelMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SuiMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "水準マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT BunCode, ZkCode, SuiCode, SuiName ";
                        tSql += " FROM SuiMaster ";
                        tSql += " ORDER BY ZkCode, SuiCode ";
                        var tData = clsSql.GetSelectResult<SuiMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SuiMasterEntity>(tPath, tData);
                    }
                }

                //商品属性マスタ
                if (ChkItemAttributeMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SyoZokSei).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "商品属性マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "";
                        if (ChkDisposal.IsChecked == true)
                        {
                            tSql = " SELECT SyoZoksei.JanCode, SyoZoksei.ZkCode, SyoZoksei.SuiCode ";
                            tSql += " FROM SyoZoksei";
                            tSql += " INNER JOIN SyoMaster ON SyoZoksei.JanCode = SyoMaster.JanCode ";
                            tSql += $" AND (SyoMaster.Attr4 IS NULL OR SyoMaster.Attr4 = '' OR SyoMaster.Attr4 > '{DateTime.Now.ToString("yyyyMMdd")}') ";
                            tSql += " ORDER BY SyoZoksei.JanCode, SyoZoksei.ZkCode, SyoZoksei.SuiCode ";
                        }
                        else
                        {
                            tSql = "SELECT JanCode, ZkCode, SuiCode ";
                            tSql += " FROM SyoZokSei ";
                            tSql += " ORDER BY JanCode, ZkCode, SuiCode ";
                        }
                        var tData = clsSql.GetSelectResult<SyoZokSeiEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SyoZokSeiEntity>(tPath, tData);
                    }
                }

                //POPマスタ
                if (ChkPopMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.PopMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "POPマスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT PopCode, PopName, W, H, PBunCode ";
                        tSql += " FROM PopMaster ";
                        tSql += " ORDER BY PopCode";
                        var tData = clsSql.GetSelectResult<PopMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<PopMasterEntity>(tPath, tData);
                    }
                }

                //POP分類マスタ
                if (ChkPopClassMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.PBunMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "POP分類マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT PBunCode, PBunName ";
                        tSql += " FROM PBunMaster ";
                        tSql += " ORDER BY PBunCode ";
                        var tData = clsSql.GetSelectResult<PBunMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<PBunMasterEntity>(tPath, tData);
                    }
                }

                //店舗マスタ
                if (ChkStoreMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.TenpoMaster).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "店舗マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT ";
                        tSql += " TenpoMaster.TenpoNumber, ";
                        tSql += " TenpoMaster.TenpoCode, ";
                        tSql += " TenpoMaster.TenpoTanName, ";
                        tSql += " TenpoMaster.TenpoName, ";
                        tSql += " CompMaster.CompCode, ";
                        tSql += " CompMaster.CompName, ";
                        tSql += " AreaMaster.AreaCode, ";
                        tSql += " AreaMaster.AreaName ";
                        tSql += " FROM ";
                        tSql += " TenpoMaster ";
                        tSql += " LEFT JOIN CompMaster ON TenpoMaster.CompCode = CompMaster.CompCode ";
                        tSql += " LEFT JOIN AreaMaster ON TenpoMaster.AreaCode = AreaMaster.AreaCode ";
                        tSql += " ORDER BY ";
                        tSql += " TenpoMaster.TenpoNumber ";
                        var tData = clsSql.GetSelectResult<TenpoMasterEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<TenpoMasterEntity>(tPath, tData);
                    }
                }

                //ゴンドラ什器マスタ
                if (ChkGondolaMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.GonJyu).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "ゴンドラ什器マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT GonJyuName ";
                        tSql += " FROM GonJyu ";
                        tSql += " ORDER BY GonJyuName ";
                        var tData = clsSql.GetSelectResult<GonJyuEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<GonJyuEntity>(tPath, tData);
                    }
                }

                //棚段什器マスタ
                if (ChkShelfMaster.IsChecked == true)
                {
                    var lqMaster = wMasters.Where(x => x.MasterCode == (int)EnumMasterInfo.SlfJyu).FirstOrDefault();
                    if (lqMaster != null)
                    {
                        wExecMaster = "棚段什器マスタ";
                        if (await wProgressWindow.SetProgress(wExecMaster) == false)
                        {
                            var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                            await wDialog.ShowAsync();
                            return;
                        }

                        string tSql = "SELECT SlfJyuName ";
                        tSql += " FROM SlfJyu ";
                        tSql += " ORDER BY SlfJyuName ";
                        var tData = clsSql.GetSelectResult<SlfJyuEntity>(tSql);
                        var tPath = Path.Combine(TxtMasterPath.Text, lqMaster.MasterFileName);
                        clsCsv.WriteMasterFile<SlfJyuEntity>(tPath, tData);
                    }
                }

                #endregion

                wSucceed = true;

            }
            catch (Exception ex)
            {
                //エラーメッセージの表示
                var errMsg = $"{wExecMaster }マスタ出力中にエラーが発生しました。";
                errMsg +="\r\n";
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
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "完了", "マスタ出力が完了しました。");
                    await wDialog.ShowAsync();
                }
            }
        }

        #endregion

        #region メソッド

        #region ステータスバー

        /// <summary>
        /// 出力マスタ数を取得
        /// </summary>
        /// <returns></returns>
        private int GetMasterCount()
        {
            int wMasterCount = 0;
            foreach (var child in GrdMasterCheckBoxes.Children)
            {
                if (child is CheckBox chk && chk.IsChecked == true)
                {
                    wMasterCount++;
                }
            }
            if (wMasterCount == 0) wMasterCount = 100;

            return wMasterCount;
        }

        #endregion

        #region 実行


        /// <summary>
        /// マスタ出力対象チェックが設定されているかチェック
        /// </summary>
        /// <returns></returns>
        private bool IsValidCheckedMaster()
        {
            foreach (var child in GrdMasterCheckBoxes.Children)
            {
                switch (child)
                {
                    case CheckBox checkBox:
                        // CheckBoxに対する処理
                        if (checkBox.IsChecked == true)
                        {
                            return true;
                        }
                        break;
                    case ComboBox comboBox:
                        break;
                }
            }

            return false;
        }

        #endregion

        #endregion
    }


}
