using FexChat.Common;
using System;
using System.ComponentModel.Composition;

namespace FexChat.Server
{
    public class Program
    {
        [Import]
        public ChatServer TcpServer { get; set; }

        static void Main(string[] args)
        {
            var program = new Program();
            var types = new[] { typeof(CompositionHelpers) };
            CompositionHelpers.InitializeComposition(types, program);
            program.Start();
        }

        private void Start()
        {
            Console.WriteLine("Waiting for clients...");

            TcpServer.Start();
            Console.ReadLine();
        }
    }
}
