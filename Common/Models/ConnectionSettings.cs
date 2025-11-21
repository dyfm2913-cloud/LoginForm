namespace Common.Models
{
    public class ConnectionSettings
    {
        public DatabaseConfig SystemDatabase { get; set; }
        public DatabaseConfig AppDatabase { get; set; }
    }

    public class DatabaseConfig
    {
        public string ServerName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
    }
}