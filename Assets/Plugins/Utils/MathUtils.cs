using UnityEngine;

namespace Utils
{
	public static class MathUtils
	{
		public static float Mod(float a, float b)
		{
			return a - b * Mathf.Floor(a / b);
		}
	}
}