// Code recycled from Spring 2020 V&V lab.
// Code originally authored by Jake Gillenwater.
// Design specifications were originally outlined by Jeff Roach.
// The code and subsequent documentation was written according to 
// the standards of the course at the time.

using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTTTServer
{
    public class TicTacToeBoard
    {
        private char[,] _board = new char[3, 3];
        const char SPACE = ' '; // Use this instead of ' ', because ' ' is easy to misinterperate for ''

        // Makes an instance of this class indexable to the _board
        public char this[int row, int col]
        {
            get
            {
                return _board[row, col];
            }
        }

        public TicTacToeBoard()
        {
            for (int row = 0; row <= _board.GetUpperBound(0); row++)
            {
                for (int column = 0; column <= _board.GetUpperBound(1); column++)
                {
                    _board[row, column] = SPACE;
                }
            }
        }

        public bool InsertX(int row, int column)
        {
            // Bounds Check
            if (row < 0 || row > _board.GetUpperBound(0) || column < 0 || column > _board.GetUpperBound(1))
            {
                return false;
            }

            bool isEmptyCell = (_board[row, column] == SPACE);
            if (isEmptyCell)
            {
                _board[row, column] = 'X';
            }
            return isEmptyCell;
        }

        public bool InsertO(int row, int column)
        {
            // Bounds Check
            if (row < 0 || row > _board.GetUpperBound(0) || column < 0 || column > _board.GetUpperBound(1))
            {
                return false;
            }

            bool isEmptyCell = (_board[row, column] == SPACE);
            if (isEmptyCell)
            {
                _board[row, column] = 'O';
            }
            return isEmptyCell;
        }

        // Determines the state of the game.
        // Usually executed after a player's move
        // {D:Draw, X: Xs Won, Y: Ys Won, N: No Result}
        public char ReportResult()
        {
            var b = _board;
            bool isDraw = boardIsFull();
            if (isDraw) return 'D';
            bool xWon = didPlayerWin('X');
            if (xWon) return 'X';
            bool oWon = didPlayerWin('O');
            if (oWon) return 'O';
            return 'N';
        }

        // Determines if a player won 
        private bool didPlayerWin(char player)
        {
            if (!(player == 'X' || player == 'O'))
            {
                throw new Exception("Player augument must be an X or an O.");
            }
            char p = player;
            return (winByRow(p) || winByColumn(p) || winByDiagonal(p));
        }

        // Determines if a player won by a row
        private bool winByRow(char player)
        {
            for (int row = 0; row <= _board.GetUpperBound(0); row++)
            {

                // Walk through each column in the row
                bool winner = true;
                for (int column = 0; column <= _board.GetUpperBound(1); column++)
                {
                    if (_board[row, column] != player)
                    { // If something in that row is not the player
                        winner = false; // Then that player could not win
                        break;
                    }
                }
                if (winner == true)
                { // If no other player was in that row
                    return true; // Then that player is the winner
                }
            }
            return false; // All rows have been checked, and there was no winner :(
        }

        // Determines if a player won by a column
        private bool winByColumn(char player)
        {
            for (int column = 0; column <= _board.GetUpperBound(1); column++)
            {
                // Walk through each row in the column
                bool winner = true;
                for (int row = 0; row <= _board.GetUpperBound(0); row++)
                {
                    if (_board[row, column] != player)
                    { // If something in that column is not the player
                        winner = false; // Then that player could not win
                        break;
                    }
                }
                if (winner == true)
                { // If no other player was in that row
                    return true; // Then that player is the winner
                }
            }
            return false; // All rows have been checked, and there was no winner :(
        }

        // Determines if a player won by a diagonal in either direction
        private bool winByDiagonal(char player)
        {
            bool playerWon = winByDiagonal_TL(player) || winByDiagonal_TR(player);
            return playerWon;
        }

        // Determines if a player won by a diagonal starting in the Top Left
        private bool winByDiagonal_TL(char player)
        {
            // Walk through each column in the row
            bool winner = true;
            for (int i = 0; i <= _board.GetUpperBound(1); i++)
            {
                if (_board[i, i] != player)
                { // If something in that row is not the player
                    winner = false; // Then that player could not win
                    break;
                }
            }
            if (winner == true)
            { // If no other player was in that row
                return true; // Then that player is the winner
            }
            return false; // All rows have been checked, and there was no winner :(
        }

        // Determines if a player won by a diagonal starting in the Top Right
        private bool winByDiagonal_TR(char player)
        {
            // Walk through each column in the row
            bool winner = true;
            for (int i = 0; i <= _board.GetUpperBound(1); i++)
            {
                if (_board[i, (_board.GetUpperBound(1) - i)] != player)
                { // If something in that row is not the player
                    winner = false; // Then that player could not win
                    break;
                }
            }
            if (winner == true)
            { // If no other player was in that row
                return true; // Then that player is the winner
            }
            return false; // All rows have been checked, and there was no winner :(
        }

        // Determines if there are any empty cells left in the board
        public bool boardIsFull()
        {
            foreach (char c in _board)
            {
                if (c == SPACE)
                {
                    return false;
                }
            }
            return true;
        }

        // Attempts to place an X or an O onto the gameboard
        // Returns True if successful, False if the space was occupied (failure)
        public bool PlaceGamePieceOntoBoard(char player, int row, int column)
        {
            bool placementWasSuccessful = false;
            if (player == 'X')
            {
                placementWasSuccessful = InsertX(row, column);
            }
            else if (player == 'O')
            {
                placementWasSuccessful = InsertO(row, column);
            }
            else
            {
                throw new Exception("Player's game piece must be an X or an O");
            }
            return placementWasSuccessful;
        }

        public override string ToString()
        {
            var ENL = Environment.NewLine;
            var b = _board;
            string board = $" {b[0, 0]} | {b[0, 1]} | {b[0, 2]} " + ENL +
                                $"---+---+---" + ENL +
                                $" {b[1, 0]} | {b[1, 1]} | {b[1, 2]} " + ENL +
                                $"---+---+---" + ENL +
                                $" {b[2, 0]} | {b[2, 1]} | {b[2, 2]} " + ENL;
            return board;
        }

        public string EncodeToString() {
            StringBuilder sb = new StringBuilder();
            var b = _board;
            sb.Append(b[0,0]);
            sb.Append(b[0,1]);
            sb.Append(b[0,2]);
            sb.Append(b[1, 0]);
            sb.Append(b[1, 1]);
            sb.Append(b[1, 2]);
            sb.Append(b[2, 0]);
            sb.Append(b[2, 1]);
            sb.Append(b[2, 2]);
            return sb.ToString();
        }

        public void DecodeFromString(string boardEncodedString)
        {
            char[] s = boardEncodedString.ToCharArray();
            var b = _board;
            b[0, 0] = s[0];
            b[0, 1] = s[1];
            b[0, 2] = s[2];
            b[1, 0] = s[3];
            b[1, 1] = s[4];
            b[1, 2] = s[5];
            b[2, 0] = s[6];
            b[2, 1] = s[7];
            b[2, 2] = s[8];
        }
    }
}
