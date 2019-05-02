using System;
using Newtonsoft.Json;

/* Data structures are defined in this file
 */

namespace MLTwitter
{
	public enum TWSearchResultType
	{
		Mixed,
		Recent,
		Popular,
	}

	public struct TWSearchParameter
	{
		public string Query { get; set; }
		public TWSearchResultType ResultType { get; set; }
		public int Count { get; set; }
		public long SinceId { get; set; }
	}

	[Serializable]
	internal class TWBearerToken
	{
		[JsonProperty("token_type")]
		public string TokenType { get; private set; }

		[JsonProperty("access_token")]
		public string AccessToken { get; private set; }
	}

	[Serializable]
	public class TWMediaUploadInitResponse
	{
		[JsonProperty("media_id_string")]
		public string MediaId { get; private set; }

		[JsonProperty("expires_after_secs")]
		public int ExpirationSeconds { get; private set; }
	}

	[Serializable]
	public class TWMediaUploadFinalizeResponse
	{
		[JsonProperty("media_id_string")]
		public string MediaId { get; private set; }

		[JsonProperty("expires_after_secs")]
		public int ExpirationSeconds { get; private set; }

		[JsonProperty("processing_info")]
		public TWMediaUploadProcessingInfo ProcessingInfo { get; private set; }

		[JsonProperty("image")]
		public TWMediaUploadImage Image { get; private set; }
	}

	[Serializable]
	public class TWMediaUploadStatusResponse
	{
		[JsonProperty("media_id_string")]
		public string MediaId { get; private set; }

		[JsonProperty("expires_after_secs")]
		public int ExpirationSeconds { get; private set; }

		[JsonProperty("processing_info")]
		public TWMediaUploadProcessingInfo ProcessingInfo { get; private set; }
	}

	[Serializable]
	public class TWMediaUploadImage
	{
		[JsonProperty("image_type")]
		public string ImageType { get; private set; }

		[JsonProperty("w")]
		public int Width { get; private set; }

		[JsonProperty("h")]
		public int Height { get; private set; }
	}

	[Serializable]
	public class TWMediaUploadProcessingInfo
	{
		[JsonProperty("state")]
		public string State { get; private set; }

		[JsonProperty("check_after_secs")]
		public int CheckSeconds { get; private set; }

		[JsonProperty("progress_percent")]
		public int Percentage { get; private set; }

		[JsonProperty("error")]
		public TWMediaUploadError Error { get; private set; }
	}

	[Serializable]
	public class TWMediaUploadError
	{
		[JsonProperty("code")]
		public int Code { get; private set; }

		[JsonProperty("name")]
		public string Name { get; private set; }

		[JsonProperty("message")]
		public string Message { get; private set; }
	}

	[Serializable]
	public class TWUser
	{
		[JsonProperty("name")]
		public string Name { get; private set; }

		[JsonProperty("screen_name")]
		public string ScreenName { get; private set; }

		[JsonProperty("profile_image_url_https")]
		public string ProfileImageUrl { get; private set; }

		[JsonProperty("status")]
		public TWStatus Status { get; private set; }
	}

	[Serializable]
	public class TWStatuses
	{
		[JsonProperty("statuses")]
		public TWStatus[] Statuses { get; private set; }
	}

	[Serializable]
	public class TWStatus
	{
		[JsonProperty("id")]
		public long Id { get; private set; }

		[JsonProperty("user")]
		public TWUser User { get; private set; }

		[JsonProperty("full_text"), JsonConverter(typeof(TWStatusTextConverter))]
		public string Text { get; private set; }

		[JsonProperty("created_at"), JsonConverter(typeof(TWDateTimeConvereter))]
		public DateTime DateTime { get; private set; }

		[JsonProperty("entities")]
		public TWEntities Entities { get; private set; }

		[JsonProperty("extended_entities")]
		public TWEntities ExtendedEntities { get; private set; }

		[JsonProperty("retweeted_status")]
		public TWStatus RetweetedStatus { get; private set; }

		[JsonProperty("retweet_count")]
		public int RetweetCount { get; private set; }

		[JsonProperty("favorite_count")]
		public int FavoriteCount { get; private set; }
	}

	[Serializable]
	public class TWEntities
	{
		[JsonProperty("media")]
		public TWMedia[] Media { get; private set; }

		[JsonProperty("urls")]
		public TWUrl[] Urls { get; private set; }
	}

	[Serializable]
	public class TWUrl
	{
		[JsonProperty("url")]
		public string Url { get; private set; }
	}

	[Serializable]
	public class TWMedia
	{
		[JsonProperty("media_url_https")]
		public string MediaUrl { get; private set; }

		[JsonProperty("type")]
		public string Type { get; private set; }

		[JsonProperty("video_info")]
		public TWVideoInfo VideoInfo { get; private set; }
	}

	[Serializable]
	public class TWVideoInfo
	{
		[JsonProperty("variants")]
		public TWVideoVariant[] Variants { get; private set; }
	}

	[Serializable]
	public class TWVideoVariant
	{
		[JsonProperty("content_type")]
		public string ContentType { get; private set; }

		[JsonProperty("url")]
		public string Url { get; private set; }
	}
}