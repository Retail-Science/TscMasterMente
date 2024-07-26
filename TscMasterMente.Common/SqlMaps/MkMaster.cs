using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.SqlMaps
{
    public class MkMaster
    {
        /// <summary>
        /// メーカーコード
        /// </summary>
        public string MkCode { get; set; } = null;
        /// <summary>
        /// メーカー名(カナ)
        /// </summary>
        public string MkName_Kana { get; set; } = null;
        /// <summary>
        /// メーカー名
        /// </summary>
        public string MkName { get; set; } = null;

    }
}
