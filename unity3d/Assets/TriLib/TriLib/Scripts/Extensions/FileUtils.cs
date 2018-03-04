using UnityEngine;
using System.Collections;

namespace TriLib {
    public static class FileUtils
    {
        public static string GetFileDirectory(string filename) {
            var lastDash = filename.LastIndexOf('/');
            return filename.Substring(0, lastDash);
        }

        public static string GetFilenameWithoutExtension(string filename) {
            var lastDash = filename.LastIndexOf('/');
            var lastDot = filename.LastIndexOf('.');
            return filename.Substring(lastDash, lastDot);
        }       
    }
}

