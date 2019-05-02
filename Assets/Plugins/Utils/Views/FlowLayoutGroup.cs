using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Views
{
	public class FlowLayoutGroup : LayoutGroup
	{
		[SerializeField]
		float _spacing;

		[SerializeField]
		Vector2 _flexible;

		[SerializeField]
		float _childWidth;

		[SerializeField]
		float _childHeight;

		[SerializeField]
		int _maxRowCount;

		FlowLayoutGroupState _state;
		FlowLayoutGroupState State => _state ?? (_state = new FlowLayoutGroupState(_maxRowCount));

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			_maxRowCount = Mathf.Max(1, _maxRowCount);

			// reset the model when configuration changes
			if (_maxRowCount != _state?.GetMaxRowCount())
			{
				_state = new FlowLayoutGroupState(_maxRowCount);
				SetDirty();
			}
		}
#endif

		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();

			State.Initialize(rectChildren.Count);
			SetLayoutInputForAxis(0);
		}

		public override void CalculateLayoutInputVertical()
		{
			State.Initialize(rectChildren.Count);
			SetLayoutInputForAxis(1);
		}

		public override void SetLayoutHorizontal()
		{
			State.Initialize(rectChildren.Count);
			SetChildrenAlongAxis(0);
		}

		public override void SetLayoutVertical()
		{
			State.Initialize(rectChildren.Count);
			SetChildrenAlongAxis(1);
		}

		void SetLayoutInputForAxis(int axis)
		{
			int num = axis == 0 ? State.GetMaxColumnCount() : State.GetRowCount();
			float allPadding = axis == 0 ? padding.horizontal : padding.vertical;
			float allSpacing = Math.Max(0, num - 1) * _spacing;
			float size = axis == 0 ? _childWidth : _childHeight;
			float totalMin = num * size + allPadding + allSpacing;

			//Debug.Log($"SetLayoutInput(axis: {axis}, total: {total}...): childrenCount: {childrenCount}");
			SetLayoutInputForAxis(totalMin, totalMin, _flexible[axis], axis);
		}

		void SetChildrenAlongAxis(int axis)
		{
			int rowCount = State.GetRowCount();
			int childIndex = 0;
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++) // for each row
			{
				//Debug.Log($"Setting row index: {rowIndex}");
				int columnCountInRow = State.GetColumnCount(rowIndex);

				if (axis == 0)
				{
					//Debug.Log($"SetRowX(offset: {offset}, start: {childIndex}, range: {columnCountInRow})");
					SetRowX(childIndex, columnCountInRow);
				}
				else
				{
					float offset = CalcRowOffsetY(rowIndex, rowCount);

					//Debug.Log($"SetRowY(offset: {offset}, start: {childIndex}, range: {columnCountInRow})");
					SetRowY(offset, childIndex, columnCountInRow);
				}

				childIndex += columnCountInRow;
			}
		}

		void SetRowX(int startChildIndex, int childrenCount)
		{
			float startOffset = CalcRowOffsetX(childrenCount);
			float sumOffset = 0;
			for (int i = 0; i < childrenCount; i++)
			{
				float offset = startOffset + sumOffset;
				RectTransform child = rectChildren[startChildIndex + i];

				//Debug.Log($"SetChild(axis: 0, rect: {child.name}, offset: {totalOffset}, size: {_childWidth})");
				SetChildAlongAxis(child, 0, offset, _childWidth);

				sumOffset += _childWidth + _spacing;
			}
		}

		void SetRowY(float startOffset, int startChildIndex, int childCount)
		{
			for (int i = 0; i < childCount; i++)
			{
				RectTransform child = rectChildren[i + startChildIndex];

				//Debug.Log($"SetChild(axis: 1, rect: {child.name}, offset: {offset}, size: {_childHeight})");
				SetChildAlongAxis(child, 1, startOffset, _childHeight);
			}
		}

		float CalcRowOffsetX(int columnCount)
		{
			float rowSize = _childWidth * columnCount + Mathf.Max(0, columnCount - 1) * _spacing;
			return CalcOffset(0, rowSize);
		}

		float CalcRowOffsetY(int rowIndex, int rowCount)
		{
			float allRowsSize = _childHeight * rowCount + Mathf.Max(0, rowCount - 1) * _spacing;
			float preRowsSize = _childHeight * rowIndex + Mathf.Max(0, rowIndex - 1) * _spacing;
			return CalcOffset(1, allRowsSize) + preRowsSize + Mathf.Min(1, rowIndex) * _spacing;
		}

		float CalcOffset(int axis, float contentSize)
		{
			float totalPadding = axis == 0 ? padding.horizontal : padding.vertical;
			float prePadding = axis == 0 ? padding.left : padding.top;
			float layoutSize = rectTransform.rect.size[axis];
			float innerSize = layoutSize - totalPadding;
			float alignment = GetAlignmentOnAxis(axis);
			float startOffset = (innerSize - contentSize) * alignment + prePadding;
			return startOffset;
		}
	}
}