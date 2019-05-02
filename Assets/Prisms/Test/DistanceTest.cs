using UnityEngine;

namespace Prisms.Test
{
	internal class DistanceTest : MonoBehaviour
	{
		[SerializeField]
		Transform _target;

		[SerializeField]
		float _inputDistance;

		[SerializeField]
		Transform _mesh;

		[SerializeField]
		float _inputMeshDistance;

		Transform _camera;
		int _spatialMeshLayerMask;
		float _inputDelta;

		float? _meshDistance;

		void Start()
		{
			_camera = Camera.main.transform;
			_spatialMeshLayerMask = 1 << LayerMask.NameToLayer("Spatial Mesh");
		}

		void Update()
		{
			if (_meshDistance is float meshDistance)
			{
				_inputDistance = Mathf.Min(meshDistance, _inputDistance);
			}

			_inputDistance += Input.mouseScrollDelta.x * 0.1f;

			_target.position = _camera.position + _camera.forward * _inputDistance;
		}

		void LateUpdate()
		{
			float delta = Input.mouseScrollDelta.y * 0.1f;
			_inputMeshDistance += delta;
			_mesh.position = _camera.position + _camera.forward * _inputMeshDistance;
		}

		void FixedUpdate()
		{
			_meshDistance = SpatialMeshDistance();
		}

		float? SpatialMeshDistance()
		{
			var ray = new Ray(_camera.position, _camera.forward);
			if (Physics.Raycast(ray, out var hit, float.MaxValue, _spatialMeshLayerMask))
			{
				return hit.distance;
			}

			return null;
		}
	}
}