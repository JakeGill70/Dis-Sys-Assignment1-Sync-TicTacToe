﻿using SharedResources;
using System;

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
                // Start the game loop
                //! Warning: PlayGame() access the Console directly to read/write/clear
                session?.PlayGame();

                // Clean up resources
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

