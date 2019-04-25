using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils;

namespace Prisms
{
	public class PrismBehaviour : MonoBehaviour
	{
		[SerializeField]
		ActionView _actionView;

		[SerializeField]
		Canvas _canvas;

		Camera _camera;
		CanvasFocusListener _focuses;

		protected bool IsFocused => _focuses?.IsFocused ?? false;
		protected bool IsActionActive { get; private set; }

		protected virtual void Awake()
		{
			_camera = Camera.main;
			_focuses = new CanvasFocusListener(Camera.main, _canvas).AddTo(this);
			_actionView.Initialize();

			foreach (Canvas canvas in GetComponentsInChildren<Canvas>())
			{
				canvas.worldCamera = _camera;
			}
		}

		protected virtual void Start()
		{
			DoStart().Forget(Debug.LogException);
		}

		async UniTask DoStart()
		{
			await RelocatePrism();

			OnSpawned();

			while (this != null)
			{
				// Wait for bumper button press
				await MLUtils.OnButtonUpAsObservable(MLInputControllerButton.Bumper)
				             .Where(_ => _focuses.IsFocused)
				             .First();

				IsActionActive = true;

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
						await RelocatePrism();
						break;
					case ActionIntent.Delete:
						DeletePrism();
						return;
					default:
						throw new ArgumentOutOfRangeException();
				}

				IsActionActive = false;
			}
		}

		protected virtual void OnSpawned()
		{
			// Override this method to initiate an app on this prism
			throw new NotImplementedException();
		}

		async UniTask RelocatePrism()
		{
			// Move this prism...
			using (new PrismLocator(transform, _camera.transform))
			{
				// ...unitil trigger is pressed
				await MLUtils.OnTriggerUpAsObservable(KeyCode.Space)
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