using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace QuirkySave
{
	public class JsonSaveSerializer : SaveSerializer
	{
		private static List<Type> converterTypes = null;
		private JsonSerializerSettings settings = null;

		static JsonSaveSerializer()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Assert.AreEqual("Assembly-CSharp", assembly.GetName().Name, "Unexpected main assembly");

			converterTypes = assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && typeof(JsonConverter).IsAssignableFrom(t))
				.ToList();
		}

		public JsonSaveSerializer()
		{
			var converters = new List<JsonConverter>();

			foreach(Type type in converterTypes)
			{
				converters.Add((JsonConverter)Activator.CreateInstance(type));
			}

			settings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				Converters = converters
			};
		}

		public override string Save(VersionString version, SaveProfile profile)
		{
			string serializedProfile = JsonConvert.SerializeObject(profile, settings);
			return serializedProfile;
		}

		public override SaveProfile Load(VersionString version, string content)
		{
			var serializer = JsonSerializer.Create(settings);
			JObject jsonObject = JObject.Parse(content);
			var jsonInstances = jsonObject[$"{nameof(SaveProfile.EntityInstances)}"].Children().ToList();
			var savedTypes = SaveSystemUtility.GetSavedComponentTypes();
			var profile = new SaveProfile
			{
				EntityInstances = new List<SaveEntityInstance>()
			};

			foreach(JToken jsonInstance in jsonInstances)
			{
				SaveIdentityId identity = jsonInstance[nameof(SaveEntityInstance.Identity)].ToObject<SaveIdentityId>(serializer);
				var instance = new SaveEntityInstance
				{
					Identity = identity,
					Components = new List<SaveEntityComponent>()
				};

				var jsonComponents = jsonInstance[nameof(SaveEntityInstance.Components)].Children().ToList();
				foreach(var jsonComponent in jsonComponents)
				{
					string componentName = jsonComponent[nameof(SaveEntityComponent.Name)].ToObject<string>(serializer);
					Type componentType = savedTypes.Find(t => t.Name == componentName);
					if(componentType == null)
					{
						Debug.LogWarning($"Saved component type {componentName} not found");
						continue;
					}

					var component = new SaveEntityComponent
					{
						Name = componentName,
						Fields = new List<SaveField>()
					};

					var savedFieldsOfType = SaveSystemUtility.GetSavedFields(componentType);
					var jsonFields = jsonComponent[nameof(SaveEntityComponent.Fields)].Children().ToList();

					foreach(JToken jsonField in jsonFields)
					{
						string fieldName = jsonField[nameof(SaveField.Name)].ToObject<string>(serializer);
						var savedFieldOfType = savedFieldsOfType.Find(f => f.Name == fieldName);
						if(savedFieldOfType == null)
						{
							continue;
						}

						Type fieldType = savedFieldOfType.FieldType;
						JToken jsonFieldValue = jsonField[nameof(SaveField.Value)];
						object fieldValue = jsonFieldValue.ToObject(fieldType, serializer);

						var saveField = new SaveField
						{
							Name = fieldName,
							Value = fieldValue
						};
						component.Fields.Add(saveField);
					}

					instance.Components.Add(component);
				}

				profile.EntityInstances.Add(instance);
			}

			return profile;
		}
	}
}