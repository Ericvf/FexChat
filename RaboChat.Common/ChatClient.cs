using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RaboChat.Common
{
    [Export]
    public class ChatClient
    {
        public class ChatMessageEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public event EventHandler<ChatMessageEventArgs> ChatMessage;

        readonly ClientModel clientModel = new ClientModel();

        public ChatClient()
        {
            clientModel.TcpClient = new TcpClient();
        }

        public async Task Start(string clientName)
        {
            await clientModel.TcpClient.ConnectAsync("localhost", 3000);

            await WriteMessage(clientName);

            Task.Run(() => ConsumeServer(clientModel.TcpClient));
        }

        private async Task ConsumeServer(TcpClient client)
        {
            var networkStream = client.GetStream();
            var reader = new StreamReader(networkStream);

            while (true)
            {
                string bas64message = await reader.ReadLineAsync();
                var message = Base64Helper.Base64Decode(bas64message);
                RaiseMessage(message);
            }
        }

        private void RaiseMessage(string message)
        {
            ChatMessage?.Invoke(this, new ChatMessageEventArgs()
            {
                Message = message
            });
        }

        public async Task WriteMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (!clientModel.TcpClient.Connected)
            {
                Close();
                return;
            }

            var networkStream = clientModel.TcpClient.GetStream();

            var writer = new StreamWriter(networkStream);
            writer.AutoFlush = true;

            var base64message = Base64Helper.Base64Encode(message);
            await writer.WriteLineAsync(base64message);
        }

        public void Close()
        {
            RaiseMessage("Not connected.");
            clientModel.TcpClient.Close();
        }
    }
}
