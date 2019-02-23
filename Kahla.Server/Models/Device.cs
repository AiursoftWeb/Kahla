namespace Kahla.Server.Models
{
    public class Device
    {
        public long Id { get; set; }
        public string UserID { get; set; }
        public string PushEndpoint { get; set; }
        public string PushP256DH { get; set; }
        public string PushAuth { get; set; }
    }
}
