using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Views
{
	public static class ViewUtils
	{
		public static void SetTexture(this RawImage image, Texture texture, AspectRatioFitter fitter = null)
		{
			if (fitter == null && (fitter = image.GetComponent<AspectRatioFitter>()) == null)
			{
				throw new Exception($"AspectRatioFitter not found with RawImage '{image.name}'");
			}

			image.texture = texture;
			fitter.aspectRatio = (float) texture.width / texture.height;
		}

		// for IsRectContained()
		static readonly Vector3[] _corners = new Vector3[4];

		public static bool IsRectContained(
			Camera camera,
			RectTransform container,
			RectTransform content)
		{
			content.GetWorldCorners(_corners);
			Vector2 minPoint = camera.WorldToScreenPoint(_corners[0]);
			Vector2 maxPoint = camera.WorldToScreenPoint(_corners[2]);

			return RectTransformUtility.RectangleContainsScreenPoint(container, minPoint, camera) ||
			       RectTransformUtility.RectangleContainsScreenPoint(container, maxPoint, camera);
		}

		// Gradually expand from zero to preferred height
		public static void SetPreferredHeightByLerp(this LayoutElement layoutElement, float t)
		{
			layoutElement.enabled = false; // for actual preferred height
			float preferredHight = LayoutUtility.GetPreferredHeight(layoutElement.transform as RectTransform);
			float targetHeight = Mathf.Lerp(0, preferredHight, t);
			layoutElement.preferredHeight = targetHeight;
			layoutElement.enabled = true;
		}
	}
}