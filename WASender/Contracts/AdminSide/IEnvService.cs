namespace WASender.Contracts.AdminSide
{
    public interface IEnvService
    {
        void EditEnv(string key, string value);
        string GetOption(string key);
        string GetMailDriver();
    }

}
