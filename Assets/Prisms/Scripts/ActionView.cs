using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils;

namespace Prisms
{
	public class ActionView : MonoBehaviour
	{
		[SerializeField]
		CanvasGroup _group;

		[SerializeField]
		ActionButton[] _buttons;

		Subject<ActionIntent> _intents;
		bool _active;
		int _selectedIndex;
		
		public IObservable<ActionIntent> OnIntent => _intents;

		public void Initialize()
		{
			_intents = new Subject<ActionIntent>().AddTo(this);

			// Receive trigger button presses
			MLUtils.OnTriggerUpAsObservable()
			       .Where(_ => _active)
			       .Subscribe(_ => OnTriggerPressed())
			       .AddTo(this);

			// Receive swipe gestures on touchpad
			MLUtils.OnTouchpadGestureEnded()
			       .Where(_ => _active)
			       .Where(g => g.Type == MLInputControllerTouchpadGestureType.Swipe)
			       .Subscribe(g => OnTouchpadSwiped(g))
			       .AddTo(this);

			// Hide this view on start
			SetActive(false, true).Forget(Debug.LogException);

			// Ensure the first button is selected by default
			ApplySelectionToButtons();
		}

		void OnTriggerPressed()
		{
			// Notify the selected intent
			ActionIntent intent = _buttons[_selectedIndex].Intent;
			_intents.OnNext(intent);
		}

		void OnTouchpadSwiped(MLInputControllerTouchpadGesture gesture)
		{
			// Increment (decrement) selected index
			_selectedIndex += SwipeDirectionToIndexDelta(gesture.Direction);

			// Prevent oob exception
			_selectedIndex = (int) MathUtils.Mod(_selectedIndex, _buttons.Length);

			// Update views
			ApplySelectionToButtons();
		}

		int SwipeDirectionToIndexDelta(MLInputControllerTouchpadGestureDirection dir)
		{
			switch (dir)
			{
				case MLInputControllerTouchpadGestureDirection.Up: return -1;
				case MLInputControllerTouchpadGestureDirection.Down: return 1;
				default: return 0;
			}
		}

		void ApplySelectionToButtons()
		{
			for (var i = 0; i < _buttons.Length; i++)
			{
				_buttons[i].SetSelected(i == _selectedIndex);
			}
		}

		public async UniTask SetActive(bool active, bool immediate = false)
		{
			_active = active;

			if (immediate)
			{
				gameObject.SetActive(active);
				return;
			}

			await Fade(active);
		}

		async UniTask Fade(bool visible)
		{
			if (this && visible)
			{
				gameObject.SetActive(true);
			}

			await UnityUtils.Animate(this, 0.5f, AnimationCurve.EaseInOut(0, 0, 1, 1), t =>
			{
				_group.alpha = visible ? t : 1 - t;
			});

			if (this && !visible)
			{
				gameObject.SetActive(false);
			}
		}
	}
}