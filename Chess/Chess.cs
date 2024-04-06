
using System.ComponentModel;
using Chess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum PieceColor
{
    White, Black
}

public enum PieceType
{
    Pawn, Knight, Bishop, Rook, Queen, King
}

public class Move : IEquatable<Move>
{
    public int x { get; set; }
    public int y { get; set; }
    public int tX { get; set; }
    public int tY { get; set; }
    public Piece? movedPiece { get; set; }
    public Piece? capturedPiece { get; set; }
    public Move(int x, int y, int tX, int tY, Piece? movedPiece, Piece? capturedPiece)
    {
        this.x = x;
        this.y = y;
        this.tX = tX;
        this.tY = tY;
        this.movedPiece = movedPiece;
        this.capturedPiece = capturedPiece;
    }

    public bool Equals(Move? other)
    {
        return other != null && other.x == this.x && other.y == this.y && other.tX == this.tX && other.tY == this.tY;
    }
}

public class Board
{
    public static (int x, int y)[] directions = new (int x, int y)[8];
    public static (int x, int y)[] directionsKnight = new (int x, int y)[8];
    public List<(int x, int y)> kingInvalidMoves { get; set; }
    public List<(int x, int y, int dirX, int dirY)> pinPieces { get; set; }
    public List<(int x, int y, int dirX, int dirY)> checkPieces { get; set; }
    public (int x, int y) whiteKingLocation { get; set; }
    public (int x, int y) blackKingLocation { get; set; }
    public bool isCheck = false;
    public bool isWhiteTurn { get; set; }
    public List<Move> moveLog = new List<Move>();
    public Piece?[][] pieces { get; set; } = new Piece[8][];
    static Board()
    {
        directions[0] = (0, 1);
        directions[1] = (-1, 0);
        directions[2] = (0, -1);
        directions[3] = (1, 0);
        directions[4] = (1, 1);
        directions[5] = (-1, 1);
        directions[6] = (1, -1);
        directions[7] = (-1, -1);
        directionsKnight[0] = (1, 2);
        directionsKnight[1] = (2, 1);
        directionsKnight[2] = (2, -1);
        directionsKnight[3] = (1, -2);
        directionsKnight[4] = (-1, -2);
        directionsKnight[5] = (-2, -1);
        directionsKnight[6] = (-2, 1);
        directionsKnight[7] = (-1, 2);
    }
    public Board()
    {
        isWhiteTurn = true;
        pinPieces = new List<(int x, int y, int dirX, int dirY)>();
        checkPieces = new List<(int x, int y, int dirX, int dirY)>();
        kingInvalidMoves = new List<(int x, int y)>();
        for (int i = 0; i < 8; i++)
        {
            pieces[i] = new Piece[8];
        }
        for (int i = 0; i < 8; i++)
        {
            pieces[i][1] = new Pawn(PieceColor.White);
            pieces[i][6] = new Pawn(PieceColor.Black);
        }
        pieces[0][0] = new Rook(PieceColor.White);
        pieces[7][0] = new Rook(PieceColor.White);
        pieces[0][7] = new Rook(PieceColor.Black);
        pieces[7][7] = new Rook(PieceColor.Black);
        pieces[1][0] = new Knight(PieceColor.White);
        pieces[6][0] = new Knight(PieceColor.White);
        pieces[1][7] = new Knight(PieceColor.Black);
        pieces[6][7] = new Knight(PieceColor.Black);
        pieces[2][0] = new Bishop(PieceColor.White);
        pieces[5][0] = new Bishop(PieceColor.White);
        pieces[2][7] = new Bishop(PieceColor.Black);
        pieces[5][7] = new Bishop(PieceColor.Black);
        pieces[3][0] = new Queen(PieceColor.White);
        pieces[4][0] = new King(PieceColor.White);
        whiteKingLocation = (4, 0);
        pieces[3][7] = new Queen(PieceColor.Black);
        pieces[4][7] = new King(PieceColor.Black);
        blackKingLocation = (4, 7);
    }

