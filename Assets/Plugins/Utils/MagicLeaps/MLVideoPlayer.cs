using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace Utils.MagicLeaps
{
	public class MLVideoPlayer : MonoBehaviour
	{
		[SerializeField]
		MLMediaPlayer _mediaPlayer;

		[SerializeField]
		RawImage _image;

		[SerializeField]
		AspectRatioFitter _fitter;

		// Shader property of video texture in MLMediaPlayer
		static readonly int _videoTexId = Shader.PropertyToID("_MainTex");

		Renderer _mediaRenderer;
		bool _loaded;
		bool _shouldPlay;
		float _videoAspectRatio;

		public Texture2D Thumbnail { private get; set; }

		void Reset()
		{
			_mediaPlayer = GetComponentInChildren<MLMediaPlayer>();
			_image = GetComponentInChildren<RawImage>();
			_fitter = _image.GetComponent<AspectRatioFitter>();
		}

		void Awake()
		{
			// MLMediaPlayer renders video in:
			// 1. Creates a new material
			// 2. Assigns it to Renderer as main material
			// 3. Creates a Texture2D that plays video content
			// 4. Assigns it to the material's main texture
			// ... so we can grab the video via the material's main texture.
			_mediaRenderer = _mediaPlayer.GetComponent<Renderer>();
		}

		void Update()
		{
			if (!_loaded) return;

			if (_shouldPlay)
			{
				// Transfer texture ref to RawImage every frame 
				// because I couldn't find any events for that in MLMediaPlayer
				_image.texture = _mediaRenderer.material?.GetTexture(_videoTexId);
				_fitter.aspectRatio = _videoAspectRatio;
			}
			else if (!(Thumbnail is null))
			{
				// Show thumbnail when video texture may be black
				_image.texture = Thumbnail;
				_fitter.aspectRatio = (float) Thumbnail.width / Thumbnail.height;
			}
		}

		public async UniTask LoadVideo(string url, bool loop)
		{
			if (!_mediaPlayer.isActiveAndEnabled)
			{
				Debug.LogWarning("MLMediaPlayer is disabled. Waiting until enabled...");
				await UniTask.WaitUntil(() => _mediaPlayer.isActiveAndEnabled);
			}

			_mediaPlayer.VideoSource = url;

			// Start observing OnMediaError before PrepareVideo because 
			// PrepareVideo itself can trigger MediaError events
			var error = _mediaPlayer.OnMediaErrorAsObservable().ToUniTask(useFirstValue: true);
			var complete = _mediaPlayer.OnVideoPreparedAsObservable().ToUniTask(useFirstValue: true);
			var frameSize = _mediaPlayer.OnFrameSizeSetupAsObservable().ToUniTask(useFirstValue: true);

			_mediaPlayer.PrepareVideo().ThrowIfFail();

			// Throw exception in case an error happens,
			// otherwise wait until preparation is completed
			await UniTask.WhenAny(error, complete);

			_videoAspectRatio = await frameSize;

			_mediaPlayer.IsLooping = loop;
			_loaded = true;

			if (_shouldPlay && !_mediaPlayer.IsPlaying)
			{
				_mediaPlayer.Play();
			}
		}

		public void TrySetVideoPlaying(bool shouldPlay)
		{
			_shouldPlay = shouldPlay;

			if (!_loaded) return;

			if (_shouldPlay && !_mediaPlayer.IsPlaying)
			{
				_mediaPlayer.Play();
			}

			if (!shouldPlay && _mediaPlayer.IsPlaying)
			{
				_mediaPlayer.Pause();
			}
		}

		public void StopVideo()
		{
			if (_loaded)
			{
				_mediaPlayer.Stop();
			}

			_loaded = false;
		}

		public void SetVolume(float volume)
		{
			_mediaPlayer.SetVolume(volume);
		}
	}
}