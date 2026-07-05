using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System;
using SMNETCORE.Logging;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Common;

namespace SMNETCORE.DataType.Extensions
{
    public class Utils
    {
        public static string GetFileName(string oldDirectory, string newDirectory, string fileName, bool isNetworkFiles)
        {
            try
            {
                if ((fileName.StartsWith(newDirectory) && File.Exists(fileName))) return fileName;

                if (!fileName.HasValue() || !oldDirectory.HasValue() || !newDirectory.HasValue() || !fileName.StartsWith(oldDirectory)) return fileName;

                var fileNameDirectory = fileName.Replace(oldDirectory, string.Empty);
                fileNameDirectory = fileNameDirectory.StartsWith("\\") ? fileNameDirectory.Remove(0, 1) : fileNameDirectory;

                var directory = newDirectory.HasValue() ? newDirectory : oldDirectory;
                directory = directory.EndsWith("\\") ? directory.Remove(directory.Length - 1, 1) : directory;

                string filePath = string.Format(@"{0}\{1}", directory, fileNameDirectory);

                return filePath;

            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

       

        public static Regex EmailValidation()
        {
            const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
            const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

            // Set explicit regex match timeout, sufficient enough for email parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set

            try
            {
                return new Regex(pattern, options);
            }
            catch(Exception exc)
            {
                throw exc;
            }

        }

        /// <summary>
        /// Checks the file name and produces a unique name if the file already exists in the directory, otherwise returns the original name.
        /// </summary>
        /// <param name="fileFullPath">Full file path.</param>
        /// <returns>Unique name if file exists in the directory, otherwise the original file name.</returns>
        public static string GetSafeFileName(string fileFullPath, bool isNetwrokDrive)
        {
            if (!File.Exists(fileFullPath))
                return fileFullPath;

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileFullPath) + "-";
            var directoryName = Path.GetDirectoryName(fileFullPath);
            var extension = Path.GetExtension(fileFullPath);

            var index = 1;
            do
            {
                if (directoryName != null)
                {
                    nameWithoutExtension += index++;
                    fileFullPath = Path.Combine(directoryName, nameWithoutExtension + extension);
                }
            } while (File.Exists(fileFullPath));


            return fileFullPath;
        }

        public static bool IsEmailValid(string email)
        {
            var emailValidation = new EmailAddressAttribute();
            return emailValidation.IsValid(email);
        }
    }
}
