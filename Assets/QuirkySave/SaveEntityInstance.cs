using System.Collections.Generic;
using System;

namespace QuirkySave
{
	[Serializable]
	public class SaveEntityInstance
	{
		public SaveIdentityId Identity { get; set; }
		public List<SaveEntityComponent> Components { get; set; }

		public override string ToString()
		{
			return Identity.ToString();
		}
	}
}