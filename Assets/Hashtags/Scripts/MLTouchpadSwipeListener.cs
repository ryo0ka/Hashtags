using System;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace Hashtags
{
	public class MLTouchpadSwipeListener : IDisposable
	{
		readonly MLInputController _controller;
		readonly CompositeDisposable _life;
		readonly ISubject<Vector2> _swipes;

		bool _touchedLastFrame;
		Vector2 _lastPosition;

		public MLTouchpadSwipeListener(MLInputController controller)
		{
			_controller = controller;
			_life = new CompositeDisposable();
			_swipes = new Subject<Vector2>().AddTo(_life);
			Observable.EveryUpdate().Subscribe(_ => Update()).AddTo(_life);
		}

		public IObservable<Vector2> OnSwiped => _swipes;

		void Update()
		{
			if (!_controller.Touch1Active) // finger not on touchpad
			{
				_touchedLastFrame = false;
				return;
			}

			Vector2 pos = _controller.Touch1PosAndForce;

			if (_touchedLastFrame)
			{
				_swipes.OnNext(pos - _lastPosition);
			}

			_touchedLastFrame = true;
			_lastPosition = pos;
		}

		public void Dispose()
		{
			_life.Dispose();
		}
	}
}