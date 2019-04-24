using System;
using UniRx;
using UnityEngine;

namespace Hashtags
{
	public class KeyboardInputReceiver : IDisposable
	{
		readonly Subject<char> _chars = new Subject<char>();
		readonly Subject<Unit> _backspaces = new Subject<Unit>();
		readonly Subject<Unit> _returns = new Subject<Unit>();
		readonly IDisposable _onGUI;

		public KeyboardInputReceiver(IObservable<Unit> onGUI)
		{
			_onGUI = onGUI.Subscribe(_ => OnGUI());
		}

		public IObservable<char> OnCharacter => _chars;
		public IObservable<Unit> OnBackspace => _backspaces;
		public IObservable<Unit> OnReturn => _returns;

		void OnGUI()
		{
			Event e = Event.current;

			if (e.type != EventType.KeyDown)
			{
				return;
			}

			if (e.keyCode == KeyCode.Return)
			{
				_returns.OnNext(Unit.Default);
				return;
			}

			if (e.keyCode == KeyCode.Backspace)
			{
				_backspaces.OnNext(Unit.Default);
				return;
			}

			if (char.IsControl(e.character))
			{
				return;
			}

			_chars.OnNext(e.character);
		}

		public void Dispose()
		{
			_chars.Dispose();
			_backspaces.Dispose();
			_returns.Dispose();
			_onGUI.Dispose();
		}
	}
}