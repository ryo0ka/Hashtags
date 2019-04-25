using System;
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

		[SerializeField, Range(0, 1)]
		float _baseAudioVolume;

		[SerializeField]
		float _duration;

		[SerializeField]
		AnimationCurve _curve;

		[SerializeField]
		CanvasGroup _group;

		DateTime _statusUploadTime;

		Camera _camera;
		RectTransform _canvas;
		RectTransform _transform;
		LayoutElement _layoutElement;
		MLVideoView _videoView;

		// We'll render this view iff this view is inside the canvas
		bool CanBeVisible => ViewUtils.IsRectContained(_camera, _canvas, _transform);

		void Start()
		{
			_camera = Camera.main;
			_canvas = GetComponentInParent<Canvas>().transform as RectTransform;
			_transform = transform as RectTransform;
			_layoutElement = GetComponent<LayoutElement>();

			SetVisible(true).Forget(Debug.LogException);

			// Refresh upload time every minute
			this.UpdateAsObservable()
			    .ThrottleFirst(1f.Minutes())
			    .Subscribe(_ =>
			    {
				    _dateTimeText.text = DateTimeToString(_statusUploadTime);
			    });

			// Render this view iff it can be visible
			this.UpdateAsObservable()
			    .Select(_ => CanBeVisible)
			    .DistinctUntilChanged()
			    .Subscribe(canBeVisible =>
			    {
				    SetVisible(canBeVisible, true).Forget(Debug.LogException);
			    });
		}

		public async UniTask SetStatus(TWStatus status)
		{
			// Hide video
			_mediaRoot.SetActive(false);

			_statusUploadTime = status.DateTime;

			_dateTimeText.text = DateTimeToString(_statusUploadTime);
			_statusText.text = status.Text;

			var user = status.User;
			_nameText.text = user.Name;
			_screenNameText.text = user.ScreenName;
			_profileImage.texture = await DownloadImage(user.ProfileImageUrl);

			if (status.TryFindMediaEntity(out var media))
			{
				// Show media view
				_mediaRoot.SetActive(true);

				_videoView?.Dispose();
				var thumbnail = await DownloadImage(media.MediaUrl);
				_mediaImage.SetTexture(thumbnail, _mediaFitter);

				// Show & play video if included in the status
				if (media.TryFindVideoUrl(out var videoUrl))
				{
					PlayVideo(videoUrl).Forget(Debug.LogException);
				}
			}
		}

		async UniTask SetVisible(bool visible, bool preserveLayout = false)
		{
			if (visible)
			{
				// Replay video (if exists) when a view appears
				PlayVideoIfExists().Forget(Debug.LogException);
			}

			await UnityUtils.Animate(this, _duration, _curve, t =>
			{
				// Gradually appear/disappear
				_group.alpha = visible ? t : 1f - t;

				if (visible & !preserveLayout)
				{
					// Gradually expand from zero to preferred size
					_layoutElement.SetPreferredHeightByLerp(t);
				}
			});

			// Disable preferred height constraint 
			// because media can appear after this animation
			_layoutElement.preferredHeight = -1;

			if (!visible)
			{
				// Stop playing video (if exists) when a view disappears
				_mediaPlayer.Stop();
			}
		}

		async UniTask PlayVideoIfExists()
		{
			if (_mediaPlayer.VideoSource != null)
			{
				await PlayVideo(_mediaPlayer.VideoSource);
			}
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
	}
}