using System.Collections.Generic;
using System.Text;
using MLTwitter;
using Prisms;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using Utils;
using Utils.MagicLeaps;
using Utils.Views;

namespace Hashtags
{
	public class PrismApp : PrismBehaviour
	{
		[SerializeField]
		CredentialRepository _credentials;

		[SerializeField]
		Text _titleText;

		[SerializeField]
		TimelineView _timelineView;

		[SerializeField]
		Visible _logoView;

		[SerializeField]
		Visible _keyboardConnectionView;

		[SerializeField]
		Visible _controllerConnectionView;

		[SerializeField]
		Visible _keyboardInstructionView;

		[SerializeField]
		GameObject _scrollView;

		// All tweets present in the timeline
		HashSet<long> _presentStatuses;

		TWClient _client;

		public string DebugKeyword { private get; set; }

		protected override void Awake()
		{
			base.Awake();

			_presentStatuses = new HashSet<long>();

			// Initialize views
			_keyboardConnectionView.HideUntilStart();
			_controllerConnectionView.HideUntilStart();
			_keyboardInstructionView.HideUntilStart();
			_scrollView.SetActive(false);
		}

		protected override void StartPrismApp()
		{
			DoStart().Forget(Debug.LogException);
		}

		async UniTask DoStart()
		{
			// Set up Twitter client
			_client = new TWClient(_credentials, _credentials);
			_credentials.LoadStorage();
			await _client.AuthorizeApp();

			await _keyboardConnectionView.SetVisible(true);

			if (string.IsNullOrEmpty(DebugKeyword))
			{
				// Wait until mobile app connection is secured
				await MLUtils.OnControllerConnected(MLInputControllerType.MobileApp)
				             .TakeUntilDestroy(this)
				             .First();
			}

			await _keyboardConnectionView.SetVisible(false);
			await _keyboardInstructionView.SetVisible(true);

			// Receive a search keyword from mobile app (or editor keyboard)
			string rawInput = DebugKeyword ?? await ReadKeyboard();

			string searchKeyword = TWUtils.MakeHashtag(rawInput);

			// Show the entered keyword atop timeline
			_titleText.text = searchKeyword;

			await _keyboardInstructionView.SetVisible(false);

			bool firstTimeUpdatingList = true;
			long lastLatestId = 0;

			while (this != null)
			{
				var stopwatch = Stopwatch.Start();

				// Fetch latest tweets with the search keyword
				var tweets = await _client.Search(new TWSearchParameter
				{
					Query = searchKeyword,
					ResultType = TWSearchResultType.Mixed,
					Count = 100,
					SinceId = lastLatestId,
				});

				if (tweets.Statuses.TryGetFirstValue(out var latest))
				{
					lastLatestId = latest.Id;
				}

				if (firstTimeUpdatingList)
				{
					firstTimeUpdatingList = false;

					// Hide logo
					await _logoView.SetVisible(false);

					// Show timeline
					_scrollView.SetActive(true);
				}

				await UpdateTimeline(tweets);

				// Refresh timeline every 10 seconds
				await stopwatch.WaitFor(10f.Seconds());

				Debug.Log("refreshed");
			}
		}

		async UniTask UpdateTimeline(TWStatuses statuses)
		{
			// Start with older tweets
			for (var i = statuses.Statuses.Length - 1; i >= 0; i--)
			{
				var status = statuses.Statuses[i];

				// Use the RT (if exists) so we don't get duplicates
				status = status.TryFindRetweetedStatus(out var rt) ? rt : status;

				// Skip already present tweets
				if (_presentStatuses.Contains(status.Id)) continue;

				// Skip tweets that don't contain media
				if (!status.TryFindMediaEntity(out _)) continue;

				var stopwatch = Stopwatch.Start();

				// Prevent duplicates later
				_presentStatuses.Add(status.Id);

				// Set data and present on timeline
				await _timelineView.Append(status);

				// Display one tweet per second
				await stopwatch.WaitFor(0.5f.Seconds());

				Debug.Log("done interval");
			}
		}

		async UniTask<string> ReadKeyboard()
		{
			var input = new StringBuilder();

			// Receive keyboard input until Return is pressed
			using (var keyboard = new MLMobileKeyboardListener(this.OnGUIAsObservable()).AddTo(this))
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