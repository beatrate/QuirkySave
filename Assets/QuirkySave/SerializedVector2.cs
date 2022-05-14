using System;

namespace QuirkySave
{
	[Serializable]
	public struct SerializedVector2
	{
		public float X { get; set; }
		public float Y { get; set; }

		public SerializedVector2(float x, float y)
		{
			X = x;
			Y = y;
		}
	}
}