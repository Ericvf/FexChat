namespace FexChat.Common
{
    public class MessageModel
    {
        public string UserName { get; set; }

        public Types Type { get; set; }

        public string Payload { get; set; }

        public MessageModel(Types type, string payload, string userName = null)
        {
            Type = type;
            Payload = payload;
            UserName = userName;
        }
    }
}