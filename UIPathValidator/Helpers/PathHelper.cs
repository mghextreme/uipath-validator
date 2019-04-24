using System;
using System.IO;

namespace UIPathValidator
{
    public static class PathHelper
    {
        public static string MakeRelativePath(string path, string fromPath)
        {
            if (string.IsNullOrEmpty(path))   throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");

            Uri uri = new Uri(path);
            Uri fromUri = new Uri(fromPath);

            if (fromUri.Scheme != uri.Scheme) { return path; }

            Uri relativeUri = fromUri.MakeRelativeUri(uri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (uri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return relativePath;
        }
    }
}