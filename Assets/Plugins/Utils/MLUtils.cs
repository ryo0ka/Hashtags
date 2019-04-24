using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace Utils
{
	public static class MLUtils
	{
		public static void ThrowIfFail(this MLResult result)
		{
			if (result.IsOk || result.Code == MLResultCode.PrivilegeGranted)
			{
				return;
			}

			throw new Exception($"IsOK: {result.IsOk}, Code: {result.Code}");
		}

		public static async UniTask RequestPrivilege(MLPrivilegeId privilege)
		{
			// Don't do privilege if app is running in neither ML device nor ZI mode
			if (!XRDevice.isPresent)
			{
				return;
			}

			if (!MLPrivileges.IsStarted)
			{
				MLPrivileges.Start().ThrowIfFail();
			}

			MLResult? result = null;
			MLPrivileges.RequestPrivilegeAsync(privilege, (r, _) =>
			{
				result = r;
			}).ThrowIfFail();

			await UniTask.WaitUntil(() => result.HasValue);

			result?.ThrowIfFail();
		}

		public static IObservable<Unit> OnTriggerUpAsObservable(KeyCode? editorKey = null)
		{
			var controller = Observable.FromEvent<Action<byte, float>, Unit>(
				f => (_, __) => f(Unit.Default),
				h => MLInput.OnTriggerUp += h,
				h => MLInput.OnTriggerUp -= h);

			// Support secondary input on keyboard if app is running in Editor
			if (Application.isEditor && editorKey is KeyCode key)
			{
				var keyboard = Observable.EveryUpdate()
				                         .Where(_ => Input.GetKeyUp(key))
				                         .AsUnitObservable();

				controller = controller.Merge(keyboard);
			}

			return controller;
		}

		public static IObservable<Unit> OnButtonUpAsObservable(MLInputControllerButton button, KeyCode? editorKey = null)
		{
			var buttons = Observable.FromEvent<Action<byte, MLInputControllerButton>, MLInputControllerButton>(
				f => (_, b) => f(b),
				h => MLInput.OnControllerButtonUp += h,
				h => MLInput.OnControllerButtonUp -= h);

			var ups = buttons.Where(b => b == button).AsUnitObservable();

			if (Application.isEditor && editorKey is KeyCode key)
			{
				var keys = Observable.EveryUpdate()
				                     .Where(_ => Input.GetKeyUp(key))
				                     .AsUnitObservable();

				ups = ups.Merge(keys);
			}

			return ups;
		}

		public static IObservable<MLInputControllerTouchpadGesture> OnTouchpadGestureEnded()
		{
			return Observable.FromEvent<Action<byte, MLInputControllerTouchpadGesture>, MLInputControllerTouchpadGesture>(
				f => (_, path) => f(path),
				h => MLInput.OnControllerTouchpadGestureEnd += h,
				h => MLInput.OnControllerTouchpadGestureEnd -= h);
		}

		public static IObservable<string> OnCaptureCompletedAsObservable()
		{
			return Observable.FromEvent<Action<MLCameraResultExtras, string>, string>(
				f => (_, path) => f(path),
				h => MLCamera.OnCaptureCompleted += h,
				h => MLCamera.OnCaptureCompleted -= h);
		}

		public static IObservable<Unit> OnVideoPreparedAsObservable(this MLMediaPlayer self)
		{
			return Observable.FromEvent(
				h => self.OnVideoPrepared += h,
				h => self.OnVideoPrepared -= h);
		}

		public static IObservable<Unit> OnMediaErrorAsObservable(this MLMediaPlayer self)
		{
			return Observable.FromEvent<Action<MLResultCode, string>, (MLResultCode, string)>(
				f => (c, m) => f((c, m)),
				h => self.OnMediaError += h,
				h => self.OnMediaError -= h).Select<(MLResultCode, string), Unit>((c, m) =>
			{
				throw new Exception($"MediaError: {c}: {m}");
			});
		}

		public static IObservable<float> OnFrameSizeSetupAsObservable(this MLMediaPlayer self)
		{
			return Observable.FromEvent<float>(
				h => self.OnFrameSizeSetup += h,
				h => self.OnFrameSizeSetup -= h);
		}

		public static IObservable<byte> OnControllerConnected(bool includeAlreadyConnected = true)
		{
			var connecting = Observable.FromEvent<byte>(
				h => MLInput.OnControllerConnected += h,
				h => MLInput.OnControllerConnected -= h);

			if (includeAlreadyConnected)
			{
				// Search for already connected controllers
				List<byte> connected = new List<byte>();
				for (byte i = 0; i < 2; i++)
				{
					if (MLInput.GetController(i) != null)
					{
						connected.Add(i);
					}
				}

				connecting = connected.ToObservable().Merge(connecting);
			}

			return connecting;
		}

		public static IObservable<byte> OnControllerDisconnected()
		{
			return Observable.FromEvent<byte>(
				h => MLInput.OnControllerDisconnected += h,
				h => MLInput.OnControllerDisconnected -= h);
		}
	}
}