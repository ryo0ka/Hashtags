using UniRx.Async;
using UnityEngine;

namespace Utils.Views
{
	public abstract class Visible : MonoBehaviour
	{
		public abstract void HideUntilStart();
		public abstract UniTask ShowForStart();
		public abstract UniTask SetVisible(bool visible, bool overwrite=true);
		public abstract UniTask SetFocus(bool focused);
		public abstract UniTask HideForDestroy();
	}
}