using WASender.Models;

namespace WASender.Contracts.UserSide
{
    public interface IUserTemplateService
    {

        List<Template> GetUserTemplates(ulong userId, out int totalTemplates, out int activeTemplates, out int inactiveTemplates, out string limit);
        Task<Template?> GetTemplateByIdAsync(ulong templateId, ulong userId);
        Task<(bool success, string message)> UpdateTemplateAsync(ulong templateId, IFormCollection form, ulong userId);
        Task<bool> DeleteTemplateAsync(ulong templateId, ulong userId);
        Task<(bool success, string message)> CreateTemplateAsync(IFormCollection form, ulong userId);
        Task<string> SaveFileAsync(IFormFile file);
    }

}
