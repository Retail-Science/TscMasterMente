using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.SqlMaps
{
    public class ZokMaster
    {
        /// <summary>
        /// 属性コード
        /// </summary>
        public string ZkCode { get; set; } = null;
        /// <summary>
        /// 属性名
        /// </summary>
        public string ZkName { get; set; } = null;
        /// <summary>
        /// 分類コード
        /// </summary>
        public string BunCode { get; set; } = null;
    }
}
