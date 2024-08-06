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
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DbMente
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DbVacuum : Window
    {
        #region コンストラクタ

        public DbVacuum()
        {
            this.InitializeComponent();

            // イベント設定
            this.Activated += DbVacuum_Activated;
        }

        #endregion

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbVacuum_Activated(object sender, WindowActivatedEventArgs e)
        {
            //アプリアイコンを設定
            WindowParts.SetAppIcon(this);
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 300);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            var clsSql = new SqliteParts();
            TxtDbPath.Text = clsSql.GetDbFilePath();

            this.Activated -= DbVacuum_Activated;
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
        /// DBファイルパスボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnDbPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.SaveFileAsync(this, IoParts.EnumFileType.SqliteFiles);
            if (wPath != null)
            {
                TxtDbPath.Text = wPath;
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

            try
            {
                //ウィンドウの無効化
                WindowParts.SetAllChildEnabled(MainContent, false);

                var clsSql = new SqliteParts();

                #region チェック処理

                if (string.IsNullOrEmpty(TxtDbPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "DBファイルパスが設定されていません。");
                    await wDialog.ShowAsync();
                    return;
                }
                else if (File.Exists(TxtDbPath.Text) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "DBが存在しません。");
                    await wDialog.ShowAsync();
                    return;
                }

                #endregion

                //プログレスウィンドウの表示                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("DBを最適化中...", 1);

                #region DB最適化処理
                if (await wProgressWindow.SetProgress("最適化中...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    return;
                }
                clsSql.ExecuteVacuum(TxtDbPath.Text);

                #endregion


                wSucceed = true;
            }
            catch (Exception ex)
            {
                var tMsgDialog = MessageParts.ShowMessageOkOnly(this, "エラー", ex.Message);
                await tMsgDialog.ShowAsync();
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
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "完了", "DBの最適化が完了しました。");
                    await wDialog.ShowAsync();
                }
            }
        }

        #endregion


    }
}
