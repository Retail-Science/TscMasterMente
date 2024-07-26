using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.SqlMaps
{
    public class TenpoMaster
    {
        /// <summary>
        /// 店舗Id
        /// </summary>
        public int TenpoId { get; set; } = -1;
        /// <summary>
        /// 店舗番号
        /// </summary>
        public string TenpoNumber { get; set; } = null;
        /// <summary>
        /// 店舗コード
        /// </summary>
        public string TenpoCode { get; set; } = null;
        /// <summary>
        /// 店舗名カナ
        /// </summary>
        public string TenpoTanName { get; set; } = null;
        /// <summary>
        /// 店舗名
        /// </summary>
        public string TenpoName { get; set; } = null;
        /// <summary>
        /// 法人コード
        /// </summary>
        public string CompCode { get; set; } = null;
        /// <summary>
        /// 法人名
        /// </summary>
        public string CompName { get; set; } = null;
        /// <summary>
        /// エリアコード
        /// </summary>
        public string AreaCode { get; set; } = null;
        /// <summary>
        /// エリア名
        /// </summary>
        public string AreaName { get; set; } = null;
    }
}
