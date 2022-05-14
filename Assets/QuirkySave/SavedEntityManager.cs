using System.Collections.Generic;
using UnityEngine;

namespace QuirkySave
{
	public enum SavedEntityRegisterResult
	{
		RegistrationNotNecessary,
		RegistrationFail,
		RegisteredSuccess
	}

	public class SavedEntityManager
	{
		private struct EntityInfo
		{
			public SavedEntity Entity;
		}

		private Dictionary<SaveIdentityId, EntityInfo> identityToEntity = new Dictionary<SaveIdentityId, EntityInfo>();

		public static SavedEntityManager Instance
		{
			get
			{
				if(instance == null)
				{
					instance = new SavedEntityManager();
				}

				return instance;
			}
		}
		private static SavedEntityManager instance = null;

		public SavedEntityRegisterResult Register(SavedEntity entity)
		{
			if(!entity.Identity.IsValid() || entity.Identity.Kind != SaveIdentityKind.Guid)
			{
				return SavedEntityRegisterResult.RegistrationNotNecessary;
			}

			if(!identityToEntity.TryGetValue(entity.Identity, out EntityInfo info))
			{
				info = new EntityInfo
				{
					Entity = entity
				};

				identityToEntity.Add(entity.Identity, info);
				return SavedEntityRegisterResult.RegisteredSuccess;
			}

			if(info.Entity != null && info.Entity != entity)
			{
				Debug.LogWarning($"Identity collision between {info.Entity.gameObject.name} and {entity.gameObject.name}", entity.gameObject);

				return SavedEntityRegisterResult.RegistrationFail;
			}

			info.Entity = entity;
			identityToEntity[entity.Identity] = info;
			return SavedEntityRegisterResult.RegisteredSuccess;
		}

		public void Unregister(SavedEntity entity)
		{
			identityToEntity.Remove(entity.Identity);
		}
	}
}