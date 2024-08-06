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
using Microsoft.Data.Sqlite;
using TscMasterMente.Common;
using TscMasterMente.DbMente;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageDbMente : Page
    {
        public PageDbMente()
        {
            this.InitializeComponent();
            //List<string> names = new List<string> { "Alice", "Bob", "Charlie", "Daisy", "Edward" };
            //itemRepeater.ItemsSource = names;

            List<TscMenuImageAndDescription> items = new List<TscMenuImageAndDescription>
            {
                new TscMenuImageAndDescription
                {
                    ImagePath = "Assets/DbBkMenuImage_32x32.png",
                    Name = "MenuBackup",
                    Title = "バックアップ",
                    Detail="データベースのバックアップを行います。",
                    tooltipText = "マスタメンテで使用するDBのバックアップを行います。"
                },

                new TscMenuImageAndDescription
                {
                    ImagePath = "Assets/VacuumMenuImage_32x32.png",
                    Name ="MenuDbVacuum",
                    Title = "最適化",
                    Detail="データベースを最適化します。",
                    tooltipText = "マスタメンテで使用するDBの最適化を庫内ます。"
                },

                new TscMenuImageAndDescription
                {
                    ImagePath = "Assets/TscConfigMenuImage.png",
                    Name = "MenuTanaIni",
                    Title = "構成ファイル",
                    Detail="棚サイエンスの構成ファイルの参照先を設定します。",
                    tooltipText = "棚サイエンスのTANA.INIの参照先を設定します。"
                },

                new TscMenuImageAndDescription
                {
                    ImagePath = "Assets/Square150x150Logo.scale-200.png",
                    Name = "MenuDummy",
                    Title = "テスト",
                    Detail="ワーク用"
                },
            };
            itemRepeater.ItemsSource = items;
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TscMenuImageAndDescription item)
            {
                var itemTitle = item.Title;
                var itemDetail = item.Detail;
                //var messageDialog = new ContentDialog
                //{
                //    Title = item.Title,
                //    Content = item.Description,
                //    CloseButtonText = "OK"
                //};

                //messageDialog.XamlRoot = this.Content.XamlRoot;
                //_ = messageDialog.ShowAsync();
            }
        }

        private void PanelMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag != null)
            {
                var clsSql = new TscMasterMente.Common.SqliteParts();
                var item = element.Tag.ToString();

                switch (item)
                {
                    case "MenuBackup":
                        if (MenteParts.FindWindowTitleActivatee("DBバックアップ")) return;

                        DbBackup wDbBk = new DbBackup();
                        ((App)Application.Current).ProWindowMng.AddWindow(wDbBk);
                        wDbBk.Activate();
                        break;
                    case "MenuDbVacuum":
                        if (MenteParts.FindWindowTitleActivatee("DB最適化")) return;
                        
                        DbVacuum wDbVac = new DbVacuum();
                        ((App)Application.Current).ProWindowMng.AddWindow(wDbVac);
                        wDbVac.Activate();
                        break;
                    case "MenuTanaIni":
                        if (MenteParts.FindWindowTitleActivatee("棚サイエンス-参照先構成ファイルの設定")) return;

                        TanaScienceSetting wTanaScience = new TanaScienceSetting();
                        ((App)Application.Current).ProWindowMng.AddWindow(wTanaScience);
                        wTanaScience.Activate();
                        break;
                    case "MenuDummy":
                        BlankWindow1 wDmy = new BlankWindow1();
                        wDmy.Activate();
                        break;
                    default:
                        break;
                }

                //var messageDialog = new ContentDialog
                //{
                //    Title = item.Title,
                //    Content = item.Description,
                //    CloseButtonText = "OK"
                //};

                //messageDialog.XamlRoot = this.Content.XamlRoot;
                //_ = messageDialog.ShowAsync();
            }
        }
    }
}
