using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace HelperLibrary
{
    public static class FileHelper
    {
        public static string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }

            //not hosted. For example, run in unit tests
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return Path.Combine(baseDirectory, path);
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                var physicalPath = HttpContext.Current.Server.MapPath(path);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string UploadBase64Image(string imageData, string filePath)
        {
            var serverPath = HttpContext.Current.Server.MapPath(filePath);
            try
            {
                using (FileStream fs = new FileStream(serverPath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        byte[] data = Convert.FromBase64String(imageData);
                        bw.Write(data);
                        bw.Close();
                    }
                }
                return filePath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool FileExist(string path)
        {
            var physicalPath = HttpContext.Current.Server.MapPath(path);
            return File.Exists(physicalPath);
        }
    }
}
