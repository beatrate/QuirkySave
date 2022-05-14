using System;
using UnityEngine;

namespace QuirkySave
{
	public enum SaveIdentityKind
	{
		Identifier = 0,
		Guid = 1
	}

	[Serializable]
	public struct SaveIdentityId : IEquatable<SaveIdentityId>
	{
		public SaveIdentityKind Kind
		{
			get => kind;
			set => kind = value;
		}
		[SerializeField]
		private SaveIdentityKind kind;

		public string Identifier
		{
			get => identifier;
			set => identifier = value;
		}
		[SerializeField]
		private string identifier;

		public void SetIdentifier(string identifier)
		{
			kind = SaveIdentityKind.Identifier;
			this.identifier = identifier;
		}

		public void SetGuid(string guid)
		{
			kind = SaveIdentityKind.Guid;
			identifier = guid;
		}

		public override string ToString()
		{
			return identifier;
		}

		public override bool Equals(object other)
		{
			return other is SaveIdentityId id && Equals(id);
		}

		public override int GetHashCode()
		{
			return identifier.GetHashCode();
		}

		public bool Equals(SaveIdentityId other)
		{
			if(kind != other.kind)
			{
				return false;
			}

			return identifier == other.identifier;
		}

		public static bool operator ==(SaveIdentityId a, SaveIdentityId b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(SaveIdentityId a, SaveIdentityId b)
		{
			return !(a == b);
		}

		public bool IsValid()
		{
			if(kind == SaveIdentityKind.Identifier)
			{
				return !string.IsNullOrWhiteSpace(identifier);
			}

			return Guid.TryParse(identifier, out _);
		}
	}
}