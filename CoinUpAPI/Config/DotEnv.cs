using System.Text;

namespace CoinUpAPI.Config;

public static class DotEnv
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(filePath, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith('#')) continue;

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0) continue;

            var key = line.Substring(0, equalsIndex).Trim();
            var value = line.Substring(equalsIndex + 1).Trim();

            value = Unquote(value);

            // Do not overwrite explicitly set environment variables.
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2)
        {
            if ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                return value.Substring(1, value.Length - 2);
            }
        }

        return value;
    }
}
