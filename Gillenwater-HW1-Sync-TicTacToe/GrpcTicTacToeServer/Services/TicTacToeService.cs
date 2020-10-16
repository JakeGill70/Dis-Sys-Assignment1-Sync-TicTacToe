using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using SyncTTTServer;

namespace GrpcTicTacToeServer
{

    public class TicTacToeService : TicTacToe.TicTacToeBase
    {
        private readonly ILogger<TicTacToeService> _logger;

        private static Dictionary<string, TicTacToeGame> allGames = new Dictionary<string, TicTacToeGame>();

        public TicTacToeService(ILogger<TicTacToeService> logger) { 
        
            _logger = logger;
        }

        private static void NewGame(string id, char pc) {
            allGames[id] = new TicTacToeGame(id, pc);
        }

        private static string GetHashString(string inputString)
        {
            // Get the hash of the string
            // using SHA-256
            using HashAlgorithm hasher = SHA256.Create();
            byte[] hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(inputString));

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public override Task<SetPlayerReply> SetPlayerCharacter(SetPlayerRequest request, ServerCallContext context)
        {
            SetPlayerReply reply = new SetPlayerReply() { InputWasAccepted = false, Message = "", PlayerCharacterC = "N" };
            string playerResponse = request.RequestedPlayerPiece;
            char playerCharacter;
            try { 
                playerCharacter = playerResponse.ToUpper()[0]; 
            } 
            catch (Exception) { 
                playerCharacter = 'N'; 
            }

            if (playerCharacter == 'X' || playerCharacter == 'O') {
                reply.InputWasAccepted = true;
                reply.PlayerCharacterC = playerCharacter.ToString();
                string playerId = GetHashString(context.Peer);
                reply.Message = playerId;

                NewGame(playerId, playerCharacter);
            }
            else {
                reply.Message = "Welcome to the Tic-Tac-Server. Are you playing X or O?";
            }

            return Task.FromResult(reply);
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

        public override Task<PlayerTurnReply> PlayerTurn(PlayerTurnRequest request, ServerCallContext context)
        {
            var game = allGames[request.PlayerId];
            PlayerTurnReply reply = new PlayerTurnReply() { 
                InputWasAccepted = false, 
                Message= "Row or Column is out of bounds.", 
                TurnResultC="N"};
            int row = request.Row;
            int column = request.Column;

            bool wasAbleToPlaceGamePiece = AttemptToPlaceGamePieceOntoBoard(game.board, game.playerCharacter, row, column);

            if (wasAbleToPlaceGamePiece)
            {
                
                game.currentState = game.board.ReportResult();
                game.currentTurn = (game.currentTurn == 'O' ? 'X' : 'O');

                reply.InputWasAccepted = true;
                reply.TurnResultC = game.currentState.ToString();
                reply.Message = game.currentTurn.ToString();
            }

            allGames[request.PlayerId] = game;
            return Task.FromResult(reply);
        }



        public override Task<ServerTurnReply> ServerTurn(ServerTurnRequest request, ServerCallContext context)
        {
            var game = allGames[request.PlayerId];
            char boardState = TicTacToeAi.AiMove(game.board, game.currentTurn);

            game.currentState = boardState;
            game.currentTurn = (game.currentTurn == 'O' ? 'X' : 'O');

            ServerTurnReply reply = new ServerTurnReply();
            reply.TurnResultC = game.currentState.ToString();
            reply.Message = game.currentTurn.ToString();

            allGames[request.PlayerId] = game;

            return Task.FromResult(reply);
        }

        public override Task<BoardReply> GetBoard(BoardRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                new BoardReply() { 
                    GameBoard = allGames[request.PlayerId].board.ToString() 
                });
        }

        public override Task<ResultReply> GetResult(ResultRequest request, ServerCallContext context)
        {
            var game = allGames[request.PlayerId];
            ResultReply reply = new ResultReply();
            reply.CurrentTurnC = game.currentTurn.ToString();
            reply.PlayerTurnC = game.playerCharacter.ToString();
            reply.ResultC = game.currentState.ToString();

            return Task.FromResult(reply);
        }
    }
}
