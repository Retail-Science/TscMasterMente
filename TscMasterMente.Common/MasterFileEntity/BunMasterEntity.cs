using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class BunMasterEntity
    {
        /// <summary>
        /// 細分類コード
        /// </summary>
        [Index(0)]
        [Name("BunCode")]
        public string BunCode { get; set; } = null;
        /// <summary>
        /// 細分類名
        /// </summary>
        [Index(1)]
        [Name("BunName")]
        public string BunName { get; set; } = null;
    }
}
