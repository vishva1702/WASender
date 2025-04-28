using WASender.Contracts.AdminSide;

namespace WASender.Services
{
    public class EnvService : IEnvService
    {
        public void EditEnv(string key, string value)
        {
            var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (!System.IO.File.Exists(envFile)) return;

            var lines = System.IO.File.ReadAllLines(envFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith($"{key}="))
                {
                    lines[i] = $"{key}={value}";
                    break;
                }
            }
            System.IO.File.WriteAllLines(envFile, lines);
        }

        public string GetOption(string key)
        {
            // Fake it for now, or read from database if needed
            return "en";
        }

        public string GetMailDriver()
        {
            var driverType = Environment.GetEnvironmentVariable("MAIL_DRIVER_TYPE");
            return driverType == "MAIL_DRIVER"
                ? Environment.GetEnvironmentVariable("MAIL_DRIVER")
                : Environment.GetEnvironmentVariable("MAIL_MAILER");
        }
    }

}
