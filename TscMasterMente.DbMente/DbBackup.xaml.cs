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
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.DbMente
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DbBackup : Window
    {
        #region コンストラクタ

        public DbBackup()
        {
            this.InitializeComponent();

            // イベント設定
            this.Activated += DbBackup_Activated;
        }

        #endregion

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbBackup_Activated(object sender, WindowActivatedEventArgs e)
        {
            var clsSql = new SqliteParts();
            TxtOrgDbPath.Text = clsSql.GetDbFilePath();

            //アプリアイコンを設定
            WindowParts.SetAppIcon(this);
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 600, 500);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            this.Activated -= DbBackup_Activated;
        }

        /// <summary>
        /// 閉じるボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// オリジナルDBファイルパスボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void BtnOrgDbPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.SqliteFiles);
            if (wPath != null)
            {
                TxtOrgDbPath.Text = wPath;
            }
        }

        /// <summary>
        /// バックアップDBファイルパスボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnBkupDbPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.SaveFileAsync(this, IoParts.EnumFileType.SqliteFiles);
            if (wPath != null)
            {
                TxtBkupDbPath.Text = wPath;
            }
        }

        /// <summary>
        /// 実行ボタンクリックイベント
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

                if (string.IsNullOrEmpty(TxtOrgDbPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "オリジナルDBファイルパスが設定されていません。");
                    await wDialog.ShowAsync();
                    return;
                }
                else if (File.Exists(TxtOrgDbPath.Text) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "オリジナルDBが存在しません。");
                    await wDialog.ShowAsync();
                    return;
                }

                if (string.IsNullOrEmpty(TxtBkupDbPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "バックアップDBファイルパスが設定されていません。");
                    await wDialog.ShowAsync();
                    return;
                }

                if(TxtOrgDbPath.Text == TxtBkupDbPath.Text)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "オリジナルDBとバックアップDBが一致しています。");
                    await wDialog.ShowAsync();
                    return;
                }

                #endregion

                //プログレスウィンドウの表示                               
                wProgressWindow.Activate();
                await wProgressWindow.SetInit("DBバックアップ中...", 2);

                #region DBバックアップ処理
                if (await wProgressWindow.SetProgress("テンプレートファイルをコピー中...") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "処理中断", "処理中にキャンセルを実行しました。");
                    await wDialog.ShowAsync();
                    return;
                }
                System.IO.File.Copy(TxtOrgDbPath.Text, TxtBkupDbPath.Text, true);
                #endregion

                #region バックアップファイルのチェック

                if (await wProgressWindow.SetProgress("バックアップDBチェック中...") == false)
                {
                    if (File.Exists(TxtBkupDbPath.Text) == false)
                    {
                        var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "バックアップファイルの作成に失敗しました。");
                        await wDialog.ShowAsync();
                        return;
                    }
                }

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
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "完了", "DBのバックアップが完了しました。");
                    await wDialog.ShowAsync();
                }
            }
        }

        #endregion

    }
}
