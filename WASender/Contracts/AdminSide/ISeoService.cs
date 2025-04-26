using WASender.Models;

namespace WASender.Contracts.AdminSide
{
    public interface ISeoService
    {
        Task<List<dynamic>> GetSeoOptionsAsync();
        Task<Option?> GetSeoOptionByIdAsync(ulong id);
        Task<Dictionary<string, string>> GetSeoMetadataAsync(Option option);
        Task<string> UpdateSeoAsync(ulong id, IFormFile? image, Dictionary<string, string> metadata);
    }
}
