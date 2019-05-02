using System;
using MLTwitter;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.MagicLeaps;
using Utils.Views;

namespace Hashtags
{
	public class StatusView : MonoBehaviour
	{
		[SerializeField]
		RawImage _profileImage;

		[SerializeField]
		Text _nameText;

		[SerializeField]
		Text _screenNameText;

		[SerializeField]
		Text _dateTimeText;

		[SerializeField]
		Text _statusText;

		[SerializeField]
		MLVideoPlayer _videoPlayer;

		[SerializeField]
		RawImage _mediaImage;

		[SerializeField]
		Text _retweetCountText;

		[SerializeField]
		Text _favoriteCountText;

		[SerializeField]
		bool _suppressNewline;

		DateTime _statusUploadTime;

		void Awake()
		{
			// Remove debug image
			_mediaImage.texture = Texture2D.blackTexture;
		}

		void Start()
		{
			// Refresh upload time every minute
			this.UpdateAsObservable().ThrottleFirst(1f.Minutes()).Subscribe(_ =>
			{
				_dateTimeText?.SetText(TWUtils.ToString(_statusUploadTime));
			});
		}

		void OnDestroy()
		{
			_videoPlayer.StopVideo();
		}

		public async UniTask SetStatus(TWStatus status)
		{
			// Set time
			_statusUploadTime = status.DateTime;
			_dateTimeText?.SetText(TWUtils.ToString(_statusUploadTime));

			string txt = status.Text;

			if (_suppressNewline)
			{
				txt = txt.Replace('\n', ' ');
			}

			_statusText.text = txt;
			_favoriteCountText.text = TWUtils.ToString(status.FavoriteCount);
			_retweetCountText.text = TWUtils.ToString(status.RetweetCount);

			await SetUser(status.User);

			if (status.TryFindMediaEntity(out TWMedia media))
			{
				// Load & show thumbnail
				var thumbnail = await TextureFactory.Download(media.MediaUrl);
				_mediaImage.SetTexture(thumbnail);

				if (status.TryFindVideoEntity(out TWMedia video) &&
				    video.TryFindVideoUrl(out string videoUrl))
				{
					// Set video player's thumbnail
					_videoPlayer.Thumbnail = thumbnail;

					// Load & play video if included
					_videoPlayer.LoadVideo(videoUrl, loop: true).Forget(Debug.LogException);
				}
			}
		}

		async UniTask SetUser(TWUser user)
		{
			_nameText?.SetText(user.Name);
			_screenNameText?.SetText(user.ScreenName);

			TextureFactory.Unuse(_profileImage?.texture);
			var profile = await TextureFactory.Download(user.ProfileImageUrl);
			_profileImage?.SetTexture(profile);
		}
	}
}