    public void Display(string? format)
    {
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < 8; i++)
            {
                if (pieces[i][j] != null)
                {
                    Console.Write(pieces[i][j]?.ToString(format, null) + " ");
                }
                else
                {
                    Console.Write(' ');
                }
            }
            Console.WriteLine();
        }
    }

    public void Display()
    {
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < 8; i++)
            {
                if (pieces[i][j] != null)
                {
                    Console.Write(pieces[i][j]);
                }
                else
                {
                    Console.Write(' ');
                }
            }
            Console.WriteLine();
        }
    }

    public bool IsOpponentPieceAt((int x, int y) pos, PieceColor allyColor)
    {
        return pieces[pos.x][pos.y] != null && pieces[pos.x][pos.y]?.PieceColor != allyColor;
    }

    public bool IsOpponentKingAt((int x, int y) pos, PieceColor allyColor)
    {
        return pieces[pos.x][pos.y] != null && pieces[pos.x][pos.y] is King && pieces[pos.x][pos.y]?.PieceColor != allyColor;
    }

    public bool IsAllyPieceAt((int x, int y) pos, PieceColor allyColor)
    {
        return pieces[pos.x][pos.y] != null && pieces[pos.x][pos.y]?.PieceColor == allyColor;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < 8 && y < 8;
    }

    public void DisplayValidMoves(int x, int y)
    {
        List<(int x, int y)>? validMoves = pieces[x][y]?.GetValidMoves(this, (x, y));
        if (validMoves != null && validMoves.Count() != 0)
        {
            Console.Write($"Valid move on ({x} {y}) are");
            foreach (var move in validMoves)
            {
                Console.Write($" ({move.x} {move.y})");
            }
        }
    }

    public void MakeMove(int x, int y, int tX, int tY)
    {
        if (pieces[x][y] == null)
        {
            return;
        }
        Piece? capturedPiece = pieces[tX][tY];
        Piece? movedPiece = pieces[x][y];
        if (movedPiece != null && !movedPiece.GetValidMoves(this, (x, y)).Contains((tX, tY)))
        {
            return;
        }
        if (movedPiece?.PieceColor == PieceColor.White && isWhiteTurn == false)
        {
            return;
        }
        if (movedPiece?.PieceColor == PieceColor.Black && isWhiteTurn == true)
        {
            return;
        }
        pieces[tX][tY] = movedPiece;
        pieces[x][y] = null;
        if (movedPiece != null) movedPiece.hasMoved = true;
        moveLog.Add(new Move(x, y, tX, tY, movedPiece, capturedPiece));
        if (movedPiece != null) movedPiece.hasMoved = true;
        if (movedPiece?.PieceColor == PieceColor.White && movedPiece?.PieceType == PieceType.King)
        {
            whiteKingLocation = (tX, tY);
        }
        else if (movedPiece?.PieceColor == PieceColor.Black && movedPiece?.PieceType == PieceType.King)
        {
            blackKingLocation = (tX, tY);
        }
        isWhiteTurn = !isWhiteTurn;
        CheckForPinsAndChecks();
        FindThreats();
    }

    public void UndoMove()
    {
        if (moveLog.Count() == 0) return;
        Move foundMove = moveLog.Last();
        pieces[foundMove.x][foundMove.y] = foundMove.movedPiece;
        pieces[foundMove.tX][foundMove.tY] = foundMove.capturedPiece;
        moveLog.Remove(foundMove);
        isWhiteTurn = !isWhiteTurn;
    }

    public void DisplayMoveLog()
    {
        if (moveLog.Count() == 0) return;
        Console.WriteLine();
        foreach (var move in moveLog)
        {
            Console.WriteLine($"({move.x} {move.y}) to ({move.tX} {move.tY})");
        }
    }

    public void CheckForPinsAndChecks()
    {
        pinPieces.Clear();
        checkPieces.Clear();
        isCheck = false;
        PieceColor allyColor = PieceColor.White;
        PieceColor enemyColor = PieceColor.Black;
        (int x, int y) location;
        if (isWhiteTurn)
        {
            location = whiteKingLocation;
            allyColor = PieceColor.White;
            enemyColor = PieceColor.Black;
        }
        else
        {
            location = blackKingLocation;
            allyColor = PieceColor.Black;
            enemyColor = PieceColor.White;
        }

        foreach (var dir in directions)
        {
            int newX = location.x + dir.x;
            int newY = location.y + dir.y;
            (int x, int y)? possiblePinLocation = null;
            while (IsInBounds(newX, newY))
            {
                if (pieces[newX][newY]?.PieceColor == allyColor)
                {
                    if (possiblePinLocation is null)
                    {
                        possiblePinLocation = (newX, newY);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (pieces[newX][newY]?.PieceColor == enemyColor)
                {
                    if (pieces[newX][newY]?.PieceType is PieceType.Pawn)
                    {
                        if (enemyColor is PieceColor.White)
                        {
                            // Black capture = (1, -1);
                            // Black capture = (-1, -1);
                            if ((newX == location.x + 1 && newY == location.y - 1) || (newX == location.x - 1 && newY == location.y - 1))
                            {
                                if (possiblePinLocation is null)
                                {
                                    isCheck = true;
                                    checkPieces.Add((newX, newY, dir.x, dir.y));
                                }
                                else
                                {
                                    pinPieces.Add((possiblePinLocation.Value.x, possiblePinLocation.Value.y, dir.x, dir.y));
                                }
                            }
                        }
                        else if (enemyColor is PieceColor.Black)
                        {
                            // White capture = (1, 1);
                            // White capture = (-1, 1);
                            if ((newX == location.x + 1 && newY == location.y + 1) || (newX == location.x - 1 && newY == location.y + 1))
                            {
                                if (possiblePinLocation is null)
                                {
                                    isCheck = true;
                                    checkPieces.Add((newX, newY, dir.x, dir.y));
                                }
                                else
                                {
                                    pinPieces.Add((possiblePinLocation.Value.x, possiblePinLocation.Value.y, dir.x, dir.y));
                                }
                            }
                        }
                        break;
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Bishop)
                    {
                        if (dir == directions[4] || dir == directions[5] || dir == directions[6] || dir == directions[7])
                        {
                            if (possiblePinLocation is null)
                            {
                                isCheck = true;
                                checkPieces.Add((newX, newY, dir.x, dir.y));
                            }
                            else
                            {
                                pinPieces.Add((possiblePinLocation.Value.x, possiblePinLocation.Value.y, dir.x, dir.y));
                            }
                            break;
                        }
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Rook)
                    {
                        if (dir == directions[0] || dir == directions[1] || dir == directions[2] || dir == directions[3])
                        {
                            if (possiblePinLocation is null)
                            {
                                isCheck = true;
                                checkPieces.Add((newX, newY, dir.x, dir.y));
                            }
                            else
                            {
                                pinPieces.Add((possiblePinLocation.Value.x, possiblePinLocation.Value.y, dir.x, dir.y));
                            }
                            break;
                        }
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Queen)
                    {
                        if (possiblePinLocation is null)
                        {
                            isCheck = true;
                            checkPieces.Add((newX, newY, dir.x, dir.y));
                        }
                        else
                        {
                            pinPieces.Add((possiblePinLocation.Value.x, possiblePinLocation.Value.y, dir.x, dir.y));
                        }
                        break;
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.King)
                    {
                        if (newX == dir.x - location.x && newY == dir.y - location.y)
                        {
                            if (possiblePinLocation is null)
                            {
                                isCheck = true;
                                checkPieces.Add((newX, newY, dir.x, dir.y));
                            }
                            else
                            {
                                pinPieces.Add((possiblePinLocation.Value.x, possiblePinLocation.Value.y, dir.x, dir.y));
                            }
                        }
                        break;
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Knight)
                    {
                        break;
                    }
                }
                newX += dir.x;
                newY += dir.y;
            }
        }
        foreach (var dir in directionsKnight)
        {
            int newX = dir.x + location.x;
            int newY = dir.y + location.y;
            if (!IsInBounds(newX, newY)) continue;
            if (pieces[newX][newY]?.PieceColor == enemyColor && pieces[newX][newY]?.PieceType == PieceType.Knight)
            {
                isCheck = true;
                checkPieces.Add((newX, newY, dir.x, dir.y));
            }
        }
    }
    public void FindThreats()
    {
        kingInvalidMoves.Clear();
        (int x, int y) pos;
        if (isWhiteTurn)
        {
            pos = whiteKingLocation;
        }
        else
        {
            pos = blackKingLocation;
        }
        PieceColor allyColor = isWhiteTurn ? PieceColor.White : PieceColor.Black;
        foreach (var dir in directions)
        {
            int newX = pos.x + dir.x;
            int newY = pos.y + dir.y;
            if (IsThreatenedByEnemy((newX, newY), allyColor))
            {
                kingInvalidMoves.Add((newX, newY));
            }
        }
    }

    public bool IsThreatenedByEnemy((int x, int y) pos, PieceColor allyColor)
    {
        foreach (var dir in directionsKnight)
        {
            int newX = dir.x + pos.x;
            int newY = dir.y + pos.y;
            if (!IsInBounds(newX, newY)) continue;
            if (pieces[newX][newY]?.PieceColor != allyColor && pieces[newX][newY]?.PieceType == PieceType.Knight)
            {
                return true;
            }
        }
        foreach (var dir in directions)
        {
            int newX = pos.x + dir.x;
            int newY = pos.y + dir.y;
            while (IsInBounds(newX, newY))
            {
                if (pieces[newX][newY]?.PieceType == PieceType.King && pieces[newX][newY]?.PieceColor == allyColor)
                {
                    newX += dir.x;
                    newY += dir.y;
                    continue;
                }
                if (pieces[newX][newY]?.PieceColor == allyColor)
                {
                    break;
                }
                else
                {
                    if (pieces[newX][newY]?.PieceType is PieceType.Pawn)
                    {
                        if (allyColor is PieceColor.Black)
                        {
                            // Black capture = (1, -1);
                            // Black capture = (-1, -1);
                            if ((newX == pos.x + 1 && newY == pos.y - 1) || (newX == pos.x - 1 && newY == pos.y - 1))
                            {
                                return true;
                            }
                        }
                        else if (allyColor is PieceColor.White)
                        {
                            // White capture = (1, 1);
                            // White capture = (-1, 1);
                            if ((newX == pos.x + 1 && newY == pos.y + 1) || (newX == pos.x - 1 && newY == pos.y + 1))
                            {
                                return true;
                            }
                        }
                        break;
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Bishop)
                    {
                        if (dir == directions[4] || dir == directions[5] || dir == directions[6] || dir == directions[7])
                        {
                            return true;
                        }
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Rook)
                    {
                        if (dir == directions[0] || dir == directions[1] || dir == directions[2] || dir == directions[3])
                        {
                            return true;
                        }
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Queen)
                    {
                        return true;
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.King)
                    {
                        if (newX == pos.x + dir.x && newY == pos.y + dir.y)
                        {
                            return true;
                        }
                    }
                    else if (pieces[newX][newY]?.PieceType is PieceType.Knight)
                    {
                        break;
                    }
                }
                newX += dir.x;
                newY += dir.y;
            }
        }
        return false;
    }

}

public class Piece : IEquatable<Piece>, IFormattable
{
    public virtual bool hasMoved { get; set; }
    public virtual PieceColor PieceColor { get; set; }
    public virtual PieceType PieceType { get; set; }
    public virtual List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        return new List<(int x, int y)>();
    }
    public virtual bool Equals(Piece? piece)
    {
        return false;
    }
    public virtual string ToString(string? format, IFormatProvider? formatProvider)
    {
        return "blank ";
    }


}

