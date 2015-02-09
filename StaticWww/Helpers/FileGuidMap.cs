using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.Hosting;

namespace StaticWww
{
	public class FileGuidMap : IFileGuidMap
	{
		private readonly string _physicalRoot;
		private readonly string _scanVirtualRoot;
		private readonly ConcurrentDictionary<Guid, string> _files = new ConcurrentDictionary<Guid, string>();
		private static readonly bool _isWindows = Path.PathSeparator == '\\';

		public FileGuidMap(string applicationPhysicalRoot, string scanVirtualRoot)
		{
			_scanVirtualRoot = EnsureNoTrailingSeparator(scanVirtualRoot);
			_physicalRoot = NormalizePhysicalPath(EnsureNoTrailingSeparator(applicationPhysicalRoot));

			this.EnumerateFiles = physicalPath => Directory.EnumerateFiles(physicalPath, "*.*", SearchOption.AllDirectories);
		}

		private static string EnsureNoTrailingSeparator(string path)
		{
			if (path.EndsWith(Path.PathSeparator.ToString()))
			{
				return path.Substring(0, path.Length-1);
			}

			return path;
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
			return NormalizePhysicalPath(_physicalRoot + virtualPath);
		}

		private static readonly Regex _guidRegex = new Regex(@"\-hc([a-f0-9]{32})", RegexOptions.IgnoreCase);

		internal static Guid ExtractGuid(string physicalPath)
		{
			Match match = _guidRegex.Match(physicalPath);
			if (match.Success)
			{
				return new Guid(match.Groups[1].Captures[0].Value);
			}
			return Guid.Empty;
		}

		private readonly object _lock = new object();

		public Func<string, IEnumerable<string>> EnumerateFiles { private get; set; }


		public void UpdateFiles()
		{
			lock (_lock)
			{
				foreach (string physicalPath in this.EnumerateFiles(this.GetPhysicalPath(_scanVirtualRoot)))
				{
					UpdateFile(physicalPath);
				}
			}
		}

		private void UpdateFile(string physicalPath)
		{
			Guid guid = ExtractGuid(physicalPath);
			if (guid != Guid.Empty)
			{
				_files[guid] = GetVirtualPath(physicalPath);
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

		private FileSystemWatcher _watcher;

		public void WatchFiles()
		{
			if (_watcher != null)
			{
				throw new Exception("WatchFiles() was already called for this FileGuidMap");
			}
			_watcher = new FileSystemWatcher(this.GetPhysicalPath(_scanVirtualRoot), "*-hc*.*");
			_watcher.Created += (sender, e) => this.UpdateFile(e.FullPath);
			_watcher.EnableRaisingEvents = true;
		}
	}
}

