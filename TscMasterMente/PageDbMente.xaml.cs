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
                    ImagePath = "Assets/Square150x150Logo.scale-200.png",
                    Name = "MenuBackup",
                    Title = "バックアップ",
                    Detail="データベースのバックアップを行います。"
                },

                new TscMenuImageAndDescription
                { 
                    ImagePath = "Assets/Square150x150Logo.scale-200.png",
                    Name ="MenuVacuum",
                    Title = "初期化", Detail="データベースを初期化します。"
                },

                new TscMenuImageAndDescription
                { 
                    ImagePath = "Assets/Square150x150Logo.scale-200.png",
                    Name = "MenuX",
                    Title = "xxx",
                    Detail="aaa。"
                },

                new TscMenuImageAndDescription
                { 
                    ImagePath = "Assets/Square150x150Logo.scale-200.png",
                    Name = "MenuY",
                    Title = "yyy",
                    Detail="デ。"
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
                var clsSql=new TscMasterMente.Common.SqliteParts();
                var item = element.Tag.ToString();

                switch (item)
                {
                    case "MenuBackup":
                        //clsSql.BackupDb();
                        break;
                    case "MenuVacuum":
                        clsSql.ExecuteSql("VACUUM;");
                        break;
                    case "MenuX":
                        BlankWindow1 w = new BlankWindow1();
                        w.Activate();
                        break;
                    case "MenuY":
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
