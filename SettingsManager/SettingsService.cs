using System;
using System.IO;
using System.Windows.Forms;  // أضف هذا السطر
using Common.Models;
using Security;



namespace SettingsManager
{
    public class SettingsService
    {
        private readonly string settingsFilePath;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "LoginSystem");
            Directory.CreateDirectory(appFolder);
            settingsFilePath = Path.Combine(appFolder, "connectionSettings.enc");
        }

        public void SaveSettings(ConnectionSettings settings)
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
                string encryptedJson = EncryptionHelper.Encrypt(json);
                File.WriteAllText(settingsFilePath, encryptedJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ الإعدادات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public ConnectionSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                    return null;

                string encryptedJson = File.ReadAllText(settingsFilePath);
                string json = EncryptionHelper.Decrypt(encryptedJson);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionSettings>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في قراءة الإعدادات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public bool SettingsExist()
        {
            return File.Exists(settingsFilePath);
        }

        public void ClearSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    File.Delete(settingsFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في مسح الإعدادات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}