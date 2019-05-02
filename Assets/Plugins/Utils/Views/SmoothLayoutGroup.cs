using UnityEngine;
using UnityEngine.UI;

namespace Utils.Views
{
	public class SmoothLayoutGroup : LayoutGroup
	{
		[SerializeField]
		bool _strict;

		[SerializeField]
		bool _isVertical;

		[SerializeField]
		float _spacing;

		[SerializeField]
		bool _childControlWidth;

		[SerializeField]
		bool _childControlHeight;

		[SerializeField]
		bool _childForceExpandWidth;

		[SerializeField]
		bool _childForceExpandHeight;

		[SerializeField]
		float _lazyDuration;

		SmoothLayoutGroupState _state;

		protected override void Awake()
		{
			base.Awake();
			ResetModels();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			ResetModels();
		}
#endif

		void Update()
		{
			if (_state == null) return;

			if (Application.isPlaying && !_strict && _state.IsDirty)
			{
				SetDirty();
			}
		}

		void ResetModels()
		{
			if (!Application.isPlaying) return;

			_state = new SmoothLayoutGroupState(name, _lazyDuration);
		}

		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalcAlongAxis(0, _isVertical);
		}

		public override void CalculateLayoutInputVertical()
		{
			CalcAlongAxis(1, _isVertical);
		}

		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongAxis(0, _isVertical);
		}

		public override void SetLayoutVertical()
		{
			SetChildrenAlongAxis(1, _isVertical);
		}

		void CalcAlongAxis(int axis, bool isVertical)
		{
			float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);
			bool controlSize = (axis == 0 ? _childControlWidth : _childControlHeight);
			bool childForceExpandSize = (axis == 0 ? _childForceExpandWidth : _childForceExpandHeight);

			float totalMin = combinedPadding;
			float totalPreferred = combinedPadding;
			float totalFlexible = 0;

			bool alongOtherAxis = (isVertical ^ (axis == 1));
			for (int i = 0; i < rectChildren.Count; i++)
			{
				RectTransform child = rectChildren[i];
				float min, preferred, flexible;
				GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible);

				if (alongOtherAxis)
				{
					totalMin = Mathf.Max(min + combinedPadding, totalMin);
					totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
					totalFlexible = Mathf.Max(flexible, totalFlexible);
				}
				else
				{
					totalMin += min + _spacing;
					totalPreferred += preferred + _spacing;

					// Increment flexible size with element's flexible size.
					totalFlexible += flexible;
				}
			}

			if (!alongOtherAxis && rectChildren.Count > 0)
			{
				totalMin -= _spacing;
				totalPreferred -= _spacing;
			}

			totalPreferred = Mathf.Max(totalMin, totalPreferred);

			if (alongOtherAxis || !Application.isPlaying || _strict)
			{
				SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
			}
			else
			{
				_state.SetTargetSize(totalMin, totalPreferred, totalFlexible);
				SetLayoutInputForAxis(_state.Min, _state.Preferred, _state.Flexible, axis);
			}
		}

		void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
			out float min, out float preferred, out float flexible)
		{
			if (!controlSize)
			{
				min = child.sizeDelta[axis];
				preferred = min;
				flexible = 0;
			}
			else
			{
				min = LayoutUtility.GetMinSize(child, axis);
				preferred = LayoutUtility.GetPreferredSize(child, axis);
				flexible = LayoutUtility.GetFlexibleSize(child, axis);
			}

			if (childForceExpand)
				flexible = Mathf.Max(flexible, 1);
		}

		void SetChildrenAlongAxis(int axis, bool isVertical)
		{
			float size = rectTransform.rect.size[axis];
			bool controlSize = (axis == 0 ? _childControlWidth : _childControlHeight);
			bool childForceExpandSize = (axis == 0 ? _childForceExpandWidth : _childForceExpandHeight);
			float alignmentOnAxis = GetAlignmentOnAxis(axis);

			bool alongOtherAxis = (isVertical ^ (axis == 1));
			if (alongOtherAxis)
			{
				float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);
				for (int i = 0; i < rectChildren.Count; i++)
				{
					RectTransform child = rectChildren[i];
					float min, preferred, flexible;
					GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible);

					float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
					float startOffset = GetStartOffset(axis, requiredSpace);
					if (controlSize)
					{
						SetChildAlongAxis(child, axis, startOffset, requiredSpace);
					}
					else
					{
						float offsetInCell = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
						SetChildAlongAxis(child, axis, startOffset + offsetInCell);
					}
				}
			}
			else
			{
				float totalPreferredSize = GetTotalPreferredSize(axis);
				float totalMinSize = GetTotalMinSize(axis);
				float totalFlexibleSize = GetTotalFlexibleSize(axis);

				float pos = axis == 0 ? padding.left : padding.top;

				if (totalFlexibleSize == 0 && totalPreferredSize < size)
				{
					float totalPadding = axis == 0 ? padding.horizontal : padding.vertical;
					pos = GetStartOffset(axis, totalPreferredSize - totalPadding);
				}

				if (_state != null)
				{
					float alignment = GetAlignmentOnAxis(axis);
					float targetPreferredSize, targetMinSize, targetFlexibleSize;
					_state.GetTargetSize(out targetMinSize, out targetPreferredSize, out targetFlexibleSize);
					pos += (totalPreferredSize - targetPreferredSize) * alignment;
				}

				float minMaxLerp = 0;
				if (totalMinSize != totalPreferredSize)
				{
					minMaxLerp = Mathf.Clamp01((size - totalMinSize) / (totalPreferredSize - totalMinSize));
				}

				float itemFlexibleMultiplier = 0;
				if (size > totalPreferredSize && totalFlexibleSize > 0)
				{
					itemFlexibleMultiplier = (size - totalPreferredSize) / totalFlexibleSize;
				}

				for (int i = 0; i < rectChildren.Count; i++)
				{
					RectTransform child = rectChildren[i];
					float min, preferred, flexible;
					GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible);

					float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
					childSize += flexible * itemFlexibleMultiplier;

					if (controlSize)
					{
						SetChildAlongAxis(child, axis, pos, childSize);
					}
					else
					{
						float offsetInCell = (childSize - child.sizeDelta[axis]) * alignmentOnAxis;
						SetChildAlongAxis(child, axis, pos + offsetInCell);
					}

					pos += childSize + _spacing;
				}
			}
		}
	}
}