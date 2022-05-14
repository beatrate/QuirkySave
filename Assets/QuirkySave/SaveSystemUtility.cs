using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace QuirkySave
{
	public static class SaveSystemUtility
	{
		private static List<Type> savedComponentTypes = null;
		private static Dictionary<Type, List<FieldInfo>> savedComponentTypeFields = null;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			savedComponentTypes = FindSavedComponentTypes();
			
			savedComponentTypeFields = new Dictionary<Type, List<FieldInfo>>();
			foreach(Type type in savedComponentTypes)
			{
				savedComponentTypeFields.Add(type, FindSavedFields(type));
			}
		}

		public static List<Type> GetSavedComponentTypes()
		{
			return savedComponentTypes;
		}

		public static List<FieldInfo> GetSavedFields(Type type)
		{
			if(savedComponentTypeFields.TryGetValue(type, out List<FieldInfo> fields))
			{
				return fields;
			}

			return new List<FieldInfo>();
		}

		public static void LoadValueInto(object o, FieldInfo field, SaveField saveField)
		{
			Type type = field.FieldType;
			object value = saveField.Value;
			field.SetValue(o, value);
		}

		public static void SaveValueFrom(object o, FieldInfo field, SaveField saveField)
		{
			Type type = field.FieldType;
			object value = field.GetValue(o);
			saveField.Value = value;
		}

		public static void SafeDestroy(GameObject o)
		{
			if(o.TryGetComponent(out SavedEntity entity))
			{
				SafeDestroy(entity);
			}
			else
			{
				GameObject.Destroy(entity.gameObject);
			}
		}

		public static void SafeDestroy(SavedEntity entity)
		{
			entity.ForceSave();
			GameObject.Destroy(entity.gameObject);
		}

		private static List<Type> FindSavedComponentTypes()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			Assert.AreEqual("Assembly-CSharp", assembly.GetName().Name, "Unexpected main assembly");

			var savedComponentTypes = assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && typeof(ISavedComponent).IsAssignableFrom(t))
				.ToList();

			return savedComponentTypes;
		}

		private static List<FieldInfo> FindSavedFields(Type type)
		{
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(f =>
				{
					if((f.Attributes & (FieldAttributes.Static | FieldAttributes.NotSerialized)) != 0)
					{
						return false;
					}

					return f.IsDefined(typeof(SaveFieldAttribute), inherit: true);
				}).ToList();
			
			return fields;
		}
	}
}