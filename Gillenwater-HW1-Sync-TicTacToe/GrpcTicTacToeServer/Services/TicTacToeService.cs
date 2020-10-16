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

    /// <summary>
    /// Provides a gRPC service for playing TicTacToe
    /// </summary>
    public class TicTacToeService : TicTacToe.TicTacToeBase
    {
        // Pregenerated code for logging messages
        private readonly ILogger<TicTacToeService> _logger;

        // Dictionary for managing games across multiple users.
        // The key is a playerId, represented as a SHA-256 hash of their IPv6 address
        // The value is structure representing the game state including the board, turn, and the player's character.
        private static Dictionary<string, TicTacToeGame> allGames = new Dictionary<string, TicTacToeGame>();

        /// <summary>
        /// Constructor that assigns the logging service.
        /// </summary>
        /// <param name="logger">logging service</param>
        public TicTacToeService(ILogger<TicTacToeService> logger) { 
        
            _logger = logger;
        }

        /// <summary>
        /// Creates a new instance of a TicTacToe game and adds
        /// that instance to the allGames dictionary.
        /// </summary>
        /// <param name="id">A unique playerId as a SHA-256 hash of their IPv6 address.</param>
        /// <param name="pc">The character they will be playing as - either an 'X' or an 'O'</param>
        private static void NewGame(string id, char pc) {
            allGames[id] = new TicTacToeGame(id, pc);
        }

        /// <summary>
        /// Converts an input string into a string representation of a SHA-256 hash.
        /// </summary>
        /// <param name="inputString">The data to hash</param>
        /// <returns>A SHA-256 hash</returns>
        private static string GetHashString(string inputString)
        {
            // Get the hash of the string
            // using SHA-256
            using HashAlgorithm hasher = SHA256.Create();
            byte[] hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(inputString));

            // Convert the hash into a string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /// <summary>
        /// Starting point for a new game.
        /// Negotiates with the client to determine which character the client will play as,
        /// must be either an 'X' or an 'O'.
        /// This function handles input validation, not the client.
        /// </summary>
        /// <param name="request">gRPC parameter. See "ticTacToe.proto" for more information.</param>
        /// <param name="context">gRPC connection. See "ticTacToe.proto" for more information.</param>
        /// <returns>gRPC reply. See "ticTacToe.proto" for more information.</returns>
        public override Task<SetPlayerReply> SetPlayerCharacter(SetPlayerRequest request, ServerCallContext context)
        {
            SetPlayerReply reply = new SetPlayerReply() { InputWasAccepted = false, Message = "", PlayerCharacterC = "N" };
            string playerResponse = request.RequestedPlayerPiece;
            char playerCharacter;
            // Attempt to get the requested character from the player.
            // Assume an impossible "N" character if the client sends an empty string.
            try { 
                playerCharacter = playerResponse.ToUpper()[0]; 
            } 
            catch (Exception) { 
                playerCharacter = 'N'; 
            }

            // If the player chose either an 'X' or an 'O'
            if (playerCharacter == 'X' || playerCharacter == 'O') {

                // Generate a new playerId as a SHA-256 hash of their IPv6 address.
                string playerId = GetHashString(context.Peer);

                // Populate the reply message
                reply.InputWasAccepted = true;
                reply.PlayerCharacterC = playerCharacter.ToString();
                reply.Message = playerId;

                // Create a new game instance for this player
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
        /// <returns>True if the character was successfully added to the board. False if the move was invalid.</returns>
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

        /// <summary>
        /// Negotiates with the client to determine where
        /// the client player would like to place their game piece.
        /// Once an agreement is made, the game is updated.
        /// </summary>
        /// <param name="request">gRPC parameter. See "ticTacToe.proto" for more information.</param>
        /// <param name="context">gRPC connection. See "ticTacToe.proto" for more information.</param>
        /// <returns>gRPC reply. See "ticTacToe.proto" for more information.</returns>
        public override Task<PlayerTurnReply> PlayerTurn(PlayerTurnRequest request, ServerCallContext context)
        {
            // Get the game context
            var game = allGames[request.PlayerId];

            // Generate a new reply message, assuming the client's inputs are invalid.
            PlayerTurnReply reply = new PlayerTurnReply() { 
                InputWasAccepted = false, 
                Message= "Row or Column is out of bounds.", 
                TurnResultC="N"};

            // Get coordinate information from the client
            int row = request.Row;
            int column = request.Column;

            // Make the move if possible
            bool wasAbleToPlaceGamePiece = AttemptToPlaceGamePieceOntoBoard(game.board, game.playerCharacter, row, column);

            // If the move was impossible
            if (wasAbleToPlaceGamePiece)
            {
                // Change the game state
                game.currentState = game.board.ReportResult();
                game.currentTurn = (game.currentTurn == 'O' ? 'X' : 'O');

                // Update the reply message to indicate that the player's move was valid
                reply.InputWasAccepted = true;
                reply.TurnResultC = game.currentState.ToString();
                reply.Message = game.currentTurn.ToString();
            }

            // Update the game state for the next turn
            allGames[request.PlayerId] = game;
            return Task.FromResult(reply);
        }

        /// <summary>
        /// Activates an "tic tac toe AI system" to make a turn on behalf of the server.
        /// The AI system's move is always valid.
        /// </summary>
        /// <param name="request">gRPC parameter. See "ticTacToe.proto" for more information.</param>
        /// <param name="context">gRPC connection. See "ticTacToe.proto" for more information.</param>
        /// <returns>gRPC reply. See "ticTacToe.proto" for more information.</returns>
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

        /// <summary>
        /// Returns a string representation of a ticTacToe board for a specific PlayerId.
        /// Assumes a Windows environment for new lines.
        /// </summary>
        /// <param name="request">gRPC parameter. See "ticTacToe.proto" for more information.</param>
        /// <param name="context">gRPC connection. See "ticTacToe.proto" for more information.</param>
        /// <returns>gRPC reply. See "ticTacToe.proto" for more information.</returns>
        public override Task<BoardReply> GetBoard(BoardRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                new BoardReply() { 
                    GameBoard = allGames[request.PlayerId].board.ToString() 
                });
        }

        /// <summary>
        /// Returns the game state information for a specific PlayerId.
        /// </summary>
        /// <param name="request">gRPC parameter. See "ticTacToe.proto" for more information.</param>
        /// <param name="context">gRPC connection. See "ticTacToe.proto" for more information.</param>
        /// <returns>gRPC reply. See "ticTacToe.proto" for more information.</returns>
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
