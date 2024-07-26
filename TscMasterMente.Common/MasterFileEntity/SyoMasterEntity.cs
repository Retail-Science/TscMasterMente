using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TscMasterMente.Common.MasterFileEntity
{
    public class SyoMasterEntity
    {
        /// <summary>
        /// JANコード
        /// </summary>
        [Index(0)]
        [Name("JanCode")]
        public string JanCode { get; set; } = null;

        /// <summary>
        /// メーカーコード
        /// </summary>
        [Index(1)]
        [Name("MkCode")]
        public string MkCode { get; set; } = null;

        /// <summary>
        /// 商品コード
        /// </summary>
        [Index(2)]
        [Name("StoCode")]
        public string StoCode { get; set; } = null;

        /// <summary>
        /// 商品名カナ
        /// </summary>
        [Index(3)]
        [Name("TanName")]
        public string TanName { get; set; } = null;

        /// <summary>
        /// 商品名
        /// </summary>
        [Index(4)]
        [Name("ItemName")]
        public string ItemName { get; set; } = null;

        /// <summary>
        /// 幅
        /// </summary>
        [Index(5)]
        [Name("W")]
        public long? W { get; set; } = null;

        /// <summary>
        /// 高さ
        /// </summary>
        [Index(6)]
        [Name("H")]
        public int? H { get; set; } = null;

        /// <summary>
        /// 奥行
        /// </summary>
        [Index(7)]
        [Name("D")]
        public int? D { get; set; } = null;

        /// <summary>
        /// 入数
        /// </summary>
        [Index(8)]
        [Name("PCase")]
        public int? PCase { get; set; } = null;

        /// <summary>
        /// 細分類コード
        /// </summary>
        [Index(9)]
        [Name("BunCode")]
        public string BunCode { get; set; } = null;

        /// <summary>
        /// 売価
        /// </summary>
        [Index(10)]
        [Name("Price")]
        public double? Price { get; set; } = null;

        /// <summary>
        /// 原価
        /// </summary>
        [Index(11)]
        [Name("Cost")]
        public double? Cost { get; set; } = null;

        /// <summary>
        /// 登録日
        /// </summary>
        [Index(12)]
        [Name("Attr3")]
        public string Attr3 { get; set; } = null;

        /// <summary>
        /// 更新日
        /// </summary>
        [Index(13)]
        [Name("Attr5")]
        public string Attr5 { get; set; } = null;
    }
}
