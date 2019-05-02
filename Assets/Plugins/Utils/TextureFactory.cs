using System;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Utils
{
	// Help download images without allocating duplicated texture objects
	// as well as help release unused texture objects
	public class TextureFactory
	{
		static readonly TextureFactory Instance = new TextureFactory();

		readonly Dictionary<string, Texture2D> _textures;
		readonly Dictionary<int, int> _usedCounts;

		TextureFactory()
		{
			_textures = new Dictionary<string, Texture2D>();
			_usedCounts = new Dictionary<int, int>();
		}

		async UniTask<Texture2D> DownloadInternal(string url)
		{
			// Check if any textures in this URL have been downloaded already
			if (TryGetUsed(url, out Texture2D texUsed))
			{
				// Don't download it. Increment the use count and return the existing one
				_usedCounts[texUsed.GetInstanceID()] += 1;
				return texUsed;
			}

			// Download texture
			var texNew = await DownloadTexture(url);

			// Somebody else has downloaded the same image in parallel
			if (TryGetUsed(url, out texUsed))
			{
				// Release the downloaded texture (we're not using it)
				Object.Destroy(texNew);

				// Increment the use count of the URL
				_usedCounts[texUsed.GetInstanceID()] += 1;

				// Return the existing texture,
				// not the one that we just downloaded
				return texUsed;
			}

			// Track this newly downloaded texture
			_textures[url] = texNew;
			_usedCounts[texNew.GetInstanceID()] = 1;

			return texNew;
		}

		bool TryGetUsed(string url, out Texture2D tex)
		{
			if (_textures.TryGetValue(url, out tex))
			{
				// Cache found & alive -> return it
				if (tex != null) return tex;

				// Cache found & dead -> invalidate it
				_textures.Remove(url);
				_usedCounts.Remove(tex.GetInstanceID());
				return false;
			}

			// No cache found -> do nothing
			return false;
		}

		static async UniTask<Texture2D> DownloadTexture(string url)
		{
			using (var req = UnityWebRequestTexture.GetTexture(url))
			{
				await req.SendWebRequest();

				if (!string.IsNullOrEmpty(req.error))
				{
					throw new Exception(req.error);
				}

				return ((DownloadHandlerTexture) req.downloadHandler).texture;
			}
		}

		// Take `Texture` type because RawImage holds Texture type
		void UnuseInternal(Texture tex)
		{
			// Skip null texture
			if (tex is null) return;
			
			// Skip unknown texture
			if (!_usedCounts.ContainsKey(tex.GetInstanceID())) return;

			// Decrement use count
			_usedCounts[tex.GetInstanceID()] -= 1;

			// Nobody is using this texture
			if (_usedCounts[tex.GetInstanceID()] <= 0)
			{
				// End tracking and release it
				_usedCounts.Remove(tex.GetInstanceID());
				Object.Destroy(tex);
			}
		}

		public static UniTask<Texture2D> Download(string url)
		{
			return Instance.DownloadInternal(url);
		}

		public static void Unuse(Texture tex)
		{
			Instance.UnuseInternal(tex);
		}
	}
}