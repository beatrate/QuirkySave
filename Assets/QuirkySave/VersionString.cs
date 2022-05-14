using System;
using System.Text;
using System.Text.RegularExpressions;

namespace QuirkySave
{
	public class InvalidVersionStringException : System.Exception
	{
		public InvalidVersionStringException() : base("Invalid version string")
		{

		}
	}

	[Serializable]
	public struct VersionString
	{
		// A valid version string consists of:
		// <major>.<minor> part (required)
		// .<patch> part (optional)
		// -<prerelease> alphanumeric part (optional)
		// -<build metadata> alphanumeric part (optional)
		private const string VersionPattern = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)(?:\.(0|[1-9]\d*))?(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

		public int Major { get; set; }
		public int Minor { get; set; }
		public int Patch { get; set; }
		public string Prerelease { get; set; }
		public string BuildMetadata { get; set; }

		public static VersionString Empty
		{
			get => new VersionString
			{
				Major = 0,
				Minor = 0,
				Patch = 0,
				Prerelease = "",
				BuildMetadata = ""
			};
		}

		public static bool TryParse(string content, out VersionString versionString)
		{
			var match = Regex.Match(content, VersionPattern);
			if(!match.Success)
			{
				versionString = VersionString.Empty;

				return false;
			}

			string majorCapture = match.Groups[1].Value;
			string minorCapture = match.Groups[2].Value;
			string patchCapture = match.Groups[3].Value;
			string prereleaseCapture = match.Groups[4].Value;
			string buildMetadataCapture = match.Groups[5].Value;

			int major = majorCapture.Length == 0 ? 0 : int.Parse(majorCapture);
			int minor = minorCapture.Length == 0 ? 0 : int.Parse(minorCapture);
			int patch = patchCapture.Length == 0 ? 0 : int.Parse(patchCapture);
			string prerelease = prereleaseCapture;
			string buildMetadata = buildMetadataCapture;

			versionString = new VersionString
			{
				Major = major,
				Minor = minor,
				Patch = patch,
				Prerelease = prerelease,
				BuildMetadata = buildMetadata
			};

			return true;
		}

		public static VersionString Parse(string content)
		{
			if(!TryParse(content, out VersionString version))
			{
				throw new InvalidVersionStringException();
			}

			return version;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			builder.Append(Major.ToString());
			builder.Append(".");
			builder.Append(Minor.ToString());
			builder.Append(".");
			builder.Append(Patch.ToString());
			if(Prerelease.Length != 0)
			{
				builder.Append("-");
				builder.Append(Prerelease);
			}

			if(BuildMetadata.Length != 0)
			{
				builder.Append("+");
				builder.Append(BuildMetadata);
			}

			return builder.ToString();
		}
	}
}