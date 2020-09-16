using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTTTServer
{
    /// <summary>
    /// A faux player for the server to use to play tic-tac-toe against human client players.
    /// </summary>
    class TicTacToeAi
    {
        /// <summary>
        /// A publically facing method call that calls for this faux player to make a move.
        /// </summary>
        /// <param name="gameBoard">An instance of a tic-tac-toe game board.</param>
        /// <param name="player">The symbol that this AI will place onto the board.</param>
        /// <returns>The resulting state of the gameboard after this AI makes its move.</returns>
        public static char AiMove(TicTacToeBoard gameBoard, char player) {
            char result;
            result = randomMove(gameBoard, player);

            return result;
        }

        /// <summary>
        /// Just one possible strategy for this AI to use. The strategy
        /// randomly picks a position to place its piece.
        /// If the position is already occupied, it just tried another 
        /// random spot until it finds one that is available.
        /// </summary>
        /// <param name="gameBoard">An instance of a tic-tac-toe game board.</param>
        /// <param name="player">The symbol that this AI will place onto the board.</param>
        /// <returns>The resulting state of the gameboard after this AI makes its move.</returns>
        private static char randomMove(TicTacToeBoard gameBoard, char player) {
            bool placementWasSuccessful = false;
            int row;
            int column;
            Random rand = new Random();

            // Make sure that the user has entered a valid position on the game board
            while (!placementWasSuccessful)
            {
                // Pick a random position
                try
                {
                    row = rand.Next(0, 3);
                    column = rand.Next(0, 3);
                }
                catch (IndexOutOfRangeException e)
                {
                    continue;
                }
                // Attempt to place the game piece
                placementWasSuccessful = gameBoard.PlaceGamePieceOntoBoard(player, row, column);
            }
            return gameBoard.ReportResult();
        }
    }
}
