using UnityEngine;
using UnityEngine.UI;

namespace Utils.Views
{
	public static class ViewUtils
	{
		public static void SetTexture(this RawImage image, Texture texture, AspectRatioFitter fitter = null)
		{
			image.texture = texture;

			fitter = fitter ?? image.GetComponent<AspectRatioFitter>();
			if (fitter == null) return;
			fitter.aspectRatio = (float) texture.width / texture.height;
		}

		static readonly Vector3[] _corners = new Vector3[4];

		// True if RectTransform contains corners of another RectTransform when viewed from a camera.
		// More specifically, evaluates the top left corner and bottom right corner.
		// Suitable for strict vertical/horizontal relationships, not for diagonal.
		// `container` and `content` don't have to be in a hierarchy.
		// Set `totally` false to tolerate one corner being outside the container.
		public static bool ContainsCorners(
			this RectTransform container,
			RectTransform content,
			Camera camera,
			bool totally)
		{
			// bottom left, top left, top right, bottom right!
			content.GetWorldCorners(_corners);
			
			Vector2 minPoint = camera.WorldToScreenPoint(_corners[0]);
			Vector2 maxPoint = camera.WorldToScreenPoint(_corners[2]);

			bool minContained = RectTransformUtility.RectangleContainsScreenPoint(container, minPoint, camera);
			bool maxContained = RectTransformUtility.RectangleContainsScreenPoint(container, maxPoint, camera);

			return totally
				? minContained && maxContained
				: minContained || maxContained;
		}

		public static bool ContainsPoint(
			this RectTransform container,
			RectTransform content,
			Vector2 normalPosition,
			Camera camera)
		{
			content.GetWorldCorners(_corners);
			Vector2 minPoint = camera.WorldToScreenPoint(_corners[0]);
			Vector2 maxPoint = camera.WorldToScreenPoint(_corners[2]);
			var point = MathUtils.Lerp(minPoint, maxPoint, normalPosition);
			return RectTransformUtility.RectangleContainsScreenPoint(container, point, camera);
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

		// Code from:
		// https://bitbucket.org/Unity-Technologies/ui/src/4f3cf8d16c1d8c6e681541a292855792e50b392e/UnityEngine.UI/UI/Core/GraphicRaycaster.cs
		public static bool IsLookingAt(this Camera camera, Canvas canvas)
		{
			Vector2 center = camera.ViewportToScreenPoint(Vector2.one / 2);

			var looked = false;
			var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
			for (var i = 0; i < graphics.Count; i++)
			{
				var g = graphics[i];
				var t = g.transform as RectTransform;
				looked |= RectTransformUtility.RectangleContainsScreenPoint(t, center, camera);
			}

			return looked;
		}

		public static bool IsLookingAt(this Camera camera, RectTransform transform)
		{
			Vector2 center = camera.ViewportToScreenPoint(Vector2.one / 2);
			return RectTransformUtility.RectangleContainsScreenPoint(transform, center, camera);
		}

		public static void SetText(this Text text, string message)
		{
			if (text != null)
			{
				text.text = message;
			}
		}
	}
}