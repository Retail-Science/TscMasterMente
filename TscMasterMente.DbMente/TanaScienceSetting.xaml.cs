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
    public sealed partial class TanaScienceSetting : Window
    {
        #region コンストラクタ
        
        public TanaScienceSetting()
        {
            this.InitializeComponent();

            // イベント設定
            this.Activated += TanaScienceSetting_Activated;
        }

        #endregion

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TanaScienceSetting_Activated(object sender, WindowActivatedEventArgs e)
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
            var wIniPath = clsSql.GetAppInfo("TscConfigPath");
            if(!string.IsNullOrEmpty(wIniPath) && File.Exists(wIniPath))
            {
                TxtTanaIniPath.Text = wIniPath;
            }
            else
            {
                TxtTanaIniPath.Text = "";
            }

            this.Activated -= TanaScienceSetting_Activated;
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
        /// 棚サイエンスIniファイルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnTanaIniPath_Click(object sender, RoutedEventArgs e)
        {
            var wPath = await IoParts.OpenFileAsync(this, IoParts.EnumFileType.IniFiles);
            if (wPath != null)
            {
                TxtTanaIniPath.Text = wPath;
            }
        }

        /// <summary>
        /// 実行ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            var wSucceed = false;

            try
            {

                var clsSql = new SqliteParts();

                #region チェック処理

                if (string.IsNullOrEmpty(TxtTanaIniPath.Text))
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "構成ファイルパスを設定してください。");
                    await wDialog.ShowAsync();
                    return;
                }
                else if (File.Exists(TxtTanaIniPath.Text) == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "構成ファイルが存在しません。");
                    await wDialog.ShowAsync();
                    return;
                }
                else if(TxtTanaIniPath.Text.ToUpper().EndsWith("TANA.INI") == false)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "エラー", "棚サイエンスの構成ファイル(Tana.Ini)ではありません。。");
                    await wDialog.ShowAsync();
                    return;
                }

                #endregion
                
                //IniファイルをDBに登録
                clsSql.UpdateAppInfo("TscConfigPath", TxtTanaIniPath.Text);

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
                if (wSucceed)
                {
                    var wDialog = MessageParts.ShowMessageOkOnly(this, "完了", "構成ファイルパスを保存しました。");
                    await wDialog.ShowAsync();
                }
            }
        }



        #endregion
    }
}
