using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class PopMasterEntity
    {
        /// <summary>
        /// POPコード
        /// </summary>
        [Index(0)]
        [Name("PopCode")]
        public string PopCode { get; set; } = null;
        /// <summary>
        /// POP名称
        /// </summary>
        [Index(1)]
        [Name("PopName")]
        public string PopName { get; set; } = null;
        /// <summary>
        /// 幅
        /// </summary>
        [Index(2)]
        [Name("W")]
        public int? W { get; set; } = null;
        /// <summary>
        /// 高さ
        /// </summary>
        [Index(3)]
        [Name("H")]
        public int? H { get; set; } = null;
        /// <summary>
        /// P分類コード
        /// </summary>
        [Index(4)][Name("PBunCode")] 
        public string PBunCode { get; set; } = null;
    }
}
