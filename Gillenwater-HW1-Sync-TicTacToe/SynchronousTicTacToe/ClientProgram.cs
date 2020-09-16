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

            // Declare outside of the try-catch a new socket that will
            // be used to reach out to the server
            ClientSocket sender = null;

            // Establish a connection to the server
            try
            {
                // Create a socket representing the connection to the server
                sender = new ClientSocket(SharedResources.ProgramMeta.PORT_NUMBER);
                sender.ConnectToEndPoint();

                PlayGame(sender);

                Console.WriteLine("Thanks for playing!");
            }
            catch (Exception e)
            {
                // Catch exceptions thrown inside the main server loop so that the program
                // will fail gracefully
                Console.Error.WriteLine("An error occured while trying to establish the server connection point.");
                Console.Error.WriteLine(e);
            }
            finally
            {
                // Close the connection
                sender?.CloseConnection();
            }

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Communicates with the Server Program to negotiate a symbol for the human player.
        /// The server enforces input validation.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void SetPlayerCharacter(SocketFacade serverSocket) {
            string serverResponse = String.Empty;

            while (!serverResponse.Equals(ProgramMeta.INPUT_ACCEPTED))
            {
                // 6. Send the data through the socket
                serverResponse = serverSocket.ReadData();
                if (!serverResponse.Equals(ProgramMeta.INPUT_ACCEPTED))
                {
                    Console.WriteLine(serverResponse);
                    serverSocket.SendData(Console.ReadLine());
                }
            }
        }

        /// <summary>
        /// Negotiates a character for the human player, then waits on instructions
        /// from the server to facilitate a game of tic-tac-toe.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void PlayGame(SocketFacade serverSocket) {
            // Set the character that the human player would like to play as: X or O
            // I.e. Communicate with the server to tell it what the human client wants to play as
            SetPlayerCharacter(serverSocket);

            while (true) {
                // Get instructions from the server
                string serverInstructions = serverSocket.ReadData();
                serverSocket.SendData(ProgramMeta.PROCEDE_WITH_DELIVERY);

                if (serverInstructions.Equals(ProgramMeta.GAMEBOARD_INCOMING))
                {
                    DisplayGameBoard(serverSocket);
                }
                else if (serverInstructions.Equals(ProgramMeta.PLAYER_MOVE)) {
                    PlayerMove(serverSocket);
                }
                else if (serverInstructions.Equals(ProgramMeta.GAME_RESULTS_INCOMING))
                {
                    DisplayResults(serverSocket);
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieves the tic-tac-toe game board from the server as a string,
        /// then immediaitely displays it to the screen.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void DisplayGameBoard(SocketFacade serverSocket) {
            string gameBoardString = serverSocket.ReadData();
            Console.WriteLine(gameBoardString);
        }

        /// <summary>
        /// Negotiates a move for the human player.
        /// The server handles input validation. 
        /// This function will loop infinately until the server approves of the human
        /// player's input.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void PlayerMove(SocketFacade serverSocket) {
            string serverResponse = String.Empty;

            while (!serverResponse.Equals(ProgramMeta.INPUT_ACCEPTED))
            {
                // Send the data through the socket
                serverResponse = serverSocket.ReadData();
                if (!serverResponse.Equals(ProgramMeta.INPUT_ACCEPTED))
                {
                    Console.WriteLine(serverResponse);
                    serverSocket.SendData(Console.ReadLine());
                }
            }
        }

        /// <summary>
        /// Communicates with the tic-tac-toe server to get the final result of the game,
        /// then immediately prints that result to the console.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void DisplayResults(SocketFacade serverSocket) {
            try
            {
                char finalResult = serverSocket.ReadData()[0];

                if(finalResult == 'D')
                {
                     Console.WriteLine("No Winner.");
                }
                else
                {
                    Console.WriteLine(finalResult + " Wins!");
                }
            }
            catch (IndexOutOfRangeException ie) {
                Console.Error.WriteLine("The server sent an empty string back as the final results of the game.");
            }
        }
    }
}

