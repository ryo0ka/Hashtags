using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Prisms
{
	// Detects whether the user is looking at a canvas or not, every frame.
	public class CanvasFocusListener : IDisposable
	{
		readonly Camera _camera;
		readonly Canvas _canvas;
		readonly CompositeDisposable _life;

		public CanvasFocusListener(Camera camera, Canvas canvas)
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
			RectTransform n = canvas.transform as RectTransform;
			return RectTransformUtility.RectangleContainsScreenPoint(n, center, camera);
		}
	}
}