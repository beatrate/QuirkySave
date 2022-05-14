using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace QuirkySave
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(-1000)]
	public class SavedEntity : MonoBehaviour, ISavedEntity, ISerializationCallbackReceiver
	{
		public SaveIdentityId Identity => identity;
		[SerializeField]
		private SaveIdentityId identity = new SaveIdentityId { Kind = SaveIdentityKind.Guid };

		private List<ISavedComponent> savedComponents = new List<ISavedComponent>();

		public bool IsSaving { get; private set; } = false;
		public bool IsLoading { get; private set; } = false;

		public void Awake()
		{
			CreateGuid();

			if(!Application.IsPlaying(gameObject))
			{
				return;
			}

			GetComponentsInChildren<ISavedComponent>(includeInactive: true, savedComponents);
		}

		public void Start()
		{
			if(!Application.IsPlaying(gameObject))
			{
				return;
			}

			SaveSystem.Instance.Register(this);
		}

		public void OnDestroy()
		{
			SavedEntityManager.Instance.Unregister(this);

			if(!Application.IsPlaying(gameObject))
			{
				return;
			}

			if(SaveSystem.Instance != null)
			{
				SaveSystem.Instance.Unregister(this);
			}
		}

		public void OnValidate()
		{
			if(identity.Kind != SaveIdentityKind.Guid)
			{
				return;
			}

#if UNITY_EDITOR
			if(IsAssetOnDisk())
			{
				identity.Identifier = string.Empty;
			}
			else
			{
				CreateGuid();
			}
#else
			CreateGuid();
#endif
		}

		public void Load(SaveEntityInstance instance)
		{
			IsLoading = true;

			for(int componentIndex = 0; componentIndex < savedComponents.Count; ++componentIndex)
			{
				ISavedComponent component = savedComponents[componentIndex];
				if(component == null)
				{
					continue;
				}

				Type componentType = component.GetType();

				int saveComponentIndex = instance.Components.FindIndex(c => c.Name == componentType.Name);
				if(saveComponentIndex == -1)
				{
					continue;
				}

				SaveEntityComponent saveComponent = instance.Components[saveComponentIndex];

				foreach(FieldInfo field in SaveSystemUtility.GetSavedFields(componentType))
				{
					int saveFieldIndex = saveComponent.Fields.FindIndex(f => f.Name == field.Name);
					if(saveFieldIndex == -1)
					{
						continue;
					}

					SaveField saveField = saveComponent.Fields[saveFieldIndex];
					SaveSystemUtility.LoadValueInto(component, field, saveField);
				}

				component.Load();
			}

			IsLoading = false;
		}

		public void Save(SaveEntityInstance instance)
		{
			IsSaving = true;

			for(int componentIndex = 0; componentIndex < savedComponents.Count; ++componentIndex)
			{
				ISavedComponent component = savedComponents[componentIndex];
				if(component == null || !component.ShouldSave())
				{
					continue;
				}

				Type componentType = component.GetType();

				int saveComponentIndex = instance.Components.FindIndex(c => c.Name == componentType.Name);
				SaveEntityComponent saveComponent;

				if(saveComponentIndex == -1)
				{
					saveComponent = new SaveEntityComponent
					{
						Name = componentType.Name,
						Fields = new List<SaveField>()
					};
					instance.Components.Add(saveComponent);
				}
				else
				{
					saveComponent = instance.Components[saveComponentIndex];
				}

				saveComponent.Fields.Clear();
				component.Save();

				foreach(FieldInfo field in SaveSystemUtility.GetSavedFields(componentType))
				{
					SaveField saveField = new SaveField
					{
						Name = field.Name
					};
					saveComponent.Fields.Add(saveField);

					SaveSystemUtility.SaveValueFrom(component, field, saveField);
				}
			}

			IsSaving = false;
		}

		public void ForceSave()
		{
			SaveSystem.Instance.ForceSave(this);
		}

		private void CreateGuid()
		{
			if(identity.Kind != SaveIdentityKind.Guid)
			{
				return;
			}

			if(identity.Identifier == null || identity.Identifier.Length != 32 || !Guid.TryParse(identity.Identifier, out _))
			{
#if UNITY_EDITOR
				if(IsAssetOnDisk())
				{
					return;
				}

				Undo.RecordObject(this, "Added SavedEntity Guid");
#endif

				identity.Identifier = System.Guid.NewGuid().ToString("N");

#if UNITY_EDITOR
				// If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
				// force a save of the modified prefab instance properties.
				if(PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
				{
					PrefabUtility.RecordPrefabInstancePropertyModifications(this);
				}
#endif
			}

			var result = SavedEntityManager.Instance.Register(this);
			switch(result)
			{
				case SavedEntityRegisterResult.RegistrationNotNecessary:
				case SavedEntityRegisterResult.RegisteredSuccess:
					break;
				case SavedEntityRegisterResult.RegistrationFail:
					identity.Identifier = string.Empty;
					CreateGuid();
					break;
			}
		}

#if UNITY_EDITOR
		private bool IsEditingInPrefabMode()
		{
			if(EditorUtility.IsPersistent(this))
			{
				// If the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset.
				return true;
			}
			else
			{
				// If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it.
				var mainStage = StageUtility.GetMainStageHandle();
				var currentStage = StageUtility.GetStageHandle(gameObject);
				if(currentStage != mainStage)
				{
					var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
					if(prefabStage != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsAssetOnDisk()
		{
			return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
		}
#endif

		public void OnBeforeSerialize()
		{
			if(identity.Kind != SaveIdentityKind.Guid)
			{
				return;
			}

#if UNITY_EDITOR
			if(IsAssetOnDisk())
			{
				identity.Identifier = string.Empty;
			}
#endif
		}

		public void OnAfterDeserialize()
		{

		}
	}
}