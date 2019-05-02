using MLTwitter;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using Utils.Views;

namespace Hashtags
{
	public class TimelineEntryView : MonoBehaviour
	{
		[SerializeField]
		StatusView statusView;

		[SerializeField]
		Visible _rootVisible;

		bool _viewReady;
		bool _viewContained;

		// True when user is looking at view
		ReactiveProperty<bool> _focused;

		// True when view (should be) presented to user
		// (i.e. view is located inside the edge of container)
		ReactiveProperty<bool> _visible;

		PrismApp _app;
		Camera _camera;
		RectTransform _appCanvas;
		RectTransform _transform;

		// for visibility check
		readonly Vector3[] _corners = new Vector3[4];

		void Awake()
		{
			_app = GetComponentInParent<PrismApp>();
			_camera = Camera.main;
			_appCanvas = GetComponentInParent<CanvasScaler>().transform as RectTransform;
			_transform = transform as RectTransform;

			_focused = new ReactiveProperty<bool>().AddTo(this);
			_visible = new ReactiveProperty<bool>().AddTo(this);
		}

		void Start()
		{
			// Render view iff view is inside prism
			_visible.Subscribe(canBeVisible =>
			{
				//Debug.Log($"{name} {canBeVisible}");
				_rootVisible.SetVisible(canBeVisible).Forget(Debug.LogException);
			});

			// Indicate whether view is looked at by user or not
			// (so that user would know which tweet is selected now)
			_focused.Subscribe(isFocused =>
			{
				_rootVisible.SetFocus(isFocused).Forget(Debug.LogException);
			});
		}

		void Update()
		{
			_transform.GetWorldCorners(_corners);
			float contentTopPos = (_corners[1].y + _corners[2].y) / 2f;
			float contentBtmPos = (_corners[0].y + _corners[3].y) / 2f;

			_appCanvas.GetWorldCorners(_corners);
			float canvasTopPos = (_corners[1].y + _corners[2].y) / 2f;
			float canvasBtmPos = (_corners[0].y + _corners[3].y) / 2f;

			_viewContained = contentTopPos >= canvasBtmPos &&
			                 contentBtmPos <= canvasTopPos;

			// View is totally or partially inside the canvas
			_visible.Value = _viewContained && _viewReady;

			// Test if user is looking at view every frame
			_focused.Value = _camera.IsLookingAt(_transform) && !_app.IsActionActive;
		}

		public async UniTask SetStatus(TWStatus status)
		{
			// Hide view until ready
			_rootVisible.HideUntilStart();

			// Set user/text
			await statusView.SetStatus(status);

			_viewReady = true;

			if (_viewContained)
			{
				await _rootVisible.ShowForStart();
			}
		}

		public async UniTask HideForDestroy()
		{
			await _rootVisible.HideForDestroy();
		}
	}
}