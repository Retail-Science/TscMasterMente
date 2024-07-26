using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.SqlMaps
{
    public class SyoZokSei
    {
        /// <summary>
        /// JANコード
        /// </summary>
        public string JanCode { get; set; } = null;
        /// <summary>
        /// 属性コード
        /// </summary>
        public string ZkCode { get; set; } = null;
        /// <summary>
        /// 水準コード
        /// </summary>
        public string SuiCode { get; set; } = null;
    }
}
