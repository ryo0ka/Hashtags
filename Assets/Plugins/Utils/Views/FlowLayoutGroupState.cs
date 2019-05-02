using UnityEngine;

namespace Utils.Views
{
	public class FlowLayoutGroupState
	{
		readonly int[] _rows;
		int _contentCount;

		public FlowLayoutGroupState(int maxRowCount)
		{
			_rows = new int[maxRowCount];
		}

		public int GetMaxRowCount()
		{
			return _rows.Length;
		}

		public void Initialize(int contentCount)
		{
			if (contentCount == _contentCount) return;
			_contentCount = contentCount;

			int sumCount = 0;
			for (int rowIndex = 0; rowIndex < _rows.Length; rowIndex++)
			{
				int count = Ceil(_contentCount - sumCount, _rows.Length - rowIndex);

				// Deal with the special case where the layout could look ugly
				// due to multiple rows having one column (looking stretched vertically).
				// If two rows would have only one column, let 1st row have two columns, 2nd zero.
				int nextCount = Ceil(_contentCount - sumCount - count, _rows.Length - rowIndex - 1);
				if (count == 1 && nextCount == 1)
				{
					count = 2;
				}

				sumCount += count;
				_rows[rowIndex] = count;
			}
		}

		// helper to halve code length
		static int Ceil(float a, float b) => Mathf.CeilToInt(a / b);

		public int GetRowCount()
		{
			int count;
			for (count = 0; count < _rows.Length; count++)
			{
				if (_rows[count] == 0) break;
			}

			return count;
		}

		public int GetMaxColumnCount()
		{
			return GetColumnCount(0);
		}

		public int GetColumnCount(int rowIndex)
		{
			return _rows[rowIndex];
		}
	}
}