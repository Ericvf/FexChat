using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RaboChat.Common
{
    [Export]
    public class ChatServer
    {
        readonly TcpListener tcpListener = new TcpListener(IPAddress.Any, 3000);

        readonly ConcurrentDictionary<string, ClientModel> clients = new ConcurrentDictionary<string, ClientModel>();

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

            BroadcastClients(clientModel, $"{clientName} entered the chat.");

            while (true)
            {
                try
                {
                    var base64message = await reader.ReadLineAsync();
                    var message = Base64Helper.Base64Decode(base64message);

                    message = $"{clientModel.Name}: {message}";
                    BroadcastClients(clientModel, message);
                }
                catch
                {
                    break;
                }
            }

            clients.TryRemove(clientModel.IP, out clientModel);

            BroadcastClients(clientModel, $"{clientModel.Name} left the chat.");
        }

        private void BroadcastClients(ClientModel clientModel, string message)
        {
            Console.WriteLine(message);

            var base64message = Base64Helper.Base64Encode(message);
            foreach (var client in clients)
            {
                var networkStream = client.Value.TcpClient.GetStream();
                var writer = new StreamWriter(networkStream)
                {
                    AutoFlush = true
                };

                writer.WriteLineAsync(base64message);
            }
        }
    }
}
