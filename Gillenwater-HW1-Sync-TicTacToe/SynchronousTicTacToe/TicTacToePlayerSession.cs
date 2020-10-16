using Grpc.Net.Client;
using GrpcTicTacToeServer;
using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTClient
{
    /// <summary>
    /// Manages a session of tic tac toe against a server
    /// </summary>
    class TicTacToePlayerSession : IDisposable
    {
        TicTacToe.TicTacToeClient server;
        GrpcChannel channel;
        String playerId;

        /// <summary>
        /// Factory pattern that creaters a new player,
        /// Then communicates with the server to start a new game
        /// </summary>
        /// <param name="serverAddress">The address and port of the gRPC TicTacToe Server. Example format: "https://localHost:5001"</param>
        /// <returns>A new TicTacToe player instance, already connected to a valid server.</returns>
        public static TicTacToePlayerSession StartNewGame(string serverAddress) {
            
            var channel = GrpcChannel.ForAddress(serverAddress);
            var ticTacToeServer = new TicTacToe.TicTacToeClient(channel);
            TicTacToePlayerSession player = new TicTacToePlayerSession(ticTacToeServer);
            player.playerId = player.SetupNewGame();
            player.channel = channel;
            return player;
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="server">A gRPC client connection to a gRPC TicTacToe server.</param>
        private TicTacToePlayerSession(TicTacToe.TicTacToeClient server) {
            this.server = server;
            this.playerId = String.Empty;
        }

        /// <summary>
        /// Communicates with the Server Program to negotiate a symbol for the human player.
        /// The server enforces input validation.
        /// </summary>
        /// <returns>The player's ID for this game session</returns>
        private string SetupNewGame()
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
        public void PlayGame()
        {
            while (true)
            {
                // Get instructions from the server
                ResultReply gameStateResult = server.GetResult(new ResultRequest() { PlayerId = this.playerId });

                // Display only the latest game state information from the server
                Console.Clear();
                DisplayGameBoard();

                if (IsGameOver(gameStateResult))
                {
                    DisplayResults();
                    break;
                }
                else if (IsPlayerTurn(gameStateResult))
                {
                    PlayerMove();
                }
                else if (IsServerTurn(gameStateResult))
                {
                    // Write a saying that the game is waiting for the server to make its turn.
                    Console.WriteLine("The server is making its turn...");
                    server.ServerTurn(new ServerTurnRequest() { PlayerId = this.playerId });
                }
            }
        }

        /// <summary>
        /// Determines if the game is over based on the game state from the server.
        /// </summary>
        /// <param name="gameState">Should be the most recent game state from the server.</param>
        /// <returns>True if the player or server has won the game, or there are no more valid moves.</returns>
        private bool IsGameOver(ResultReply gameState)
        {
            return !(gameState.ResultC.Equals("N"));
        }

        /// <summary>
        /// Determines if it is the player's turn based on the game state from the server.
        /// </summary>
        /// <param name="gameState">Should be the most recent game state from the server.</param>
        /// <returns>True if the next character's turn is the player's character.</returns>
        private bool IsPlayerTurn(ResultReply gameState)
        {
            return gameState.CurrentTurnC.Equals(gameState.PlayerTurnC);
        }

        /// <summary>
        /// Determines if it is the server's turn based on the game state from the server.
        /// </summary>
        /// <param name="gameState">Should be the most recent game state from the server.</param>
        /// <returns>True if the next character's turn is not the player's character.</returns>
        private bool IsServerTurn(ResultReply gameState)
        {
            return !(gameState.CurrentTurnC.Equals(gameState.PlayerTurnC));
        }

        /// <summary>
        /// Retrieves the tic-tac-toe game board from the server as a string,
        /// then immediaitely displays it to the screen.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        public void DisplayGameBoard()
        {
            BoardReply boardReply = server.GetBoard(new BoardRequest() { PlayerId = this.playerId });
            Console.WriteLine(boardReply.GameBoard);
        }

        /// <summary>
        /// Negotiates a move for the human player.
        /// The server handles input validation. 
        /// This function will loop infinately until the server approves of the human
        /// player's input.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        private void PlayerMove()
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
                    request.PlayerId = this.playerId;
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
        /// Communicates with the tic-tac-toe server to get the final result of the game,
        /// then immediately prints that result to the console.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the server.</param>
        public void DisplayResults()
        {
            ResultReply result = server.GetResult(new ResultRequest() { PlayerId = this.playerId });
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

        /// <summary>
        /// Cleans up any left over resources
        /// </summary>
        public void Dispose()
        {
            this.server = null;
            this.playerId = null;
            this.channel.Dispose();
        }
    }

}
