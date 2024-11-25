using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using static System.Windows.Forms.AxHost;
//new update

namespace Chess
{

    public partial class Form1 : Form
    {
        private const int BoardSize = 8;
        private Button[,] buttons = new Button[BoardSize, BoardSize];
        private ChessBoard chessBoard = new ChessBoard();
        private int selectedX = -1;
        private int selectedY = -1;
        bool canMoveAI = false;


        public enum GameMode
        {
            TwoPlayer,
            SinglePlayer,
            none
        }
        public enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard
        }
        private GameMode gameMode;
        private DifficultyLevel currentDifficultyLevel;
        private Label lblWhiteCaptured;
        private Label lblBlackCaptured;

        public Form1()
        {
            InitializeComponent();
            ShowModeSelectionMenu();
            chessBoard.DisableButtonsEvent += DisableChessBoardButtons;

        }

        private void BtnTwoPlayerMode_Click(object sender, EventArgs e)
        {
            StartTwoPlayerGame();
            gameMode = GameMode.TwoPlayer;
        }

        private void BtnSinglePlayerMode_Click(object sender, EventArgs e)
        {
            StartSinglePlayerGame();
            gameMode = GameMode.SinglePlayer;
        }

        private void ShowModeSelectionMenu()
        {
            this.ClientSize = new Size(225, 150);
            Button btnTwoPlayerMode = new Button
            {
                Text = "Two Player Mode",
                Location = new Point(10, 70),
                Width = 200,
                Height = 50
            };
            btnTwoPlayerMode.Click += new EventHandler(BtnTwoPlayerMode_Click);

            Button btnSinglePlayerMode = new Button
            {
                Text = "Single Player Mode",
                Location = new Point(10, 10),
                Width = 200,
                Height = 50
            };
            btnSinglePlayerMode.Click += new EventHandler(BtnSinglePlayerMode_Click);

            this.Controls.Add(btnTwoPlayerMode);
            this.Controls.Add(btnSinglePlayerMode);

            btnTwoPlayerMode.Click += (sender, e) => StartGame(GameMode.TwoPlayer);
            btnSinglePlayerMode.Click += (sender, e) => StartGame(GameMode.SinglePlayer);
        }

