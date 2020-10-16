using Grpc.Net.Client;
using GrpcTicTacToeServer;
using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTClient
{
    class ClientProgram
    {

        static void Main(string[] args)
        {

            // Set console window name
            Console.Title = "Client";

            // Display some house keeping information
            Console.WriteLine(ProgramMeta.GetProgramHeaderInfo());

            // Establish a connection to the server
            TicTacToePlayerSession session = null;
            try
            {
                session = TicTacToePlayerSession.StartNewGame("https://localhost:5001");
            }
            catch (Exception e)
            {
                // Catch exceptions thrown while trying to connect to the server.
                Console.Error.WriteLine("An error occured while trying to establish a connection to the server.");
                Console.Error.WriteLine(e);
            }

            try
            {
                session?.PlayGame();
                session?.Dispose();

                Console.WriteLine("Thanks for playing!");
            }
            catch (Exception e) {
                // Catch exceptions thrown during a game
                Console.Error.WriteLine("An error occured during your game while trying to communicate with the server.");
                Console.Error.WriteLine(e);
            }

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }

    }

}

