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
            TicTacToe.TicTacToeClient ticTacToeServer;
            try
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                ticTacToeServer = new TicTacToe.TicTacToeClient(channel);

                PlayGame(ticTacToeServer);

                Console.WriteLine("Thanks for playing!");
            }
            catch (Exception e)
            {
                // Catch exceptions thrown inside the main server loop so that the program
                // will fail gracefully
                Console.Error.WriteLine("An error occured while trying to establish a connection to the server.");
                Console.Error.WriteLine(e);
            }

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Communicates with the Server Program to negotiate a symbol for the human player.
        /// The server enforces input validation.
        /// </summary>
        /// <param name="server">A reference to the Socket Facade connecting to the server.</param>
        /// <returns>The player's ID for this game session</returns>
        private static string StartNewGame(TicTacToe.TicTacToeClient server)
        {
            bool isInputAcceptable = false;
            SetPlayerRequest request = new SetPlayerRequest() { RequestedPlayerPiece = "N" };
            SetPlayerReply reply = server.SetPlayerCharacter(request);

            while (!isInputAcceptable)
            {
                Console.WriteLine(reply.Message);
                request.RequestedPlayerPiece = Console.ReadLine();
                reply = server.SetPlayerCharacter(request);
                isInputAcceptable = reply.InputWasAccepted;
            }

            return reply.Message;
        }

        /// <summary>
        /// Negotiates a character for the human player, then waits on instructions
        /// from the server to facilitate a game of tic-tac-toe.
        /// </summary>
        /// <param name="server">A reference to the Socket Facade connecting to the server.</param>
        private static void PlayGame(TicTacToe.TicTacToeClient server)
        {
            // Set the character that the human player would like to play as: X or O
            // I.e. Communicate with the server to tell it what the human client wants to play as
            string myId = StartNewGame(server);
            ResultReply gameStateResult;

            while (true)
            {
                // Get instructions from the server
                gameStateResult = server.GetResult(new ResultRequest() { PlayerId = myId });

                // Display only the latest game state information from the server
                ClearScreen();
                DisplayGameBoard(server, myId);

                if (IsGameOver(gameStateResult))
                {
                    DisplayResults(server, myId);
                    break;
                }
                else if (IsPlayerTurn(gameStateResult))
                {
                    PlayerMove(server, myId);
                }
                else if (IsServerTurn(gameStateResult))
                {
                    // Write a saying that the game is waiting for the server to make its turn.
                    Console.WriteLine("The server is making its turn...");
                    server.ServerTurn(new ServerTurnRequest() { PlayerId = myId });
                }
            }
        }

        /// <summary>
        /// Determines if the game is over based on the game state from the server.
        /// </summary>
        /// <param name="gameState">Should be the most recent game state from the server.</param>
        /// <returns>True if the player or server has won the game, or there are no more valid moves.</returns>
        private static bool IsGameOver(ResultReply gameState)
        {
            return !(gameState.ResultC.Equals("N"));
        }

        /// <summary>
        /// Determines if it is the player's turn based on the game state from the server.
        /// </summary>
        /// <param name="gameState">Should be the most recent game state from the server.</param>
        /// <returns>True if the next character's turn is the player's character.</returns>
        private static bool IsPlayerTurn(ResultReply gameState)
        {
            return gameState.CurrentTurnC.Equals(gameState.PlayerTurnC);
        }

        /// <summary>
        /// Determines if it is the server's turn based on the game state from the server.
        /// </summary>
        /// <param name="gameState">Should be the most recent game state from the server.</param>
        /// <returns>True if the next character's turn is not the player's character.</returns>
        private static bool IsServerTurn(ResultReply gameState)
        {
            return !(gameState.CurrentTurnC.Equals(gameState.PlayerTurnC));
        }

        /// <summary>
        /// Retrieves the tic-tac-toe game board from the server as a string,
        /// then immediaitely displays it to the screen.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void DisplayGameBoard(TicTacToe.TicTacToeClient server, string playerId)
        {
            BoardReply boardReply = server.GetBoard(new BoardRequest() { PlayerId = playerId });
            Console.WriteLine(boardReply.GameBoard);
        }

        /// <summary>
        /// Negotiates a move for the human player.
        /// The server handles input validation. 
        /// This function will loop infinately until the server approves of the human
        /// player's input.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void PlayerMove(TicTacToe.TicTacToeClient server, string playerId)
        {
            // Create some local variables used inside of the loop below
            bool isInputAcceptable = false;
            PlayerTurnRequest request;
            PlayerTurnReply reply;

            while (!isInputAcceptable)
            {
                // Prompt the user for valid input
                Console.WriteLine($"Please enter the row and column, seperated by a single space, to place your piece onto the board.");

                // Attempt to parse the user's input for a valid Row and Column
                try
                {
                    // Get user input
                    string[] coordsParts = Console.ReadLine().Split(" ");
                    // Attempt to parse input
                    int row = int.Parse(coordsParts[0]);
                    int column = int.Parse(coordsParts[1]);

                    // Create a request for the server
                    request = new PlayerTurnRequest();
                    request.PlayerId = playerId;
                    request.Row = row;
                    request.Column = column;

                    // Send the request to the server
                    reply = server.PlayerTurn(request);

                    // Check if the input was valid
                    isInputAcceptable = reply.InputWasAccepted;
                }
                catch (IndexOutOfRangeException)
                {
                    // Continue to prompt for input if the user did not include a space in their input.
                    continue;
                }
                catch (FormatException)
                {
                    // Continue to prompt for input if the user did not use valid integers in their input.
                    continue;
                }
            }
        }

        /// <summary>
        /// Clears the console of any text.
        /// </summary>
        private static void ClearScreen()
        {
            Console.Clear();
        }

        /// <summary>
        /// Communicates with the tic-tac-toe server to get the final result of the game,
        /// then immediately prints that result to the console.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private static void DisplayResults(TicTacToe.TicTacToeClient server, string playerId)
        {
            ResultReply result = server.GetResult(new ResultRequest() { PlayerId = playerId });
            char finalResult = result.ResultC[0];

            if (finalResult == 'D')
            {
                Console.WriteLine("No Winner.");
            }
            else
            {
                Console.WriteLine(finalResult + " Wins!");
            }
        }
    }

}

