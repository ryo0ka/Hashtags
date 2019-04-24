using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Hashtags
{
	// Detects whether the user is looking at a canvas or not, every frame.
	public class MLHeadFocusListener : IDisposable
	{
		readonly Camera _camera;
		readonly Canvas _canvas;
		readonly CompositeDisposable _life;

		public MLHeadFocusListener(Camera camera, Canvas canvas)
		{
			_camera = camera;
			_canvas = canvas;
			_life = new CompositeDisposable();

			_canvas.UpdateAsObservable()
			       .Subscribe(_ => Update())
			       .AddTo(_life);
		}

		public bool IsFocused { get; private set; }

		void Update()
		{
			IsFocused = IsCameraLookingAtCanvas(_camera, _canvas);
		}

		public void Dispose()
		{
			_life.Dispose();
		}

		// Code from:
		// https://bitbucket.org/Unity-Technologies/ui/src/4f3cf8d16c1d8c6e681541a292855792e50b392e/UnityEngine.UI/UI/Core/GraphicRaycaster.cs
		static bool IsCameraLookingAtCanvas(Camera camera, Canvas canvas)
		{
			Vector2 center = camera.ViewportToScreenPoint(Vector2.one / 2);
			var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
			for (var i = 0; i < graphics.Count; i++)
			{
				var t = graphics[i].rectTransform;
				//Debug.Log($"name: {t.name}");
				if (RectTransformUtility.RectangleContainsScreenPoint(t, center, camera))
				{
					return true;
				}
			}

			return false;
		}
	}
}