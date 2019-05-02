using UnityEngine;

namespace Utils
{
	public static class MathUtils
	{
		public static float Mod(float a, float b)
		{
			return a - b * Mathf.Floor(a / b);
		}

		public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 t)
		{
			return new Vector3(
				x: Mathf.Lerp(a.x, b.x, t.x),
				y: Mathf.Lerp(a.y, b.y, t.y),
				z: Mathf.Lerp(a.z, b.z, t.z));
		}
	}
}