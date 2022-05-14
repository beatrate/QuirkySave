namespace QuirkySave
{
	public interface ISavedEntity
	{
		SaveIdentityId Identity { get; }

		void Load(SaveEntityInstance instance);
		void Save(SaveEntityInstance instance);
	}
}