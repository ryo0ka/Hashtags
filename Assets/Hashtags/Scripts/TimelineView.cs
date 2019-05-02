using System;
using System.Collections.Generic;
using MLTwitter;
using Prisms;
using UnityEngine;
using UnityEngine.UI;
using Utils.MagicLeaps;
using UniRx;
using UniRx.Async;

namespace Hashtags
{
	public class TimelineView : MonoBehaviour
	{
		[SerializeField]
		ScrollRect _scrollRect;

		[SerializeField]
		TimelineEntryView timelineEntryViewTemplate;

		[SerializeField]
		Transform _statusViewRoot;

		[SerializeField]
		float _scrollMagnitude;

		[SerializeField]
		TouchpadScrollHandler _touchpadScrollHandler;

		Queue<TimelineEntryView> _views;
		PrismBehaviour _prism;
		MLTouchpadListener _touchpad;

		void Awake()
		{
			_views = new Queue<TimelineEntryView>();
			_prism = GetComponent<PrismBehaviour>();
		}

		void Start()
		{
			MLUtils.LatestTouchpadListenerAsObservable()
			       .Subscribe(l => _touchpad = l)
			       .AddTo(this);

			// Clear debug objects
			foreach (Transform o in _statusViewRoot)
			{
				Destroy(o.gameObject);
			}
		}

		void Update()
		{
			var delta = _touchpadScrollHandler.Update(_touchpad?.Update());

			if (_prism.IsFocused && !_prism.IsActionActive)
			{
				float scrollDelta = delta?.y * _scrollMagnitude ?? 0f;
				_scrollRect.content.anchoredPosition += Vector2.up * scrollDelta;
			}
		}

		public async UniTask Append(TWStatus status)
		{
			if (_statusViewRoot == null)
			{
				throw new Exception("no root specified");
			}
			
			// Instantiate and track new view
			var view = Instantiate(timelineEntryViewTemplate, _statusViewRoot);
			view.transform.SetSiblingIndex(0);
			view.name = $"{status.Id}";
			_views.Enqueue(view);

			// Set data and present on timeline
			await view.SetStatus(status);

			// Remove oldest (earliest added) tweet from timeline
			await RemoveOverflowEntries();

			Debug.Log(_views.Count);
		}

		async UniTask RemoveOverflowEntries()
		{
			if (_views.Count > 30)
			{
				var oldest = _views.Dequeue();

				await oldest.HideForDestroy();
				Destroy(oldest.gameObject);
			}
		}
	}
}