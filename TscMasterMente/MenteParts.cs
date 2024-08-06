using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace TscMasterMente
{
    public static class MenteParts
    {
        /// <summary>
        /// Windowタイトルを検索してアクティブ化
        /// </summary>
        /// <param name="argtitle">Windowタイトル</param>
        /// <returns></returns>
        public static bool FindWindowTitleActivatee(string argtitle)
        {
            var opWindows = ((App)Application.Current).ProWindowMng.GetOpenWindows();
            foreach (var iWin in opWindows)
            {
                if (iWin.Title == argtitle)
                {
                    iWin.Activate();
                    return true;
                }
            }
            return false;
        }
    }
}
