using System;
using System.Web;
using Newtonsoft.Json;

namespace MLTwitter
{
	public class TWStatusTextConverter : JsonConverter
	{
		public override bool CanRead => true;
		public override bool CanWrite => false;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string raw = serializer.Deserialize<string>(reader);
			return HttpUtility.HtmlDecode(raw);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}