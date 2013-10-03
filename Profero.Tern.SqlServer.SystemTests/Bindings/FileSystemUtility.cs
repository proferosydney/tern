using System;
using System.IO;
using System.Threading;

namespace Profero.Tern.SqlServer.SystemTests.Bindings
{
    public class FileSystemUtility
    {
        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Retry(() => Directory.Delete(path, true), 3);
            }
        }

        public static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        static void Retry(Action action, int times)
        {
            int numberOfErrors = 0;

            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception)
                {
                    if (++numberOfErrors > times)
                    {
                        throw;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                Retry(() => File.Delete(path), 3);
            }
        }
    }
}
