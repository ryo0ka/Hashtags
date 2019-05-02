using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace Utils.MagicLeaps
{
	public static class MLUtils
	{
		static MLLatestControllerObserver _latestControllerObserver;

		public static void ThrowIfFail(this MLResult result, bool onDeviceOnly = false)
		{
			if (onDeviceOnly && !XRDevice.isPresent) return;

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

			MLResult? result = null;
			MLPrivileges.RequestPrivilegeAsync(privilege, (r, _) =>
			{
				result = r;
			}).ThrowIfFail(onDeviceOnly: true);

			await UniTask.WaitUntil(() => result.HasValue);

			result?.ThrowIfFail();
		}

		public static IObservable<int> OnTriggerUpAsObservable(KeyCode? editorKey = null)
		{
			var controller = Observable.FromEvent<MLInput.TriggerDelegate, int>(
				f => (c, __) => f(c),
				h => MLInput.OnTriggerUp += h,
				h => MLInput.OnTriggerUp -= h);

			// Support secondary input on keyboard if app is running in Editor
			if (Application.isEditor && editorKey is KeyCode key)
			{
				var keyboard = Observable.EveryUpdate()
				                         .Where(_ => Input.GetKeyUp(key))
				                         .Select(_ => 0);

				controller = controller.Merge(keyboard);
			}

			return controller;
		}

		public static IObservable<int> OnTriggerDownAsObservable(KeyCode? editorKey = null)
		{
			var controller = Observable.FromEvent<MLInput.TriggerDelegate, int>(
				f => (c, __) => f(c),
				h => MLInput.OnTriggerDown += h,
				h => MLInput.OnTriggerDown -= h);

			// Support secondary input on keyboard if app is running in Editor
			if (Application.isEditor && editorKey is KeyCode key)
			{
				var keyboard = Observable.EveryUpdate()
				                         .Where(_ => Input.GetKeyDown(key))
				                         .Select(_ => 0);

				controller = controller.Merge(keyboard);
			}

			return controller;
		}

		public static IObservable<int> OnButtonUpAsObservable(MLInputControllerButton button, KeyCode? editorKey = null)
		{
			var buttons = Observable.FromEvent<MLInput.ControllerButtonDelegate, (int, MLInputControllerButton)>(
				f => (c, b) => f((c, b)),
				h => MLInput.OnControllerButtonUp += h,
				h => MLInput.OnControllerButtonUp -= h);

			var ups = buttons.Where(p => p.Item2 == button).Select(p => p.Item1);

			if (Application.isEditor && editorKey is KeyCode key)
			{
				var keys = Observable.EveryUpdate()
				                     .Where(_ => Input.GetKeyUp(key))
				                     .Select(_ => 0);

				ups = ups.Merge(keys);
			}

			return ups;
		}

		public static IObservable<int> OnButtonDownAsObservable(MLInputControllerButton button, KeyCode? editorKey = null)
		{
			var buttons = Observable.FromEvent<MLInput.ControllerButtonDelegate, (int, MLInputControllerButton)>(
				f => (c, b) => f((c, b)),
				h => MLInput.OnControllerButtonDown += h,
				h => MLInput.OnControllerButtonDown -= h);

			var ups = buttons.Where(p => p.Item2 == button).Select(p => p.Item1);

			if (Application.isEditor && editorKey is KeyCode key)
			{
				var keys = Observable.EveryUpdate()
				                     .Where(_ => Input.GetKeyDown(key))
				                     .Select(_ => 0);

				ups = ups.Merge(keys);
			}

			return ups;
		}

		public static IObservable<int> OnAnyButtonDownAsObservable()
		{
			return Observable.FromEvent<MLInput.ControllerButtonDelegate, int>(
				f => (c, b) => f(c),
				h => MLInput.OnControllerButtonDown += h,
				h => MLInput.OnControllerButtonDown -= h);
		}

		public static IObservable<int> OnTouchpadGestureStartAsObservable()
		{
			return Observable.FromEvent<MLInput.ControllerTouchpadGestureDelegate, int>(
				f => (id, _) => f(id),
				h => MLInput.OnControllerTouchpadGestureStart += h,
				h => MLInput.OnControllerTouchpadGestureStart -= h);
		}

		public static IObservable<MLInputControllerTouchpadGesture> OnTouchpadGestureEndedAsObservable()
		{
			return Observable.FromEvent<MLInput.ControllerTouchpadGestureDelegate, MLInputControllerTouchpadGesture>(
				f => (_, path) => f(path),
				h => MLInput.OnControllerTouchpadGestureEnd += h,
				h => MLInput.OnControllerTouchpadGestureEnd -= h);
		}

		public static IObservable<Unit> OnTouchpadSwipeAsObservable(MLInputControllerTouchpadGestureDirection direction)
		{
			return OnTouchpadGestureEndedAsObservable()
			       .Where(g => g.Type == MLInputControllerTouchpadGestureType.Swipe)
			       .Where(g => g.Direction == MLInputControllerTouchpadGestureDirection.Right)
			       .AsUnitObservable();
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

		public static IObservable<int> OnControllerConnected(bool includeConnected = true)
		{
			var connecting = Observable.FromEvent<MLInput.ControllerConnectionDelegate, int>(
				f => b => f(b),
				h => MLInput.OnControllerConnected += h,
				h => MLInput.OnControllerConnected -= h);

			if (includeConnected)
			{
				// Search for already connected controllers
				var connected = new List<int>();
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

		public static IObservable<MLInputController> OnControllerConnected(MLInputControllerType type, bool includeConnected = true)
		{
			return OnControllerConnected(includeConnected)
			       .Select(id => MLInput.GetController(id))
			       .Where(c => c.Type == type);
		}

		public static IObservable<int> OnControllerDisconnected()
		{
			return Observable.FromEvent<MLInput.ControllerConnectionDelegate, int>(
				f => b => f(b),
				h => MLInput.OnControllerDisconnected += h,
				h => MLInput.OnControllerDisconnected -= h);
		}

		public static IObservable<MLHeadTrackingMapEvent> OnHeadTrackingMapEventAsObservable()
		{
			return Observable.FromEvent<MLHeadTrackingMapEvent>(
				h => MagicLeapDevice.RegisterOnHeadTrackingMapEvent(h),
				h => MagicLeapDevice.UnregisterOnHeadTrackingMapEvent(h));
		}

		public static IObservable<TrackableId> OnMeshAddedAsObservable(this MLSpatialMapper self)
		{
			return Observable.FromEvent<TrackableId>(
				h => self.meshAdded += h,
				h => self.meshAdded -= h);
		}

		public static IObservable<TrackableId> OnMeshUpdadedAsObservable(this MLSpatialMapper self)
		{
			return Observable.FromEvent<TrackableId>(
				h => self.meshUpdated += h,
				h => self.meshUpdated -= h);
		}

		public static IObservable<float> TouchpadForceAsObservable(this MLInputController controller)
		{
			return Observable.EveryUpdate()
			                 .Select(_ => controller.Touch1Active
				                 ? controller.Touch1PosAndForce.z
				                 : 0f);
		}

		public static IObservable<MLInputController> LatestControllerAsObservable()
		{
			_latestControllerObserver = _latestControllerObserver ?? new MLLatestControllerObserver();
			return _latestControllerObserver.OnControllerChanged;
		}

		public static IObservable<MLTouchpadListener> LatestTouchpadListenerAsObservable()
		{
			return LatestControllerAsObservable()
				.Select(c => c != null ? new MLTouchpadListener(c) : null);
		}

		public static void OpenUrl(string url)
		{
			if (Application.isEditor)
			{
				Application.OpenURL(url);
				return;
			}

			MLDispatcher.TryOpenAppropriateApplication(url).ThrowIfFail();
		}
	}
}