public class Pawn : Piece
{
    public Pawn(PieceColor pieceColor)
    {
        hasMoved = false;
        PieceColor = pieceColor;
        PieceType = PieceType.Pawn;
        switch (pieceColor)
        {
            case PieceColor.White:
                directions[0] = (0, 1);
                directions[1] = (1, 1);
                directions[2] = (-1, 1);
                directions[3] = (0, 2);
                break;

            case PieceColor.Black:
                directions[0] = (0, -1);
                directions[1] = (1, -1);
                directions[2] = (-1, -1);
                directions[3] = (0, -2);
                break;
        }
    }
    private (int x, int y)[] directions = new (int x, int y)[4];
    override public bool hasMoved { get; set; }
    override public PieceColor PieceColor { get; set; }
    override public PieceType PieceType { get; set; }
    override public List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        bool isPinned = false;
        (int dirX, int dirY) pinDirection = (0, 0);
        foreach (var pinPiece in board.pinPieces)
        {
            if (pos.x == pinPiece.x && pos.y == pinPiece.y)
            {
                isPinned = true;
                pinDirection.dirX = pinPiece.dirX;
                pinDirection.dirY = pinPiece.dirY;
            }
        }
        if (this.PieceColor == PieceColor.White && !board.isWhiteTurn)
        {
            return validMoves;
        }
        if (this.PieceColor == PieceColor.Black && board.isWhiteTurn)
        {
            return validMoves;
        }
        if (board.checkPieces.Count == 1)
        {
            if (board.pieces[board.checkPieces[0].x][board.checkPieces[0].y]?.PieceType == PieceType.Knight)
            {
                foreach (var dir in directions)
                {
                    int newX = pos.x + dir.x;
                    int newY = pos.y + dir.y;

                    if (!board.IsInBounds(newX, newY)) continue;

                    switch (dir)
                    {
                        case var move when move.x != 0 && board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsOpponentKingAt((newX, newY), this.PieceColor):
                            if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                            {
                                validMoves.Add((newX, newY));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                List<(int x, int y)> potentialValidMoves = new List<(int x, int y)>();
                if (PieceColor == PieceColor.White)
                {
                    int newX = board.whiteKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.whiteKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        if (!board.IsInBounds(newX, newY)) continue;
                        switch (dir)
                        {
                            case var move when move.x == 0 && !board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor):
                                if (Math.Abs(move.y) == 2 && hasMoved) break;
                                if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                                break;
                            case var move when move.x != 0 && board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsOpponentKingAt((newX, newY), this.PieceColor):
                                if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                                break;
                        }
                    }
                }
                else
                {
                    int newX = board.blackKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.blackKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        if (!board.IsInBounds(newX, newY)) continue;
                        switch (dir)
                        {
                            case var move when move.x == 0 && !board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor):
                                if (Math.Abs(move.y) == 2 && hasMoved) break;
                                if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                                break;
                            case var move when move.x != 0 && board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsOpponentKingAt((newX, newY), this.PieceColor):
                                if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                                break;
                        }
                    }
                }
            }
        }
        else if (board.checkPieces.Count > 1)
        {
            return validMoves;
        }
        else
        {
            foreach (var dir in directions)
            {
                int newX = pos.x + dir.x;
                int newY = pos.y + dir.y;
                if (!board.IsInBounds(newX, newY)) continue;
                switch (dir)
                {
                    case var move when move.x == 0 && !board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor):
                        if (Math.Abs(move.y) == 2 && hasMoved) break;
                        if (!isPinned || (pinDirection.dirX == dir.x && pinDirection.dirY == dir.y)) validMoves.Add((newX, newY));
                        break;
                    case var move when move.x != 0 && board.IsOpponentPieceAt((newX, newY), this.PieceColor) && !board.IsOpponentKingAt((newX, newY), this.PieceColor):
                        if (!isPinned || (pinDirection.dirX == dir.x && pinDirection.dirY == dir.y)) validMoves.Add((newX, newY));
                        break;
                }
            }
        }
        return validMoves;
    }

    override public bool Equals(Piece? piece)
    {
        return piece != null && piece.PieceColor == this.PieceColor && this.PieceType == this.PieceType;
    }

    override public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "fullName")
        {
            return this.PieceColor.ToString() + " Pawn";
        }
        else
        {
            return "p";
        }
    }
}

