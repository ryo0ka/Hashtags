using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace Utils.Views
{
	public class MLVideoView : IDisposable
	{
		// Transfers MLMediaPlayer's texture to RawImage
		// so that Unity UI can implement media player.
		// Do dispose this instance to detach video from RawImage

		// Shader property of video texture in MLMediaPlayer
		static readonly int _videoTexId = Shader.PropertyToID("_MainTex");

		readonly IDisposable _life;

		public MLVideoView(RawImage image, MLMediaPlayer videoPlayer)
		{
			// MLMediaPlayer renders video in:
			// 1. Creates a new material
			// 2. Assigns it to Renderer as main material
			// 3. Creates a Texture2D that plays video content
			// 4. Assigns it to the material's main texture
			// ... so we can grab the video via the material's main texture.
			var videoRenderer = videoPlayer.GetComponent<Renderer>();

			_life = image.UpdateAsObservable().TakeUntilDestroy(videoRenderer).Subscribe(_ =>
			{
				// Transfer texture ref to RawImage every frame 
				// because I couldn't find an event to do it in MLMediaPlayer
				image.texture = videoRenderer.material?.GetTexture(_videoTexId);
			});
		}

		public void Dispose()
		{
			// Doesn't guarantee that RawImage will not show the video anymore
			_life?.Dispose();
		}
	}
}