using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using Common.Models;
using Common.Enums;
using Security;

namespace DatabaseManager
{
    public class DatabaseService
    {
        private ConnectionSettings _settings;

        public DatabaseService(ConnectionSettings settings)
        {
            _settings = settings;
        }

        public SqlConnection GetConnection(DatabaseType dbType)
        {
            string connectionString = GetConnectionString(dbType);
            return new SqlConnection(connectionString);
        }

        private string GetConnectionString(DatabaseType dbType)
        {
            DatabaseConfig config = dbType == DatabaseType.SystemDatabase
                ? _settings.SystemDatabase
                : _settings.AppDatabase;

            return $"Server={config.ServerName};Database={config.DatabaseName};User Id={config.Username};Password={config.Password};";
        }

        public bool TestConnection(DatabaseType dbType)
        {
            try
            {
                using (var connection = GetConnection(dbType))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل الاتصال بقاعدة البيانات: {ex.Message}", "خطأ اتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}