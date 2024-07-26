using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class GonJyuEntity
    {
        /// <summary>
        /// ゴンドラ什器名
        /// </summary>
        [Index(0)]
        [Name("GonJyuName")]
        public string GonJyuName { get; set; } = null;
    }
}
