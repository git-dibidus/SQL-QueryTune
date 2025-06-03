using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QueryTune.Core.Models;

namespace QueryTune.WPF.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        
        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SQLQueryTune");
            
            Directory.CreateDirectory(appDataPath);
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        }

        private class StoredSettings
        {
            public string? ServerName { get; set; }
            public string? DatabaseName { get; set; }
            public bool UseWindowsAuthentication { get; set; }
            public string? UserId { get; set; }
            public string? EncryptedPassword { get; set; }
            public string? LastQuery { get; set; }
        }

        public async Task SaveConnectionParametersAsync(ConnectionParameters parameters)
        {
            var settings = new StoredSettings
            {
                ServerName = parameters.ServerName,
                DatabaseName = parameters.DatabaseName,
                UseWindowsAuthentication = parameters.UseWindowsAuthentication,
                UserId = parameters.UserId,
                EncryptedPassword = !string.IsNullOrEmpty(parameters.Password) 
                    ? ProtectPassword(parameters.Password)
                    : null
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        public async Task<ConnectionParameters> LoadConnectionParametersAsync()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new ConnectionParameters();
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<StoredSettings>(json);

            if (settings == null)
            {
                return new ConnectionParameters();
            }

            var parameters = new ConnectionParameters
            {
                ServerName = settings.ServerName ?? string.Empty,
                DatabaseName = settings.DatabaseName ?? string.Empty,
                UseWindowsAuthentication = settings.UseWindowsAuthentication,
                UserId = settings.UserId ?? string.Empty
            };

            if (!string.IsNullOrEmpty(settings.EncryptedPassword))
            {
                parameters.Password = UnprotectPassword(settings.EncryptedPassword);
            }

            return parameters;
        }

        public async Task SaveLastQueryAsync(string query)
        {
            var settings = await LoadSettingsFileAsync();
            settings.LastQuery = query;
            await SaveSettingsFileAsync(settings);
        }

        public async Task<string> LoadLastQueryAsync()
        {
            var settings = await LoadSettingsFileAsync();
            return settings.LastQuery ?? string.Empty;
        }

        private async Task<StoredSettings> LoadSettingsFileAsync()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new StoredSettings();
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            return JsonSerializer.Deserialize<StoredSettings>(json) ?? new StoredSettings();
        }

        private async Task SaveSettingsFileAsync(StoredSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        private string ProtectPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        private string UnprotectPassword(string encryptedPassword)
        {
            try
            {
                var bytes = Convert.FromBase64String(encryptedPassword);
                var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
