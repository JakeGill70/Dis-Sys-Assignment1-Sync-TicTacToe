using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcTicTacToeServer
{
    public class TicTacToeService : TicTacToe.TicTacToeBase
    {
        private readonly ILogger<TicTacToeService> _logger;

        public TicTacToeService(ILogger<TicTacToeService> logger) { 
        
            _logger = logger;
        }
    }
}
