namespace AwsDbUpMySql.Models
{
    public class Secret
    {
        public string DbClusterIdentifier { get; set; }
        public string Engine { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}