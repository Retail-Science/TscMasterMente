using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class SlfJyuEntity
    {
        /// <summary>
        /// 棚段什器名
        /// </summary>
        [Index(0)]
        [Name("SlfJyuName")]
        public string SlfJyuName { get; set; } = null;
    }
}
