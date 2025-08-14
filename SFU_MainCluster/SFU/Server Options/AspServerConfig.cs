namespace SFU_MainCluster.SFU.Server_Options
{
    public class AspServerConfig
    {
        public Database Database { get; set; } = new Database();
    }
    
    public class Database
    {
        public string? ConnectionString { get; set; }
    }
}

