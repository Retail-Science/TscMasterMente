using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class ZokMasterEntity
    {
        /// <summary>
        /// 分類コード
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
        /// 属性名
        /// </summary>
        [Index(2)]
        [Name("ZkName")]
        public string ZkName { get; set; } = null;
    }
}
