using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.SqlMaps
{
    public class Masters
    {
        /// <summary>
        /// マスタコード
        /// </summary>
        public int MasterCode { get; set; } = 0;
        /// <summary>
        /// マスタ名
        /// </summary>
        public string MasterName { get; set; } = null;
        /// <summary>
        /// マスタファイル名
        /// </summary>
        public string MasterFileName { get; set; } = null;
        /// <summary>
        /// エラーマスタファイル名
        /// </summary>
        public string ErrFileName { get; set; } = null;
    }
}
