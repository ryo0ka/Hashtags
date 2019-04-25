using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLTwitter;
using Prisms;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using Utils;

namespace Hashtags
{
	public class PrismApp : PrismBehaviour
	{
		[SerializeField]
		CredentialRepository _credentials;

		[SerializeField]
		Text _titleText;

		[SerializeField]
		StatusView _statusViewTemplate;

		[SerializeField]
		Transform _statusViewRoot;

		[SerializeField]
		GameObject _keyboardConnectionView;

		[SerializeField]
		GameObject _controllerConnectionView;

		[SerializeField]
		ScrollRect _scroll;

		[SerializeField]
		float _scrollMagnitude;

		Subject<Unit> _onGUIs;

		public string DebugKeyword { private get; set; }

		protected override void Awake()
		{
			base.Awake();

			_onGUIs = new Subject<Unit>();

			// Initialize views
			_keyboardConnectionView.SetActive(false);
			_controllerConnectionView.SetActive(false);
		}

		protected override void OnSpawned()
		{
			DoStart().Forget(Debug.LogException);
		}

		void OnGUI()
		{
			// Receive OnGUIs for keyboard input
			_onGUIs?.OnNext(Unit.Default);
		}

		async UniTask DoStart()
		{
			// Set up Twitter client
			var client = new TWClient(_credentials, _credentials);
			_credentials.LoadStorage();
			await client.AuthorizeApp();

			_keyboardConnectionView.SetActive(true);

			// Wait until mobile app connection is secured
			await MLUtils.OnControllerConnected(MLInputControllerType.MobileApp)
			             .TakeUntilDestroy(this)
			             .First();

			_keyboardConnectionView.SetActive(false);

			// Receive a search keyword from mobile app (or editor keyboard)
			string searchKeyword = DebugKeyword ?? await ReadMobileKeyboard();

			// Show the entered keyword atop timeline
			_titleText.text = searchKeyword;

			_controllerConnectionView.SetActive(true);

			// Wait until controller connection is secured
			var controller = await MLUtils.OnControllerConnected(MLInputControllerType.Control)
			                              .TakeUntilDestroy(this)
			                              .First();

			_controllerConnectionView.SetActive(false);

			// Scroll timeline by swipes on touchpad
			var swipes = new MLTouchpadSwipeListener(controller).AddTo(this);
			swipes.OnSwiped.Where(_ => IsFocused && !IsActionActive).Subscribe(delta =>
			{
				_scroll.content.anchoredPosition += Vector2.up * delta.y * _scrollMagnitude;
			});

			// All tweet views present in the timeline
			var views = new SortedDictionary<long, StatusView>();

			while (this != null)
			{
				// Refresh timeline every 10 seconds
				var refresh = UniTask.Delay(10f.Seconds());

				// Fetch latest tweets with the search keyword
				var tweets = await client.Search(new TWSearchParameter
				{
					Query = searchKeyword,
					ResultType = TWSearchResultType.Mixed,
					Count = 50,
				});

				// Insert tweets to timeline
				foreach (var status in tweets.Statuses.Reverse())
				{
					// Skip already presented tweets
					if (views.ContainsKey(status.Id)) continue;

					// Skip tweets that don't contain media
					if (!status.TryFindMediaEntity(out _)) continue;

					// Instantiate a tweet view and add to the timeline
					var view = Instantiate(_statusViewTemplate, _statusViewRoot);
					view.transform.SetSiblingIndex(0);
					views.Add(status.Id, view);

					// Set data and present on timeline
					await view.SetStatus(status);
				}

				await refresh;
			}
		}

		async UniTask<string> ReadMobileKeyboard()
		{
			var input = new StringBuilder();

			// Receive keyboard input until Return is pressed
			using (var keyboard = new MLMobileKeyboardListener(_onGUIs).AddTo(this))
			{
				// Receive characters
				keyboard.OnCharacter.Subscribe(c =>
				{
					input.Append(c);
				});

				// Receive backspaces
				keyboard.OnBackspace.Subscribe(_ =>
				{
					input.Remove(input.Length - 1, 1);
				});

				// Finish input when Return is pressed
				await keyboard.OnReturn.First();
			}

			return input.ToString();
		}
	}
}