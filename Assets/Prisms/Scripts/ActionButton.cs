using UnityEngine;
using UnityEngine.UI;

namespace Prisms
{
	public class ActionButton : MonoBehaviour
	{
		[SerializeField]
		ActionIntent _intent;

		[SerializeField]
		Selectable _button;

		public ActionIntent Intent => _intent;

		public void SetSelected(bool focus)
		{
			_button.interactable = focus;
		}
	}
}