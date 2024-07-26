using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TscMasterMente.Common
{
    public static class MessageParts
    {
        /// <summary>
        /// OKボタンのみのメッセージボックスを表示
        /// </summary>
        /// <param name="argWindow">Window</param>
        /// <param name="argTitle">タイトル</param>
        /// <param name="argMessage">メッセージ内容</param>
        /// <returns></returns>
        public static ContentDialog ShowMessageOkOnly(Window argWindow,string argTitle, string argMessage)
        {
            ContentDialog wDialog = new ContentDialog
            {
                Title = argTitle,
                Content = argMessage,
                PrimaryButtonText = "OK",
                XamlRoot = argWindow.Content.XamlRoot 
            };

            return wDialog;
        }

        /// <summary>
        /// Yes/Noボタンのメッセージボックスを表示
        /// </summary>
        /// <param name="argWindow">Window</param>
        /// <param name="argTitle">タイトル</param>
        /// <param name="argMessage">メッセージ内容</param>
        /// <returns></returns>
        public static ContentDialog ShowMessageYesNo(Window argWindow, string argTitle, string argMessage)
        {
            ContentDialog wDialog = new ContentDialog
            {
                Title = argTitle,
                Content = argMessage,
                PrimaryButtonText = "はい",
                SecondaryButtonText = "いいえ",
                XamlRoot = argWindow.Content.XamlRoot
            };

            return wDialog;
        }

    }
}
