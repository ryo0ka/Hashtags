using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils;
using Utils.MagicLeaps;

namespace Prisms
{
	public class SpatialMeshController : MonoBehaviour
	{
		[SerializeField]
		GameObject _root;

		[SerializeField]
		MLSpatialMapper _meshMapper;

		[SerializeField]
		Material _meshMaterial;

		[SerializeField]
		float _spaceScale;

		[SerializeField]
		float _duration;

		[SerializeField]
		AnimationCurve _curve;

		readonly int _matAlpha = Shader.PropertyToID("_WireAlpha");
		readonly int _matCenterPosition = Shader.PropertyToID("_CenterPosition");

		Camera _camera;
		Transform _target;

		void Start()
		{
			DoStart().Forget(Debug.LogException);
		}

		async UniTask DoStart()
		{
			_camera = Camera.main;
			_meshMapper.transform.localScale = Vector3.one * _spaceScale;

			_meshMapper.DestroyAllMeshes();
			_meshMapper.RefreshAllMeshes();
			
			await UniTask.WaitUntil(() => MLInput.IsStarted);

			// Initialize meshes when head tracking is lost
			MLUtils.OnHeadTrackingMapEventAsObservable()
			       .Where(e => e.IsLost())
			       .Subscribe(_ =>
			       {
				       _meshMapper.DestroyAllMeshes();
				       _meshMapper.RefreshAllMeshes();
			       })
			       .AddTo(this);

			// Keep mesh visuals updated
			_meshMapper.OnMeshAddedAsObservable()
			           .Merge(_meshMapper.OnMeshUpdadedAsObservable())
			           .Where(id => _meshMapper.meshIdToGameObjectMap.ContainsKey(id))
			           .Select(id => _meshMapper.meshIdToGameObjectMap[id])
			           .Select(mesh => mesh.GetComponent<Renderer>())
			           .Subscribe(meshRenderer =>
			           {
				           meshRenderer.enabled = true;
				           meshRenderer.sharedMaterial = _meshMaterial;
			           })
			           .AddTo(this);

			while (this != null)
			{
				// Sync Unity camera position
				_meshMapper.transform.position = _camera.transform.position;

				if (_target != null)
				{
					_meshMaterial.SetVector(_matCenterPosition, _target.position);
				}

				await UniTask.Yield();
			}
		}

		public async UniTask SetActive(bool active)
		{
			if (active)
			{
				_root.SetActive(true);
			}

			await UnityUtils.Animate(this, _duration, _curve, t =>
			{
				t = active ? t : 1f - t;
				_meshMaterial.SetFloat(_matAlpha, t);
			});

			if (!active)
			{
				_root.SetActive(false);
			}
		}

		public void SetTrackingTarget(Transform target)
		{
			_target = target;
		}
	}
}