public class Knight : Piece
{
    public (int x, int y)[] directions { get; set; }
    public Knight(PieceColor pieceColor)
    {
        directions = new (int x, int y)[8];
        directions[0] = (1, 2);
        directions[1] = (2, 1);
        directions[2] = (2, -1);
        directions[3] = (1, -2);
        directions[4] = (-1, -2);
        directions[5] = (-2, -1);
        directions[6] = (-2, 1);
        directions[7] = (-1, 2);
        hasMoved = false;
        PieceColor = pieceColor;
        PieceType = PieceType.Knight;
    }
    override public bool hasMoved { get; set; }
    override public PieceColor PieceColor { get; set; }
    override public PieceType PieceType { get; set; }
    override public List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        bool isPinned = false;
        (int dirX, int dirY) pinDirection = (0, 0);
        foreach (var pinPiece in board.pinPieces)
        {
            if (pos.x == pinPiece.x && pos.y == pinPiece.y)
            {
                isPinned = true;
                pinDirection.dirX = pinPiece.dirX;
                pinDirection.dirY = pinPiece.dirY;
            }
        }
        if (this.PieceColor == PieceColor.White && !board.isWhiteTurn)
        {
            return validMoves;
        }
        if (this.PieceColor == PieceColor.Black && board.isWhiteTurn)
        {
            return validMoves;
        }
        if (board.checkPieces.Count == 1)
        {
            if (board.pieces[board.checkPieces[0].x][board.checkPieces[0].y]?.PieceType == PieceType.Knight)
            {
                foreach (var dir in directions)
                {
                    int newX = pos.x + dir.x;
                    int newY = pos.y + dir.y;
                    if (!board.IsInBounds(newX, newY)) continue;
                    if (!board.IsOpponentKingAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor))
                    {
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY) validMoves.Add((newX, newY));
                    }
                }
            }
            else
            {
                List<(int x, int y)> potentialValidMoves = new List<(int x, int y)>();
                if (board.pieces[pos.x][pos.y]?.PieceColor == PieceColor.White)
                {
                    int newX = board.whiteKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.whiteKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        if (!board.IsInBounds(newX, newY)) continue;
                        if (!board.IsOpponentKingAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor))
                        {
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                        }
                    }
                }
                else
                {
                    int newX = board.blackKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.blackKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        if (!board.IsInBounds(newX, newY)) continue;
                        if (!board.IsOpponentKingAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor))
                        {
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                        }
                    }
                }
            }
        }
        else if (board.checkPieces.Count > 1)
        {
            return validMoves;
        }
        else
        {
            foreach (var dir in directions)
            {
                int newX = pos.x + dir.x;
                int newY = pos.y + dir.y;
                if (!board.IsInBounds(newX, newY)) continue;
                if (!board.IsOpponentKingAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor))
                {
                    if (!isPinned || (pinDirection.dirX == newX && pinDirection.dirY == newY)) validMoves.Add((newX, newY));
                }
            }
        }

        return validMoves;
    }
    override public bool Equals(Piece? piece)
    {
        return piece != null && piece.PieceColor == this.PieceColor && this.PieceType == this.PieceType;
    }

    override public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "fullName")
        {
            return this.PieceColor.ToString() + " Knight";
        }
        else
        {
            return "k";
        }
    }
}

