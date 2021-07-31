using FexChat.Common;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FexChat.Client
{
    public class Program
    {
        [Import]
        public ChatClient TcpClient { get; set; }

        static async Task Main(string[] args)
        {
            var program = new Program();

            var types = new[] { typeof(CompositionHelpers) };

            CompositionHelpers.InitializeComposition(types, program);

            await program.Start();
        }

        private async Task Start()
        {
            var clientName = Console.ReadLine();
            await TcpClient.Start(clientName);
            TcpClient.ChatMessage += (s, e) => Console.WriteLine($"User: {e.Message.UserName} says: {e.Message.Payload}");

            while (true)
            {
                var message = Console.ReadLine();
                await TcpClient.WriteText(message);
            }
        }
    }
}
