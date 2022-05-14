namespace QuirkySave
{
	public interface ISavedComponent
	{
		bool ShouldSave();
		void Save();
		void Load();
	}
}