using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.SqlMaps
{
    /// <summary>
    /// 商品マスタ
    /// </summary>
    public class SyoMaster
    {
        /// <summary>
        /// JANコード
        /// </summary>
        public string JanCode { get; set; } = null;

        /// <summary>
        /// メーカーコード
        /// </summary>
        public string MkCode { get; set; } = null;

        /// <summary>
        /// 商品コード
        /// </summary>
        public string StoCode { get; set; } = null;

        /// <summary>
        /// 商品名カナ
        /// </summary>
        public string TanName { get; set; } = null;

        /// <summary>
        /// 商品名
        /// </summary>
        public string ItemName { get; set; } = null;

        /// <summary>
        /// 幅
        /// </summary>
        public int? W { get; set; } = null;

        /// <summary>
        /// 高さ
        /// </summary>
        public int? H { get; set; } = null;

        /// <summary>
        /// 奥行
        /// </summary>
        public int? D { get; set; } = null;

        /// <summary>
        /// 入数
        /// </summary>
        public int? PCase { get; set; } = null;

        /// <summary>
        /// 細分類コード
        /// </summary>
        public string BunCode { get; set; } = null;

        /// <summary>
        /// 売価
        /// </summary>
        public double? Price { get; set; } = null;

        /// <summary>
        /// 原価
        /// </summary>
        public double? Cost { get; set; } = null;

        /// <summary>
        /// 登録日
        /// </summary>
        public string Attr3 { get; set; } = null;

        /// <summary>
        /// 廃止日
        /// </summary>
        public string Attr4 { get; set; } = null;

        /// <summary>
        /// 更新日
        /// </summary>
        public string Attr5 { get; set; } = null;

        /// <summary>
        /// ???
        /// </summary>
        public string Attr6 { get; set; } = null;

        /// <summary>
        /// ???
        /// </summary>
        public int? Attr7 { get; set; } = null;

        /// <summary>
        /// ???
        /// </summary>
        public string Attr8 { get; set; } = null;
    }

    /// <summary>
    /// Excel登録用の商品マスタ
    /// </summary>
    public class SyoMasterExcel : SyoMaster
    {
        /// <summary>
        /// 分類名
        /// </summary>
        public string BunName { get; set; } = null;

        /// <summary>
        /// メーカー名
        /// </summary>
        public string MkName { get; set; } = null;
    }
}
