using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using TscMasterMente.Common;
using System.Diagnostics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DataProc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExcelItemMasterBlank : Window
    {
        #region コンストラクタ

        public ExcelItemMasterBlank()
        {
            this.InitializeComponent();

            // イベント設定
            this.Activated += ExcelItemMasterBlank_Activated;
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
        private void ExcelItemMasterBlank_Activated(object sender, WindowActivatedEventArgs e)
        {
            var clsSql = new SqliteParts();
            TxtMasterPath.Text = clsSql.GetAppInfo("MasterExcelDir");

            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 300);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= ExcelItemMasterBlank_Activated;
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

                //プログレスウィンドウの表示                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("Excelマスタファイル出力中...", 3);

                #region テンプレートのコピー
                if (await wProgressWindow.SetProgress("テンプレートファイルをコピー中...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    return;
                }
                var wFilePath = Path.Combine(wDirPath, $"MstMnt_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
                System.IO.File.Copy(orgFilePath, wFilePath, true);
                #endregion

                #region Excelファイルの保存
                if (await wProgressWindow.SetProgress("設定中...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    return;
                }
                //ファイルコピーだけでは更新日時が変更されないので、保存を実施する
                using (var xlBook = new ClosedXML.Excel.XLWorkbook(wFilePath))
                {
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
    }
}
