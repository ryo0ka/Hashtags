using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils.MagicLeaps;
using Utils.Views;

namespace Prisms
{
	public class PrismActionView : MonoBehaviour
	{
		[SerializeField]
		Visible _visible;

		[SerializeField]
		ActionButton[] _buttons;

		Subject<ActionIntent> _intents;
		bool _active;
		int _selectedIndex;

		public IObservable<ActionIntent> OnIntent => _intents;

		public void Initialize()
		{
			_active = false;
			_intents = new Subject<ActionIntent>().AddTo(this);

			// Receive trigger button presses
			MLUtils.OnTriggerUpAsObservable()
			       .Where(_ => _active)
			       .Subscribe(_ => OnTriggerPressed())
			       .AddTo(this);

			// Receive swipe gestures on touchpad
			MLUtils.OnTouchpadGestureEndedAsObservable()
			       .Where(_ => _active)
			       .Where(g => g.Type == MLInputControllerTouchpadGestureType.Swipe)
			       .Subscribe(g => OnTouchpadSwiped(g))
			       .AddTo(this);

			// Hide this view on start
			_visible.HideUntilStart();

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
			_selectedIndex = Mathf.Clamp(_selectedIndex, 0, _buttons.Length);

			// Update views
			ApplySelectionToButtons();
		}

		int SwipeDirectionToIndexDelta(MLInputControllerTouchpadGestureDirection dir)
		{
			switch (dir)
			{
				case MLInputControllerTouchpadGestureDirection.Left: return -1;
				case MLInputControllerTouchpadGestureDirection.Right: return 1;
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

		public async UniTask SetActive(bool active)
		{
			_active = active;
			await _visible.SetVisible(active);
		}
	}
}