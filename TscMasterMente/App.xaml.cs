using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TscMasterMente.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

        #region private変数

        /// <summary>
        /// ウィンドウ管理クラス
        /// </summary>
        public WindowMng _WinMng = new WindowMng();

        #endregion

        #region プロパティ

        /// <summary>
        /// ウィンドウ管理クラス
        /// </summary>
        public WindowMng ProWindowMng => _WinMng;

        #endregion

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var clsSql = new TscMasterMente.Common.SqliteParts();
            clsSql.CretateTables();
            var wDbVer = clsSql.GetAppInfo("DbVer");

            #region アプリケーションバージョン取得(Unpakeagedではエラーになるのでコメント化)
            //var package = Package.Current;
            //var packageId = package.Id;
            //var version = packageId.Version;
            //string wAppVer = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            #endregion

            #region アプリケーションバージョン取得(Unpakeagedでの取得方法)
            // 実行ファイルのパスを取得
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            // ファイルバージョン情報を取得
            FileVersionInfo verInf = FileVersionInfo.GetVersionInfo(exePath);
            // バージョン情報を表示
            string wAppVer = verInf.FileVersion;
            #endregion


            var m_window = new MainMenu(wAppVer, wDbVer);
            m_window.Activate();
        }

    }
}
