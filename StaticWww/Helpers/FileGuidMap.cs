using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace StaticWww
{
	public class FileGuidMap
	{
		private readonly string _physicalRoot;
		private readonly ConcurrentDictionary<Guid, string> _files = new ConcurrentDictionary<Guid, string>();
		private static readonly bool _isWindows = Path.PathSeparator == '\\';

		public FileGuidMap(string physicalRoot)
		{
			_physicalRoot = physicalRoot;
			this.EnumerateFiles = physicalPath => Directory.EnumerateFiles(physicalPath, "*.*", SearchOption.AllDirectories);
		}

		private static string NormalizeVirtualPath(string path)
		{
			if (_isWindows)
			{
				return path.Replace("\\", "/");
			}

			return path;
		}

		private static string NormalizePhysicalPath(string path)
		{
			if (_isWindows)
			{
				return path.Replace("/", "\\");
			}

			return path;
		}

		internal string GetVirtualPath(string physicalPath)
		{
			return NormalizeVirtualPath(physicalPath.Substring(_physicalRoot.Length));
		}

		internal string GetPhysicalPath(string virtualPath)
		{
			return NormalizePhysicalPath(Path.Combine(_physicalRoot, virtualPath));
		}

		private static readonly Regex _guidRegex = new Regex(@"\-hc([a-f0-9]{32})", RegexOptions.IgnoreCase);

		internal static Guid ExtractGuid(string physicalPath)
		{
			Match match = _guidRegex.Match(physicalPath);
			return new Guid(match.Groups[1].Captures[0].Value);
		}

		private readonly object _lock = new object();

		public Func<string, IEnumerable<string>> EnumerateFiles { private get; set; }


		public void UpdateFiles()
		{
			lock (_lock)
			{
				foreach (string physicalPath in this.EnumerateFiles(_physicalRoot))
				{
					Guid guid = ExtractGuid(physicalPath);
					_files[guid] = GetVirtualPath(physicalPath);
				}
			}
		}

		public string Get(Guid guid)
		{
			string virtualPath;
			if (_files.TryGetValue(guid, out virtualPath))
			{
				return virtualPath;
			}

			return null;
		}
	}
}

