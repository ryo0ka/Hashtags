using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace Utils.MagicLeaps
{
	public class MLTouchpadListener
	{
		readonly MLInputController _controller;

		bool _wasActiveLastFrame;
		Vector2 _valueLastFarme;
		float _lastTime;

		public MLTouchpadListener(MLInputController controller)
		{
			_controller = controller;
		}

		public Vector2? Update()
		{
			bool touchpadActive = _controller.Touch1Active;
			Vector2 value = _controller.Touch1PosAndForce;

			if (!touchpadActive)
			{
				_wasActiveLastFrame = false;
				return null;
			}

			if (!_wasActiveLastFrame)
			{
				_wasActiveLastFrame = true;
				_valueLastFarme = value;
				_lastTime = Time.time;
				return null;
			}

			_wasActiveLastFrame = true;

			var delta = value - _valueLastFarme;
			var deltaTime = Time.time - _lastTime;
			
			_valueLastFarme = value;
			_lastTime = Time.time;

			return delta * deltaTime;
		}
	}
}