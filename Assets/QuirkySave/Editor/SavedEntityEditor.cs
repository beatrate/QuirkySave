using UnityEditor;
using UnityEngine;

namespace QuirkySave
{
	[CustomEditor(typeof(SavedEntity))]
	public class SavedEntityEditor : Editor
	{
		private SerializedProperty identity = null;

		public void OnEnable()
		{
			identity = serializedObject.FindProperty("identity");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("SAVED ENTITY");

			var kind = identity.FindPropertyRelative("kind");
			var identifier = identity.FindPropertyRelative("identifier");
			
			using(var horizontal = new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Identity", GUILayout.Width(45.0f));
				EditorGUILayout.PropertyField(kind, GUIContent.none, GUILayout.Width(70.0f));
				SaveIdentityKind kindValue = (SaveIdentityKind)kind.intValue;

				if(kindValue == SaveIdentityKind.Guid)
				{
					GUI.enabled = false;

					EditorGUILayout.PropertyField(identifier, GUIContent.none);

					GUI.enabled = true;
				}
				else
				{
					EditorGUILayout.PropertyField(identifier, GUIContent.none);
				}
			}

			serializedObject.ApplyModifiedProperties();

			var entity = target as SavedEntity;
			if(!entity.Identity.IsValid())
			{
				EditorGUILayout.HelpBox("Identity is invalid", MessageType.Warning, true);
			}
		}
	}
}

