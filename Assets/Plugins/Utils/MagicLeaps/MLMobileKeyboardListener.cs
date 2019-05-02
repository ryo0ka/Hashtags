using System;
using UniRx;
using UnityEngine;

namespace Utils.MagicLeaps
{
	// Receive keyboard events from Mobile App keyboard until Disposed.
	// Works in Unity Editor with PC keyboard, NOT the phone's keyboard.
	// Ensure BT connection with the device before using this class.
	// This class doesn't take a responsibility to handle BT connection.
	public class MLMobileKeyboardListener : IDisposable
	{
		readonly Subject<char> _chars = new Subject<char>();
		readonly Subject<Unit> _backspaces = new Subject<Unit>();
		readonly Subject<Unit> _returns = new Subject<Unit>();
		readonly IDisposable _onGUIs;

		// Pass OnGUI() events to the stream
		public MLMobileKeyboardListener(IObservable<Unit> onGUI)
		{
			_onGUIs = onGUI.Subscribe(_ => OnGUI());
		}

		// Note that you won't receive any characters
		// until user hits that "send" button on mobile UI.
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
			_onGUIs.Dispose();
		}
	}
}