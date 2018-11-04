using System;
using System.IO;
using Keeper2018.Properties;

namespace Keeper2018
{
    public static class DbUtils
    {
       
        public static string GetTxtFullPath(string filename)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = (string)Settings.Default["DataFolder"];
            var dataPath = Path.Combine(path, dataFolder);
            return Path.Combine(dataPath, filename);
        }
    }
}