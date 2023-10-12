namespace Radical.Security.Identity
{
    [Serializable]
    public enum ServiceSite
    {
        Client,
        Server
    }

    public enum DataSite
    {
        None,
        Client,
        Endpoint
    }

    public enum IdentityType
    {
        User,
        Server,
        Service
    }

    [Serializable]
    public class MemberIdentity
    {
        public ServiceSite Site;
        public IdentityType Type;

        public bool Active { get; set; }

        public string AuthId { get; set; }

        public string DataPath { get; set; }

        public string ClientId { get; set; }

        public string Host { get; set; }

        public int Id { get; set; }

        public string Ip { get; set; }

        public string Key { get; set; }

        public DateTime LastAction { get; set; }

        public DateTime LifeTime { get; set; }

        public int Limit { get; set; }

        public string Name { get; set; }

        public int Port { get; set; }

        public DateTime RegisterTime { get; set; }

        public string Salt { get; set; }

        public int Scale { get; set; }

        public string Token { get; set; }

        public string UserId { get; set; }
    }
}
