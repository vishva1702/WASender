namespace WASender.Contracts.AdminSide
{
    public interface IAdminTemplateService
    {
        Task<bool> DeleteTemplateAsync(ulong templateId, ulong userId);
    }
}
