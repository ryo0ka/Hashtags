using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils;
using Utils.MagicLeaps;

namespace Prisms
{
	// Move prism according to camera location until disposed
	public class PrismTransformer : MonoBehaviour
	{
		[SerializeField]
		Transform _prism;

		[SerializeField]
		Transform _scalePrism;

		[SerializeField]
		Vector2 _scaleRange;

		[SerializeField]
		float _scaleMagnitude;

		[SerializeField]
		float _distanceMagnitude;

		[SerializeField]
		float _prismDistance;

		[SerializeField]
		float _prismScale;

		[SerializeField]
		float _minPrismDistance;

		int _spatialMeshLayerMask;
		float? _spatialMeshDistance;

		Transform _camera;

		BehaviorSubject<bool> _acceptXAngle;
		Queue<Vector3> _posBuffer; // smooth motion
		MLTouchpadListener _touchpadListener;

		void Awake()
		{
			_spatialMeshLayerMask = 1 << LayerMask.NameToLayer("Spatial Mesh");
			_posBuffer = new Queue<Vector3>();
			_acceptXAngle = new BehaviorSubject<bool>(false).AddTo(this);

			_camera = Camera.main.transform;
		}

		void Start()
		{
			// Observe bumper and accept X-axis rotation while pressed
			var bumperDowns = MLUtils.OnButtonDownAsObservable(MLInputControllerButton.Bumper).Select(_ => true);
			var bumperUps = MLUtils.OnButtonUpAsObservable(MLInputControllerButton.Bumper).Select(_ => false);
			bumperDowns.Merge(bumperUps).Subscribe(_acceptXAngle).AddTo(this);

			MLUtils.LatestTouchpadListenerAsObservable()
			       .Subscribe(c => _touchpadListener = c)
			       .AddTo(this);

			_prismScale = Mathf.Clamp(_prismScale, _scaleRange.x, _scaleRange.y);
			_scalePrism.localScale = Vector3.one * _prismScale;
		}

		void Update()
		{
			// Handle scaling input 
			Vector2 swipeDelta = _touchpadListener?.Update() ?? Vector2.zero;

			if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y)) // scale
			{
				_prismScale += swipeDelta.x * _scaleMagnitude;
				_prismScale = Mathf.Clamp(_prismScale, _scaleRange.x, _scaleRange.y);
				_scalePrism.localScale = Vector3.one * _prismScale;
			}
			else // distance
			{
				// Handle touchpad swipe for prism position
				_prismDistance += swipeDelta.y * _distanceMagnitude;
			}

			// Let spatial mesh "push" prism back to camera
			var ray = new Ray(_camera.position, _camera.forward);
			if (Physics.Raycast(ray, out var hit, float.MaxValue, _spatialMeshLayerMask))
			{
				var spatialMeshDistance = hit.distance - 0.1f;
				_prismDistance = Mathf.Min(_prismDistance, spatialMeshDistance);
			}

			// Prevent coming up to camera too closely
			_prismDistance = Mathf.Max(_prismDistance, _minPrismDistance);

			// Set position via distance and append smoothing
			Vector3 worldPosition = _camera.position + _camera.forward * _prismDistance;
			_prism.position = EnqueuePositionBuffer(worldPosition);

			// Apply rotation
			_prism.LookAt(_camera);
			if (!_acceptXAngle.Value)
			{
				_prism.SetLocalEulerAngles(x: 0);
			}
		}

		Vector3 EnqueuePositionBuffer(Vector3 pos)
		{
			const int MaxBufferSize = 10;

			_posBuffer.Enqueue(pos);

			while (_posBuffer.Count > MaxBufferSize)
			{
				_posBuffer.Dequeue();
			}

			Vector3 sumPos = Vector3.zero;
			foreach (Vector3 p in _posBuffer)
			{
				sumPos += p;
			}

			return sumPos / _posBuffer.Count;
		}

		public void SetActive(bool active)
		{
			enabled = active;
		}
	}
}