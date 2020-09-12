using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTServer
{
    class Program
    {

        static void Main(string[] args)
        {
            // Set console window name
            Console.Title = "Server";

            // Display some house keeping information
            Console.WriteLine(ProgramMeta.GetProgramHeaderInfo());

            // Declare outside of the try-catch a new socket that will
            // be used to represent the connection point to the server.
            SocketFacade listener = null;

            // Establish a server connection point
            try
            {
                // Create a socket representing the connection point to the server.
                listener = new SocketFacade(SharedResources.ProgramMeta.PORT_NUMBER);
                listener.BindSocket();
                listener.ListenForIncomingConnections();

                while (true)
                {
                    // Listen for incoming connections
                    SocketFacade handler = listener.AcceptIncomingConnection();

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

        public static char GetPlayerCharacter(SocketFacade playerSocket) {
            string playerResponse = string.Empty;
            while (!(playerResponse.Equals("X", StringComparison.OrdinalIgnoreCase) || playerResponse.Equals("O", StringComparison.OrdinalIgnoreCase)))
            {
                // Ask the player if they want to be an X or an O
                playerSocket.SendData("Welcome to the Tic-Tac-Server. Are you playing X or O?");
                playerResponse = playerSocket.ReadData();
            }

            playerSocket.SendData("200");

            char playerCharacter;
            playerCharacter = playerResponse.ToUpper()[0];

            return playerCharacter;
        }

        // Attempts to place an X or an O onto the gameboard
        // Returns True if successful, False if the space was occupied (failure)
        private static bool AttemptToPlaceGamePieceOntoBoard(TicTacToeBoard gameBoard, char player, int row, int column)
        {
            bool placementWasSuccessful = false;
            if (player == 'X')
            {
                placementWasSuccessful = gameBoard.InsertX(row, column);
            }
            else if (player == 'O')
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
        public static char PlayerMove(TicTacToeBoard gameBoard, char playerTurn, SocketFacade playerSocket) {
            bool placementWasSuccessful = false;
            String[] positionInput;
            int row;
            int column;

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
                catch (IndexOutOfRangeException e)
                {
                    continue;
                }
                // Attempt to place the game piece
                placementWasSuccessful = AttemptToPlaceGamePieceOntoBoard(gameBoard, playerTurn, row, column);
            }
            return gameBoard.ReportResult();
        }

        public static void PlayGame(SocketFacade playerSocket) {
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
                playerSocket.SendData(gameBoard.ToString());

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
            playerSocket.SendData(boardState.ToString());
        }
    }
}

