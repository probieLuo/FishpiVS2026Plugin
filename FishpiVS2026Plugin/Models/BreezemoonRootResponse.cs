using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace FishpiVS2026Plugin.Models
{
    /// <summary>
    /// 接口返回的根模型
    /// </summary>
    public class BreezemoonRootResponse
    {
        /// <summary>
        /// 状态码：0 表示获取成功
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// 清风明月数据列表
        /// </summary>
        [JsonPropertyName("breezemoons")]
        public List<BreezemoonItem> Breezemoons { get; set; } = new List<BreezemoonItem>();
    }

    /// <summary>
    /// 清风明月单条数据模型
    /// </summary>
    public class BreezemoonItem
    {
        /// <summary>
        /// 发布者用户名
        /// </summary>
        [JsonPropertyName("breezemoonAuthorName")]
        public string BreezemoonAuthorName { get; set; } = string.Empty;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [JsonPropertyName("breezemoonUpdated")]
        public long BreezemoonUpdated { get; set; }

        /// <summary>
        /// 清风明月ID
        /// </summary>
        [JsonPropertyName("oId")]
        public string OId { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间（字段1）
        /// </summary>
        [JsonPropertyName("breezemoonCreated")]
        public long BreezemoonCreated { get; set; }  

        /// <summary>
        /// 发布者头像URL（48尺寸）
        /// </summary>
        [JsonPropertyName("breezemoonAuthorThumbnailURL48")]
        public string BreezemoonAuthorThumbnailURL48 { get; set; } = string.Empty;

        /// <summary>
        /// 发布时间（相对时间，如"10分钟前"）
        /// </summary>
        [JsonPropertyName("timeAgo")]
        public string TimeAgo { get; set; } = string.Empty;

        /// <summary>
        /// 正文内容
        /// </summary>
        [JsonPropertyName("breezemoonContent")]
        public string BreezemoonContent { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间（字段2）
        /// </summary>
        [JsonPropertyName("breezemoonCreateTime")]
        public string BreezemoonCreateTime { get; set; } = string.Empty;

        /// <summary>
        /// 发布城市（可能为空，需做非空判断）
        /// </summary>
        [JsonPropertyName("breezemoonCity")]
        public string BreezemoonCity { get; set; } = "";
    }
}



