
namespace ChessAI;

public class ChessBot
{
    public int lastMove { get; set; }
    public Move? bestMove { get; set; }
    public async Task<Move?> EvaluateMove(Board gameState)
    {
        lastMove = gameState.moveLog.Count;
        int heuristicScore = AlphaBetaPruningAsync(gameState, 1, int.MinValue, int.MaxValue, true);
        return bestMove;
    }

    public int AlphaBetaPruningAsync(Board gameState, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || gameState.gameStatus != GameStatus.Ongoing)
        {
            if (gameState.moveLog.Count != lastMove) bestMove = gameState.moveLog[lastMove];
            return EvaluateGameState(gameState);
        }


        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            List<Move> moves = gameState.GetAllValidMoves();
            foreach (Move move in moves)
            {
                Board newGameState = (Board)gameState.Clone();
                newGameState.MakeMove(move.x, move.y, move.tX, move.tY);
                int eval = AlphaBetaPruningAsync(newGameState, depth - 1, alpha, beta, false);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            List<Move> moves = gameState.GetAllValidMoves();
            foreach (Move move in moves)
            {
                Board newGameState = (Board)gameState.Clone();
                newGameState.MakeMove(move.x, move.y, move.tX, move.tY);
                int eval = AlphaBetaPruningAsync(newGameState, depth - 1, alpha, beta, true);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }

    public int EvaluateGameState(Board gameState)
    {
        int score = 0;
        score += EvaluateMaterialBalance(gameState);
        score += EvaluateValidMovesCount(gameState);
        return score;
    }

    private int EvaluateValidMovesCount(Board gameState)
    {
        int pieceValue = 0;
        pieceValue += gameState.GetAllValidMoves().Count;
        return pieceValue;
    }

    public int EvaluateMaterialBalance(Board gameState)
    {
        int whiteScore = CalculatePieceValue(gameState, PieceColor.White);
        int blackScore = CalculatePieceValue(gameState, PieceColor.Black);
        int scoreDifference = Math.Abs(whiteScore - blackScore);
        return scoreDifference;
    }

    private int CalculatePieceValue(Board gameState, PieceColor color)
    {
        int pieceValue = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (gameState.pieces[i][j]?.PieceColor == color)
                {
                    switch (gameState.pieces[i][j]?.PieceType)
                    {
                        case PieceType.Pawn:
                            pieceValue += 1;
                            break;
                        case PieceType.Knight:
                            pieceValue += 3;
                            break;
                        case PieceType.Bishop:
                            pieceValue += 3;
                            break;
                        case PieceType.Rook:
                            pieceValue += 5;
                            break;
                        case PieceType.Queen:
                            pieceValue += 9;
                            break;
                    }
                }
            }
        }
        return pieceValue;
    }

}