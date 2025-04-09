using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

public static class FileUploader
{
    public static async Task<string> SaveFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", DateTime.UtcNow.ToString("yy/MM"));
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Path.GetRandomFileName()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/uploads/{DateTime.UtcNow:yy/MM}/{fileName}";
    }

    public static bool RemoveFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return false;

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }

        return false;
    }
}
