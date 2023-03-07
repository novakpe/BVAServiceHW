// .NET
using System;

namespace XUtils.DirsAndFiles
{
    internal class SpecialDirs
    {
        public enum EnumLocationType
        {
            None,                                                   // nedefinovano
            StartUpExe,                                             // tam kde je spousteci EXE soubor (pri volani jinout aplikaci)
            LocationExe,                                            // tam kde je umisten EXE soubor
            ActualUserApplicationData, AllUsersApplicationData,     // pro EXE aplikaci
            IsolatedStorageApplication, IsolatedStorageSite,        // pro WWW aplikaci
            InPath                                                  // bere se pouze 'Path'
        };

        public static string GetDirectory(EnumLocationType dirType)
        {
            switch (dirType)
            {
                case EnumLocationType.None: { return null; }
                case EnumLocationType.StartUpExe: { return System.AppDomain.CurrentDomain.BaseDirectory; }
                case EnumLocationType.LocationExe: { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.ToString()); }
                case EnumLocationType.ActualUserApplicationData: { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
                case EnumLocationType.AllUsersApplicationData: { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
                default: { goto case EnumLocationType.StartUpExe; }
            }
        }

        public static string GetDirectory(EnumLocationType dirType, params string[] dirs)
        {
            string path = GetDirectory(dirType);
            if ((dirs != null) && (dirs.Length > 0)) { foreach (string dir in dirs) { path = System.IO.Path.Combine(path, dir); } }
            return path;
        }

        public static string GetDirectoryFullPath(EnumLocationType dirType, params string[] dirs)
        {
            string path = GetDirectory(dirType);
            if ((dirs != null) && (dirs.Length > 0)) { foreach (string dir in dirs) { path = System.IO.Path.Combine(path, dir); } }
            path = System.IO.Path.GetFullPath(path);
            return path;
        }
    }
}
