using Utils;

namespace MLTwitter
{
	public static class TWUtils
	{
		public static bool TryFindMediaEntity(this TWStatus status, out TWMediaObject mediaFound)
		{
			if (status.ExtendedEntities?.Media is TWMediaObject[] mediaObjs &&
			    mediaObjs.TryGetFirstValue(out var media, m => m.Type == "photo" || m.Type == "video"))
			{
				mediaFound = media;
				return true;
			}

			mediaFound = null;
			return false;
		}

		public static bool TryFindVideoUrl(this TWMediaObject media, out string videoUrl)
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
	}
}