public class Bishop : Piece
{
    public (int x, int y)[] directions { get; set; }
    public Bishop(PieceColor pieceColor)
    {
        directions = new (int x, int y)[4];
        directions[0] = (1, 1);
        directions[1] = (1, -1);
        directions[2] = (-1, -1);
        directions[3] = (-1, 1);
        hasMoved = false;
        PieceColor = pieceColor;
        PieceType = PieceType.Bishop;
    }
    override public bool hasMoved { get; set; }
    override public PieceColor PieceColor { get; set; }
    override public PieceType PieceType { get; set; }
    override public List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        bool isPinned = false;
        (int dirX, int dirY) pinDirection = (0, 0);
        foreach (var pinPiece in board.pinPieces)
        {
            if (pos.x == pinPiece.x && pos.y == pinPiece.y)
            {
                isPinned = true;
                pinDirection.dirX = pinPiece.dirX;
                pinDirection.dirY = pinPiece.dirY;
            }
        }
        if (this.PieceColor == PieceColor.White && !board.isWhiteTurn)
        {
            return validMoves;
        }
        if (this.PieceColor == PieceColor.Black && board.isWhiteTurn)
        {
            return validMoves;
        }

        if (board.checkPieces.Count == 1)
        {
            if (board.pieces[board.checkPieces[0].x][board.checkPieces[0].y]?.PieceType == PieceType.Knight)
            {
                foreach (var dir in directions)
                {
                    int newX = pos.x + dir.x;
                    int newY = pos.y + dir.y;
                    while (board.IsInBounds(newX, newY))
                    {
                        if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                        {
                            break;
                        }
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY) validMoves.Add((newX, newY));
                        if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                        {
                            break;
                        }
                        newX += dir.x;
                        newY += dir.y;
                    }
                }
            }
            else
            {
                List<(int x, int y)> potentialValidMoves = new List<(int x, int y)>();
                if (board.pieces[pos.x][pos.y]?.PieceColor == PieceColor.White)
                {
                    int newX = board.whiteKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.whiteKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    while (board.IsInBounds(newX, newY) && newX != board.whiteKingLocation.x && newY != board.whiteKingLocation.y)
                    {
                        potentialValidMoves.Add((newX, newY));
                        newX += board.checkPieces[0].dirX;
                        newX += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        while (board.IsInBounds(newX, newY))
                        {
                            if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                            if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            newX += dir.x;
                            newY += dir.y;
                        }
                    }
                }
                else
                {
                    int newX = board.blackKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.blackKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        while (board.IsInBounds(newX, newY))
                        {
                            if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                            if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            newX += dir.x;
                            newY += dir.y;
                        }
                    }
                }
            }
        }
        else if (board.checkPieces.Count > 1)
        {
            return validMoves;
        }
        else
        {
            foreach (var dir in directions)
            {
                int newX = pos.x + dir.x;
                int newY = pos.y + dir.y;
                while (board.IsInBounds(newX, newY))
                {
                    if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                    {
                        break;
                    }
                    if (!isPinned || (pinDirection.dirX == dir.x && pinDirection.dirY == dir.y)) validMoves.Add((newX, newY));
                    if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                    {
                        break;
                    }
                    newX += dir.x;
                    newY += dir.y;
                }
            }
        }

        return validMoves;
    }
    override public bool Equals(Piece? piece)
    {
        return piece != null && piece.PieceColor == this.PieceColor && this.PieceType == this.PieceType;
    }

    override public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "fullName")
        {
            return this.PieceColor.ToString() + " Bishop";
        }
        else
        {
            return "b";
        }
    }
}

