using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker.Infraestructure.Files
{
    /// <summary>
    /// Static class that handles everything related to files
    /// </summary>
    public static class FilesUtils
    {
        /// <summary>
        /// Get the current directory of the application
        /// </summary>
        /// <returns>Current directory of the application</returns>
        public static string CurretDirectory()
        {
            return Path.GetDirectoryName(Environment.ProcessPath);
        }

        /// <summary>
        /// Sanitizes the file name so that it is compatible with any file system
        /// </summary>
        /// <param name="name">file name</param>
        /// <returns></returns>
        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
