using System;
using System.IO;
using System.Text.RegularExpressions;


namespace WASender.Helpers
{
    public class DotenvService
    {
        private readonly string _envFilePath;

        public DotenvService()
        {
            _envFilePath = Path.Combine(AppContext.BaseDirectory, ".env");
        }

        public bool EditEnv(string key, string value, bool isBool = false)
        {
            if (!File.Exists(_envFilePath))
            {
                throw new FileNotFoundException(".env file not found at " + _envFilePath);
            }

            var envText = File.ReadAllText(_envFilePath);
            var currentEnvValue = GetCurrentEnvValue(envText, key);

            string newText;

            if (isBool)
            {
                string boolKey = $"{key}={(currentEnvValue == "true" ? "true" : "false")}";
                string boolValue = $"{key}={(value == "true" ? "true" : "false")}";
                newText = envText.Contains(boolKey)
                    ? envText.Replace(boolKey, boolValue)
                    : $"{envText}\n{boolValue}";
            }
            else
            {
                string cleanValue = RemoveEmptySpace(value);
                if (!string.IsNullOrEmpty(currentEnvValue))
                {
                    newText = envText.Replace($"{key}={currentEnvValue}", $"{key}={cleanValue}");
                }
                else
                {
                    newText = $"{envText}\n{key}={cleanValue}";
                }
            }

            File.WriteAllText(_envFilePath, newText);

            return true;
        }

        private string GetCurrentEnvValue(string envText, string key)
        {
            var match = Regex.Match(envText, @$"^{Regex.Escape(key)}=(.*)$", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private string RemoveEmptySpace(string value)
        {
            return Regex.Replace(value ?? string.Empty, @"\s+", "");
        }
    }
}
