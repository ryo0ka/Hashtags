using System;
using UniRx;
using UnityEngine.XR.MagicLeap;

namespace Utils.MagicLeaps
{
	public class MLLatestControllerObserver : IDisposable
	{
		readonly CompositeDisposable _life;
		readonly ReactiveProperty<MLInputController> _controller;

		public MLLatestControllerObserver()
		{
			_life = new CompositeDisposable();
			_controller = new ReactiveProperty<MLInputController>().AddTo(_life);
			var controllers = new ReactiveDictionary<int, byte>().AddTo(_life);

			// Publish a controller interacted by user
			var inputs = MLUtils.OnTriggerDownAsObservable()
			                    .Merge(MLUtils.OnAnyButtonDownAsObservable())
			                    .Merge(MLUtils.OnTouchpadGestureStartAsObservable());

			// Publish a controller connected when no other controllers are connected
			var first = controllers.ObserveAdd()
			                       .Where(_ => controllers.Count == 1)
			                       .Select(e => e.Key);

			// Publish "null" when no controllers are connected
			var none = controllers.ObserveRemove()
			                      .Where(_ => controllers.Count == 0)
			                      .Select(e => -1); // null

			// Start observing
			inputs.Merge(first, none)
			      .Select(id => MLInput.GetController(id))
			      .Subscribe(c => _controller.Value = c)
			      .AddTo(_life);

			// Set it on fire
			MLUtils.OnControllerConnected(includeConnected: true)
			       .Subscribe(id => controllers[id] = 0)
			       .AddTo(_life);

			// Set it on fire
			MLUtils.OnControllerDisconnected()
			       .Subscribe(id => controllers.Remove(id))
			       .AddTo(_life);
		}

		public IObservable<MLInputController> OnControllerChanged => _controller;

		public void Dispose()
		{
			_life.Dispose();
		}
	}
}