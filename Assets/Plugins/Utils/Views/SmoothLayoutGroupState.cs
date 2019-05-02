using UnityEngine;

namespace Utils.Views
{
	public class SmoothLayoutGroupState
	{
		readonly SmoothSize _min;
		readonly SmoothSize _preferred;
		readonly SmoothSize _flexible;

		public float Min => _min.Current;
		public float Preferred => _preferred.Current;
		public float Flexible => _flexible.Current;
		public bool IsDirty => _min.IsDirty || _preferred.IsDirty || _flexible.IsDirty;

		public SmoothLayoutGroupState(string name, float duration)
		{
			_min = new SmoothSize($"{name} min", duration);
			_preferred = new SmoothSize($"{name} preferred", duration);
			_flexible = new SmoothSize($"{name} flexible", duration);
		}

		public void SetTargetSize(float min, float preferred, float flexible)
		{
			_min.SetTarget(min);
			_preferred.SetTarget(preferred);
			_flexible.SetTarget(flexible);
		}

		public void GetTargetSize(out float min, out float preferred, out float flexible)
		{
			min = _min.Target;
			preferred = _preferred.Target;
			flexible = _flexible.Target;
		}

		class SmoothSize
		{
			readonly string _name;
			readonly float _duration;

			float _velocity;

			public float Current { get; private set; }
			public float Target { get; private set; }
			public bool IsDirty => !Mathf.Approximately(Current, Target);

			public SmoothSize(string name, float duration)
			{
				_name = name;
				_duration = duration;
			}

			public void SetTarget(float target)
			{
				Target = target;
				Current = Mathf.SmoothDamp(Current, Target, ref _velocity, _duration);
				Current = Mathf.Abs(Current) < 0.001f ? 0 : Current; // Otherwise UI disappers occasionally
				//Debug.Log(${this}: {Current}");
			}

			public override string ToString()
			{
				return $"SmoothSize({_name})";
			}
		}
	}
}