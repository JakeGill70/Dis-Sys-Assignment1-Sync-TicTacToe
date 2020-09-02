using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTTTServer
{
    class TicTacToeAi
    {
        public static char AiMove(TicTacToeBoard gameBoard, char player) {
            char result;
            result = randomMove(gameBoard, player);

            return result;
        }

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
