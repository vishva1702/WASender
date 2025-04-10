using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WASender.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);
        Task DeleteFileAsync(string filePath);
    }
}