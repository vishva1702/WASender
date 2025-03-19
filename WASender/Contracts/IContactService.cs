using System.Threading.Tasks;

namespace WASender.Services
{
    public interface IContactService
    {
        Task<Dictionary<string, string>> GetContactDataAsync();
    }
}
