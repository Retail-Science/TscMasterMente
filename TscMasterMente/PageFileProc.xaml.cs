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
using TscMasterMente.DataProc;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageFileProc : Page
    {
        #region コンストラクタ

        public PageFileProc()
        {
            this.InitializeComponent();
        }

        #endregion

        #region イベント

        /// <summary>
        /// 棚サイエンスマスタ出力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMasterExport_Click(object sender, RoutedEventArgs e)
        {

            if (MenteParts.FindWindowTitleActivatee("マスタファイル出力")) return;            

            MasterExport wMstExp = new MasterExport();
            ((App)Application.Current).ProWindowMng.AddWindow(wMstExp);
            wMstExp.Activate();
        }

        /// <summary>
        /// 棚サイエンスマスタ取込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuMasterImport_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("マスタファイル取込")) return;

            MasterImport wMstImp = new MasterImport();
            ((App)Application.Current).ProWindowMng.AddWindow(wMstImp);
            wMstImp.Activate();
        }

        /// <summary>
        /// Excel出力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemExp_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("Excel商品マスタ出力")) return;

            ExcelItemMasterExport wExp = new ExcelItemMasterExport();
            ((App)Application.Current).ProWindowMng.AddWindow(wExp);
            wExp.Activate();
            
        }

        /// <summary>
        /// Excel取込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemImp_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("Excel商品マスタ取込")) return;

            ExcelItemMasterImport wImp = new ExcelItemMasterImport();
            ((App)Application.Current).ProWindowMng.AddWindow(wImp);
            wImp.Activate();
        }

        /// <summary>
        /// Excel新規作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuExcelItemBlank_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("Excel商品マスタ出力-ブランク")) return;

            ExcelItemMasterBlank wBlank = new ExcelItemMasterBlank();
            ((App)Application.Current).ProWindowMng.AddWindow(wBlank);
            wBlank.Activate();
        }

        /// <summary>
        /// Planetデータ変換
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlanet_Click(object sender, RoutedEventArgs e)
        {
            if (MenteParts.FindWindowTitleActivatee("プラネットデータ出力")) return;

            PlanetConvert wPlanet = new PlanetConvert();
            ((App)Application.Current).ProWindowMng.AddWindow(wPlanet);
            wPlanet.Activate();
        }
        #endregion


    }
}
