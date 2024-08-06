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
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TscMasterMente.Common;
using TscMasterMente.DataProc;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Windowing;
using Microsoft.VisualBasic;
using Windows.Management.Update;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : Window
    {
        #region private変数

        /// <summary>
        /// DBバージョン
        /// </summary>
        private string _DbVer = "";

        /// <summary>
        /// アプリケーションバージョン
        /// </summary>
        private string _AppVer = "";

        #endregion

        #region コンストラクタ


        public MainMenu(string dbVer, string appVer)
        {
            this.InitializeComponent();
            this.Activated += MainMenu_Activated;
            this.AppWindow.Closing += AppWindow_Closing;

            _DbVer = dbVer;
            _AppVer = appVer;

            NvMain.SelectionChanged += NavView_SelectionChanged;
            // 初期ページを表示する
            contentFrame.Navigate(typeof(PageFileProc));
        }

        #endregion

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
        private void MainMenu_Activated(object sender, WindowActivatedEventArgs args)
        {

            this.Title = $"マスタメンテ AppVer({_DbVer}) DbVer({_AppVer})";

            //アプリアイコンを設定
            WindowParts.SetAppIcon(this);
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 800, 600);
            //windowを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            NvMain.SelectedItem = NvMain.MenuItems[0];

            this.Activated -= MainMenu_Activated;
        }

        /// <summary>
        /// Closingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            //終了を一旦キャンセルしないと、awaitできない
            args.Cancel = true;

            //終了確認
            var msgText = "アプリケーションを終了しますか？";
            msgText += Environment.NewLine;
            msgText += "※ 現在進行中の処理データについては保存されません。";
            var msgDialog = MessageParts.ShowMessageYesNo(this, "終了確認", msgText);
            var wDialogResult = await msgDialog.ShowAsync();
            if (wDialogResult == ContentDialogResult.Primary)
            {
                args.Cancel = false;
                var opWindows = ((App)Application.Current).ProWindowMng.GetOpenWindows();
                if (opWindows.Count > 0)
                {
                    foreach (var iWin in opWindows.ToList())
                    {
                        iWin.Close();
                    }
                }
                Application.Current.Exit();
            }
        }

        /// <summary>
        /// ナビゲーションビュー選択変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer.Tag.ToString() == "PageFileProc")
            {
                contentFrame.Navigate(typeof(PageFileProc), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (args.SelectedItemContainer.Tag.ToString() == "PageMasterMente")
            {
                contentFrame.Navigate(typeof(PageMasterMente), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else if (args.SelectedItemContainer.Tag.ToString() == "PageDbMente")
            {
                contentFrame.Navigate(typeof(PageDbMente), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        #endregion
    }
}
