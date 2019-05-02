using System;
using UnityEngine;

namespace Hashtags
{
	[Serializable]
	public class TouchpadScrollHandler
	{
		[SerializeField]
		float _inertialTime;

		[SerializeField]
		float _inertiaLength;

		Vector2 _lastDelta;
		Vector2 _lastVel;
		Vector2 _lastPos;
		Vector2 _targetPos;

		float _lastTime;
		float _deltaTime;

		// Append inertia to input scroll delta
		public Vector2? Update(Vector2? rawDelta)
		{
			_deltaTime = Time.time - _lastTime;
			_lastTime = Time.time;

			// input exists
			if (rawDelta is Vector2 delta)
			{
				// init inertia
				_lastVel = Vector2.zero;
				_lastPos = Vector2.zero;
				_targetPos = delta * _inertiaLength;
				_lastDelta = delta;
				return _lastDelta;
			}

			// Do inertia
			var nextPos = Vector2.SmoothDamp(
				_lastPos,
				_targetPos,
				ref _lastVel,
				_inertialTime,
				float.MaxValue,
				_deltaTime);

			_lastDelta = nextPos - _lastPos;
			_lastPos = nextPos;
			return _lastDelta;
		}
	}
}