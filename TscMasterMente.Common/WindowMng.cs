using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common
{
    public class WindowMng
    {
        private List<Window> _windows = new List<Window>();

        /// <summary>
        /// 管理ウィンドウの追加
        /// </summary>
        /// <param name="window">管理対象ウィンドウ</param>
        public void AddWindow(Window window)
        {
            _windows.Add(window);
            window.Closed += (sender, args) => _windows.Remove(window);
        }

        /// <summary>
        /// 開いているウィンドウの取得
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<Window> GetOpenWindows()
        {
            return _windows.AsReadOnly();
        }
    }
}
