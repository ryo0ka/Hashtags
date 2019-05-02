using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using Utils.MagicLeaps;
using Utils.Views;

namespace Prisms
{
	public class PrismBehaviour : MonoBehaviour
	{
		[SerializeField]
		PrismActionView _actionView;

		[SerializeField]
		RectTransform _canvas;

		[SerializeField]
		Visible _visible;

		[SerializeField]
		Selectable _focusIndicator;

		[SerializeField]
		PrismTransformer _transformer;

		Camera _camera;
		SpatialMeshController _spatialMesh;

		ReactiveProperty<bool> _actionActive;
		Subject<Unit> _onPrismInitialized;

		public bool IsFocused { get; private set; }
		public bool IsActionActive => _actionActive.Value;
		public IObservable<Unit> PrismInitialized => _onPrismInitialized;

		protected virtual void Awake()
		{
			_camera = Camera.main;
			_spatialMesh = FindObjectOfType<SpatialMeshController>();

			_actionActive = new ReactiveProperty<bool>().AddTo(this);
			_onPrismInitialized = new Subject<Unit>().AddTo(this);

			_actionView.Initialize();
			_transformer.enabled = false;

			// Set up canvases
			foreach (Canvas canvas in GetComponentsInChildren<Canvas>())
			{
				canvas.worldCamera = _camera;
			}

			// Make the view look unfocused during action 
			_actionActive.Subscribe(actionActive =>
			{
				_visible.SetFocus(!actionActive).Forget(Debug.LogException);
			});
		}

		void Start()
		{
			DoStart().Forget(Debug.LogException);
		}

		void Update()
		{
			IsFocused = _camera.IsLookingAt(_canvas);

			// Quick highlight animation
			_focusIndicator.interactable = IsFocused;
		}

		async UniTask DoStart()
		{
			// Wait until this prism is placed at initial poisition
			await TransformPrism();

			// Notify app that it's ready to start
			StartPrismApp();
			_onPrismInitialized.OnNext(Unit.Default);

			while (this != null)
			{
				// Wait for bumper button press
				await MLUtils.OnButtonUpAsObservable(MLInputControllerButton.Bumper)
				             .Where(_ => IsFocused)
				             .First();

				_actionActive.Value = true;

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
					case ActionIntent.Transform:
						await TransformPrism();
						break;
					case ActionIntent.Delete:
						await DeletePrism();
						return;
					default:
						throw new ArgumentOutOfRangeException();
				}

				_actionActive.Value = false;
			}
		}

		protected virtual void StartPrismApp()
		{
			Debug.LogWarning("You should be overriding this method");
		}

		async UniTask TransformPrism()
		{
			_spatialMesh.SetTrackingTarget(transform);

			_transformer.SetActive(true);
			_spatialMesh.SetActive(true).Forget(Debug.LogWarning);

			// Transform this prism unitil trigger is pressed
			await MLUtils.OnTriggerUpAsObservable(KeyCode.Space)
			             .TakeUntilDestroy(this)
			             .First();

			_transformer.SetActive(false);
			_spatialMesh.SetActive(false).Forget(Debug.LogException);
		}

		async UniTask DeletePrism()
		{
			// Do the "I'm dying" animation
			await _visible.SetVisible(false);

			// Destroy this prism
			Destroy(gameObject);
		}
	}
}