public class Rook : Piece
{
    public (int x, int y)[] directions { get; set; }

    public Rook(PieceColor pieceColor)
    {
        directions = new (int x, int y)[8];
        directions[0] = (0, 1);
        directions[1] = (0, -1);
        directions[2] = (1, 0);
        directions[3] = (-1, 0);
        hasMoved = false;
        PieceColor = pieceColor;
        PieceType = PieceType.Rook;
    }
    override public bool hasMoved { get; set; }
    override public PieceColor PieceColor { get; set; }
    override public PieceType PieceType { get; set; }
    override public List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        bool isPinned = false;
        (int dirX, int dirY) pinDirection = (0, 0);
        foreach (var pinPiece in board.pinPieces)
        {
            if (pos.x == pinPiece.x && pos.y == pinPiece.y)
            {
                isPinned = true;
                pinDirection.dirX = pinPiece.dirX;
                pinDirection.dirY = pinPiece.dirY;
            }
        }
        if (this.PieceColor == PieceColor.White && !board.isWhiteTurn)
        {
            return validMoves;
        }
        if (this.PieceColor == PieceColor.Black && board.isWhiteTurn)
        {
            return validMoves;
        }

        if (board.checkPieces.Count == 1)
        {
            if (board.pieces[board.checkPieces[0].x][board.checkPieces[0].y]?.PieceType == PieceType.Knight)
            {
                foreach (var dir in directions)
                {
                    int newX = pos.x + dir.x;
                    int newY = pos.y + dir.y;
                    while (board.IsInBounds(newX, newY))
                    {
                        if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                        {
                            break;
                        }
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY) validMoves.Add((newX, newY));
                        if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                        {
                            break;
                        }
                        newX += dir.x;
                        newY += dir.y;
                    }
                }
            }
            else
            {
                List<(int x, int y)> potentialValidMoves = new List<(int x, int y)>();
                if (board.pieces[pos.x][pos.y]?.PieceColor == PieceColor.White)
                {
                    int newX = board.whiteKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.whiteKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        while (board.IsInBounds(newX, newY))
                        {
                            if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                            if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            newX += dir.x;
                            newY += dir.y;
                        }
                    }
                }
                else
                {
                    int newX = board.blackKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.blackKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        while (board.IsInBounds(newX, newY))
                        {
                            if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                            if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            newX += dir.x;
                            newY += dir.y;
                        }
                    }
                }
            }
        }
        else if (board.checkPieces.Count > 1)
        {
            return validMoves;
        }
        else
        {
            foreach (var dir in directions)
            {
                int newX = pos.x + dir.x;
                int newY = pos.y + dir.y;
                while (board.IsInBounds(newX, newY))
                {
                    if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                    {
                        break;
                    }
                    if (!isPinned || (pinDirection.dirX == dir.x && pinDirection.dirY == dir.y)) validMoves.Add((newX, newY));
                    if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                    {
                        break;
                    }
                    newX += dir.x;
                    newY += dir.y;
                }
            }
        }
        return validMoves;
    }
    override public bool Equals(Piece? piece)
    {
        return piece != null && piece.PieceColor == this.PieceColor && this.PieceType == this.PieceType;
    }

    override public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "fullName")
        {
            return this.PieceColor.ToString() + " Rook";
        }
        else
        {
            return "r";
        }
    }
}

