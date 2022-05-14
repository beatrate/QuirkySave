namespace QuirkySave
{
	public abstract class SaveSerializer
	{
		public abstract string Save(VersionString version, SaveProfile profile);
		public abstract SaveProfile Load(VersionString version, string content);
	}
}