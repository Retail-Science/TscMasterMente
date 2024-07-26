using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class SyoZokSeiEntity
    {
        /// <summary>
        /// JANコード
        /// </summary>
        [Index(0)]
        [Name("JanCode")]
        public string JanCode { get; set; } = null;
        /// <summary>
        /// 属性コード
        /// </summary>
        [Index(1)]
        [Name("ZkCode")]
        public string ZkCode { get; set; } = null;
        /// <summary>
        /// 水準コード
        /// </summary>
        [Index(2)]
        [Name("SuiCode")]
        public string SuiCode { get; set; } = null;
    }
}
