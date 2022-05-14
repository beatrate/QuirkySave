using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace QuirkySave
{
	public class SaveSystem : MonoBehaviour
	{
		private class CachedSaveProfile
		{
			public Dictionary<SaveIdentityId, SaveEntityInstance> EntityInstances { get; set; }
		}

		private class SaveRequest
		{
			public VersionString Version { get; set; }
			public string SerializedProfile { get; set; }
		}

		private const string TemporarySaveFileName = "tempsave.save";
		private const string SaveFileName = "save.save";

		private List<ISavedEntity> savedEntities = new List<ISavedEntity>();

		private Task saveTask = null;
		private Queue<SaveRequest> queuedSaveRequests = new Queue<SaveRequest>();
		private CachedSaveProfile cachedProfile = new CachedSaveProfile
		{
			EntityInstances = new Dictionary<SaveIdentityId, SaveEntityInstance>()
		};

		public static SaveSystem Instance { get; private set; } = null;

		public bool HasSave { get; private set; } = false;

		public void Awake()
		{
			Instance = this;
		}

		public void Update()
		{
			if(saveTask == null && queuedSaveRequests.Count != 0)
			{
				var request = queuedSaveRequests.Dequeue();
				string saveFilePath = GetSaveFilePath();
				string temporarySaveFilePath = GetTemporarySaveFilePath();
				saveTask = Task.Run(() => SaveSync(request, saveFilePath, temporarySaveFilePath));
			}

			if(saveTask != null && saveTask.Status != TaskStatus.Running)
			{
				if(saveTask.IsFaulted)
				{
					Debug.LogException(saveTask.Exception);
				}

				saveTask = null;
			}
		}

		public bool IsSavingInProgress()
		{
			return saveTask != null;
		}

		public void Load()
		{
			try
			{
				string saveFilePath = GetSaveFilePath();

				VersionString version = VersionString.Empty;
				SaveProfile profile = null;

				if(File.Exists(saveFilePath))
				{
					using(var stream = new StreamReader(saveFilePath, encoding: System.Text.Encoding.UTF8))
					{
						string versionText = stream.ReadLine();
						version = VersionString.Parse(versionText);

						string saveContent = stream.ReadToEnd();
						SaveSerializer serializer = GetSerialzier();
						profile = serializer.Load(version, saveContent);
					}
				}
				else
				{
					version = VersionString.Parse(Application.version);
				}

				if(profile == null)
				{
					HasSave = false;

					profile = new SaveProfile
					{
						EntityInstances = new List<SaveEntityInstance>()
					};
				}
				else
				{
					HasSave = true;
				}

				var instanceDictionary = new Dictionary<SaveIdentityId, SaveEntityInstance>();
				for(int instanceIndex = 0; instanceIndex < profile.EntityInstances.Count; ++instanceIndex)
				{
					SaveEntityInstance instance = profile.EntityInstances[instanceIndex];
					instanceDictionary.Add(instance.Identity, instance);
				}

				cachedProfile = new CachedSaveProfile
				{
					EntityInstances = instanceDictionary
				};

				Debug.Log($"Loaded save version {version}");
			}
			catch(Exception e)
			{
				Debug.LogWarning("Loading failed");
				Debug.LogException(e);

				cachedProfile = new CachedSaveProfile
				{
					EntityInstances = new Dictionary<SaveIdentityId, SaveEntityInstance>()
				};
			}
		}

		public void Save()
		{
			try
			{
				for(int entityIndex = 0; entityIndex < savedEntities.Count; ++entityIndex)
				{
					ISavedEntity entity = savedEntities[entityIndex];
					Save(entity);
				}

				var profile = new SaveProfile()
				{
					EntityInstances = new List<SaveEntityInstance>()
				};

				profile.EntityInstances.AddRange(cachedProfile.EntityInstances.Values);

				VersionString version = VersionString.Parse(Application.version);
				SaveSerializer serializer = GetSerialzier();
				string serializedProfile = serializer.Save(version, profile);

				var request = new SaveRequest()
				{
					Version = version,
					SerializedProfile = serializedProfile
				};

				queuedSaveRequests.Enqueue(request);
				HasSave = true;

				Debug.Log("Saving");
			}
			catch(Exception e)
			{
				Debug.LogWarning("Saving failed");
				Debug.LogException(e);
			}
		}

		public void ForceSave(ISavedEntity entity)
		{
			Save(entity);
		}

		public void DeleteSave()
		{
			try
			{
				string saveFilePath = GetSaveFilePath();
				File.Delete(saveFilePath);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}

			cachedProfile = new CachedSaveProfile
			{
				EntityInstances = new Dictionary<SaveIdentityId, SaveEntityInstance>()
			};
			HasSave = false;
		}

		public void Register(ISavedEntity entity)
		{
			savedEntities.Add(entity);
			Load(entity);
		}

		public void Unregister(ISavedEntity entity)
		{
			savedEntities.Remove(entity);
		}

		private void Save(ISavedEntity entity)
		{
			Assert.IsNotNull(cachedProfile);

			SaveEntityInstance instance = null;

			if(!entity.Identity.IsValid())
			{
				instance = new SaveEntityInstance
				{
					Identity = entity.Identity,
					Components = new List<SaveEntityComponent>()
				};

				if(entity is MonoBehaviour behaviour)
				{
					Debug.LogWarning($"{behaviour.gameObject.name} has invalid identity");
				}
			}
			else
			{
				if(cachedProfile.EntityInstances.TryGetValue(entity.Identity, out instance))
				{
					instance.Components.Clear();
				}
				else
				{
					instance = new SaveEntityInstance
					{
						Identity = entity.Identity,
						Components = new List<SaveEntityComponent>()
					};

					cachedProfile.EntityInstances.Add(entity.Identity, instance);
				}
			}

			entity.Save(instance);
		}

		private void Load(ISavedEntity entity)
		{
			Assert.IsNotNull(cachedProfile);

			if(!cachedProfile.EntityInstances.TryGetValue(entity.Identity, out SaveEntityInstance instance))
			{
				instance = new SaveEntityInstance
				{
					Identity = entity.Identity,
					Components = new List<SaveEntityComponent>()
				};
			}

			entity.Load(instance);
		}

		private string GetSaveFilePath()
		{
			string saveFilePath = Application.persistentDataPath + "/" + SaveFileName;
			return saveFilePath;
		}

		private string GetTemporarySaveFilePath()
		{
			string saveFilePath = Application.persistentDataPath + "/" + TemporarySaveFileName;
			return saveFilePath;
		}

		private void SaveSync(SaveRequest request, string saveFilePath, string temporarySaveFilePath)
		{
			try
			{
				using(var stream = new StreamWriter(temporarySaveFilePath, append: false, encoding: System.Text.Encoding.UTF8))
				{
					stream.WriteLine(request.Version.ToString());
					stream.Write(request.SerializedProfile);
				}

				File.Delete(saveFilePath);
				File.Move(temporarySaveFilePath, saveFilePath);

				Debug.Log($"Saved to {saveFilePath}");
			}
			catch(Exception e)
			{
				Debug.LogWarning("Saving failed");
				Debug.LogException(e);
			}
		}

		private SaveSerializer GetSerialzier()
		{
			return new JsonSaveSerializer();
		}
	}
}