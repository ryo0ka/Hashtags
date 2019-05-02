using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UniRx;
using UniRx.Async;
using Utils.MagicLeaps;

namespace Hashtags
{
	public class Main : MonoBehaviour
	{
		[SerializeField]
		PrismApp _prismTemplate;

		[SerializeField]
		bool _debug;

		void Start()
		{
			DoStart().Forget(Debug.LogException);
		}

		async UniTask DoStart()
		{
			MLPrivileges.Start().ThrowIfFail();
			MLInput.Start().ThrowIfFail();

			// Wait until all privileges are granted
			await MLUtils.RequestPrivilege(MLPrivilegeId.LocalAreaNetwork);

			while (this != null)
			{
				var app = Instantiate(_prismTemplate);

				if (_debug && Application.isEditor)
				{
					app.DebugKeyword = "#magic leap";
				}

				await app.PrismInitialized.First();

				await MLUtils.OnButtonUpAsObservable(MLInputControllerButton.HomeTap).First();
			}
		}

		void OnDrawGizmos()
		{
			Gizmos.DrawRay(Camera.main.ViewportPointToRay(Vector2.one / 2));
		}

		void OnDestroy()
		{
			MLPrivileges.Stop();
			MLInput.Stop();
		}
	}
}