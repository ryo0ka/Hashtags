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
	public class Main : MonoBehaviour
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
		Prism _prism;

		[SerializeField]
		float _scrollMagnitude;

		readonly Subject<Unit> _onGUIs = new Subject<Unit>();

		void Awake()
		{
			// Initialize views
			_statusViewTemplate.SetVisible(false, true).Forget(Debug.LogException);
			_keyboardConnectionView.SetActive(false);
			_controllerConnectionView.SetActive(false);
		}

		void Start()
		{
			DoStart().Forget(Debug.LogException);
		}

		void OnGUI()
		{
			// Send OnGUIs into a stream (for keyboard input)
			_onGUIs?.OnNext(Unit.Default);
		}

		async UniTask DoStart()
		{
			// Wait until all privileges are granted
			await MLUtils.RequestPrivilege(MLPrivilegeId.LocalAreaNetwork);

			// Set up Twitter client
			var client = new TWClient(_credentials, _credentials);
			_credentials.LoadStorage();
			await client.AuthorizeApp();

			_keyboardConnectionView.SetActive(true);

			// Wait until mobile app connection is secured
			await MLUtils.OnControllerConnected()
			             .TakeUntilDestroy(this)
			             .Select(id => MLInput.GetController(id).Type)
			             .Where(type => type == MLInputControllerType.MobileApp)
			             .First();

			_keyboardConnectionView.SetActive(false);

			// Receive a search keyword from mobile app keyboard (or editor)
			string searchKeyword = await ReadKeybordInput();

			// Show the entered keyword atop timeline
			_titleText.text = searchKeyword;

			_controllerConnectionView.SetActive(true);

			// Wait until controller connection is secured
			var controller = await MLUtils.OnControllerConnected()
			                              .TakeUntilDestroy(this)
			                              .Select(id => MLInput.GetController(id))
			                              .Where(c => c.Type == MLInputControllerType.Control)
			                              .First();

			_controllerConnectionView.SetActive(false);

			// Scroll timeline by swipes on touchpad
			var swipes = new MLTouchpadSwipeListener(controller).AddTo(this);
			swipes.OnSwiped.Where(_ => _prism.IsFocused).Subscribe(delta =>
			{
				_scroll.content.anchoredPosition += Vector2.up * delta.y * _scrollMagnitude;
			});

			// All tweet views present in the timeline
			var views = new SortedDictionary<long, StatusView>();

			while (this != null)
			{
				// Fetch latest tweets with the search keyword
				var tweets = await client.Search(searchKeyword, TWSearchResultType.Mixed);

				// Insert tweets to timeline
				foreach (var status in tweets.Statuses.Reverse())
				{
					// Skip already presented tweets
					if (views.ContainsKey(status.Id)) continue;

					// Instantiate a tweet view and add to the timeline
					var view = Instantiate(_statusViewTemplate, _statusViewRoot);
					views.Add(status.Id, view);

					// Set up data and present on timeline
					await view.SetStatus(status, true);
				}

				// Refresh timeline every 10 seconds
				await UniTask.Delay(10f.Seconds());
			}
		}

		async UniTask<string> ReadKeybordInput()
		{
			var input = new StringBuilder();

			// Receive keyboard input until Return is pressed
			using (var listener = new KeyboardInputReceiver(_onGUIs).AddTo(this))
			{
				listener.OnCharacter.Subscribe(c =>
				{
					input.Append(c);
				});

				listener.OnBackspace.Subscribe(_ =>
				{
					input.Remove(input.Length - 1, 1);
				});

				// Finish input when Return is pressed
				await listener.OnReturn.First();
			}

			return input.ToString();
		}
	}
}