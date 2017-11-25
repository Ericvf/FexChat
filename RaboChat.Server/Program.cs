using System;
using System.ComponentModel.Composition;
using RaboChat.Common;

namespace RaboChat.Server
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
