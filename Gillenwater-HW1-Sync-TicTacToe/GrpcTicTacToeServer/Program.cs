using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SharedResources;

namespace GrpcTicTacToeServer
{
    /// <summary>
    /// Generated code.
    /// Entry point for the server, resposible for ensure the TicTacToeService is initialized.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            // Set console window name
            Console.Title = "Server";

            // Display some house keeping information
            Console.WriteLine(ProgramMeta.GetProgramHeaderInfo());

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
