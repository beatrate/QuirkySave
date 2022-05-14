using System;

namespace QuirkySave
{
	[Serializable]
	public class SaveField
	{
		public string Name { get; set; }
		public object Value { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}