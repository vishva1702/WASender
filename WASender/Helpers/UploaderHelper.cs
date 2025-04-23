using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WASender.Helpers
{
    public class UploaderHelper
    {
        // Upload File
        public static async Task<string> SaveFileAsync(IFormFile file, IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return null;

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Guid.NewGuid().ToString().Substring(0, 8)}{ext}";

            var relativePath = Path.Combine("uploads", DateTime.UtcNow.ToString("yy"), DateTime.UtcNow.ToString("MM"));
            var absolutePath = Path.Combine(env.WebRootPath, relativePath);

            Directory.CreateDirectory(absolutePath);

            var filePath = Path.Combine(absolutePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + Path.Combine(relativePath, fileName).Replace("\\", "/");
        }

        // Remove File
        public static bool RemoveFile(string url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            var fileName = url.Split(new[] { "uploads" }, StringSplitOptions.None);

            if (fileName.Length > 1)
            {
                var filePath = Path.Combine("uploads", fileName[1].TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
            }

            return false;
        }
    }
}



//using Microsoft.AspNetCore.Http;
//using System;
//using System.IO;
//using System.Threading.Tasks;

//namespace WASender.Helpers
//{
//    public class UploaderHelper
//    {
//        private readonly IWebHostEnvironment _env;

//        public UploaderHelper(IWebHostEnvironment env)
//        {
//            _env = env;
//        }

//        public async Task<string> SaveFileAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return null;

//            var ext = Path.GetExtension(file.FileName);
//            var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Guid.NewGuid().ToString().Substring(0, 8)}{ext}";

//            var relativePath = Path.Combine("uploads", DateTime.UtcNow.ToString("yy"), DateTime.UtcNow.ToString("MM"));
//            var absolutePath = Path.Combine(_env.WebRootPath, relativePath);

//            Directory.CreateDirectory(absolutePath);

//            var filePath = Path.Combine(absolutePath, fileName);

//            using (var stream = new FileStream(filePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream);
//            }

//            return "/" + Path.Combine(relativePath, fileName).Replace("\\", "/");
//        }

//        public bool RemoveFile(string url)
//        {
//            if (string.IsNullOrEmpty(url))
//                return true;

//            var fileName = url.Split(new[] { "uploads" }, StringSplitOptions.None);

//            if (fileName.Length > 1)
//            {
//                var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName[1].TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

//                if (File.Exists(filePath))
//                {
//                    File.Delete(filePath);
//                    return true;
//                }
//            }

//            return false;
//        }
//    }
//}