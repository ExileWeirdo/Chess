using System;

namespace Chess
{
    public enum PieceType
    {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }
    public abstract class ChessPiece
    {
        public PieceType Type { get; set; }

        public bool IsWhite { get; private set; }
        public bool HasMoved { get; set; }

        protected ChessPiece(bool isWhite)
        {
            Type = Type;
            IsWhite = isWhite;
        }


        public abstract bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY);
    }

    public class Rook : ChessPiece
    {
        public Rook(bool isWhite) : base(isWhite) { 
            Type = PieceType.Rook;
        }


        public override bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
        {
            if (startX != endX && startY != endY)
                return false;

            if (board[endX, endY] != null && board[endX, endY].IsWhite == IsWhite)
                return false;

            if (startX == endX)
            {
                int step = startY < endY ? 1 : -1;
                for (int y = startY + step; y != endY; y += step)
                {
                    if (board[startX, y] != null)
                        return false;
                }
            }
            else
            {
                int step = startX < endX ? 1 : -1;
                for (int x = startX + step; x != endX; x += step)
                {
                    if (board[x, startY] != null)
                        return false;
                }
            }
            return true;
        }
    }

    public class Knight : ChessPiece
    {
        public Knight(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
        {
            // Validate that endX and endY are within bounds
            if (endX < 0 || endX >= board.GetLength(0) || endY < 0 || endY >= board.GetLength(1))
                return false;

            // Calculate the L-shaped move
            int dx = Math.Abs(startX - endX);
            int dy = Math.Abs(startY - endY);

            // Check if the move matches the knight's pattern
            if ((dx == 2 && dy == 1) || (dx == 1 && dy == 2))
            {
                // Validate the destination square
                return board[endX, endY] == null || board[endX, endY].IsWhite != IsWhite;
            }

            return false;
        }

    }

    public class Bishop : ChessPiece
    {
        public Bishop(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
        {
            if (Math.Abs(startX - endX) != Math.Abs(startY - endY))
                return false;

            if (board[endX, endY] != null && board[endX, endY].IsWhite == IsWhite)
                return false;

            int stepX = startX < endX ? 1 : -1;
            int stepY = startY < endY ? 1 : -1;
            for (int x = startX + stepX, y = startY + stepY; x != endX; x += stepX, y += stepY)
            {
                if (board[x, y] != null)
                    return false;
            }
            return true;
        }
    }

    public class Queen : ChessPiece
    {
        public Queen(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
        {
            if (startX == endX || startY == endY)
            {
                Rook tempRook = new Rook(IsWhite);
                return tempRook.IsValidMove(board, startX, startY, endX, endY);
            }
            else
            {
                Bishop tempBishop = new Bishop(IsWhite);
                return tempBishop.IsValidMove(board, startX, startY, endX, endY);
            }
        }
    }

    public class King : ChessPiece
    {
        public King(bool isWhite) : base(isWhite) {
            Type = PieceType.King;
        }

        public override bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
        {
            int dx = Math.Abs(startX - endX);
            int dy = Math.Abs(startY - endY);
            if (dx <= 1 && dy <= 1)
            {
                return board[endX, endY] == null || board[endX, endY].IsWhite != IsWhite;
            }
            return false;
        }
    }

    public class Pawn : ChessPiece
    {
        public Pawn(bool isWhite) : base(isWhite) { }

        public override bool IsValidMove(ChessPiece[,] board, int startX, int startY, int endX, int endY)
        {
            int direction = IsWhite ? -1 : 1;
            if (startX == endX && board[endX, endY] == null)
            {
                if (startY + direction == endY)
                    return true;
                if ((IsWhite && startY == 6 || !IsWhite && startY == 1) && startY + 2 * direction == endY)
                    return true;
            }
            else if (Math.Abs(startX - endX) == 1 && startY + direction == endY && board[endX, endY] != null)
            {
                return board[endX, endY].IsWhite != IsWhite;
            }
            return false;
        }
    }
}