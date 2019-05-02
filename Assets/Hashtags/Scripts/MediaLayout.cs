using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hashtags
{
	[RequireComponent(typeof(LayoutElement))]
	public class MediaLayout : UIBehaviour
	{
		[SerializeField]
		RawImage _image;

		[SerializeField]
		float _minAspectRatio;

		RectTransform _rectTransform;
		LayoutElement _layoutElement;

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();

			if (_layoutElement == null)
			{
				_layoutElement = GetComponent<LayoutElement>();
			}

			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}

			// Manipulate this view's dimension so that
			// too vertical media will "fit" in 16:10 window and
			// too horizontal media will shrink down the view's height
			float imageAspectRatio = (float) _image.texture.width / _image.texture.height;
			float aspectRatio = Mathf.Max(imageAspectRatio, _minAspectRatio);
			float width = _rectTransform.rect.width;
			_layoutElement.preferredHeight = width / aspectRatio;
		}
	}
}