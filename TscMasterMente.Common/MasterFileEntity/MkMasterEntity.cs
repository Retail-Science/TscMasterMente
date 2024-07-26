using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public  class MkMasterEntity
    {
        /// <summary>
        /// メーカーコード
        /// </summary>
        [Index(0)]
        [Name("MkCode")]
        public string MkCode { get; set; } = null;
        /// <summary>
        /// メーカー名カナ
        /// </summary>
        [Index(1)]
        [Name("MkName_Kana")]
        public string MkName_Kana { get; set; } = null;
        /// <summary>
        /// メーカー名
        /// </summary>
        [Index(2)]
        [Name("MkName")]
        public string MkName { get; set; } = null;
    }
}
