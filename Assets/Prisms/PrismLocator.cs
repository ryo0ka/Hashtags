using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utils;

namespace Prisms
{
	// Move prism according to camera location until disposed
	public class PrismLocator : IDisposable
	{
		readonly CompositeDisposable _life;
		readonly Transform _prism;
		readonly Transform _camera;

		public PrismLocator(Transform prism, Transform camera)
		{
			_prism = prism;
			_camera = camera;
			_life = new CompositeDisposable();
			prism.LateUpdateAsObservable().Subscribe(_ => LateUpdate()).AddTo(_life);
		}

		void LateUpdate()
		{
			const float Distance = 1.5f;

			_prism.position = _camera.position + _camera.forward * Distance;
			_prism.LookAt(_camera);
			_prism.SetLocalEulerAngles(x: 0, z: 0);
		}

		public void Dispose()
		{
			_life.Dispose();
		}
	}
}