using Microsoft.UI.Windowing;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace TscMasterMente.Common
{
    public static class WindowParts
    {
        #region カプセル化

        /// <summary>
        /// AppWindowを取得
        /// </summary>
        /// <param name="argWindow">ウィンドウ</param>
        /// <returns></returns>
        private static AppWindow GetAppWindow(Window argWindow)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(argWindow);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            return appWindow;
        }

        #endregion

        #region 呼出メソッド

        /// <summary>
        /// ウィンドウを中央に表示
        /// </summary>
        /// <param name="argWindow">ウィンドウ</param>
        public static void SetCenterPosition(Window argWindow)
        {

            // ウィンドウサイズを取得
            var windowWidth = argWindow.Bounds.Width;
            var windowHeight = argWindow.Bounds.Height;

            // 画面サイズを取得
            var displayArea = DisplayArea.Primary;
            var displayWidth = displayArea.WorkArea.Width;
            var displayHeight = displayArea.WorkArea.Height;

            // 中央位置を計算
            var centerX = (int)((displayWidth - windowWidth) / 2);
            var centerY = (int)((displayHeight - windowHeight) / 2);

            // ウィンドウ位置を設定
            var appWindow = GetAppWindow(argWindow);
            if (appWindow != null)
            {
                appWindow.MoveAndResize(new RectInt32(centerX, centerY, (int)windowWidth, (int)windowHeight));
            }
        }

        /// <summary>
        /// ウィンドウサイズを設定
        /// </summary>
        /// <param name="argWidth">幅</param>
        /// <param name="argHeight">高さ</param>
        public static void SetWindowSize(Window argWindow, int argWidth, int argHeight)
        {
            // AppWindowオブジェクトを取得
            var appWindow = GetAppWindow(argWindow);
            if (appWindow != null)
            {
                // 新しいウィンドウサイズを設定
                appWindow.Resize(new SizeInt32(argWidth, argHeight));
            }
        }

        /// <summary>
        /// 子コントロールの有効/無効を設定
        /// </summary>
        /// <param name="element">親コントロール</param>
        /// <param name="isEnabled">true:有効/false:無効</param>
        public static void SetAllChildEnabled(UIElement element, bool isEnabled)
        {
            var allCtrl = GetAllChildControls(element);
            foreach (var ctrl in allCtrl)
            {
                if (ctrl is Control control)
                {
                    control.IsEnabled = isEnabled;
                }
            }
        }

        /// <summary>
        /// 子コントロールを取得
        /// </summary>
        /// <param name="parent">親コントロール</param>
        /// <returns></returns>
        public static List<UIElement> GetAllChildControls(DependencyObject parent)
        {
            List<UIElement> controls = new List<UIElement>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                UIElement child = VisualTreeHelper.GetChild(parent, i) as UIElement;
                if (child != null)
                {
                    controls.Add(child);
                    controls.AddRange(GetAllChildControls(child));
                }
            }

            return controls;
        }

        /// <summary>
        /// ウィンドウの最大化と最小化のボタンの有効無効を設定する
        /// </summary>
        /// <param name="argWindow">ウィンドウ</param>
        /// <param name="argMax">最大化設定(true:有効/false:無効)</param>
        /// <param name="argMin">最小化設定(true:有効/false:無効)</param>
        public static void SetMaxAndMinBox(Window argWindow, bool argMax, bool argMin)
        {
            // AppWindowオブジェクトを取得
            var appWindow = GetAppWindow(argWindow);
            if (appWindow != null)
            {
                // ウィンドウのプレゼンターを取得し、OverlappedPresenterにキャスト
                var presenter = appWindow.Presenter as OverlappedPresenter;

                // 最大化と最小化のボタンを無効にする
                presenter.IsMaximizable = argMax;
                presenter.IsMinimizable = argMin;
            }
        }

        /// <summary>
        /// ウィンドウのサイズを固定するメソッド
        /// </summary>
        /// <param name="argWindow">ウィンドウ</param>
        public static void SetWindowSizeFixed(Window argWindow)
        {
            var appWindow = GetAppWindow(argWindow);
            if (appWindow != null)
            {
                appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);

                var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
            }
        }

        /// <summary>
        /// ウィンドウのタイトルバーアイコンを設定
        /// </summary>
        /// <param name="argWindow">ウィンドウ</param>
        public static void SetAppIcon(Window argWindow)
        {
            // AppWindowオブジェクトを取得
            var appWindow = GetAppWindow(argWindow);
            if (appWindow != null)
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exeDirPath = System.IO.Path.GetDirectoryName(exePath);
                var xPath = System.IO.Path.Combine(exeDirPath, "Assets", "AppIcon.ico");
                appWindow.SetIcon(xPath);
            }
        }
        #endregion
    }
}
