using System.Text.Json.Serialization;

namespace FishpiVS2026Plugin.Models
{
    public class ChatRoomMessage
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        [JsonPropertyName("userOId")]
        public long UserOId { get; set; }

        /// <summary>
        /// 用户头像URL
        /// </summary>
        [JsonPropertyName("userAvatarURL")]
        public string UserAvatarURL { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        [JsonPropertyName("userNickname")]
        public string UserNickname { get; set; }

        /// <summary>
        /// 消息唯一标识
        /// </summary>
        [JsonPropertyName("oId")]
        public string OId { get; set; }

        /// <summary>
        /// 用户名/账号
        /// </summary>
        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// 消息类型（如msg）
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// 消息内容（HTML格式）
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// 消息内容（Markdown格式）
        /// </summary>
        [JsonPropertyName("md")]
        public string Md { get; set; }

        /// <summary>
        /// 用户头像URL（20x20）
        /// </summary>
        [JsonPropertyName("userAvatarURL20")]
        public string UserAvatarURL20 { get; set; }

        /// <summary>
        /// 系统勋章（嵌套JSON字符串）
        /// </summary>
        //[JsonPropertyName("sysMetal")]
        //public string SysMetal { get; set; }

        /// <summary>
        /// 客户端标识
        /// </summary>
        [JsonPropertyName("client")]
        public string Client { get; set; }

        /// <summary>
        /// 消息发送时间
        /// </summary>
        [JsonPropertyName("time")]
        public string Time { get; set; }

        /// <summary>
        /// 用户头像URL（210x210）
        /// </summary>
        [JsonPropertyName("userAvatarURL210")]
        public string UserAvatarURL210 { get; set; }

        /// <summary>
        /// 用户头像URL（48x48）
        /// </summary>
        [JsonPropertyName("userAvatarURL48")]
        public string UserAvatarURL48 { get; set; }

        [JsonIgnore]
        public bool IsSelf { get; set; } = false;

	}
}
