using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class SuiMasterEntity
    {
        /// <summary>
        /// 細分類コード
        /// </summary>
        [Index(0)]
        [Name("BunCode")]
        public string BunCode { get; set; } = null;
        /// <summary>
        /// 属性コード
        /// </summary>
        [Index(1)]
        [Name("ZkCode")]
        public string ZkCode { get; set; } = null;
        /// <summary>
        /// 水準コード
        /// </summary>
        [Index(2)][Name("SuiCode")] 
        public string SuiCode { get; set; } = null;
        /// <summary>
        /// 水準名
        /// </summary>
        [Index(3)]
        [Name("SuiName")]
        public string SuiName { get; set; } = null;

    }
}
