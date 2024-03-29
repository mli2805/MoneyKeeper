using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Threading;
using System;
using System.Runtime.InteropServices;

namespace KeeperDomain
{
    public class LogFile
    {
        private const string ToCompress = ".toCompress";

        private StreamWriter _logFile;
        public string Culture { get; } = "ru-RU";
        public int SizeLimitKb { get; } = 20_000;
        public int LogLevel { get; } = 4;
        public string LogFullFileName { get; private set; }
        private readonly object _obj = new object();

        public LogFile AssignFile(string filename)
        {
            if (filename == "")
                return this;

            LogFullFileName = Utils.FileNameForSure(@"..\Log\", filename, true);
            if (LogFullFileName == null) return this;
            lock (_obj)
            {
                _logFile = File.AppendText(LogFullFileName);
                _logFile.AutoFlush = true;

                EmptyLine();
                EmptyLine('-');
            }

            return this;
        }

        public void EmptyLine(char ch = ' ')
        {
            lock (_obj)
            {
                string message = new string(ch, 78);
                if (_logFile != null)
                    _logFile.WriteLine(message);
                else Console.WriteLine(message);
            }
        }

        // LogFile gets its LogLevel due initialization
        // if messageLevel more than LogLevel message is NOT logged
        public void AppendLine(string message, int offset = 0, int messageLevel = 2, string prefix = "")
        {
            lock (_obj)
            {
                if (_logFile == null)
                    return;
                if (messageLevel > LogLevel)
                    return;

                CheckSizeLimit();

                var offsetStr = new string(' ', offset);
                if (!string.IsNullOrEmpty(prefix))
                    prefix += " ";
                foreach (var str in SplitMessageOnLines(message))
                {
                    var msg = DateTime.Now.ToString(new CultureInfo(Culture)) + "  " + offsetStr + prefix + str.Trim();
                    _logFile.WriteLine(msg);
                }
            }
        }

        private static string[] SplitMessageOnLines(string message)
        {
            message = message.Replace("\0", string.Empty);
            message = message.Trim();
            message = message.Replace("\r\n", "\r");
            message = message.Replace("\n\r", "\r");
            message = message.Replace("\n", "\r");
            return message.Split('\r');
        }

        private void CheckSizeLimit()
        {
            if (SizeLimitKb > 0 && _logFile.BaseStream.Length > SizeLimitKb * 1024)
            {
                _logFile.Close();
                File.Copy(LogFullFileName, LogFullFileName + ToCompress, true);
                var newEmptyLogFile = Utils.FileNameForSure(@"..\Log\", "empty.log", true);
                File.Copy(newEmptyLogFile, LogFullFileName, true);

                _logFile = File.AppendText(LogFullFileName);
                _logFile.AutoFlush = true;
                var folder = Path.GetDirectoryName(LogFullFileName);
                if (folder != null)
                    File.Delete(Path.Combine(folder, @"empty.log"));

                var thread = new Thread(Pack);
                thread.Start();
            }
        }

        private void Pack()
        {
            FileInfo fileToCompress = new FileInfo(LogFullFileName + ToCompress);
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                FileStream compressedFileStream;
                try
                {
                    compressedFileStream = File.Create(LogFullFileName + ".gz");
                }
                catch (Exception)
                {
                    compressedFileStream = File.Create(LogFullFileName + $".{Guid.NewGuid()}.gz");
                }

                using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressionStream);
                }
            }

            File.Delete(LogFullFileName + ToCompress);
        }
    }
}

public static class Utils
{
    public static string FileNameForSure(string subDir, string filename, bool isBoomNeeded, bool isSubDirAbsolute = false)
    {
        try
        {
            string folder = isSubDirAbsolute
                ? subDir
                : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subDir));
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fullPath = Path.GetFullPath(Path.Combine(folder, filename));
            if (File.Exists(fullPath))
                return fullPath;
            using (FileStream fs = File.Create(fullPath))
            {
                if (isBoomNeeded)
                { fs.WriteByte(239); fs.WriteByte(187); fs.WriteByte(191); }
            }
            return fullPath;
        }
        catch (COMException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
