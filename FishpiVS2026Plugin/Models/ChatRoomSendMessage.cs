using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FishpiVS2026Plugin.Models
{
	public class ChatRoomSendMessage
	{
		[JsonPropertyName("apiKey")]
		public string ApiKey { get; set; }

		[JsonPropertyName("client")]
		public string Client { get; set; }

		[JsonPropertyName("content")]
		public string Content { get; set; }
	}
}
