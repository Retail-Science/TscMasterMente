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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TscMasterMente
{

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankWindow1 : Window
    {
        public BlankWindow1()
        {
            this.InitializeComponent();


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
    }
}
