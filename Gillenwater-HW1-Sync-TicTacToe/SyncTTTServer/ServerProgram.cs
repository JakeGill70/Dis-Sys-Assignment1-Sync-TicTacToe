using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTServer
{
    class ServerProgram
    {

        static void Main(string[] args)
        {
            // Set console window name
            Console.Title = "Server";

            // Display some house keeping information
            Console.WriteLine(ProgramMeta.GetProgramHeaderInfo());

            // Declare outside of the try-catch a new socket that will
            // be used to represent the connection point to the server.
            ServerSocket listener = null;

            // Establish a server connection point
            try
            {
                // Create a socket representing the connection point to the server.
                listener = new ServerSocket(SharedResources.ProgramMeta.PORT_NUMBER);
                listener.BindSocket();
                listener.ListenForIncomingConnections();

                while (true)
                {
                    // Listen for incoming connections
                    // This is a blocking call
                    SocketFacade handler = listener.AcceptIncomingConnection();

                    // Play a game of tic-tac-toe against the entity that connected
                    PlayGame(handler);
                }
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
                listener?.CloseConnection();
            }

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();

        }

        /// <summary>
        /// Communicates with the Client Program to negotiate a symbol for the human player.
        /// Sends a prompt to the player to ask if they want to be an X or an O.
        /// The server enforces input validation.
        /// </summary>
        /// <param name="serverSocket">A reference to the Socket Facade connecting to the client.</param>
        private static char GetPlayerCharacter(SocketFacade playerSocket) {
            string playerResponse = string.Empty;
            while (!(playerResponse.Equals("X", StringComparison.OrdinalIgnoreCase) || playerResponse.Equals("O", StringComparison.OrdinalIgnoreCase)))
            {
                // Ask the player if they want to be an X or an O
                playerSocket.SendData("Welcome to the Tic-Tac-Server. Are you playing X or O?");
                playerResponse = playerSocket.ReadData();
            }

            playerSocket.SendData(ProgramMeta.INPUT_ACCEPTED);

            char playerCharacter;
            playerCharacter = playerResponse.ToUpper()[0];

            return playerCharacter;
        }

        
        /// <summary>
        /// Attempts to place an X or an O onto the gameboard.
        /// Returns True if successful, False if the space was occupied (failure)
        /// </summary>
        /// <param name="gameBoard">The tic-tac-toe board to place the piece onto</param>
        /// <param name="playerTurn">Symbol to be placed - must be either an 'X' or an 'O'</param>
        /// <param name="row">The row on the board to place the piece.</param>
        /// <param name="column">The column on the board to place the piece.</param>
        /// <returns></returns>
        private static bool AttemptToPlaceGamePieceOntoBoard(TicTacToeBoard gameBoard, char playerTurn, int row, int column)
        {
            bool placementWasSuccessful = false;
            if (playerTurn == 'X')
            {
                placementWasSuccessful = gameBoard.InsertX(row, column);
            }
            else if (playerTurn == 'O')
            {
                placementWasSuccessful = gameBoard.InsertO(row, column);
            }
            else
            {
                throw new Exception("Player's game piece must be an X or an O");
            }
            return placementWasSuccessful;
        }

        // Communicate with the client to facilitate the player's move
        /// <summary>
        /// Communicate with the client connection to place the remote player's game piece.
        /// </summary>
        /// <param name="gameBoard">The tic-tac-toe board to place the piece onto</param>
        /// <param name="playerTurn">Symbol to be placed - must be either an 'X' or an 'O'</param>
        /// <param name="playerSocket">The remote socket connected to the client player.</param>
        /// <returns>The resulting state of the gameBoard after the player has placed their piece.</returns>
        private static char PlayerMove(TicTacToeBoard gameBoard, char playerTurn, SocketFacade playerSocket) {
            bool placementWasSuccessful = false;
            String[] positionInput;
            int row;
            int column;

            // Tell the player that it is their move
            playerSocket.SendData(ProgramMeta.PLAYER_MOVE);
            GetPermissionForDataDelivery(playerSocket);

            // Make sure that the user has entered a valid position on the game board
            while (!placementWasSuccessful)
            {
                // Provide instruction
                //Console.WriteLine($"Please enter the row and column to place an {player} onto the board");
                playerSocket.SendData($"Please enter the row and column, seperated by a single space, to place an {playerTurn} onto the board.");
                // Get the position from the player
                try
                {
                    positionInput = playerSocket.ReadData().Split();
                    row = int.Parse(positionInput[0]);
                    column = int.Parse(positionInput[1]);
                }
                catch (Exception e)
                {
                    // If any problem occurs while deciphering the number, just try again on the next loop around
                    continue;
                }
                // Attempt to place the game piece
                placementWasSuccessful = AttemptToPlaceGamePieceOntoBoard(gameBoard, playerTurn, row, column);
            }

            // Inform the client that the input was accepted.
            playerSocket.SendData(ProgramMeta.INPUT_ACCEPTED);

            return gameBoard.ReportResult();
        }

        /// <summary>
        /// Blocking call that waits for a confirmation message from the remote client player.
        /// </summary>
        /// <param name="playerSocket">The remote socket connected to the client player.</param>
        /// <returns>True if the remote client approved the delivery of the rest of the data.</returns>
        private static bool GetPermissionForDataDelivery(SocketFacade playerSocket) { 
            string messageFromSocket = playerSocket.ReadData(); // Wait for a message to procede
            return messageFromSocket.Equals(ProgramMeta.PROCEDE_WITH_DELIVERY);
        }

        /// <summary>
        /// Sends a string representation of the tic-tac-toe game board to the remote client player.
        /// </summary>
        /// <param name="gameBoard">The tic-tac-toe board to place the piece onto</param>
        /// <param name="playerSocket">The remote socket connected to the client player.</param>
        private static void SendGameBoard(TicTacToeBoard gameBoard, SocketFacade playerSocket) {
            playerSocket.SendData(ProgramMeta.GAMEBOARD_INCOMING);
            GetPermissionForDataDelivery(playerSocket);
            playerSocket.SendData(gameBoard.ToString());
        }

        /// <summary>
        /// Initializes and walks through a game of tic-tac-toe with a remote client player.
        /// This function accepts a client connection as input, then initializes a new game
        /// of tic-tac-toe, lets the player choose their symbol, then procedes to play the game
        /// until either the remote client player or a server-side AI system wins, or the game
        /// is declared a draw.
        /// </summary>
        /// <param name="playerSocket">The remote socket connected to the client player.</param>
        private static void PlayGame(SocketFacade playerSocket) {
            // Create Game Board
            TicTacToeBoard gameBoard = new TicTacToeBoard();

            // Get the character that the human player would like to play as: X or O
            char playerCharacter = GetPlayerCharacter(playerSocket);

            // Initialize to 'O' so that when the player 
            // turn switches at the start of the loop,
            // 'X' gets to go first.
            char playerTurn = 'O';

            // The state of the game board after each player's move
            char boardState = 'N'; 

            // Actually play the game
            while (boardState == 'N')
            {
                // Switch turns
                playerTurn = (playerTurn == 'O' ? 'X' : 'O');

                // Send a display of the new game state
                SendGameBoard(gameBoard, playerSocket);

                // Make moves
                if (playerTurn == playerCharacter)
                {
                    // Do the fancy server talk stuff
                    boardState = PlayerMove(gameBoard, playerTurn, playerSocket);
                }
                else {
                    // Handle fancy AI stuff
                    boardState = TicTacToeAi.AiMove(gameBoard, playerTurn);
                }

            }

            //  Send a display of the final results
            playerSocket.SendData(ProgramMeta.GAME_RESULTS_INCOMING);
            GetPermissionForDataDelivery(playerSocket);
            playerSocket.SendData(boardState.ToString());
        }
    }
}

