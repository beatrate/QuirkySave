using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace QuirkySave
{
	public class Vector2Converter : JsonConverter<Vector2>
	{
		public override Vector2 ReadJson(JsonReader reader, Type objectType, [AllowNull] Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			SerializedVector2 serializedValue = serializer.Deserialize<SerializedVector2>(reader);
			return new Vector2(serializedValue.X, serializedValue.Y);
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Vector2 value, JsonSerializer serializer)
		{
			SerializedVector2 serializedValue = new SerializedVector2(value.x, value.y);
			serializer.Serialize(writer, serializedValue);
		}
	}

	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override Vector3 ReadJson(JsonReader reader, Type objectType, [AllowNull] Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			SerializedVector3 serializedValue = serializer.Deserialize<SerializedVector3>(reader);
			return new Vector3(serializedValue.X, serializedValue.Y, serializedValue.Z);
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Vector3 value, JsonSerializer serializer)
		{
			SerializedVector3 serializedValue = new SerializedVector3(value.x, value.y, value.z);
			serializer.Serialize(writer, serializedValue);
		}
	}

	public class Vector4Converter : JsonConverter<Vector4>
	{
		public override Vector4 ReadJson(JsonReader reader, Type objectType, [AllowNull] Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			SerializedVector4 serializedValue = serializer.Deserialize<SerializedVector4>(reader);
			return new Vector4(serializedValue.X, serializedValue.Y, serializedValue.Z, serializedValue.W);
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Vector4 value, JsonSerializer serializer)
		{
			SerializedVector4 serializedValue = new SerializedVector4(value.x, value.y, value.z, value.w);
			serializer.Serialize(writer, serializedValue);
		}
	}

	public class QuaternionConverter : JsonConverter<Quaternion>
	{
		public override Quaternion ReadJson(JsonReader reader, Type objectType, [AllowNull] Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			SerializedQuaternion serializedValue = serializer.Deserialize<SerializedQuaternion>(reader);
			return new Quaternion(serializedValue.X, serializedValue.Y, serializedValue.Z, serializedValue.W).normalized;
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Quaternion value, JsonSerializer serializer)
		{
			SerializedQuaternion serializedValue = new SerializedQuaternion(value.x, value.y, value.z, value.w);
			serializer.Serialize(writer, serializedValue);
		}
	}
}
