using System.Collections.Generic;
using System;

namespace QuirkySave
{
	[Serializable]
	public class SaveProfile
	{
		public List<SaveEntityInstance> EntityInstances { get; set; }
	}
}