using MLTwitter;
using UnityEngine;

namespace Hashtags
{
	public class CredentialRepository : ScriptableObject, ITWCredentialRepository, ITWCredentialStorage
	{
		[Header("Twitter app credentials")]
		[SerializeField]
		string _consumerKey;

		[SerializeField]
		string _consumerSecret;

		[SerializeField]
		string _accessToken;

		[SerializeField]
		string _accessTokenSecret;

		[SerializeField, Header("Remember to update manifest.json as well")]
		string _callbackUrl;

		[Header("Runtime token storage. Leave these empty")]
		[SerializeField]
		string _appAccessToken;

		[SerializeField]
		string _userAccessToken;

		[SerializeField]
		string _userAccessTokenSecret;

		const string Key_AppAccessToken = "AppAccessToken";
		const string Key_UserAccessToken = "UserAccessToken";
		const string Key_UserAccessTokenSecret = "UserAccessTokenSecret";

		string ITWCredentialRepository.ConsumerKey => _consumerKey;
		string ITWCredentialRepository.ConsumerSecret => _consumerSecret;
		string ITWCredentialRepository.AccessToken => _accessToken;
		string ITWCredentialRepository.AccessTokenSecret => _accessTokenSecret;

		string ITWCredentialStorage.AppAccessToken
		{
			get => _appAccessToken;
			set => _appAccessToken = value;
		}

		string ITWCredentialStorage.UserAccessToken
		{
			get => _userAccessToken;
			set => _userAccessToken = value;
		}

		string ITWCredentialStorage.UserAccessTokenSecret
		{
			get => _userAccessTokenSecret;
			set => _userAccessTokenSecret = value;
		}

		public bool AppTokenExists => !string.IsNullOrEmpty(_appAccessToken);
		public bool UserTokenExists => !string.IsNullOrEmpty(_userAccessToken);

		public void ClearStorage()
		{
			_appAccessToken = null;
			_userAccessToken = null;
			_userAccessTokenSecret = null;

			PlayerPrefs.DeleteKey(Key_AppAccessToken);
			PlayerPrefs.DeleteKey(Key_UserAccessToken);
			PlayerPrefs.DeleteKey(Key_UserAccessTokenSecret);
		}

		public void SaveStorage()
		{
			PlayerPrefs.SetString(Key_AppAccessToken, _appAccessToken);
			PlayerPrefs.SetString(Key_UserAccessToken, _userAccessToken);
			PlayerPrefs.SetString(Key_UserAccessTokenSecret, _userAccessTokenSecret);
			PlayerPrefs.Save();
		}

		public void LoadStorage()
		{
			_appAccessToken = PlayerPrefs.GetString(Key_AppAccessToken);
			_userAccessToken = PlayerPrefs.GetString(Key_UserAccessToken);
			_userAccessTokenSecret = PlayerPrefs.GetString(Key_UserAccessTokenSecret);
		}

		public string CallbackUrl => _callbackUrl;
	}
}