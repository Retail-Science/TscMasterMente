using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente
{
    public class TscMenuImageAndDescription
    {
        /// <summary>
        /// 画像ファイルパス
        /// </summary>
        public string ImagePath { get; set; } = null;
        /// <summary>
        /// キー名称
        /// </summary>
        public string Name { get; set; } = null;
        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; } = null;
        /// <summary>
        /// 詳細
        /// </summary>
        public string Detail { get; set; } = null;
        /// <summary>
        /// ツールチップテキスト
        /// </summary>
        public string tooltipText { get; set; } = null;
    }
}
