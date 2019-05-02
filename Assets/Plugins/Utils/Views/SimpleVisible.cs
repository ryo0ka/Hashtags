using UniRx.Async;
using UnityEngine;

namespace Utils.Views
{
	public class SimpleVisible : Visible
	{
		[SerializeField]
		CanvasGroup _group;

		[SerializeField]
		float _duration;

		[SerializeField]
		AnimationCurve _curve;

		[SerializeField]
		float _focusDepth;

		[SerializeField, Range(0, 1)]
		float _focusMinAlpha;

		[SerializeField]
		bool _controlGameObjectActive;

		bool _animatingVisible;

		void Reset()
		{
			_group = GetComponent<CanvasGroup>();
			_duration = 0.1f;
			_curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		}

		public override void HideUntilStart()
		{
			_group.alpha = 0;

			if (_controlGameObjectActive)
			{
				gameObject.SetActive(false);
			}
		}

		public override UniTask ShowForStart()
		{
			return SetVisible(true);
		}

		public override async UniTask SetVisible(bool visible, bool overwrite = true)
		{
			if (_animatingVisible && !overwrite) return;

			_animatingVisible = true;

			if (_controlGameObjectActive && visible)
			{
				_group.gameObject.SetActive(true);
			}

			var initialAlpha = _group.alpha;

			await UnityUtils.Animate(this, _duration, _curve, t =>
			{
				var alpha = Mathf.Lerp(initialAlpha, visible ? 1 : 0, t);
				_group.alpha = alpha;
			});

			if (_controlGameObjectActive && !visible)
			{
				_group.gameObject.SetActive(false);
			}

			_animatingVisible = false;
		}

		public override UniTask SetFocus(bool focused)
		{
			return UnityUtils.Animate(this, _duration, _curve, t =>
			{
				t = focused ? t : 1f - t;

				float depth = Mathf.Lerp(0, _focusDepth, t);
				(_group.transform as RectTransform).SetAnchoredPosition(z: depth);

				float alpha = Mathf.Lerp(_focusMinAlpha, 1f, t);
				_group.alpha = alpha;
			});
		}

		public override UniTask HideForDestroy()
		{
			return SetVisible(false);
		}
	}
}