        private void StartGame(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.TwoPlayer:
                    StartTwoPlayerGame();
                    break;
                case GameMode.SinglePlayer:
                    StartSinglePlayerGame();
                    break;
            }
        }

        private void StartTwoPlayerGame()
        {
            this.Controls.Clear();
            InitializeBoard();
        }

        private void StartSinglePlayerGame()
        {
            this.Controls.Clear();
            this.ClientSize = new Size(225, 280);
            Button btnBack = new Button
            {
                Text = "Back",
                Location = new Point(10, 220),
                Width = 200,
                Height = 50
            };
            btnBack.Click += BtnBack_Click;

            Button btnEasy = new Button
            {
                Text = "Easy",
                Location = new Point(10, 10),
                Width = 200,
                Height = 50
            };
            btnEasy.Click += new EventHandler(BtnEasy_Click);

            Button btnMedium = new Button
            {
                Text = "Medium",
                Location = new Point(10, 70),
                Width = 200,
                Height = 50
            };
            btnMedium.Click += new EventHandler(BtnMedium_Click);

            Button btnHard = new Button
            {
                Text = "Hard",
                Location = new Point(10, 130),
                Width = 200,
                Height = 50
            };
            btnHard.Click += new EventHandler(BtnHard_Click);

            this.Controls.Add(btnEasy);
            this.Controls.Add(btnMedium);
            this.Controls.Add(btnHard);
            this.Controls.Add(btnBack);
        }

        private void BtnHard_Click(object sender, EventArgs e)
        {
            currentDifficultyLevel = DifficultyLevel.Hard;
            StartAIGame();
        }

        private void BtnMedium_Click(object sender, EventArgs e)
        {
            currentDifficultyLevel = DifficultyLevel.Medium;
            StartAIGame();
        }

        private void BtnEasy_Click(object sender, EventArgs e)
        {
            currentDifficultyLevel = DifficultyLevel.Easy;
            StartAIGame();
        }
        private void StartAIGame()
        {
            this.Controls.Clear();

            // Initialize game board and UI
            InitializeBoard();
        }


        private List<(int startX, int startY, int endX, int endY)> GetAvailableMovesForAI()
        {
            List<(int startX, int startY, int endX, int endY)> availableMoves = new List<(int, int, int, int)>();

            // Iterate over the board to find all available moves for AI's pieces
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = chessBoard.Board[x, y];
                    if (piece != null && !piece.IsWhite)
                    {
                        for (int endX = 0; endX < BoardSize; endX++)
                        {
                            for (int endY = 0; endY < BoardSize; endY++)
                            {
                                if (piece.IsValidMove(chessBoard.Board, x, y, endX, endY))
                                {
                                    availableMoves.Add((x, y, endX, endY));
                                }
                            }
                        }
                    }
                }
            }

            return availableMoves;
        }
        private static List<(int startX, int startY, int endX, int endY)> GetPossibleMovesForPiece(ChessPiece[,] board, int startX, int startY)
        {
            List<(int startX, int startY, int endX, int endY)> moves = new List<(int, int, int, int)>();
            for (int endX = 0; endX < 8; endX++)
            {
                for (int endY = 0; endY < 8; endY++)
                {
                    ChessPiece piece = board[startX, startY];
                    if (piece != null && piece.IsValidMove(board, startX, startY, endX, endY))
                    {
                        moves.Add((startX, startY, endX, endY));
                    }
                }
            }
            return moves;
        }



        private void MakeEasyAIMove()
        {
            bool isInCheck = chessBoard.IsInCheck(false);
            List<(int startX, int startY, int endX, int endY)> availableMoves = GetAvailableMovesForAI();

            if (availableMoves.Count == 0)
            {
                return;
            }

            if (isInCheck)
            {
                // If in check, get defensive moves
                availableMoves = GetDefensiveMovesForAI();
            }
            else
            {
                // Otherwise, get all available moves
                availableMoves = GetAvailableMovesForAI();
            }


            // Randomly select a move from available moves
            Random rand = new Random();
            var move = availableMoves[rand.Next(availableMoves.Count)];

            // Execute the selected move
            chessBoard.MovePiece(move.startX, move.startY, move.endX, move.endY);



            UpdateBoardUI();



        }





        private void MakeMediumAIMove()
        {
            List<(int startX, int startY, int endX, int endY)> availableMoves = GetAvailableMovesForAI();

            if (availableMoves.Count == 0)
            {
                return;
            }

            // Define the value of each chess piece
            Dictionary<Type, int> pieceValues = new Dictionary<Type, int>
            {
                { typeof(Pawn), 1 },
                { typeof(Knight), 3 },
                { typeof(Bishop), 3 },
                { typeof(Rook), 5 },
                { typeof(Queen), 9 },
                { typeof(King), 100 } // King's value is high to prioritize checkmate
            };

            bool isInCheck = chessBoard.IsInCheck(false); // Check if AI's king is in check (assuming AI is black)

            if (isInCheck)
            {
                List<(int startX, int startY, int endX, int endY)> defendMoves = GetDefensiveMovesForAI();
                if (defendMoves.Count > 0)
                {
                    // Randomly select a defensive move
                    Random rand = new Random();
                    var defensiveMove = defendMoves[rand.Next(defendMoves.Count)];
                    chessBoard.MovePiece(defensiveMove.startX, defensiveMove.startY, defensiveMove.endX, defensiveMove.endY);
                    UpdateBoardUI();
                    return;
                }
            }

            int maxDifference = int.MinValue;
            (int startX, int startY, int endX, int endY) bestMove = availableMoves[0];

            foreach (var move in availableMoves)
            {
                ChessPiece targetPiece = chessBoard.Board[move.endX, move.endY];
                if (targetPiece != null && targetPiece.IsWhite != chessBoard.IsWhiteTurn)
                {
                    int valueDifference = pieceValues[targetPiece.GetType()] - pieceValues[chessBoard.Board[move.startX, move.startY].GetType()];
                    if (valueDifference > maxDifference)
                    {
                        maxDifference = valueDifference;
                        bestMove = move;
                    }
                }
            }

            // If no capturing moves found, select a random move
            if (maxDifference == int.MinValue)
            {
                Random rand = new Random();
                bestMove = availableMoves[rand.Next(availableMoves.Count)];
            }

            // Execute the best move found
            chessBoard.MovePiece(bestMove.startX, bestMove.startY, bestMove.endX, bestMove.endY);

            // Update the board UI
            UpdateBoardUI();
        }
        private void MakeHardAIMove()
        {
            ChessAI ai = new ChessAI(false, chessBoard, false); // Assuming AI plays black
            Move bestMove = ai.FindBestMove(4); // Depth 4 for example

            if (bestMove != null)
            {
                chessBoard.MakeMove(bestMove);
            }
            UpdateBoardUI();
        }
        private static int EvaluateControlOfMiddle(ChessPiece piece, int x, int y, ChessBoard chessBoard)
        {
            int[,] middleSquares = { { 3, 3 }, { 3, 4 }, { 4, 3 }, { 4, 4 } };
            for (int i = 0; i < middleSquares.GetLength(0); i++)
            {
                int middleX = middleSquares[i, 0];
                int middleY = middleSquares[i, 1];

                if (piece.IsValidMove(chessBoard.Board, x, y, middleX, middleY))
                {
                    return 20; 
                }
            }
            return 0;
        }

        private static int EvaluatePressureOnKing(ChessPiece[,] board, Position kingPosition, ChessPiece piece, int startX, int startY, int endX, int endY)
        {
            int score = 0;

            // Simulate the move
            ChessPiece[,] simulatedBoard = CloneBoard(board); // Ensure CloneBoard is also static
            simulatedBoard[endX, endY] = simulatedBoard[startX, startY];
            simulatedBoard[startX, startY] = null;

            // Check restricted squares around the king
            int restrictedSquares = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int kingX = kingPosition.Row + i;
                    int kingY = kingPosition.Col + j;

                    if (kingX >= 0 && kingX < 8 && kingY >= 0 && kingY < 8)
                    {
                        if (simulatedBoard[kingX, kingY] == null || simulatedBoard[kingX, kingY].IsWhite != piece.IsWhite)
                        {
                            restrictedSquares++;
                        }
                    }
                }
            }

            

            score += restrictedSquares * 15;

            if (IsPieceCapturedForLessMaterial(simulatedBoard, endX, endY)) 
            {
                score -= 30; 
            }

            return score;
        }


        private static bool IsPieceCapturedForLessMaterial(ChessPiece[,] board, int x, int y)
        {
            ChessPiece targetPiece = board[x, y];
            if (targetPiece == null) return false;


            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int attackX = x + i;
                    int attackY = y + j;

                    if (attackX >= 0 && attackX < 8 && attackY >= 0 && attackY < 8)
                    {
                        ChessPiece attackingPiece = board[attackX, attackY];
                        if (attackingPiece != null && attackingPiece.IsWhite != targetPiece.IsWhite && GetPieceValue(attackingPiece) < GetPieceValue(targetPiece))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private int EvaluateKingPositionSafety(ChessPiece[,] board, Position kingPosition, PieceColor color)
        {
            int score = 0;

            // Penalize open files around the king
            for (int i = -1; i <= 1; i++)
            {
                int file = kingPosition.Col + i;
                if (file >= 0 && file < BoardSize && !IsPawnOnFile(board, kingPosition.Row, file, color == PieceColor.White))
                {
                    score -= 10;
                }
            }

            // Penalize exposed positions (fewer defenders)
            int defenders = CountDefenders(board, kingPosition, color);
            score -= (3 - defenders) * 10; // Less than 3 defenders is penalized

            return score;
        }
        private int CountDefenders(ChessPiece[,] board, Position pos, PieceColor color)
        {
            int defenders = 0;

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = board[x, y];
                    if (piece != null && piece.IsWhite == (color == PieceColor.White)) // Friendly piece
                    {
                        if (piece.IsValidMove(board, x, y, pos.Row, pos.Col))
                        {
                            defenders++;
                        }
                    }
                }
            }

            return defenders;
        }
        public enum PieceColor
        {
            White,
            Black
        }


        private int EvaluatePawnStructure(bool isWhite)
        {
            int score = 0;

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (chessBoard.Board[x, y] is Pawn pawn && pawn.IsWhite == isWhite)
                    {
                        // Penalize doubled pawns
                        for (int i = 0; i < BoardSize; i++)
                        {
                            if (i != x && chessBoard.Board[i, y] is Pawn otherPawn && otherPawn.IsWhite == isWhite)
                            {
                                score -= 10;
                            }
                        }

                        // Penalize isolated pawns
                        if ((y == 0 || !IsPawnOnFile(chessBoard.Board, x, y - 1, isWhite)) &&
                            (y == 7 || !IsPawnOnFile(chessBoard.Board, x, y + 1, isWhite)))
                        {
                            score -= 15;
                        }

                        // Reward passed pawns
                        if (IsPassedPawn(chessBoard.Board, x, y, isWhite))
                        {
                            score += 20;
                        }
                    }
                }
            }

            return score;
        }
        private bool IsPawnOnFile(ChessPiece[,] board, int x, int y, bool isWhite)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize) return false;
            return board[x, y] is Pawn pawn && pawn.IsWhite == isWhite;
        }
        private bool IsPassedPawn(ChessPiece[,] board, int x, int y, bool isWhite)
        {
            int direction = isWhite ? -1 : 1;

            for (int newY = y + direction; newY >= 0 && newY < BoardSize; newY += direction)
            {
                for (int i = 0; i < BoardSize; i++)
                {
                    if (i != x && board[i, newY] is Pawn pawn && pawn.IsWhite != isWhite)
                    {
                        return false; // Opponent pawn blocks the advance
                    }
                }
            }
            return true;
        }



        public class ChessAI
        {
            public bool IsWhite { get; private set; }
            public ChessBoard Board { get; private set; }
            public bool IsWhiteTurn { get; set; }  // Change to set so it can be modified

            public ChessAI(bool isWhite, ChessBoard board, bool isAIStartingTurn)
            {
                IsWhite = isWhite;
                Board = board;
                IsWhiteTurn = isAIStartingTurn;
            }

            public void UndoMove(Move move)
            {
                int startX = move.StartX;
                int startY = move.StartY;
                int endX = move.EndX;
                int endY = move.EndY;

                ChessPiece movedPiece = Board.Board[endX, endY];
                ChessPiece capturedPiece = move.CapturedPiece; // Assume the move object contains information about the captured piece

                // Restore the moved piece to its original position
                Board.Board[endX, endY] = null;
                Board.Board[startX, startY] = movedPiece;

                // If a piece was captured, restore it to its original position
                if (capturedPiece != null)
                {
                    Board.Board[endX, endY] = capturedPiece;
                    // Update the captured pieces list accordingly
                    if (capturedPiece.IsWhite)
                    {
                        Board.CapturedWhitePieces.Remove(capturedPiece);
                    }
                    else
                    {
                        Board.CapturedBlackPieces.Remove(capturedPiece);
                    }
                }

                // Toggle the turn after undoing the move
                IsWhiteTurn = !IsWhiteTurn;
            }


            public Move FindBestMove(int depth)
            {
                Move bestMove = null;
                int bestScore = int.MinValue;
                List<Move> bestMoves = new List<Move>();

                List<Move> possibleMoves = GenerateMoves();
                foreach (Move move in possibleMoves)
                {
                    Board.MakeMove(move);
                    int score = AlphaBeta(depth - 1, int.MinValue, int.MaxValue, false);
                    UndoMove(move);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoves.Clear();
                        bestMoves.Add(move);
                    }
                    else if (score == bestScore)
                    {
                        bestMoves.Add(move);
                    }
                }

                Random rand = new Random();
                bestMove = bestMoves[rand.Next(bestMoves.Count)]; // Select a random move among the best moves
                return bestMove;
            }

            private List<Move> GenerateMoves()
            {
                List<Move> possibleMoves = new List<Move>();

                // Iterate over the board to generate all possible moves
                for (int startX = 0; startX < 8; startX++)
                {
                    for (int startY = 0; startY < 8; startY++)
                    {
                        ChessPiece piece = Board.Board[startX, startY];
                        if (piece != null && piece.IsWhite == IsWhite)
                        {
                            // Generate moves for each piece
                            for (int endX = 0; endX < 8; endX++)
                            {
                                for (int endY = 0; endY < 8; endY++)
                                {
                                    if (piece.IsValidMove(Board.Board, startX, startY, endX, endY))
                                    {
                                        possibleMoves.Add(new Move(startX, startY, endX, endY, piece));
                                    }
                                }
                            }
                        }
                    }
                }

                return possibleMoves;
            }
            private int Minimax(int depth, bool isMaximizingPlayer)
            {
                if (depth == 0)
                {
                    return EvaluateBoard(this.Board); // Pass the ChessBoard instance
                }

                List<Move> possibleMoves = GenerateMoves();

                if (isMaximizingPlayer)
                {
                    int maxEval = int.MinValue;
                    foreach (Move move in possibleMoves)
                    {
                        this.Board.MakeMove(move); // Apply the move
                        int eval = Minimax(depth - 1, false); // Recurse
                        UndoMove(move); // Undo the move
                        maxEval = Math.Max(maxEval, eval); // Maximize
                    }
                    return maxEval;
                }
                else
                {
                    int minEval = int.MaxValue;
                    foreach (Move move in possibleMoves)
                    {
                        this.Board.MakeMove(move);
                        int eval = Minimax(depth - 1, true);
                        UndoMove(move);
                        minEval = Math.Min(minEval, eval); // Minimize
                    }
                    return minEval;
                }
            }


            private int AlphaBeta(int depth, int alpha, int beta, bool isMaximizingPlayer)
            {
                if (depth == 0)
                {
                    return EvaluateBoard(this.Board); // Pass the ChessBoard instance
                }

                List<Move> possibleMoves = GenerateMoves();

                if (isMaximizingPlayer)
                {
                    int maxEval = int.MinValue;
                    foreach (Move move in possibleMoves)
                    {
                        this.Board.MakeMove(move);
                        int eval = AlphaBeta(depth - 1, alpha, beta, false);
                        UndoMove(move);
                        maxEval = Math.Max(maxEval, eval);
                        alpha = Math.Max(alpha, eval);
                        if (beta <= alpha)
                            break; // Beta cut-off
                    }
                    return maxEval;
                }
                else
                {
                    int minEval = int.MaxValue;
                    foreach (Move move in possibleMoves)
                    {
                        this.Board.MakeMove(move);
                        int eval = AlphaBeta(depth - 1, alpha, beta, true);
                        UndoMove(move);
                        minEval = Math.Min(minEval, eval);
                        beta = Math.Min(beta, eval);
                        if (beta <= alpha)
                            break; // Alpha cut-off
                    }
                    return minEval;
                }
            }


            private int EvaluateBoard(ChessBoard chessBoard)
            {
                int score = 0;

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = chessBoard.Board[x, y];
                        if (piece != null)
                        {
                            int pieceValue = GetPieceValue(piece);
                            score += piece.IsWhite == chessBoard.IsWhiteTurn ? pieceValue : -pieceValue;

                            // Positional adjustments
                            score += EvaluateControlOfMiddle(piece, x, y, chessBoard);

                            // Evaluate pressure based on possible moves
                            if (piece.IsWhite != chessBoard.IsWhiteTurn)
                            {
                                Position opponentKingPosition = FindOpponentKing(chessBoard, piece.IsWhite);
                                var possibleMoves = GetPossibleMovesForPiece(chessBoard.Board, x, y);

                                foreach (var move in possibleMoves)
                                {
                                    score += EvaluatePressureOnKing(
                                        chessBoard.Board,
                                        opponentKingPosition,
                                        piece,
                                        move.startX,
                                        move.startY,
                                        move.endX,
                                        move.endY
                                    );
                                }
                            }
                        }
                    }
                }

                return score;
            }


            private Position FindOpponentKing(ChessBoard chessBoard, bool isWhite)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = chessBoard.Board[x, y];
                        if (piece is King && piece.IsWhite != isWhite)
                        {
                            return new Position(x, y);
                        }
                    }
                }
                return null;
            }



            private int EvaluatePosition(ChessPiece piece, int x, int y)
            {
                int positionScore = 0;
                // Example: Favor central squares for pieces
                if (piece is Pawn)
                {
                    positionScore += (int)(10 - Math.Abs(x - 3.5) - Math.Abs(y - 3.5));
                }
                else if (piece is Knight || piece is Bishop)
                {
                    positionScore += (int)(5 - Math.Abs(x - 3.5) - Math.Abs(y - 3.5));
                }
                // Add more rules for other pieces...

                return positionScore;
            }

            private int GetPieceValue(ChessPiece piece)
            {
                switch (piece.Type)
                {
                    case PieceType.Pawn: return 100;
                    case PieceType.Knight: return 320;
                    case PieceType.Bishop: return 330;
                    case PieceType.Rook: return 500;
                    case PieceType.Queen: return 900;
                    case PieceType.King: return 20000;
                    default: return 0;
                }
            }
        }







        private List<(int startX, int startY, int endX, int endY)> GetDefensiveMovesForAI()
        {
            List<(int startX, int startY, int endX, int endY)> defensiveMoves = new List<(int, int, int, int)>();

            // Iterate over the board to find defensive moves that protect the king
            for (int startX = 0; startX < BoardSize; startX++)
            {
                for (int startY = 0; startY < BoardSize; startY++)
                {
                    ChessPiece piece = chessBoard.Board[startX, startY];
                    if (piece != null && !piece.IsWhite) // Only consider black pieces
                    {
                        // Check if the piece can move to a position that defends the king
                        for (int endX = 0; endX < BoardSize; endX++)
                        {
                            for (int endY = 0; endY < BoardSize; endY++)
                            {
                                if (piece.IsValidMove(chessBoard.Board, startX, startY, endX, endY))
                                {
                                    // Simulate the move
                                    ChessPiece[,] simulatedBoard = CloneBoard(chessBoard.Board);
                                    simulatedBoard[endX, endY] = simulatedBoard[startX, startY];
                                    simulatedBoard[startX, startY] = null;

                                    // Check if the move defends the king
                                    if (!IsKingInCheck(simulatedBoard, false))
                                    {
                                        defensiveMoves.Add((startX, startY, endX, endY));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return defensiveMoves;
        }

        private static ChessPiece[,] CloneBoard(ChessPiece[,] originalBoard)
        {
            ChessPiece[,] clonedBoard = new ChessPiece[BoardSize, BoardSize];
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    clonedBoard[x, y] = originalBoard[x, y];
                }
            }
            return clonedBoard;
        }



        private bool IsKingInCheck(ChessPiece[,] board, bool isWhite)
        {
            // Find the king's position
            int kingX = -1;
            int kingY = -1;
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = board[x, y];
                    if (piece != null && piece.GetType() == typeof(King) && piece.IsWhite == isWhite)
                    {
                        kingX = x;
                        kingY = y;
                        break;
                    }
                }
            }

            // Check if the king is threatened by any opponent's piece
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = board[x, y];
                    if (piece != null && piece.IsWhite != isWhite && piece.IsValidMove(board, x, y, kingX, kingY))
                    {
                        return true;
                    }
                }
            }

            return false;
        }





        private void BtnBack_Click(object sender, EventArgs e)
        {
           
            this.Controls.Clear();
            ShowModeSelectionMenu();
        }
        private static int GetPieceValue(ChessPiece piece)
        {
            // Assign values to each piece type
            switch (piece.Type)
            {
                case PieceType.Pawn: return 1;
                case PieceType.Knight: return 3;
                case PieceType.Bishop: return 3;
                case PieceType.Rook: return 5;
                case PieceType.Queen: return 9;
                case PieceType.King: return 1000; 
                default: return 0;
            }
        }

        private void InitializeBoard()
        {
            int tileSize = 60;
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    Button button = new Button
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Location = new Point(x * tileSize, y * tileSize),
                        Font = new Font("Arial", 24, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    button.Click += new EventHandler(Button_Click);
                    buttons[x, y] = button;
                    this.Controls.Add(button);
                }
            }
            this.ClientSize = new Size(BoardSize * tileSize + 200, BoardSize * tileSize); // Extra space for captured pieces
            AddGameplayButtons();
            UpdateBoardUI();
            InitializeCapturedPiecesUI();
        }

        private void AddGameplayButtons()
        {
            Button quitButton = new Button
            {
                Width = 150,
                Height = 50,
                Location = new Point(this.ClientSize.Width - 175, this.ClientSize.Height - 60),
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Quit"
            };
            quitButton.Click += (sender, e) => Application.Exit();

            Button restartButton = new Button
            {
                Width = 150,
                Height = 50,
                Location = new Point(this.ClientSize.Width - 175, this.ClientSize.Height - 60 - 60),
                Font = new Font("Arial", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Restart"
            };
            restartButton.Click += new EventHandler(RestartGame);

            this.Controls.Add(restartButton);
            this.Controls.Add(quitButton);
        }

        private void RestartGame(object sender, EventArgs e)
        {
            chessBoard.InitializeBoard();
            UpdateBoardUI();
            selectedX = -1;
            selectedY = -1;
            this.Controls.Clear();
            chessBoard.boardStates.Clear();
            ShowModeSelectionMenu();
            lblWhiteCaptured.Text = "White Captured:\n";
            lblBlackCaptured.Text = "Black Captured:\n";
            gameMode = GameMode.none;
        }

        private void InitializeCapturedPiecesUI()
        {
            lblWhiteCaptured = new Label
            {
                Location = new Point(this.ClientSize.Width - 175, 200),
                Size = new Size(180, 150),
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.TopLeft,
                Text = "White Captured:" + "\n" + "(+0)"
            };
            this.Controls.Add(lblWhiteCaptured);

            lblBlackCaptured = new Label
            {
                Location = new Point(this.ClientSize.Width - 175, 20),
                Size = new Size(180, 150),
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.TopLeft,
                Text = "Black Captured:" + "\n" + "(+0)"
            };
            this.Controls.Add(lblBlackCaptured);
        }


        private void MakeAIMove()
        {
            if (currentDifficultyLevel == DifficultyLevel.Easy)
            {
                MakeEasyAIMove();
            }
            else if (currentDifficultyLevel == DifficultyLevel.Medium)
            {
                MakeMediumAIMove();
            }
            else if (currentDifficultyLevel == DifficultyLevel.Hard)
            {
                MakeHardAIMove();
            }
            Thread.Sleep(1000);
            chessBoard.IsWhiteTurn = true;
            EnableChessBoardButtons();


        }



        private void Button_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(chessBoard.IsWhiteTurn);
            Button clickedButton = sender as Button;
            int x = clickedButton.Location.X / 60;
            int y = clickedButton.Location.Y / 60;

            // Check if it's AI's turn and the game mode is single player

            // Human player's turn
            if (selectedX == -1 && selectedY == -1)
            {
                if (chessBoard.Board[x, y] != null && chessBoard.Board[x, y].IsWhite == chessBoard.IsWhiteTurn)
                {
                    selectedX = x;
                    selectedY = y;
                    clickedButton.BackColor = Color.Yellow;
                }
            }
            else
            {
                if (chessBoard.MovePiece(selectedX, selectedY, x, y))
                {
                    UpdateBoardUI();
                   

                    // Check for the opponent's check and checkmate
                    bool isCheck = chessBoard.IsInCheck(chessBoard.IsWhiteTurn);
                    bool isCheckmate = chessBoard.IsCheckmate(chessBoard.IsWhiteTurn);


                   

                    if (isCheck)
                    {
                        if (isCheckmate)
                        {
                            MessageBox.Show($"{(!chessBoard.IsWhiteTurn ? "White" : "Black")} wins by checkmate!");
                            canMoveAI = false;
                        }
                        else
                        {
                            MessageBox.Show($"{(!chessBoard.IsWhiteTurn ? "Black" : "White")} is in check!");
                        }
                    }
                    else
                    {
                        this.BackColor = SystemColors.Control;
                    }
                    if (gameMode == GameMode.SinglePlayer)
                    {
                        Thread.Sleep(1500);
                        MakeAIMove();
                    }
                    

                }

                buttons[selectedX, selectedY].BackColor = (selectedX + selectedY) % 2 == 0 ? Color.White : Color.Gray;
                selectedX = -1;
                selectedY = -1;
                UpdateBoardUI();


            }

        }

        private void DisableChessBoardButtons()
        {
            foreach (Control c in Controls)
            {
                if (c is Button button && button.Text != "Restart" && button.Text != "Quit")
                {
                    button.Enabled = false;
                    button.Click -= new EventHandler(Button_Click);
                }
            }
        }

        private void EnableChessBoardButtons()
        {
            foreach (Control c in Controls)
            {
                if (c is Button button && button.Text != "Restart" && button.Text != "Quit")
                {
                    button.Enabled = true;
                }
            }
        }



        private void UpdateBoardUI()
        {
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = chessBoard.Board[x, y];
                    buttons[x, y].Text = piece != null ? GetPieceSymbol(piece) : "";
                    buttons[x, y].BackColor = (x + y) % 2 == 0 ? Color.White : Color.Gray;
                }
            }
            UpdateCapturedPiecesUI();
        }

        private void UpdateCapturedPiecesUI()
        {
            if (lblWhiteCaptured == null || lblBlackCaptured == null) return;

            string whiteCapturedText = "White Captured:\n";
            foreach (var piece in chessBoard.CapturedWhitePieces)
            {
                whiteCapturedText += GetPieceSymbol(piece) + " ";
            }
            int whiteScore = chessBoard.CalculateScore(chessBoard.CapturedWhitePieces);
            whiteCapturedText += $"(+{whiteScore})";

            string blackCapturedText = "Black Captured:\n";
            foreach (var piece in chessBoard.CapturedBlackPieces)
            {
                blackCapturedText += GetPieceSymbol(piece) + " ";
            }
            int blackScore = chessBoard.CalculateScore(chessBoard.CapturedBlackPieces);
            blackCapturedText += $"(+{blackScore})";

            lblWhiteCaptured.Text = whiteCapturedText;
            lblBlackCaptured.Text = blackCapturedText;
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       
    }


    public class Move
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public ChessPiece Piece { get; set; }
        public ChessPiece CapturedPiece { get; set; }

        public Move(int startX, int startY, int endX, int endY, ChessPiece piece)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            Piece = piece;
        }
    }
    public class Position
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }

}