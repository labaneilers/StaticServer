using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace StaticWwwHelpers
{
	internal class ManifestEntryTree
	{
		public string Name { get; set; }
		public ManifestEntry ManifestEntry { get; set; }

		public ManifestEntryTree Parent { get; set; }
		public IDictionary<string, ManifestEntryTree> Children { get; set; }

		/// <summary>
		/// Creates a directory node
		/// </summary>
		public ManifestEntryTree(ManifestEntryTree parent, string directoryName) 
			: this()
		{
			this.Parent = parent;
			this.Name = directoryName;
		}

		/// <summary>
		/// Creates a root node
		/// </summary>
		public ManifestEntryTree()
		{
			this.Children = new Dictionary<string, ManifestEntryTree>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Creates a file (leaf) node
		/// </summary>
		public ManifestEntryTree(ManifestEntryTree parent, ManifestEntry manifestEntry)
		{
			this.Name = VirtualPathUtility.GetFileName(manifestEntry.Path);
			this.ManifestEntry = manifestEntry;
			this.Parent = parent;
		}

		public string GetFullPath()
		{
			var list = new List<ManifestEntryTree>();
			ManifestEntryTree currentNode = this;
			while (currentNode != null)
			{
				list.Add(currentNode);
				currentNode = currentNode.Parent;
			}

			var sb = new StringBuilder();
			foreach (ManifestEntryTree node in Enumerable.Reverse(list))
			{
				sb.Append(node.Name);
				if (node.ManifestEntry == null)
				{
					sb.Append('/');
				}
			}

			return sb.ToString();
		}

		public ManifestEntryTree GetNode(string path)
		{
			string[] parts = SplitPath(path);

			ManifestEntryTree currentNode = this;
			foreach (string part in parts)
			{
				ManifestEntryTree childNode;
				if (!currentNode.Children.TryGetValue(part, out childNode))
				{
					return null;
				}

				currentNode = childNode;
			}

			return currentNode;
		}

		public IEnumerable<ManifestEntryTree> ListContents(bool recursive)
		{
			foreach (KeyValuePair<string, ManifestEntryTree> child in this.Children)
			{
				yield return child.Value;

				if (recursive && child.Value.Children != null)
				{
					foreach (var subChild in child.Value.ListContents(true))
					{
						yield return subChild;
					}
				}
			}
		}

		private static readonly string[] EMPTY_STRING_ARRAY = { };

		private static string[] SplitPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return EMPTY_STRING_ARRAY;
			}
			return path.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries);
		}

		private static void Create(ManifestEntryTree root, ManifestEntry entry)
		{
			string[] parts = SplitPath(entry.Path);

			ManifestEntryTree currentNode = root;
			for (int i=0; i<parts.Length; i++)
			{
				string part = parts[i];

				ManifestEntryTree child;
				if (!currentNode.Children.TryGetValue(part, out child))
				{
					child = i==(parts.Length-1) ? new ManifestEntryTree(currentNode, entry) : new ManifestEntryTree(currentNode, part);
					currentNode.Children[part] = child;
				}

				currentNode = child;
			}
		}

		public static ManifestEntryTree Build(IEnumerable<ManifestEntry> entries)
		{
			var root = new ManifestEntryTree();
			foreach (ManifestEntry entry in entries)
			{
				Create(root, entry);
			}

			return root;
		}
	}
}