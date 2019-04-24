using System;
using System.Linq;
using MLTwitter;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using Utils;
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
		GameObject _mediaRoot;

		[SerializeField]
		MLMediaPlayer _mediaPlayer;

		[SerializeField]
		RawImage _mediaImage;

		[SerializeField]
		AspectRatioFitter _mediaFitter;

		[SerializeField]
		CanvasGroup _group;

		[SerializeField, Range(0, 1)]
		float _baseAudioVolume;

		MLVideoView _videoView;
		DateTime _statusUploadTime;

		void Awake()
		{
			this.UpdateAsObservable().ThrottleFirst(1f.Minutes()).Subscribe(_ =>
			{
				// Refresh upload time every minute
				_dateTimeText.text = DateTimeToString(_statusUploadTime);
			});
		}

		public async UniTask SetStatus(TWStatus status, bool setVisible)
		{
			// Hide video
			_mediaRoot.SetActive(false);

			// Show the entire view
			Fade(true).Forget(Debug.LogException);

			_statusUploadTime = status.DateTime;

			_dateTimeText.text = DateTimeToString(_statusUploadTime);
			_statusText.text = status.Text;

			var user = status.User;
			_nameText.text = user.Name;
			_screenNameText.text = user.ScreenName;
			_profileImage.texture = await DownloadImage(user.ProfileImageUrl);

			if (TryFindMediaEntity(status, out var media))
			{
				// Show media view
				_mediaRoot.SetActive(true);

				_videoView?.Dispose();
				_mediaImage.texture = await DownloadImage(media.MediaUrl);
				_mediaFitter.aspectRatio = _mediaImage.GetAspectRatio();

				// Show & play video if included in the status
				if (TryFindVideoUrl(media, out var videoUrl))
				{
					PlayVideo(videoUrl).Forget(Debug.LogException);
				}
			}

			if (setVisible)
			{
				await SetVisible(true);
			}
		}

		bool TryFindMediaEntity(TWStatus status, out TWMediaObject mediaFound)
		{
			if (status.ExtendedEntities?.Media is TWMediaObject[] mediaObjs &&
			    mediaObjs.FirstOrDefault(m => m.Type == "photo" || m.Type == "video") is TWMediaObject media)
			{
				mediaFound = media;
				return true;
			}

			mediaFound = null;
			return false;
		}

		bool TryFindVideoUrl(TWMediaObject media, out string videoUrl)
		{
			if (media.Type != "video")
			{
				videoUrl = null;
				return false;
			}

			var variants = media.VideoInfo.Variants;
			if (variants.Where(v => v.Url.Contains(".mp4")).TryGetFirstValue(out var variant))
			{
				videoUrl = variant.Url;
				return true;
			}

			videoUrl = null;
			return false;
		}

		async UniTask PlayVideo(string url)
		{
			_videoView?.Dispose();
			_videoView = new MLVideoView(_mediaImage, _mediaPlayer);

			_mediaPlayer.VideoSource = url;

			// Start observing OnMediaError before PrepareVideo because 
			// PrepareVideo itself can trigger MediaError events
			var error = _mediaPlayer.OnMediaErrorAsObservable().ToUniTask(useFirstValue: true);
			var complete = _mediaPlayer.OnVideoPreparedAsObservable().ToUniTask(useFirstValue: true);
			var aspectRatio = _mediaPlayer.OnFrameSizeSetupAsObservable().ToUniTask(useFirstValue: true);

			_mediaPlayer.PrepareVideo().ThrowIfFail();

			// Throw exception in case an error happens,
			// otherwise wait until preparation is completed
			await UniTask.WhenAny(error, complete);

			_mediaFitter.aspectRatio = await aspectRatio;

			// Play video
			_mediaPlayer.IsLooping = true;
			_mediaPlayer.Play().ThrowIfFail();

			_mediaPlayer.SetVolume(0);
			_mediaPlayer.GetComponent<AudioSource>().volume = 0;
		}

		public async UniTask SetVisible(bool visible, bool immediately = false, bool preserveLayout = false)
		{
			if (immediately && !preserveLayout)
			{
				gameObject.SetActive(visible);
				return;
			}

			await Fade(visible);
		}

		async UniTask Fade(bool visible, bool preserveLayout = false)
		{
			if (!preserveLayout && visible)
			{
				gameObject.SetActive(true);
			}

			await UnityUtils.Animate(this, 0.5f, AnimationCurve.EaseInOut(0, 0, 1, 1), t =>
			{
				_group.alpha = visible ? t : 1 - t;
			});

			if (!visible)
			{
				_mediaPlayer.Stop();
			}

			if (!preserveLayout && !visible)
			{
				gameObject.SetActive(false);
			}
		}

		static async UniTask<Texture> DownloadImage(string url)
		{
			using (var req = UnityWebRequestTexture.GetTexture(url))
			{
				await req.SendWebRequest();
				return ((DownloadHandlerTexture) req.downloadHandler).texture;
			}
		}

		// Pritty-print uploaded time based on official app
		static string DateTimeToString(DateTime then)
		{
			DateTime now = DateTime.Now;

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
	}
}