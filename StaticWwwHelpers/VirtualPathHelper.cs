using System;

namespace StaticWwwHelpers
{
	/// <summary>
	/// Utilities for manipulating virtual paths for file mapping.
	/// </summary>
	public class VirtualPathHelper
	{
		/// <summary>
		/// Returns true if the specified path is in the specified rootDirectory.
		/// </summary>
		public static bool IsInRootDirectory(string virtualPath, string rootDirectory)
		{
			if (string.IsNullOrEmpty(virtualPath))
			{
				if (string.IsNullOrEmpty(rootDirectory))
				{
					return true;
				}

				return false;
			}

			if (string.IsNullOrEmpty(rootDirectory) || rootDirectory == "/")
			{
				return true;
			}

			if (virtualPath.StartsWith(rootDirectory, StringComparison.InvariantCultureIgnoreCase))
			{
				if (virtualPath.Length == rootDirectory.Length)
				{
					return true;
				}

				if (virtualPath[rootDirectory.Length] == '/')
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the root directory from a path.
		/// For example, GetRootDirectory("/a/b/c") would return "/a"
		/// </summary>
		public static string GetRootDirectory(string virtualPath)
		{
			if (string.IsNullOrWhiteSpace(virtualPath))
			{
				return string.Empty;
			}

			int secondSlash = virtualPath.IndexOf("/", 1, StringComparison.Ordinal);
			if (secondSlash <= 0)
			{
				// If the path contains only one section (i.e. /something.aspx),
				// it may be a file in the root (/something.aspx) or a directory root (/something)

				// If it doesn't contain a ".", treat it as a directory.
				if (virtualPath.IndexOf(".", 1, StringComparison.Ordinal) < 0)
				{
					secondSlash = virtualPath.Length;
				}

				if (secondSlash <= 1)
				{
					return string.Empty;
				}
			}

			return virtualPath.Substring(0, secondSlash);
		}

		/// <summary>
		/// Given a virtual path and a root directory, strips the root directory and returns the unrooted path.
		/// </summary>
		/// <param name="virtualPath">The virtual path (i.e. /foo/bar/baz)</param>
		/// <param name="rootVirtualDir">The root directory (i.e. /foo)</param>
		/// <param name="validateRoot">If true, the virtual path is checked to ensure it is in the root path before it is processed. 
		/// Only pass false if the caller has already performed this validation (as an optimization)</param>
		/// <returns>A striped path. For example, for "/foo/bar/baz", "/foo", the stripped path would be "/bar/baz"</returns>
		public static string StripRoot(string virtualPath, string rootVirtualDir, bool validateRoot = true)
		{
			if (string.IsNullOrEmpty(virtualPath))
			{
				return null;
			}

			if (!string.IsNullOrEmpty(rootVirtualDir))
			{
				if (validateRoot && !IsInRootDirectory(virtualPath, rootVirtualDir))
				{
					return null;
				}

				return virtualPath.Substring(rootVirtualDir.Length);
			}

			return virtualPath;
		}

		/// <summary>
		/// Returns true if the paths match.
		/// </summary>
		public static bool Matches(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
	}
}