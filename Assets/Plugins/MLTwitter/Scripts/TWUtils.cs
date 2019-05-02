using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Utils;

namespace MLTwitter
{
	public static class TWUtils
	{
		public static bool TryFindMediaEntity(this TWStatus status, out TWMedia mediaFound)
		{
			if (status.Entities?.Media is TWMedia[] mediaObjs &&
			    mediaObjs.FirstOrDefault(m => m.Type == "photo") is TWMedia media)
			{
				mediaFound = media;
				return true;
			}

			if (TryFindVideoEntity(status, out mediaFound))
			{
				return true;
			}

			mediaFound = null;
			return false;
		}

		public static bool TryFindVideoEntity(this TWStatus status, out TWMedia mediaFound)
		{
			if (status.ExtendedEntities?.Media is TWMedia[] mediaObjs &&
			    mediaObjs.TryGetFirstValue(out var media, m => m.Type == "video"))
			{
				mediaFound = media;
				return true;
			}

			mediaFound = null;
			return false;
		}

		public static bool TryFindVideoUrl(this TWMedia media, out string videoUrl)
		{
			if (media.Type != "video")
			{
				videoUrl = null;
				return false;
			}

			var variants = media.VideoInfo.Variants;
			if (variants.TryGetFirstValue(out var variant, v => v.Url.Contains(".mp4")))
			{
				videoUrl = variant.Url;
				return true;
			}

			videoUrl = null;
			return false;
		}

		public static bool TryFindRetweetedStatus(this TWStatus status, out TWStatus rt)
		{
			if (status.RetweetedStatus is TWStatus rtStatus)
			{
				rt = rtStatus;
				return true;
			}

			rt = null;
			return false;
		}

		public static string MakeHashtag(string input)
		{
			return "#" + Regex.Replace(input, @"[\n\s\t #]", "");
		}

		// Pritty-print uploaded time based on official app
		public static string ToString(DateTime then)
		{
			DateTime now = DateTime.UtcNow;

			//Debug.Log($"{then:yyyy MMMM dd HH:mm:ss}, {now:yyyy MMMM dd HH:mm:ss}");

			// Show "now" until a minute
			double seconds = (now - then).TotalSeconds;
			if (seconds <= 60f)
			{
				return "now";
			}

			// Count by minutes until an hour
			double minutes = (now - then).TotalMinutes;
			if (minutes <= 60f)
			{
				int m = (int) Mathf.Ceil((float) minutes);
				return $"{m}m";
			}

			// Count by hours until a day
			double hours = (now - then).TotalHours;
			if (hours <= 24f)
			{
				int h = (int) Mathf.Floor((float) hours);
				return $"{h}h";
			}

			// Show year if necessary
			if (then.Year != now.Year)
			{
				return $"{then:MM dd YYYY}";
			}

			// Show date
			return $"{then:MMM dd}";
		}

		public static string ToString(int count)
		{
			if (count > 1000000)
			{
				var n = (float) count / 1000000;
				return $"{n:0.0}M";
			}

			if (count > 1000)
			{
				var n = (float) count / 1000;
				return $"{n:0.0}K";
			}

			return $"{count}";
		}
	}
}