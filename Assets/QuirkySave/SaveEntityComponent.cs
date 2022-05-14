using System.Collections.Generic;
using System;

namespace QuirkySave
{
	[Serializable]
	public class SaveEntityComponent
	{
		public string Name { get; set; }
		public List<SaveField> Fields { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}