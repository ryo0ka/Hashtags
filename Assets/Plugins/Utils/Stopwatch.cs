using System;
using UniRx.Async;

namespace Utils
{
	public struct Stopwatch
	{
		internal DateTime StartTime { get; }

		Stopwatch(DateTime startTime)
		{
			StartTime = startTime;
		}

		public static Stopwatch Start()
		{
			return new Stopwatch(DateTime.Now);
		}
	}

	public static class Stopwatches
	{
		public static UniTask WaitFor(this Stopwatch self, TimeSpan span)
		{
			return UniTask.WaitUntil(() => DateTime.Now - self.StartTime > span);
		}
	}
}