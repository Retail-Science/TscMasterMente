using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class PBunMasterEntity
    {
        /// <summary>
        /// P分類コード
        /// </summary>
        [Index(0)]
        [Name("PBunCode")]
        public string PBunCode { get; set; } = null;
        /// <summary>
        /// P分類名
        /// </summary>
        [Index(1)]
        [Name("PBunName")]
        public string PBunName { get; set; } = null;
    }
}
