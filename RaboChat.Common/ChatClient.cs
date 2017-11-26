using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RaboChat.Common
{
    [Export]
    public class ChatClient
    {
        public class ChatMessageEventArgs : EventArgs
        {
            public MessageModel Message { get; set; }
        }

        public event EventHandler<ChatMessageEventArgs> ChatMessage;

        readonly ClientModel clientModel = new ClientModel();

        public async Task Start(string clientName)
        {
            clientModel.TcpClient = new TcpClient();
            clientModel.Name = clientName;

            var hostname = ConfigurationManager.AppSettings["hostname"] ?? throw new ArgumentNullException("hostname");
            var port = ConfigurationManager.AppSettings["port"] ?? throw new ArgumentNullException("port");

            await clientModel.TcpClient.ConnectAsync(hostname, Convert.ToInt32(port));

            await WriteMessage(clientName);

            Task.Run(() => ConsumeServer(clientModel));
        }

        private async Task ConsumeServer(ClientModel client)
        {
            var networkStream = client.TcpClient.GetStream();
            var reader = new StreamReader(networkStream);

            try
            {
                while (true)
                {
                    string bas64message = await reader.ReadLineAsync();
                    var message = Base64Helper.Base64Decode(bas64message);

                    var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                    RaiseMessage(messageModel);
                }
            }
            finally
            {
                client.TcpClient.Close();
                client.TcpClient = null;
            }
        }

        private void RaiseMessage(MessageModel message)
        {
            ChatMessage?.Invoke(this, new ChatMessageEventArgs()
            {
                Message = message
            });
        }

        public async Task WriteText(string message, Types type = Types.Text)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var messageModel = new MessageModel(type, message, clientModel.Name);
            var payload = JsonConvert.SerializeObject(messageModel);
            await WriteMessage(payload);
        }

        public async Task WriteImage(byte[] bytes)
        {
            var base64image = Convert.ToBase64String(bytes);
            var messageModel = new MessageModel(Types.Image, base64image, clientModel.Name);
            var payload = JsonConvert.SerializeObject(messageModel);
            await WriteMessage(payload);
        }

        private async Task WriteMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (clientModel.TcpClient == null)
            {
                try
                {
                    await Start(clientModel.Name);
                }
                catch
                {
                    var messageModel = new MessageModel(Types.Announcement, "Not connected");
                    RaiseMessage(messageModel);
                    clientModel.TcpClient.Close();
                    clientModel.TcpClient = null;
                    return;
                }
            }

            var networkStream = clientModel.TcpClient.GetStream();

            var writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;

            var base64message = Base64Helper.Base64Encode(message);
            await writer.WriteLineAsync(base64message);
        }
    }
}