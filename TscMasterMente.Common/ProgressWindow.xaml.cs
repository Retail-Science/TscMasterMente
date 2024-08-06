using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente.Common
{

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressWindow : Window
    {
        private bool IsCancelled { get; set; } = false;

        #region コンストラクタ

        public ProgressWindow()
        {
            this.InitializeComponent();
            this.Activated += ProgressWindow_Activated;
        }
        #endregion

        #region イベント

        /// <summary>
        /// Activatedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            //ウィンドウサイズを設定
            WindowParts.SetWindowSize(this, 500, 300);
            //ウィンドウを中央に表示
            WindowParts.SetCenterPosition(this);
            //ウィンドウサイズ固定
            WindowParts.SetWindowSizeFixed(this);

            //タイトルバーを非表示
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var m_AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            var op = OverlappedPresenter.Create();
            op.IsResizable = false;
            op.IsAlwaysOnTop = false;
            op.SetBorderAndTitleBar(false, false);
            m_AppWindow.SetPresenter(op);

            this.Activated -= ProgressWindow_Activated;
        }


        /// <summary>
        /// キャンセルボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // キャンセルイベントを発生させる
            IsCancelled = true;
        }

        #endregion

        #region メソッド

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="argProcess"></param>
        /// <param name="argMaxCnt"></param>
        /// <returns></returns>
        public async Task<bool> SetInit(string argProcess, int argMaxCnt)
        {
            //ProgressBarの初期化
            PbStatus.Minimum = 0;
            if (argMaxCnt <= 0)
            {
                PbStatus.Maximum = 100;
            }
            else
            {
                PbStatus.Maximum = argMaxCnt;
            }
            PbStatus.Value = 0;

            //テキストの初期化
            TxtProcess.Text = argProcess;
            TxtProcessNumCnt.Text = "";
            TxtDetail.Text = "準備中...";

            //キャンセルフラグの初期化
            IsCancelled = false;

            await Task.Delay(100);
            return true;
        }

        /// <summary>
        /// プログレスバーの進捗を設定する
        /// </summary>
        /// <param name="argDtlMsg"></param>
        /// <returns></returns>
        public async Task<bool> SetProgress(string argDtlMsg)
        {
            if (IsCancelled)
            {
                //中断確認ダイアログを表示
                //呼び出し元とプログレス画面の同期をとる必要があるので、このタイミングでメッセージボックスを表示する
                var wDialog = MessageParts.ShowMessageYesNo(this, "中断確認", "実行中の処理を中断しますか？。");
                if (await wDialog.ShowAsync() == ContentDialogResult.Secondary)
                {
                    IsCancelled = false;
                    await SetProgress(argDtlMsg);
                    return true;
                }
                return false;
            }
            else
            {
                TxtDetail.Text = argDtlMsg;
                PbStatus.Value += 1;
                TxtProcessNumCnt.Text = PbStatus.Value.ToString() + " / " + PbStatus.Maximum.ToString();

                await Task.Delay(100);
                return true;
            }
        }

        #endregion
    }
}
