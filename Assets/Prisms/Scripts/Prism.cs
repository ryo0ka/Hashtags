using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils;

namespace Prisms
{
	public class Prism : MonoBehaviour
	{
		[SerializeField]
		ActionView _actionView;

		[SerializeField]
		Canvas _canvas;

		Camera _camera;
		CanvasFocusListener _focuses;

		public bool IsFocused => _focuses?.IsFocused ?? false;

		protected virtual void Awake()
		{
			_camera = Camera.main;
			_focuses = new CanvasFocusListener(Camera.main, _canvas).AddTo(this);
			_canvas.worldCamera = _camera;
			_actionView.Initialize();
		}

		protected virtual void Start()
		{
			RunActionView().Forget(Debug.LogException);
		}

		async UniTask RunActionView()
		{
			while (this != null)
			{
				// Wait for bumper button press
				await MLUtils.OnButtonUpAsObservable(MLInputControllerButton.Bumper)
				             .Where(_ => _focuses.IsFocused)
				             .First();

				// Show action view
				_actionView.SetActive(true).Forget(Debug.LogException);

				// Pressing bumper again == cancelling action
				var cancels = MLUtils.OnButtonUpAsObservable(MLInputControllerButton.Bumper)
				                     .Select(_ => ActionIntent.Cancel);

				// Wait for an action or cancelling
				var intent = await _actionView.OnIntent.Merge(cancels).First();

				// Hide action view
				_actionView.SetActive(false).Forget(Debug.LogException);

				switch (intent)
				{
					case ActionIntent.Cancel:
						Debug.Log("Cancelled");
						break;
					case ActionIntent.Relocate:
						await ReplacePrism();
						break;
					case ActionIntent.Delete:
						DeletePrism();
						return;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		async UniTask ReplacePrism()
		{
			// Move this prism...
			using (new PrismLocator(transform, _camera.transform))
			{
				// ...unitil trigger is pressed
				await MLUtils.OnTriggerUpAsObservable()
				             .TakeUntilDestroy(this)
				             .FirstOrDefault();
			}
		}

		void DeletePrism()
		{
			Destroy(gameObject);
		}
	}
}