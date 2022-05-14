using System;

namespace QuirkySave
{
	[Serializable]
	public struct SerializedVector3
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public SerializedVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}