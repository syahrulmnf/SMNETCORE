using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Extensions
{
    public static class Files
    {
        public static string GetUniqueFileNames(this string pathNameStr)
        {
            string pathName = pathNameStr;
            int idx = 1;
            var fileName = Path.GetFileNameWithoutExtension(pathName);
            var directory = Path.GetDirectoryName(pathName);

            var ext = Path.GetExtension(pathName);
            while (File.Exists(pathName))
            {

                var filenameFound = string.Concat(fileName, "_", idx.ToString(), ext);
                pathName = Path.Combine(directory, filenameFound);
                idx += 1;
            }

            return pathName;
        }

        public static string GetUniqueDirectory(this string pathNameStr)
        {
            string pathName = pathNameStr;
            int idx = 1;
            while (File.Exists(pathName))
            {
                pathName = string.Concat(pathNameStr, "_", idx.ToString());
                idx += 1;
            }

            return pathName;
        }

        public static bool TryCopy(this string fromPath, string toPath, string backupPath, bool needRenameExists, bool deleteSuccesfully, bool needBackup)
        {
            if (needRenameExists)
            {
                toPath = File.GetAttributes(fromPath).HasFlag(FileAttributes.Directory) ? GetUniqueDirectory(toPath) : GetUniqueFileNames(toPath);
                if(needBackup && backupPath.HasStringValue()) backupPath = File.GetAttributes(fromPath).HasFlag(FileAttributes.Directory) ? GetUniqueDirectory(backupPath) : GetUniqueFileNames(backupPath);
            }

            try
            {
                if (!Directory.Exists(fromPath) && !File.Exists(fromPath)) throw new Exception("Does not exists in the system : " + fromPath);
                File.Copy(fromPath, toPath, true);
            }
            catch(Exception exc)
            {
                throw exc;
            }

            if(needBackup)
            {
                try
                {
                    File.Copy(fromPath, backupPath, true);
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            }

            if (deleteSuccesfully)
            {
                try
                {
                    if(File.GetAttributes(fromPath).HasFlag(FileAttributes.Directory))
                    {
                        Directory.Delete(fromPath, true);
                    }
                    else
                    {
                        File.Delete(fromPath);
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            }
            return true;
        }
    }
}
