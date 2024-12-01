using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Chess
{
    public class ChessBoard
    {
        private const int BoardSize = 8;
        public ChessPiece[,] Board { get; private set; }
        public bool IsWhiteTurn { get; set; }
        public List<ChessPiece> CapturedWhitePieces { get; private set; }
        public List<ChessPiece> CapturedBlackPieces { get; private set; }
        public Move LastMove { get; set; }
        public Dictionary<string, int> boardStates;
        public event Action DisableButtonsEvent;
        public int TurnCount { get; private set; } = 0;

        public ChessBoard()
        {
            Board = new ChessPiece[8, 8];
            CapturedWhitePieces = new List<ChessPiece>();
            CapturedBlackPieces = new List<ChessPiece>();
            boardStates = new Dictionary<string, int>();
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

            // Track captured piece and update board
            move.CapturedPiece = capturedPiece;
            if (capturedPiece != null)
            {
                Debug.WriteLine($"Captured White Pieces: {CapturedWhitePieces.Count}, Captured Black Pieces: {CapturedBlackPieces.Count}");
                if (capturedPiece.IsWhite) // Captured piece is white
                {
                    CapturedWhitePieces.Add(capturedPiece); // Add to black's captured pieces
                }
                else // Captured piece is black
                {
                    CapturedBlackPieces.Add(capturedPiece); // Add to white's captured pieces
                }
            }

            // Move the piece to the new position
            Board[endX, endY] = piece;
            Board[startX, startY] = null;

            // Switch turns
            IsWhiteTurn = !IsWhiteTurn;

            // Record the last move
            LastMove = move;

            // Increment the turn counter
            TurnCount++;
        }

        public void SimulateMove(Move move)
        {
            int startX = move.StartX;
            int startY = move.StartY;
            int endX = move.EndX;
            int endY = move.EndY;

            ChessPiece piece = Board[startX, startY];
            ChessPiece capturedPiece = Board[endX, endY];

            // Don't actually modify the captured pieces list during simulation
            move.CapturedPiece = capturedPiece;

            // Temporarily execute the move, without updating the captured pieces
            Board[endX, endY] = piece;
            Board[startX, startY] = null;
        }

        public void UndoSimulatedMove(Move move)
        {
            int startX = move.StartX;
            int startY = move.StartY;
            int endX = move.EndX;
            int endY = move.EndY;

            ChessPiece piece = Board[endX, endY];
            ChessPiece capturedPiece = move.CapturedPiece;

            // Undo the simulated move by restoring the previous board state
            Board[startX, startY] = piece;
            Board[endX, endY] = capturedPiece;
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
            LastMove = null;
            TrackBoardState();
        }

        private void TrackBoardState()
        {
            string boardState = GetBoardState();
            if (boardStates.ContainsKey(boardState))
            {
                boardStates[boardState]++;
            }
            else
            {
                boardStates[boardState] = 1;
            }
        }

        private string GetBoardState()
        {
            string boardState = "";
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = Board[x, y];
                    boardState += piece != null ? GetPieceSymbol(piece) : ".";
                }
            }
            return boardState;
        }

        private string GetPieceSymbol(ChessPiece piece)
        {
            if (piece is Rook) return piece.IsWhite ? "♖" : "♜";
            if (piece is Knight) return piece.IsWhite ? "♘" : "♞";
            if (piece is Bishop) return piece.IsWhite ? "♗" : "♝";
            if (piece is Queen) return piece.IsWhite ? "♕" : "♛";
            if (piece is King) return piece.IsWhite ? "♔" : "♚";
            if (piece is Pawn) return piece.IsWhite ? "♙" : "♟";
            return "";
        }

        public bool MovePiece(int startX, int startY, int endX, int endY)
        {
            ChessPiece piece = Board[startX, startY];
            if (piece == null || piece.IsWhite != IsWhiteTurn)
                return false;

            if (piece is Pawn pawn && IsValidEnPassantMove(startX, startY, endX, endY))
            {
                // Perform En passant capture
                Board[endX, endY] = piece;
                Board[startX, startY] = null;
                Board[endX, startY] = null; // Remove the captured pawn
                ChessPiece capturedPiece = Board[endX, endY];
                if (piece.IsWhite)
                    CapturedWhitePieces.Add(capturedPiece);
                else
                    CapturedBlackPieces.Add(capturedPiece);
                TrackBoardState();
                CheckForDrawByRepetition();
                CheckForStalemateOrCheckmate();
                
                IsWhiteTurn = !IsWhiteTurn;
                LastMove = new Move(startX, startY, endX, endY, piece);
                return true;
            }

            if (piece is King king && Math.Abs(startX - endX) == 2 && startY == endY)
            {
                if (CanCastle(piece.IsWhite, endX == 6))
                {
                    // Perform castling
                    Board[endX, endY] = piece;
                    Board[startX, startY] = null;

                    if (endX == 6) // Kingside
                    {
                        Board[5, startY] = Board[7, startY];
                        Board[7, startY] = null;
                    }
                    else // Queenside
                    {
                        Board[3, startY] = Board[0, startY];
                        Board[0, startY] = null;
                    }

                    IsWhiteTurn = !IsWhiteTurn;
                    king.HasMoved = true;
                    if (endX == 6)
                        ((Rook)Board[5, startY]).HasMoved = true;
                    else
                        ((Rook)Board[3, startY]).HasMoved = true;
                    TrackBoardState();
                    CheckForDrawByRepetition();
                    CheckForStalemateOrCheckmate();
                    return true;
                }
                else
                {
                    return false;
                }
            }
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

                if (piece is Pawn)
                {
                    if ((piece.IsWhite && endY == 0) || (!piece.IsWhite && endY == 7))
                    {
                        PromotePawn(endX, endY);
                    }
                }

                bool isCheck = IsInCheck(!IsWhiteTurn);
                bool isCheckmate = IsCheckmate(!IsWhiteTurn);

               
                IsWhiteTurn = !IsWhiteTurn;

                piece.HasMoved = true;
                LastMove = new Move(startX, startY, endX, endY, piece);
                TrackBoardState();
                CheckForDrawByRepetition();
                CheckForStalemateOrCheckmate();
                return true;
            }

            return false;
        }

        private void CheckForDrawByRepetition()
        {
            string boardState = GetBoardState();
            if (boardStates.ContainsKey(boardState) && boardStates[boardState] >= 3)
            {
                MessageBox.Show("Draw by threefold repetition!");
                DisableButtonsEvent?.Invoke();
            }
        }

        private void CheckForStalemateOrCheckmate()
        {
            if (IsCheckmate(IsWhiteTurn))
            {
                MessageBox.Show($"{(!IsWhiteTurn ? "White" : "Black")} wins by checkmate!");
                DisableButtonsEvent?.Invoke();
                
            }
            else if (IsStalemate(IsWhiteTurn))
            {
                MessageBox.Show("Stalemate!");
                DisableButtonsEvent?.Invoke();
            }
        }

        public bool IsStalemate(bool isWhite)
        {
            Debug.WriteLine($"checking if {(IsWhiteTurn ? "White" : "Black")} is stalemated");
            if (IsInCheck(isWhite))
                return false;

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = Board[x, y];
                    if (piece != null && piece.IsWhite == isWhite)
                    {
                        for (int endX = 0; endX < BoardSize; endX++)
                        {
                            for (int endY = 0; endY < BoardSize; endY++)
                            {
                                if (piece.IsValidMove(Board, x, y, endX, endY))
                                {
                                    // Simulate the move
                                    ChessPiece capturedPiece = Board[endX, endY];
                                    Board[endX, endY] = piece;
                                    Board[x, y] = null;

                                    bool isInCheck = IsInCheck(isWhite);

                                    // Undo the move
                                    Board[x, y] = piece;
                                    Board[endX, endY] = capturedPiece;

                                    if (!isInCheck)
                                    {
                                        Debug.WriteLine($"{(IsWhiteTurn ? "White" : "Black")} is not stalemated");
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Debug.WriteLine($"{(IsWhiteTurn ? "White" : "Black")} is stalemated");
            return true;
        }


        public bool IsValidEnPassantMove(int startX, int startY, int endX, int endY)
        {
            if (LastMove == null)
                return false;

            ChessPiece piece = Board[startX, startY];
            if (!(piece is Pawn))
                return false;

            int direction = piece.IsWhite ? -1 : 1;
            if (startY == (piece.IsWhite ? 3 : 4) && Math.Abs(startX - endX) == 1 && endY - startY == direction)
            {
                if (LastMove.Piece is Pawn && LastMove.Piece.IsWhite != piece.IsWhite &&
                    LastMove.EndX == endX && LastMove.EndY == startY && Math.Abs(LastMove.StartY - LastMove.EndY) == 2)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInCheckAfterMove(int startX, int startY, int endX, int endY, bool isWhite)
        {
            ChessPiece originalPiece = Board[endX, endY];
            ChessPiece movingPiece = Board[startX, startY];

            // Simulate the move
            Board[endX, endY] = movingPiece;
            Board[startX, startY] = null;

            bool isInCheck = IsInCheck(isWhite);

            // Undo the move
            Board[startX, startY] = movingPiece;
            Board[endX, endY] = originalPiece;

            return isInCheck;
        }

        public void PromotePawn(int x, int y)
        {
            // Create a form to select the piece type
            Form promotionForm = new Form
            {
                Width = 200,
                Height = 150,
                Text = "Promote Pawn"
            };

            ComboBox pieceSelection = new ComboBox
            {
                Width = 100,
                Location = new Point(50, 20)
            };
            pieceSelection.Items.AddRange(new string[] { "Queen", "Rook", "Bishop", "Knight" });
            pieceSelection.SelectedIndex = 0; // Default to Queen

            Button confirmButton = new Button
            {
                Text = "Confirm",
                Width = 100,
                Location = new Point(50, 60)
            };
            confirmButton.Click += (sender, e) =>
            {
                string selectedPiece = pieceSelection.SelectedItem.ToString();
                ChessPiece newPiece;
                switch (selectedPiece)
                {
                    case "Rook":
                        newPiece = new Rook(Board[x, y].IsWhite);
                        break;
                    case "Bishop":
                        newPiece = new Bishop(Board[x, y].IsWhite);
                        break;
                    case "Knight":
                        newPiece = new Knight(Board[x, y].IsWhite);
                        break;
                    case "Queen":
                    default:
                        newPiece = new Queen(Board[x, y].IsWhite);
                        break;
                }
                Board[x, y] = newPiece;
                promotionForm.Close();

            };

            promotionForm.Controls.Add(pieceSelection);
            promotionForm.Controls.Add(confirmButton);
            promotionForm.ShowDialog();
        }

        public bool CanCastle(bool isWhite, bool kingside)
        {
            int y = isWhite ? 7 : 0;

            if (kingside)
            {
                // Check if the squares between the king and rook are empty
                if (Board[5, y] == null && Board[6, y] == null)
                {
                    // Check if the king and rook have not moved and the squares are not attacked
                    if (Board[4, y] is King king && !king.HasMoved &&
                        Board[7, y] is Rook rook && !rook.HasMoved)
                    {
                        // Check if the king is in check, or if it moves through or into check
                        if (!IsInCheck(isWhite) && !IsInCheckAfterMove(4, y, 5, y, isWhite) && !IsInCheckAfterMove(4, y, 6, y, isWhite))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                // Check if the squares between the king and rook are empty
                if (Board[1, y] == null && Board[2, y] == null && Board[3, y] == null)
                {
                    // Check if the king and rook have not moved and the squares are not attacked
                    if (Board[4, y] is King king && !king.HasMoved &&
                        Board[0, y] is Rook rook && !rook.HasMoved)
                    {
                        // Check if the king is in check, or if it moves through or into check
                        if (!IsInCheck(isWhite) && !IsInCheckAfterMove(4, y, 3, y, isWhite) && !IsInCheckAfterMove(4, y, 2, y, isWhite))
                        {
                            return true;
                        }
                    }
                }
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
                
                return false;
            }

           

            // Check if any opponent's piece can move to the king's position
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Board[x, y] != null && Board[x, y].IsWhite != isWhite)
                    {
                        if (Board[x, y].IsValidMove(Board, x, y, kingX, kingY))
                        {
                            
                            return true;
                        }
                    }
                }
            }

           
            return false;
        }

        public bool IsCheckmate(bool isWhite)
        {
            if (!IsInCheck(isWhite))
            {
                
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