public class Queen : Piece
{
    public (int x, int y)[] directions { get; set; }
    public Queen(PieceColor pieceColor)
    {
        directions = new (int x, int y)[8];
        directions[0] = (0, 1);
        directions[1] = (0, -1);
        directions[2] = (1, 0);
        directions[3] = (-1, 0);
        directions[4] = (1, 1);
        directions[5] = (1, -1);
        directions[6] = (-1, -1);
        directions[7] = (-1, 1);

        hasMoved = false;
        PieceColor = pieceColor;
        PieceType = PieceType.Queen;
    }
    override public bool hasMoved { get; set; }
    override public PieceColor PieceColor { get; set; }
    override public PieceType PieceType { get; set; }
    override public List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        bool isPinned = false;
        (int dirX, int dirY) pinDirection = (0, 0);
        foreach (var pinPiece in board.pinPieces)
        {
            if (pos.x == pinPiece.x && pos.y == pinPiece.y)
            {
                isPinned = true;
                pinDirection.dirX = pinPiece.dirX;
                pinDirection.dirY = pinPiece.dirY;
            }
        }
        if (this.PieceColor == PieceColor.White && !board.isWhiteTurn)
        {
            return validMoves;
        }
        if (this.PieceColor == PieceColor.Black && board.isWhiteTurn)
        {
            return validMoves;
        }

