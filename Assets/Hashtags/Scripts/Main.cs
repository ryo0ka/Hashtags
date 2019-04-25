using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Utils;
using UniRx;
using UniRx.Async;

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
			// Wait until all privileges are granted
			await MLUtils.RequestPrivilege(MLPrivilegeId.LocalAreaNetwork);

			var app = Instantiate(_prismTemplate);

			if (_debug)
			{
				app.DebugKeyword = "#magicleap";
			}

			if (!MLInput.IsStarted)
			{
				MLInput.Start().ThrowIfFail();
			}

			MLUtils.OnButtonUpAsObservable(MLInputControllerButton.HomeTap).Subscribe(_ =>
			{
				Instantiate(_prismTemplate);
			});
		}
	}
}