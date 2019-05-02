using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils.Views
{
	public class IconLayoutElement : UIBehaviour, ILayoutElement
	{
		[SerializeField]
		float _layoutSize;
		
		public void CalculateLayoutInputHorizontal()
		{
		}

		public void CalculateLayoutInputVertical()
		{
		}

		public float minWidth => _layoutSize;
		public float preferredWidth => _layoutSize;
		public float flexibleWidth => 0;
		public float minHeight => _layoutSize;
		public float preferredHeight => _layoutSize;
		public float flexibleHeight => 0;
		public int layoutPriority => 1;
	}
}