        if (board.checkPieces.Count == 1)
        {
            if (board.pieces[board.checkPieces[0].x][board.checkPieces[0].y]?.PieceType == PieceType.Knight)
            {
                foreach (var dir in directions)
                {
                    int newX = pos.x + dir.x;
                    int newY = pos.y + dir.y;
                    while (board.IsInBounds(newX, newY))
                    {
                        if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                        {
                            break;
                        }
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY) validMoves.Add((newX, newY));
                        if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                        {
                            break;
                        }
                        newX += dir.x;
                        newY += dir.y;
                    }
                }
            }
            else
            {
                List<(int x, int y)> potentialValidMoves = new List<(int x, int y)>();
                if (board.pieces[pos.x][pos.y]?.PieceColor == PieceColor.White)
                {
                    int newX = board.whiteKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.whiteKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        while (board.IsInBounds(newX, newY))
                        {
                            if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                            if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            newX += dir.x;
                            newY += dir.y;
                        }
                    }
                }
                else
                {
                    int newX = board.blackKingLocation.x + board.checkPieces[0].dirX;
                    int newY = board.blackKingLocation.y + board.checkPieces[0].dirY;
                    while (board.IsInBounds(newX, newY))
                    {
                        potentialValidMoves.Add((newX, newY));
                        if (board.checkPieces[0].x == newX && board.checkPieces[0].y == newY)
                        {
                            break;
                        }
                        newX += board.checkPieces[0].dirX;
                        newY += board.checkPieces[0].dirY;
                    }
                    foreach (var dir in directions)
                    {
                        newX = pos.x + dir.x;
                        newY = pos.y + dir.y;
                        while (board.IsInBounds(newX, newY))
                        {
                            if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            if (potentialValidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
                            if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                            {
                                break;
                            }
                            newX += dir.x;
                            newY += dir.y;
                        }
                    }
                }
            }
        }
        else if (board.checkPieces.Count > 1)
        {
            return validMoves;
        }
        else
        {
            foreach (var dir in directions)
            {
                int newX = pos.x + dir.x;
                int newY = pos.y + dir.y;
                while (board.IsInBounds(newX, newY))
                {
                    if (board.IsOpponentKingAt((newX, newY), this.PieceColor) || board.IsAllyPieceAt((newX, newY), this.PieceColor))
                    {
                        break;
                    }
                    if (!isPinned || (pinDirection.dirX == dir.x && pinDirection.dirY == dir.y)) validMoves.Add((newX, newY));
                    if (board.IsOpponentPieceAt((newX, newY), this.PieceColor))
                    {
                        break;
                    }
                    newX += dir.x;
                    newY += dir.y;
                }
            }
        }

        return validMoves;
    }
    override public bool Equals(Piece? piece)
    {
        return piece != null && piece.PieceColor == this.PieceColor && this.PieceType == this.PieceType;
    }

    override public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "fullName")
        {
            return this.PieceColor.ToString() + " Queen";
        }
        else
        {
            return "q";
        }
    }
}

public class King : Piece
{
    public (int x, int y)[] directions { get; set; }
    public King(PieceColor pieceColor)
    {
        directions = new (int x, int y)[8];
        directions[0] = (0, 1);
        directions[1] = (0, -1);
        directions[2] = (1, 0);
        directions[3] = (-1, 0);
        directions[4] = (1, 1);
        directions[5] = (1, -1);
        directions[6] = (-1, -1);
        directions[7] = (-1, 1);
        hasMoved = false;
        PieceColor = pieceColor;
        PieceType = PieceType.King;
    }
    override public bool hasMoved { get; set; }
    override public PieceColor PieceColor { get; set; }
    override public PieceType PieceType { get; set; }
    override public List<(int x, int y)> GetValidMoves(Board board, (int x, int y) pos)
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        if (this.PieceColor == PieceColor.White && !board.isWhiteTurn)
        {
            return validMoves;
        }
        if (this.PieceColor == PieceColor.Black && board.isWhiteTurn)
        {
            return validMoves;
        }
        foreach (var dir in directions)
        {
            int newX = pos.x + dir.x;
            int newY = pos.y + dir.y;
            if (board.IsInBounds(newX, newY) && !board.IsOpponentKingAt((newX, newY), this.PieceColor) && !board.IsAllyPieceAt((newX, newY), this.PieceColor))
            {
                if (!board.kingInvalidMoves.Contains((newX, newY))) validMoves.Add((newX, newY));
            }
        }


        return validMoves;
    }
    override public bool Equals(Piece? piece)
    {
        return piece != null && piece.PieceColor == this.PieceColor && this.PieceType == this.PieceType;
    }

    override public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "fullName")
        {
            return this.PieceColor.ToString() + " King";
        }
        else
        {
            return "K";
        }
    }
}
