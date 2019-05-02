using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.MagicLeaps;
using Utils.Views;

namespace Hashtags
{
	public class TimelineEntryVisible : Visible
	{
		[SerializeField]
		GameObject _content;

		[SerializeField]
		CanvasGroup _group;

		[SerializeField]
		LayoutElement _layoutElement;

		[SerializeField]
		MLVideoPlayer _videoPlayer;

		[SerializeField]
		Selectable _focusIndicator;

		[SerializeField]
		float _duration;

		[SerializeField]
		AnimationCurve _curve;

		[SerializeField]
		float _focusDepth;

		[SerializeField]
		float _height;

		[SerializeField, Range(0, 1)]
		float _maxAudioVolume;

		bool _animatingVisible;
		float _audioVolume;
		bool _visible;
		bool _expandedLayout;

		void Reset()
		{
			_group = GetComponent<CanvasGroup>();
			_duration = 0.25f;
			_curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		}

		void Awake()
		{
			_focusIndicator.interactable = false;
		}

		void Update()
		{
			_videoPlayer.SetVolume(_audioVolume);
		}

		public override void HideUntilStart()
		{
			_group.alpha = 0;
			_layoutElement.preferredHeight = 0;
			_content.SetActive(false);
		}

		public override UniTask ShowForStart()
		{
			return SetVisibleInternal(true, true, true);
		}

		public override UniTask SetVisible(bool visible, bool overwrite = true)
		{
			return SetVisibleInternal(visible, false, overwrite);
		}

		public override UniTask HideForDestroy()
		{
			return SetVisibleInternal(false, true, true);
		}

		async UniTask SetVisibleInternal(bool visible, bool controlLayout, bool overwrite)
		{
			// Use this field to check if view should be visible
			// because this method can be called during the last one.
			_visible = visible;

			if (_animatingVisible && !overwrite) return;

			_animatingVisible = true;

			if (_visible)
			{
				_content.SetActive(true);
			}

			float initialAlpha = _group.alpha;
			bool expandedLayout = false;

			await UnityUtils.Animate(this, _duration, _curve, t =>
			{
				t = Mathf.Lerp(initialAlpha, _visible ? 1 : 0, t);

				// Gradually appear
				_group.alpha = t;

				// Gradually come up front
				// Note: 0 is the max up-front value
				float depth = Mathf.Lerp(_focusDepth, 0, t);
				(transform as RectTransform).SetAnchoredPosition(z: depth);

				if (controlLayout || visible && !_expandedLayout)
				{
					// Gradually expand from zero to preferred size
					_layoutElement.preferredHeight = Mathf.Lerp(0, _height, t);

					expandedLayout = visible;
				}
			});

			if (expandedLayout)
			{
				_expandedLayout = true;
			}

			if (!_visible)
			{
				_content.SetActive(false);
			}

			_animatingVisible = false;
		}

		public override UniTask SetFocus(bool focused)
		{
			_focusIndicator.interactable = focused;
			_videoPlayer.TrySetVideoPlaying(focused);

			return UnityUtils.Animate(this, _duration, _curve, t =>
			{
				t = focused ? t : 1f - t;

				_audioVolume = Mathf.Lerp(0, _maxAudioVolume, t);
			});
		}
	}
}