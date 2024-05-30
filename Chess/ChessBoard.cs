using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chess
{
    public class ChessBoard
    {
        public ChessPiece[,] Board { get; private set; }
        public bool IsWhiteTurn { get; set; }
        public List<ChessPiece> CapturedWhitePieces { get; private set; }
        public List<ChessPiece> CapturedBlackPieces { get; private set; }

        public ChessBoard()
        {
            Board = new ChessPiece[8, 8];
            CapturedWhitePieces = new List<ChessPiece>();
            CapturedBlackPieces = new List<ChessPiece>();
            InitializeBoard();
        }
        public void MakeMove(Move move)
        {
            int startX = move.StartX;
            int startY = move.StartY;
            int endX = move.EndX;
            int endY = move.EndY;

            ChessPiece piece = Board[startX, startY];
            ChessPiece capturedPiece = Board[endX, endY];

            // Store the captured piece in the move object
            move.CapturedPiece = capturedPiece;

            if (capturedPiece != null)
            {
                if (capturedPiece.IsWhite)
                    CapturedWhitePieces.Add(capturedPiece);
                else
                    CapturedBlackPieces.Add(capturedPiece);
            }

            Board[endX, endY] = piece;
            Board[startX, startY] = null;

            IsWhiteTurn = !IsWhiteTurn;
        }


        public void InitializeBoard()
        {
            Array.Clear(Board, 0, Board.Length);
            CapturedWhitePieces.Clear();
            CapturedBlackPieces.Clear();

            // Place black pieces
            Board[0, 0] = new Rook(false);
            Board[1, 0] = new Knight(false);
            Board[2, 0] = new Bishop(false);
            Board[3, 0] = new Queen(false);
            Board[4, 0] = new King(false);
            Board[5, 0] = new Bishop(false);
            Board[6, 0] = new Knight(false);
            Board[7, 0] = new Rook(false);
            for (int i = 0; i < 8; i++)
                Board[i, 1] = new Pawn(false);

            // Place white pieces
            Board[0, 7] = new Rook(true);
            Board[1, 7] = new Knight(true);
            Board[2, 7] = new Bishop(true);
            Board[3, 7] = new Queen(true);
            Board[4, 7] = new King(true);
            Board[5, 7] = new Bishop(true);
            Board[6, 7] = new Knight(true);
            Board[7, 7] = new Rook(true);
            for (int i = 0; i < 8; i++)
                Board[i, 6] = new Pawn(true);

            IsWhiteTurn = true;
        }

        public bool MovePiece(int startX, int startY, int endX, int endY)
        {
            ChessPiece piece = Board[startX, startY];
            if (piece == null || piece.IsWhite != IsWhiteTurn)
                return false;

            if (piece.IsValidMove(Board, startX, startY, endX, endY))
            {
                // Simulate move
                ChessPiece capturedPiece = Board[endX, endY];
                Board[endX, endY] = piece;
                Board[startX, startY] = null;

                bool isInCheck = IsInCheck(IsWhiteTurn);

                // Undo move
                Board[startX, startY] = piece;
                Board[endX, endY] = capturedPiece;

                if (isInCheck)
                    return false;

                // Make move
                if (capturedPiece != null)
                {
                    if (piece.IsWhite)
                        CapturedWhitePieces.Add(capturedPiece);
                    else
                        CapturedBlackPieces.Add(capturedPiece);
                }

                Board[endX, endY] = piece;
                Board[startX, startY] = null;

                bool isCheck = IsInCheck(!IsWhiteTurn);
                bool isCheckmate = IsCheckmate(!IsWhiteTurn);

                Debug.WriteLine($"isCheck: {isCheck}, isCheckmate: {isCheckmate}");
                IsWhiteTurn = !IsWhiteTurn;

                return true;
            }

            return false;
        }

        public bool IsInCheck(bool isWhite)
        {
            int kingX = -1, kingY = -1;

            // Find the king's position
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Board[x, y] is King && Board[x, y].IsWhite == isWhite)
                    {
                        kingX = x;
                        kingY = y;
                        break;
                    }
                }
                if (kingX != -1)
                    break;
            }

            if (kingX == -1 || kingY == -1)
            {
                Debug.WriteLine($"King for {(isWhite ? "white" : "black")} not found!");
                return false;
            }

            Debug.WriteLine($"King for {(isWhite ? "white" : "black")} found at ({kingX}, {kingY})");

            // Check if any opponent's piece can move to the king's position
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Board[x, y] != null && Board[x, y].IsWhite != isWhite)
                    {
                        if (Board[x, y].IsValidMove(Board, x, y, kingX, kingY))
                        {
                            Debug.WriteLine($"King for {(isWhite ? "white" : "black")} is in check by piece at ({x}, {y})");
                            return true;
                        }
                    }
                }
            }

            Debug.WriteLine($"King for {(isWhite ? "white" : "black")} is not in check");
            return false;
        }

        public bool IsCheckmate(bool isWhite)
        {
            if (!IsInCheck(isWhite))
            {
                Debug.WriteLine($"{(isWhite ? "White" : "Black")} is not in check, so cannot be checkmate");
                return false;
            }

            for (int startX = 0; startX < 8; startX++)
            {
                for (int startY = 0; startY < 8; startY++)
                {
                    if (Board[startX, startY] != null && Board[startX, startY].IsWhite == isWhite)
                    {
                        for (int endX = 0; endX < 8; endX++)
                        {
                            for (int endY = 0; endY < 8; endY++)
                            {
                                if (Board[startX, startY].IsValidMove(Board, startX, startY, endX, endY))
                                {
                                    // Simulate move
                                    ChessPiece capturedPiece = Board[endX, endY];
                                    Board[endX, endY] = Board[startX, startY];
                                    Board[startX, startY] = null;

                                    bool isInCheck = IsInCheck(isWhite);

                                    // Undo move
                                    Board[startX, startY] = Board[endX, endY];
                                    Board[endX, endY] = capturedPiece;

                                    if (!isInCheck)
                                    {
                                        Debug.WriteLine($"Move from ({startX}, {startY}) to ({endX}, {endY}) prevents checkmate");
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Debug.WriteLine($"{(isWhite ? "White" : "Black")} is in checkmate");
            return true;
        }

        public int CalculateScore(List<ChessPiece> capturedPieces)
        {
            int score = 0;
            foreach (var piece in capturedPieces)
            {
                if (piece is Pawn) score += 1;
                if (piece is Knight) score += 3;
                if (piece is Bishop) score += 3;
                if (piece is Rook) score += 5;
                if (piece is Queen) score += 9;
            }
            return score;
        }
    }
}