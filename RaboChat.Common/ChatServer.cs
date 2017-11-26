using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static RaboChat.Common.ChatClient;

namespace RaboChat.Common
{
    [Export]
    public class ChatServer
    {
        readonly TcpListener tcpListener;

        readonly ConcurrentDictionary<string, ClientModel> clients = new ConcurrentDictionary<string, ClientModel>();

        public ChatServer()
        {
            var port = ConfigurationManager.AppSettings["port"] ?? throw new ArgumentNullException("port");
            tcpListener = new TcpListener(IPAddress.Any, Convert.ToInt32(port));
        }

        public void Start()
        {
            tcpListener.Start();

            Task.Run(() => HandleClients());
        }

        async void HandleClients()
        {
            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                var clientModel = new ClientModel()
                {
                    TcpClient = tcpClient,
                    IP = tcpClient.Client.RemoteEndPoint.ToString()
                };

                clients.GetOrAdd(clientModel.IP, clientModel);

                Task.Run(() => ConsumeClient(clientModel));
            }
        }

        private async Task ConsumeClient(ClientModel clientModel)
        {
            var networkStream = clientModel.TcpClient.GetStream();
            var reader = new StreamReader(networkStream);

            var clientName = await reader.ReadLineAsync();
            clientModel.Name = clientName = Base64Helper.Base64Decode(clientName);

            BroadcastAnnouncement(clientModel, $"{clientName} entered the chat");

            while (true)
            {
                try
                {
                    var base64message = await reader.ReadLineAsync();
                    BroadcastClients(clientModel, base64message);
                }
                catch
                {
                    break;
                }
            }

            clients.TryRemove(clientModel.IP, out clientModel);

            BroadcastAnnouncement(clientModel, $"{clientName} left the chat");
        }

        private void BroadcastAnnouncement(ClientModel clientModel, string announcement)
        {
            Console.WriteLine(announcement);
            var messageModel = new MessageModel(Types.Announcement, announcement);
            var message = JsonConvert.SerializeObject(messageModel);
            var base64message = Base64Helper.Base64Encode(message);
            BroadcastClients(clientModel, base64message);
        }

        private void BroadcastClients(ClientModel clientModel, string message)
        {
            foreach (var client in clients)
            {
                var networkStream = client.Value.TcpClient.GetStream();
                var writer = new StreamWriter(networkStream)
                {
                    AutoFlush = true
                };

                writer.WriteLineAsync(message);
            }
        }
    }
}
