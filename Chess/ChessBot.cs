

namespace ChessAI;

public class ChessBot
{
    public int states = 0;
    public int lastMove { get; set; }
    public Move? bestMove { get; set; }
    public int bestScore { get; set; } = int.MinValue;
    public async Task<Move?> EvaluateMove(Board gameState)
    {
        Board newBoard = (Board)gameState.Clone();
        lastMove = newBoard.moveLog.Count;
        states = 0;
        await AlphaBetaPruning(newBoard, 2, int.MinValue, int.MaxValue, true);
        bestScore = int.MinValue;
        Console.WriteLine(states);
        Console.WriteLine($"Best score: ({bestMove?.x} {bestMove?.y}) to ({bestMove?.tX} {bestMove?.tY})");
        return bestMove;
    }

    public async Task<int> AlphaBetaPruning(Board gameState, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        states++;
        if (depth == 0 || gameState.gameStatus != GameStatus.Ongoing)
        {
            int score = await EvaluateGameState(gameState);
            if (gameState.moveLog.Count != lastMove && score > bestScore)
            {
                bestMove = gameState.moveLog[lastMove];
                bestScore = score;
            }
            return score;
        }


        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            List<Move> moves = new List<Move>(gameState.GetAllValidMoves());
            foreach (Move move in moves)
            {
                Board newGameState = (Board)gameState.Clone();
                newGameState.MakeMove(move.x, move.y, move.tX, move.tY);
                int eval = await AlphaBetaPruning(newGameState, depth - 1, alpha, beta, false);
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
            List<Move> moves = new List<Move>(gameState.GetAllValidMoves());
            foreach (Move move in moves)
            {
                Board newGameState = (Board)gameState.Clone();
                newGameState.MakeMove(move.x, move.y, move.tX, move.tY);
                int eval = await AlphaBetaPruning(newGameState, depth - 1, alpha, beta, true);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }

    public async Task<int> EvaluateGameState(Board gameState)
    {
        int score = 0;
        score += EvaluateMaterialBalance(gameState);
        //score += EvaluateValidMovesCount(gameState);
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
        int result = 0;
        int whiteScore = CalculatePieceValue(gameState, PieceColor.White);
        int blackScore = CalculatePieceValue(gameState, PieceColor.Black);
        if (gameState.isWhiteTurn)
        {
            result = whiteScore - blackScore;
        }
        else
        {
            result = blackScore - whiteScore;
        }
        return result;
    }

    private static readonly Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>
    {
        { PieceType.Pawn, 1 },
        { PieceType.Knight, 3 },
        { PieceType.Bishop, 3 },
        { PieceType.Rook, 5 },
        { PieceType.Queen, 9 }
    };

    private int CalculatePieceValue(Board gameState, PieceColor color)
    {
        int pieceValue = 0;
        foreach (var row in gameState.pieces)
        {
            foreach (var piece in row)
            {
                if (piece?.PieceColor == color && PieceValues.TryGetValue(piece.PieceType, out int value))
                {
                    pieceValue += value;
                }
            }
        }
        return pieceValue;
    }


}