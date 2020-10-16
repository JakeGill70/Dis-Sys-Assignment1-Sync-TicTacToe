using SyncTTTServer;

namespace GrpcTicTacToeServer
{
    public struct TicTacToeGame {
        public string playerID;
        public char currentTurn;
        public char currentState;
        public char playerCharacter;
        public TicTacToeBoard board;

        public TicTacToeGame(string id, char pc) {
            playerID = id;
            currentTurn = 'X';
            currentState = 'N';
            playerCharacter = pc;
            board = new TicTacToeBoard();
        }
    }
}
