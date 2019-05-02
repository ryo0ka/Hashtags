using System;
using UniRx;
using UnityEngine;

namespace Utils.MagicLeaps
{
	public class OnGuiObserver : MonoBehaviour
	{
		Subject<Unit> _onGUIs;

		public IObservable<Unit> OnGUIs => _onGUIs;

		void Awake()
		{
			_onGUIs = new Subject<Unit>().AddTo(this);
		}

		void OnGUI()
		{
			_onGUIs.OnNext(Unit.Default);
		}
	}

	public static class OnGuiObservers
	{
		public static IObservable<Unit> OnGUIAsObservable(this Behaviour b)
		{
			var o = b.gameObject.GetComponent<OnGuiObserver>() ??
			        b.gameObject.AddComponent<OnGuiObserver>();

			return o.OnGUIs;
		}
	}
}