// .NET
using System;
using System.IO;

namespace XUtils.DirsAndFiles
{
    internal class DirPath
    {
        private string directory = String.Empty;

        public void Clear() { directory = String.Empty; }
        public bool Add(params string[] dirs)
        {
            foreach (string dir in dirs) { directory = Path.Combine(directory, dir); }
            return true;
        }

        public string ActualPath { get { return directory; } }

        public bool IsExist { get { return Directory.Exists(directory); } }
        
        public bool CreateIfNotExist()
        {
            if (Directory.Exists(directory) == false) { Directory.CreateDirectory(directory); }
            return true;
        }

        public bool Delete()
        {
            Directory.Delete(directory, true);
            return true;
        }

        // 'absolutni cesta' -> pokud velikost retezce je minimalne tri znaky, na druhem miste je ':' a na tretim miste je '\'
        public static bool IsAbsolutePath(string path)
        {
            return ((path.Length >= 3) && (path[1] == ':') && (path[2] == '\\'));
        }
    }
    
    public class Dirs
    {        
        public static string CreatePath(params string[] pathParts)
        {
            string pathOut = String.Empty;
            foreach (string pathPart in pathParts) { pathOut = Path.Combine(pathOut, pathPart); }
            return pathOut;
        }
        
        public static string CreateIfNotExist(string directory)
        {
            if (Directory.Exists(directory) == false) { Directory.CreateDirectory(directory); }
            return directory;
        }

        public static string CreateIfNotExist(params string[] dirs)
        {
            string directory = Path.Combine(dirs);
            if (Directory.Exists(directory) == false) { Directory.CreateDirectory(directory); }
            return directory;
        }

        public static string CreateIfNotExist(string dirBase, params string[] dirs)
        {
            //List<string> dirsList = new List<string>(dirs); dirsList.Insert(0, dirBase);
            string directory = dirBase;
            foreach (string dir in dirs) { directory = Path.Combine(directory, dir); }
            if (Directory.Exists(directory) == false) { Directory.CreateDirectory(directory); }
            return directory;
        }

        #if SILVERLIGHT
        #else
          public static void CopyAllFiles(string sourceDir, string targetDir)
          {
            string[] files = System.IO.Directory.GetFiles(sourceDir);
            // Copy the files and overwrite destination files if they already exist.
            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                string fileName = System.IO.Path.GetFileName(s);
                string destFile = System.IO.Path.Combine(targetDir, fileName);
                System.IO.File.Copy(s, destFile, true);
            }

          }
#endif

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        // --- zobarzeni obsahu ---

        // zobrazeni obsahu adresare v 'FileExplorer'
        public static void ShowLocationInFileExplorer(string path)
        {
            System.Diagnostics.Process.Start(path);
        }
    }

    public class Files
    {
        // zobrazeni obsahuje souboru v 'NotePad'
        public static void ShowFileInNotePad(string fileathName)
        {
            System.Diagnostics.Process.Start("notepad.exe", fileathName);
        }
    }
}
