using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class TenpoMasterEntity
    {
        /// <summary>
        /// 店舗番号
        /// </summary>
        [Index(0)]
        [Name("TenpoNumber")]
        public string TenpoNumber { get; set; } = null;
        /// <summary>
        /// 店舗コード
        /// </summary>
        [Index(1)]
        [Name("TenpoCode")]
        public string TenpoCode { get; set; } = null;
        /// <summary>
        /// 店舗単コード
        /// </summary>
        [Index(2)]
        [Name("TenpoTanName")]
        public string TenpoTanName { get; set; } = null;
        /// <summary>
        /// 店舗名
        /// </summary>
        [Index(3)]
        [Name("TenpoName")]
        public string TenpoName { get; set; } = null;
        /// <summary>
        /// 空白カラム1
        /// </summary>
        [Index(4)]
        [Name("BlankCol1")]
        public string BlankCol1 { get; set; } = null;
        /// <summary>
        /// 法人コード
        /// </summary>
        [Index(5)]
        [Name("CompCode")]
        public string CompCode { get; set; } = null;
        /// <summary>
        /// 法人名
        /// </summary>
        [Index(6)]
        [Name("CompName")]
        public string CompName { get; set; } = null;
        /// <summary>
        /// エリアコード
        /// </summary>
        [Index(7)]
        [Name("AreaCode")]
        public string AreaCode { get; set; } = null;
        /// <summary>
        /// エリア名
        /// </summary>
        [Index(8)]
        [Name("AreaName")]
        public string AreaName { get; set; } = null;
    }

}
