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
        private Button[,] ChessBoardButtons = new Button[BoardSize, BoardSize];
        private ChessBoard chessBoard = new ChessBoard();
        private int selectedX = -1;
        private int selectedY = -1;
        bool canMoveAI = false;
        public List<ChessPiece[,]> MoveHistory { get; private set; } = new List<ChessPiece[,]>();
        public int CurrentPreviewIndex { get; private set; } = -1; // -1 means no preview is active

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
            byte[] largeArray = new byte[1_000_000_000];
            InitializeComponent();
            ShowModeSelectionMenu();

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


        public List<(int startX, int startY, int endX, int endY)> GetAvailableMovesForAI()
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
            // Skapa en instans av ChessAI
            ChessAI ai = new ChessAI(false, chessBoard, false); // AI spelar svart här

            // Försök hitta ett schackmattsdrag via AI-instansen
            var checkmateMove = ai.FindCheckmateMoveForAI();
            if (checkmateMove.HasValue)
            {
                // Utför schackmattsdraget
                chessBoard.MovePiece(checkmateMove.Value.startX, checkmateMove.Value.startY, checkmateMove.Value.endX, checkmateMove.Value.endY);
                UpdateBoardUI();
                MessageBox.Show("Schackmatt! AI vinner!");
                DisableChessBoard(); // Inaktivera brädet efter schackmattsdrag
                return;
            }

            // Om inget schackmattsdrag hittades, hitta det bästa draget med hjälp av AI
            Move bestMove = ai.FindBestMove(4); // Använder djupet 4 för bästa draget
            if (bestMove != null)
            {
                chessBoard.MakeMove(bestMove); // Utför bästa draget
                UpdateBoardUI();

                // Kontrollera om draget resulterar i schack eller schackmatt
                bool isCheck = chessBoard.IsInCheck(chessBoard.IsWhiteTurn);
                bool isCheckmate = chessBoard.IsCheckmate(chessBoard.IsWhiteTurn);

                if (isCheckmate)
                {
                    MessageBox.Show($"{(!chessBoard.IsWhiteTurn ? "White" : "Black")} wins by checkmate!");
                    DisableChessBoard();
                    return;
                }
                else if (isCheck)
                {
                    MessageBox.Show($"{(!chessBoard.IsWhiteTurn ? "Black" : "White")} is in check!");
                }
            }
            else
            {
                MessageBox.Show("Inga möjliga drag kvar! Oavgjort.");
                DisableChessBoard();
            }
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

                InitializeZobrist(); // Ensure zobristTable is initialized
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

                // Generate all possible moves
                List<Move> allMoves = GenerateMoves(Board.Board, IsWhite);

                foreach (Move move in allMoves)
                {
                    // Simulate the move
                    Board.MakeMove(move);

                    // Use EvaluateMove to assess the move

                    int score = EvaluateMove(move);
                    // If depth allows, refine score using AlphaBeta for deeper analysis
                    if (depth > 1)
                    {
                        score += AlphaBeta(depth - 1, int.MinValue, int.MaxValue, false);
                    }

                    // Undo the move
                    UndoMove(move);

                    // Update the best move if the score is higher
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }

                return bestMove;
            }



            private int EvaluateMove(Move move)
            {
                int score = 0;

                // Simulate the board state after the move
                ChessPiece[,] simulatedBoard = CloneBoard(Board.Board);
                simulatedBoard[move.EndX, move.EndY] = simulatedBoard[move.StartX, move.StartY];
                simulatedBoard[move.StartX, move.StartY] = null;

                // 1. Evaluate positional advantages (retain existing logic)
                score += EvaluateControlOfMiddle(move.Piece, move.EndX, move.EndY, Board);

                // 2. Penalize leaving high-value pieces exposed
                if (IsPieceThreatened(simulatedBoard, move.EndX, move.EndY,IsWhite))
                {
                    score -= GetPieceValue(move.Piece) * 2; // High penalty for exposing valuable pieces
                }

                // 3. Evaluate potential threats against opponent pieces
                score += EvaluatePressureOnOpponent(simulatedBoard, move.EndX, move.EndY);

                // 4. Encourage safety for all valuable pieces
                if (IsAnyPieceThreatened(simulatedBoard, move.Piece.IsWhite))
                {
                    score -= 50; // Penalize positions where any piece is under attack
                }

                // 5. Add existing evaluations (e.g., material advantage, piece development)
                score += EvaluateMaterialAdvantage(simulatedBoard);

                return score;
            }
            private int EvaluatePressureOnOpponent(ChessPiece[,] board, int x, int y)
            {
                int pressureScore = 0;

                // Ensure board[x, y] is not null before trying to access it
                ChessPiece piece = board[x, y];
                if (piece == null)
                {
                    return 0; // If the piece at (x, y) is null, there's no pressure to evaluate
                }

                // Iterate through the board to evaluate threats against opponent pieces
                for (int targetX = 0; targetX < 8; targetX++)
                {
                    for (int targetY = 0; targetY < 8; targetY++)
                    {
                        ChessPiece targetPiece = board[targetX, targetY];

                        // Proceed if targetPiece is not null and is an opponent's piece
                        if (targetPiece != null && targetPiece.IsWhite != piece.IsWhite)
                        {
                            // Check if the piece at (x, y) threatens the opponent's piece
                            if (piece.IsValidMove(board, x, y, targetX, targetY))
                            {
                                // Reward for threatening opponent pieces
                                pressureScore += GetPieceValue(targetPiece) / 2; // Higher value for threatening high-value pieces
                            }
                        }
                    }
                }

                return pressureScore;
            }


            private int EvaluateMaterialAdvantage(ChessPiece[,] board)
            {
                int whiteScore = 0;
                int blackScore = 0;

                // Iterate through the board and calculate the total material value for each side
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null)
                        {
                            int pieceValue = GetPieceValue(piece);
                            if (piece.IsWhite)
                            {
                                whiteScore += pieceValue;
                            }
                            else
                            {
                                blackScore += pieceValue;
                            }
                        }
                    }
                }

                // Return the advantage as positive if white is ahead, or negative if black is ahead
                return (IsWhite ? whiteScore - blackScore : blackScore - whiteScore);
            }

            private bool IsEarlyGame()
            {
                // Kontrollera baserat på antal drag eller om bara få pjäser har flyttats
                int moveCount = Board.TurnCount; // Förutsätter att vi håller koll på antal drag
                return moveCount < 10; // Tidigt spel om färre än 10 drag har gjorts
            }

            private int EvaluateEarlyGameMove(Move move)
            {
                int score = 0;

                // Belöna utveckling av springare och löpare
                if (move.Piece is Knight || move.Piece is Bishop)
                {
                    score += 50;
                }

                // Belöna kontroll av centrum
                score += EvaluateControlOfMiddle(move.Piece, move.StartX, move.StartY, Board);

                // Straffa kungsdrag
                if (move.Piece is King)
                {
                    score -= 100;
                }

                return score;
            }








            private List<Move> GetThreatenedPiecesMoves(ChessBoard board)
            {
                List<Move> threatenedMoves = new List<Move>();

                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece piece = board.Board[x, y];
                        if (piece != null && piece.IsWhite == IsWhite) // AI's egna pjäser
                        {
                            // Hitta alla attackerare och försvarare för denna pjäs
                            List<Move> attackers = GetMovesTargetingSquare(x, y, board, !IsWhite);
                            List<Move> defenders = GetMovesTargetingSquare(x, y, board, IsWhite);

                            // Kontrollera hot mot pjäsen
                            if (attackers.Count > defenders.Count) // Fler attackerare än försvarare
                            {
                                // Hitta värdet på den billigaste attackeraren
                                int minAttackerValue = attackers
                                    .Select(move => GetPieceValue(board.Board[move.StartX, move.StartY]))
                                    .Min();

                                // Om pjäsen är värd mer än billigaste attackeraren
                                if (GetPieceValue(piece) > minAttackerValue)
                                {
                                    // Lägg till hotande drag
                                    threatenedMoves.AddRange(attackers);
                                }
                            }
                        }
                    }
                }

                return threatenedMoves;
            }
            private List<Move> GetMovesTargetingSquare(int targetX, int targetY, ChessBoard board, bool isWhite)
            {
                List<Move> moves = new List<Move>();

                foreach (var move in GenerateMoves(board.Board, isWhite))
                {
                    if (move.EndX == targetX && move.EndY == targetY)
                    {
                        moves.Add(move);
                    }
                }

                return moves;
            }











            private List<Move> GenerateMoves(ChessPiece[,] board, bool isWhite)
            {
                List<Move> possibleMoves = new List<Move>();

                for (int startX = 0; startX < 8; startX++)
                {
                    for (int startY = 0; startY < 8; startY++)
                    {
                        ChessPiece piece = board[startX, startY];
                        if (piece != null && piece.IsWhite == isWhite)
                        {
                            for (int endX = 0; endX < 8; endX++)
                            {
                                for (int endY = 0; endY < 8; endY++)
                                {
                                    if (piece.IsValidMove(board, startX, startY, endX, endY))
                                    {
                                        if (!IsInCheckAfterMove(board, startX, startY, endX, endY, isWhite))
                                        {
                                            possibleMoves.Add(new Move(startX, startY, endX, endY, piece));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return possibleMoves;
            }

            private bool IsInCheckAfterMove(ChessPiece[,] board, int startX, int startY, int endX, int endY, bool isWhite)
            {
                ChessPiece[,] simulatedBoard = CloneBoard(board);
                ChessPiece movingPiece = simulatedBoard[startX, startY];

                // Simulate the move
                simulatedBoard[endX, endY] = movingPiece;
                simulatedBoard[startX, startY] = null;

                // Check if the king is in check after the move
                return IsKingInCheck(simulatedBoard, isWhite);
            }










            private int Minimax(int depth, bool isMaximizingPlayer)
            {
                if (depth == 0)
                {
                    return EvaluateBoard(Board); // Pass the ChessBoard instance
                }

                // Pass board and turn information
                List<Move> possibleMoves = GenerateMoves(Board.Board, isMaximizingPlayer);

                if (isMaximizingPlayer)
                {
                    int maxEval = int.MinValue;
                    foreach (Move move in possibleMoves)
                    {
                        Board.SimulateMove(move);
                        int eval = Minimax(depth - 1, false);
                        Board.UndoSimulatedMove(move);
                        maxEval = Math.Max(maxEval, eval);
                    }
                    return maxEval;
                }
                else
                {
                    int minEval = int.MaxValue;
                    foreach (Move move in possibleMoves)
                    {
                        Board.SimulateMove(move);
                        int eval = Minimax(depth - 1, true);
                        Board.UndoSimulatedMove(move);
                        minEval = Math.Min(minEval, eval);
                    }
                    return minEval;
                }
            }



            // Optimizing AI move calculation by prioritizing free captures and assuming opponent will do the same.

            private int AlphaBeta(int depth, int alpha, int beta, bool isMaximizingPlayer)
            {
                if (depth == 0)
                    return QuiescenceSearch(alpha, beta); // Use Quiescence Search for better static evaluation

                ulong currentHash = ComputeZobristHash(Board.Board);
                if (TryGetTransposition(currentHash, depth, out int transpositionScore))
                    return transpositionScore; // Retrieve score if position has been analyzed

                List<Move> possibleMoves = GenerateMoves(Board.Board, isMaximizingPlayer);
                possibleMoves = OrderMoves(possibleMoves, Board); // Prioritize free captures and strong moves

                int bestScore = isMaximizingPlayer ? int.MinValue : int.MaxValue;

                foreach (var move in possibleMoves)
                {
                    if (IsFreeCapture(move, isMaximizingPlayer))
                    {
                        return isMaximizingPlayer ? int.MaxValue : int.MinValue; // Prioritize free captures immediately
                    }

                    Board.SimulateMove(move); // Simulate the move
                    int eval = AlphaBeta(depth - 1, alpha, beta, !isMaximizingPlayer);
                    Board.UndoSimulatedMove(move); // Undo the simulated move

                    if (isMaximizingPlayer)
                    {
                        bestScore = Math.Max(bestScore, eval);
                        alpha = Math.Max(alpha, eval);
                    }
                    else
                    {
                        bestScore = Math.Min(bestScore, eval);
                        beta = Math.Min(beta, eval);
                    }

                    if (beta <= alpha)
                        break; // Prune branches
                }

                // Store the result in the transposition table
                StoreInTranspositionTable(currentHash, depth, bestScore);
                return bestScore;
            }
            public List<(int startX, int startY, int endX, int endY)> GetAiMoves()
            {
                List<(int startX, int startY, int endX, int endY)> availableMoves = new List<(int, int, int, int)>();

                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece piece = Board.Board[x, y];
                        if (piece != null && piece.IsWhite == IsWhite) // Endast AI:s egna pjäser
                        {
                            for (int endX = 0; endX < BoardSize; endX++)
                            {
                                for (int endY = 0; endY < BoardSize; endY++)
                                {
                                    if (piece.IsValidMove(Board.Board, x, y, endX, endY) &&
                                        !IsInCheckAfterMove(Board.Board, x, y, endX, endY, IsWhite))
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

            private bool IsCheckmateAfterMove(ChessPiece[,] board, int startX, int startY, int endX, int endY, bool isWhite)
            {
                // Klona brädet för att simulera draget
                ChessPiece[,] simulatedBoard = CloneBoard(board);
                ChessPiece movingPiece = simulatedBoard[startX, startY];
                simulatedBoard[endX, endY] = movingPiece;
                simulatedBoard[startX, startY] = null;

                // Kontrollera om motståndarens kung är i schackmatt
                bool isCheckmate = !HasLegalMoves(simulatedBoard, !isWhite) &&
                                   IsKingInCheck(simulatedBoard, !isWhite);

                return isCheckmate;
            }
            public (int startX, int startY, int endX, int endY)? FindCheckmateMoveForAI()
            {
                List<(int startX, int startY, int endX, int endY)> availableMoves = GetAiMoves();

                foreach (var move in availableMoves)
                {
                    if (IsCheckmateAfterMove(Board.Board, move.startX, move.startY, move.endX, move.endY, false))
                    {
                        return move; // Returnera schackmattsdraget
                    }
                }

                return null; // Inga schackmattsdrag hittades
            }
            private bool HasLegalMoves(ChessPiece[,] board, bool isWhite)
            {
                for (int startX = 0; startX < BoardSize; startX++)
                {
                    for (int startY = 0; startY < BoardSize; startY++)
                    {
                        ChessPiece piece = board[startX, startY];
                        if (piece != null && piece.IsWhite == isWhite)
                        {
                            // För varje pjäs, hitta alla giltiga drag
                            for (int endX = 0; endX < BoardSize; endX++)
                            {
                                for (int endY = 0; endY < BoardSize; endY++)
                                {
                                    if (piece.IsValidMove(board, startX, startY, endX, endY) &&
                                        !IsInCheckAfterMove(board, startX, startY, endX, endY, isWhite))
                                    {
                                        return true; // Om ett giltigt drag finns, returnera true
                                    }
                                }
                            }
                        }
                    }
                }
                return false; // Om inga giltiga drag finns, returnera false
            }



            private int QuiescenceSearch(int alpha, int beta)
            {
                int standPat = EvaluateBoard(Board); // Static evaluation of the current board

                if (standPat >= beta)
                    return beta; // Beta cutoff
                if (alpha < standPat)
                    alpha = standPat; // Update alpha if current evaluation is better

                List<Move> captureMoves = GenerateCaptureMoves(Board.Board, IsWhite);
                foreach (var move in captureMoves)
                {
                    Board.MakeMove(move);
                    int eval = -QuiescenceSearch(-beta, -alpha); // Recurse with negated scores
                    UndoMove(move);

                    if (eval >= beta)
                        return beta; // Beta cutoff
                    if (eval > alpha)
                        alpha = eval; // Update alpha
                }

                return alpha;
            }

            private ulong ComputeZobristHash(ChessPiece[,] board)
            {
                ulong hash = 0;
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null)
                        {
                            int pieceIndex = GetPieceIndex(piece);
                            hash ^= zobristTable[y * 8 + x, pieceIndex];
                        }
                    }
                }
                return hash;
            }

            private void StoreInTranspositionTable(ulong hash, int depth, int score)
            {
                transpositionTable[hash] = (depth, score);
            }

            private bool TryGetTransposition(ulong hash, int depth, out int score)
            {
                if (transpositionTable.TryGetValue(hash, out var entry))
                {
                    if (entry.depth >= depth)
                    {
                        score = entry.score;
                        return true;
                    }
                }
                score = 0;
                return false;
            }


            private int EvaluateBoardForDevelopmentAndCentralControl()
            {
                int score = 0;

                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece piece = Board.Board[x, y];
                        if (piece != null)
                        {
                            // Reward control of central squares
                            score += EvaluateCentralControl(piece, x, y);
                        }
                    }
                }

                return score;
            }

            private bool IsEndGame(ChessPiece[,] board)
            {
                int pieceCount = 0;

                // Count remaining non-pawn pieces
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null && !(piece is Pawn))
                            pieceCount++;
                    }
                }

                // Consider endgame if there are fewer than a specific number of non-pawn pieces
                return pieceCount <= 6;
            }



            private bool IsFreeCapture(Move move, bool isMaximizingPlayer)
            {
                bool isFree = false;
                Board.SimulateMove(move);
                isFree = !IsPieceThreatened(Board.Board, move.EndX, move.EndY, isMaximizingPlayer);
                Board.UndoSimulatedMove(move); // Undo the simulated move
                return isFree;
            }

            private List<Move> OrderMoves(List<Move> moves, ChessBoard board)
            {
                return moves.OrderByDescending(move =>
                {
                    int value = 0;

                    // Prioritize free captures
                    if (IsFreeCapture(move, board.IsWhiteTurn))
                        value += 10000;

                    if (board.Board[move.EndX, move.EndY] != null)
                        value += GetPieceValue(board.Board[move.EndX, move.EndY]); // Value for captures

                    if (IsCheck(move, board))
                        value += 500; // Value for checks

                    return value;
                }).ToList();
            }

            private int EvaluateMoveSafetyIntegration(ChessPiece piece, int startX, int startY, int endX, int endY, ChessPiece[,] board)
            {
                int score = 0;

                // Check if the move lands on an attacked square
                score += EvaluateMoveSafety(board, piece, endX, endY);

                return score;
            }

            private List<ChessPiece> GetPiecesUnderAttack(ChessPiece[,] board, bool isWhite)
            {
                List<ChessPiece> underAttackPieces = new List<ChessPiece>();

                // Iterate through all pieces on the board
                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null && piece.IsWhite == isWhite) // Friendly piece
                        {
                            // Check if the piece is under attack
                            for (int oppX = 0; oppX < BoardSize; oppX++)
                            {
                                for (int oppY = 0; oppY < BoardSize; oppY++)
                                {
                                    ChessPiece opponentPiece = board[oppX, oppY];
                                    if (opponentPiece != null && opponentPiece.IsWhite != isWhite) // Opponent's piece
                                    {
                                        // If the opponent can attack this piece
                                        if (opponentPiece.IsValidMove(board, oppX, oppY, x, y))
                                        {
                                            // Check value disparity
                                            if (GetPieceValue(opponentPiece) < GetPieceValue(piece))
                                            {
                                                underAttackPieces.Add(piece);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return underAttackPieces;
            }

            private int EvaluateMoveSafety(ChessPiece[,] board, ChessPiece piece, int endX, int endY)
            {
                int penalty = 0;

                // Iterate through all opponent pieces
                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece opponentPiece = board[x, y];

                        if (opponentPiece != null && opponentPiece.IsWhite != piece.IsWhite) // Opponent's piece
                        {
                            // Check if the opponent piece can attack the destination square
                            if (opponentPiece.IsValidMove(board, x, y, endX, endY))
                            {
                                int pieceValue = GetPieceValue(piece);
                                int opponentPieceValue = GetPieceValue(opponentPiece);

                                // Penalize if the opponent's piece is of lesser value
                                if (opponentPieceValue < pieceValue)
                                    penalty -= pieceValue - opponentPieceValue; // Higher penalty for bigger disparity
                            }
                        }
                    }
                }

                return penalty;
            }

            private int EvaluateCapturePotential(Move move, ChessBoard board)
            {
                int score = 0;

                ChessPiece targetPiece = board.Board[move.EndX, move.EndY];
                if (targetPiece != null)
                {
                    // Reward capturing high-value pieces
                    score += GetPieceValue(targetPiece) * 10; // Weight captures heavily
                }

                // Reward moves that lead to potential captures
                var subsequentMoves = GenerateMoves(board.Board, move.Piece.IsWhite);
                foreach (var subsequentMove in subsequentMoves)
                {
                    ChessPiece potentialTarget = board.Board[subsequentMove.EndX, subsequentMove.EndY];
                    if (potentialTarget != null && potentialTarget.IsWhite != move.Piece.IsWhite)
                    {
                        score += GetPieceValue(potentialTarget);
                    }
                }

                return score;
            }


            bool IsPromotionMove(Move move)
            {
                return move.Piece is Pawn && (move.EndY == 0 || move.EndY == 7);
            }

            

            private List<Move> GenerateCaptureMoves(ChessPiece[,] board, bool isWhite)
            {
                List<Move> captureMoves = new List<Move>();

                foreach (var move in GenerateMoves(board, isWhite))
                {
                    if (board[move.EndX, move.EndY] != null) // Check if the target square is occupied
                    {
                        captureMoves.Add(move);
                    }
                }

                return captureMoves;
            }




            private bool IsCheck(Move move, ChessBoard board)
            {
                bool isCheck = false;
                Board.SimulateMove(move);
                isCheck = board.IsInCheck(!move.Piece.IsWhite);
                Board.UndoSimulatedMove(move); // Undo the simulated move
                return isCheck;
            }




            private Dictionary<ulong, (int depth, int score)> transpositionTable = new Dictionary<ulong, (int depth, int score)>();

            private ulong[,] zobristTable;
            private Random random = new Random();

            
            private void InitializeZobrist()
            {
                zobristTable = new ulong[64, 12]; // 64 squares, 12 possible pieces (6 types x 2 colors)

                Random random = new Random();
                for (int square = 0; square < 64; square++)
                {
                    for (int piece = 0; piece < 12; piece++)
                    {
                        // Generate a random 64-bit number for each piece and square combination
                        zobristTable[square, piece] = ((ulong)random.Next() << 32) | (ulong)random.Next();
                    }
                }
            }





            private int GetPieceIndex(ChessPiece piece)
            {
                int baseIndex;
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        baseIndex = 0;
                        break;
                    case PieceType.Knight:
                        baseIndex = 1;
                        break;
                    case PieceType.Bishop:
                        baseIndex = 2;
                        break;
                    case PieceType.Rook:
                        baseIndex = 3;
                        break;
                    case PieceType.Queen:
                        baseIndex = 4;
                        break;
                    case PieceType.King:
                        baseIndex = 5;
                        break;
                    default:
                        throw new ArgumentException("Unknown piece type");
                }

                
                return baseIndex + (piece.IsWhite ? 0 : 6); 
            }



            private int EvaluateControlOfMiddle(ChessPiece piece, int x, int y, ChessBoard chessBoard)
            {
                int[,] middleSquares = { { 3, 3 }, { 3, 4 }, { 4, 3 }, { 4, 4 } };
                for (int i = 0; i < middleSquares.GetLength(0); i++)
                {
                    int middleX = middleSquares[i, 0];
                    int middleY = middleSquares[i, 1];

                    if (piece.IsValidMove(chessBoard.Board, x, y, middleX, middleY))
                    {
                        if (piece is Knight || piece is Bishop)
                            return 30; // Högre poäng för springare och löpare
                        else if (piece is Pawn)
                            return 20;
                    }
                }
                return 0;
            }



            private int EvaluateKingSafetyInOpening(ChessPiece[,] board, Position kingPosition, bool isWhite)
            {
                int penalty = 0;

                // Penalize central positions early in the game
                if ((kingPosition.Row > 2 && kingPosition.Row < 5) && (kingPosition.Col > 2 && kingPosition.Col < 5))
                    penalty -= 100; // Strong penalty for central king in opening

                // Reward castling positions
                if ((isWhite && kingPosition.Row == 7 && (kingPosition.Col == 1 || kingPosition.Col == 6)) ||
                    (!isWhite && kingPosition.Row == 0 && (kingPosition.Col == 1 || kingPosition.Col == 6)))
                    penalty += 50; // Reward castling

                return penalty;
            }

            private int EvaluateDevelopment(ChessPiece piece, int x, int y)
            {
                if (!(piece is Knight || piece is Bishop)) return 0;

                int score = 0;

                // Reward leaving initial squares
                if (piece.IsWhite && y > 1) score += 30; // White knights/bishops
                if (!piece.IsWhite && y < 6) score += 30; // Black knights/bishops

                // Reward moving toward the center
                if ((x == 2 || x == 5) && (y == 2 || y == 5))
                    score += 20;

                return score;
            }





            private int EvaluatePressureOnKing(Position kingPosition, ChessPiece piece, int startX, int startY, int endX, int endY)
            {
                if (kingPosition == null)
                {
                    // King is missing; likely checkmate or error. Return high score for win.
                    return piece.IsWhite ? int.MaxValue : int.MinValue;
                }

                ChessPiece[,] simulatedBoard = CloneBoard(Board.Board);
                simulatedBoard[endX, endY] = simulatedBoard[startX, startY];
                simulatedBoard[startX, startY] = null;

                int restrictedSquares = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int kingX = kingPosition.Row + dx;
                        int kingY = kingPosition.Col + dy;
                        if (IsValidSquare(kingX, kingY) &&
                            (simulatedBoard[kingX, kingY] == null || simulatedBoard[kingX, kingY].IsWhite != piece.IsWhite))
                        {
                            restrictedSquares++;
                        }
                    }
                }

                int score = restrictedSquares * 15;
                if (IsPieceCapturedForLessMaterial(simulatedBoard, endX, endY))
                    score -= 30;

                return score;
            }

            private bool IsValidSquare(int x, int y)
            {
                return x >= 0 && x < 8 && y >= 0 && y < 8;
            }


            private bool IsPieceCapturedForLessMaterial(ChessPiece[,] board, int x, int y)
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


            private int EvaluatePawnStructure(ChessBoard chessBoard, int x, int y, bool isWhite)
            {
                int score = 0;

                // Penalize doubled pawns
                for (int i = 0; i < 8; i++)
                {
                    if (i != x && chessBoard.Board[i, y] is Pawn otherPawn && otherPawn.IsWhite == isWhite)
                    {
                        score -= 10; // Arbitrary penalty value
                    }
                }

                // Penalize isolated pawns
                if ((y == 0 || !IsPawnOnFile(chessBoard.Board, x, y - 1, isWhite)) &&
                    (y == 7 || !IsPawnOnFile(chessBoard.Board, x, y + 1, isWhite)))
                {
                    score -= 15; // Arbitrary penalty value
                }

                // Reward passed pawns
                if (IsPassedPawn(chessBoard.Board, x, y, isWhite))
                {
                    score += 20; // Arbitrary reward value
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
                            int positionalScore = EvaluatePosition(piece, x, y);
                            int centerControlScore = EvaluateCenterControl(piece, x, y, chessBoard);

                            // Basic material value
                            score += piece.IsWhite == chessBoard.IsWhiteTurn ? pieceValue : -pieceValue;

                            // Positional and central control
                            score += piece.IsWhite == chessBoard.IsWhiteTurn
                                ? (positionalScore + centerControlScore)
                                : -(positionalScore + centerControlScore);
                        }
                    }
                }

                // Adjust score for overall map control
                score += EvaluateMapControl(chessBoard);

                return score;
            }
            private int EvaluateMapControl(ChessBoard chessBoard)
            {
                int aiControlledSquares = 0;
                int opponentControlledSquares = 0;

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = chessBoard.Board[x, y];
                        if (piece != null)
                        {
                            List<(int, int)> validMoves = GetValidMovesForPiece(chessBoard.Board, x, y);

                            foreach (var move in validMoves)
                            {
                                int targetX = move.Item1;
                                int targetY = move.Item2;

                                if (piece.IsWhite == chessBoard.IsWhiteTurn)
                                {
                                    if (targetY < 4) // AI controls opponent's side
                                        aiControlledSquares++;
                                }
                                else
                                {
                                    if (targetY >= 4) // Opponent controls AI's side
                                        opponentControlledSquares++;
                                }
                            }
                        }
                    }
                }

                // Score adjustment
                return (aiControlledSquares - opponentControlledSquares) * 5; // Adjust weight as needed
            }


            private List<(int, int)> GetValidMovesForPiece(ChessPiece[,] board, int startX, int startY)
            {
                List<(int, int)> validMoves = new List<(int, int)>();

                ChessPiece piece = board[startX, startY];
                if (piece != null)
                {
                    for (int endX = 0; endX < BoardSize; endX++)
                    {
                        for (int endY = 0; endY < BoardSize; endY++)
                        {
                            if (piece.IsValidMove(board, startX, startY, endX, endY))
                            {
                                validMoves.Add((endX, endY));
                            }
                        }
                    }
                }

                return validMoves;
            }

            private int EvaluateCenterControl(ChessPiece piece, int x, int y, ChessBoard chessBoard)
            {
                // Central squares on the chessboard
                int[,] centralSquares = { { 3, 3 }, { 3, 4 }, { 4, 3 }, { 4, 4 } };

                for (int i = 0; i < centralSquares.GetLength(0); i++)
                {
                    int centerX = centralSquares[i, 0];
                    int centerY = centralSquares[i, 1];

                    if (piece.IsValidMove(chessBoard.Board, x, y, centerX, centerY))
                    {
                        return piece is Pawn ? 10 : 20; // Higher reward for central pawn control
                    }
                }

                return 0;
            }





            private int EvaluatePawns(ChessBoard chessBoard)
            {
                int score = 0;

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (chessBoard.Board[x, y] is Pawn pawn)
                        {
                            score += EvaluatePawnStructure(chessBoard, x, y, pawn.IsWhite);
                        }
                    }
                }

                return score;
            }

            private int EvaluatePiece(ChessPiece piece, int x, int y, ChessBoard chessBoard)
            {
                int score = 0;

                // Material Value
                score += piece.IsWhite == chessBoard.IsWhiteTurn
                    ? GetPieceValue(piece)
                    : -GetPieceValue(piece);

                // Positional Adjustments
                score += EvaluateControlOfMiddle(piece, x, y, chessBoard);

                // Evaluate Pressure
                score += EvaluatePiecePressure(piece, x, y, chessBoard);

                return score;
            }

            private int EvaluatePiecePressure(ChessPiece piece, int x, int y, ChessBoard chessBoard)
            {
                int pressureScore = 0;

                // Only evaluate pressure if the piece targets the opponent
                if (piece.IsWhite != chessBoard.IsWhiteTurn)
                {
                    Position opponentKingPosition = FindOpponentKing(chessBoard, piece.IsWhite);
                    var possibleMoves = GetPossibleMovesForPiece(chessBoard.Board, x, y);

                    foreach (var move in possibleMoves)
                    {
                        pressureScore += EvaluatePressureOnKing(
                            opponentKingPosition,
                            piece,
                            x,
                            y,
                            move.endX,
                            move.endY
                        );
                    }
                }

                return pressureScore;
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

                // Return null if the king is not found
                return null;
            }

            private int EvaluateKingSafety(ChessPiece[,] board, ChessPiece piece, int x, int y)
            {
                if (!(piece is King)) return 0;

                int score = 0;

                // Penalize king in the center early
                if ((x > 2 && x < 5) && (y > 2 && y < 5))
                    score -= 100;

                // Reward castling positions
                if ((piece.IsWhite && y == 7 && (x == 2 || x == 6)) || (!piece.IsWhite && y == 0 && (x == 2 || x == 6)))
                    score += 50;

                // Penalize lack of pawn cover
                for (int dx = -1; dx <= 1; dx++)
                {
                    int pawnRow = piece.IsWhite ? y - 1 : y + 1;
                    int pawnCol = x + dx;
                    if (IsValidSquare(pawnRow, pawnCol) && !(board[pawnCol, pawnRow] is Pawn))
                        score -= 10;
                }

                return score;
            }


            private int EvaluateCenterControl(ChessBoard chessBoard)
            {
                int score = 0;

                // Define the central squares
                var centralSquares = new List<Position>
                {
                    new Position(3, 3), new Position(3, 4),
                    new Position(4, 3), new Position(4, 4)
                };

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = chessBoard.Board[x, y];
                        if (piece != null)
                        {
                            foreach (var square in centralSquares)
                            {
                                if (piece.IsValidMove(chessBoard.Board, x, y, square.Row, square.Col))
                                {
                                    score += piece.IsWhite ? 15 : -15; // Reward influence over central squares
                                }
                            }
                        }
                    }
                }

                return score;
            }


            private Position FindKingPosition(ChessBoard chessBoard, bool isWhite)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = chessBoard.Board[x, y];
                        if (piece is King && piece.IsWhite == isWhite)
                        {
                            return new Position(x, y);
                        }
                    }
                }

                // Return null if the king is not found
                return null;
            }


            // Rewards moves that develop pieces
            private int EvaluatePieceDevelopment(ChessPiece piece, int startX, int startY, int endX, int endY)
            {
                int score = 0;

                // Early game (first 20 moves total), prioritize piece development
                if (Board.TurnCount < 20)
                {
                    // Encourage knights to move to f3, c3 (for white) or f6, c6 (for black)
                    if (piece is Knight)
                    {
                        if ((endX == 2 || endX == 5) && (endY == (piece.IsWhite ? 5 : 2)))
                        {
                            score += 20; // Reward central development of knights
                        }
                    }

                    // Encourage bishops to move to open diagonals
                    if (piece is Bishop)
                    {
                        if ((endX > 1 && endX < 6) && (endY > 1 && endY < 6))
                        {
                            score += 15; // Reward central positioning of bishops
                        }
                    }

                    // Discourage leaving back-rank pieces undeveloped
                    if (startY == (piece.IsWhite ? 7 : 0))
                    {
                        if (piece is Bishop || piece is Knight)
                        {
                            score -= 10; // Penalize keeping pieces on the back rank
                        }
                    }
                }

                // Reward moving pieces off the starting squares
                if (startY == (piece.IsWhite ? 7 : 0))
                {
                    score += 5; // Encourage moving any piece out of its initial position
                }

                return score;
            }


            // Rewards moves that control central squares
            private int EvaluateCentralControl(ChessPiece piece, int endX, int endY)
            {
                int score = 0;

                // High priority central squares: d4, d5, e4, e5
                if ((endX == 3 || endX == 4) && (endY == 3 || endY == 4))
                {
                    score += 50; // Increase the weight significantly
                }
                // Secondary central squares
                else if ((endX >= 2 && endX <= 5) && (endY >= 2 && endY <= 5))
                {
                    score += 25; // Provide a moderate reward for these squares
                }

                return score;
            }


            // Penalizes unnecessary piece retreat
            private int PenalizePieceRetreat(ChessPiece piece, int startX, int startY, int endX, int endY)
            {
                int score = 0;

                // Penalize if a piece retreats without a valid reason
                if (piece.IsWhite && endY < startY || !piece.IsWhite && endY > startY)
                {
                    score -= 10;
                }

                return score;
            }



            private int EvaluatePieceSafety(ChessPiece piece, int x, int y, ChessBoard chessBoard)
            {
                int score = 0;

                var possibleMoves = GetPossibleMovesForPiece(chessBoard.Board, x, y);
                foreach (var move in possibleMoves)
                {
                    // Simulate the move
                    ChessPiece[,] simulatedBoard = CloneBoard(chessBoard.Board);
                    simulatedBoard[move.endX, move.endY] = piece;
                    simulatedBoard[x, y] = null;

                    // Check if the destination square is attacked
                    if (IsSquareUnderAttack(simulatedBoard, move.endX, move.endY, piece.IsWhite))
                    {
                        score -= 10; // Penalize unsafe moves
                    }
                    else
                    {
                        score += 5; // Reward safe moves
                    }
                }

                return score;
            }

            private bool IsSquareUnderAttack(ChessPiece[,] board, int x, int y, bool isWhite)
            {
                for (int startX = 0; startX < 8; startX++)
                {
                    for (int startY = 0; startY < 8; startY++)
                    {
                        ChessPiece attacker = board[startX, startY];
                        if (attacker != null && attacker.IsWhite != isWhite && attacker.IsValidMove(board, startX, startY, x, y))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private List<Move> GetMovesForThreatenedPieces(ChessPiece[,] board, bool isWhite)
            {
                List<Move> defensiveMoves = new List<Move>();
                Dictionary<Type, int> pieceValues = new Dictionary<Type, int>
    {
        { typeof(Pawn), 1 },
        { typeof(Knight), 3 },
        { typeof(Bishop), 3 },
        { typeof(Rook), 5 },
        { typeof(Queen), 9 },
        { typeof(King), 1000 }
    };

                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null && piece.IsWhite == isWhite)
                        {
                            List<(int, int)> threats = GetThreats(board, x, y, !isWhite);

                            foreach (var threat in threats)
                            {
                                ChessPiece attackingPiece = board[threat.Item1, threat.Item2];
                                if (attackingPiece != null)
                                {
                                    int attackerValue = pieceValues[attackingPiece.GetType()];
                                    int pieceValue = pieceValues[piece.GetType()];

                                    if (attackerValue < pieceValue)
                                    {
                                        // Flytta till säker position
                                        defensiveMoves.AddRange(GetSafeMoves(board, x, y));
                                    }
                                    else
                                    {
                                        // Försvara pjäsen
                                        defensiveMoves.AddRange(GetMovesToDefend(board, x, y, isWhite));
                                    }
                                }
                            }
                        }
                    }
                }

                return defensiveMoves;
            }

            private List<(int, int)> GetThreats(ChessPiece[,] board, int x, int y, bool isOpponent)
            {
                List<(int, int)> threats = new List<(int, int)>();

                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        ChessPiece attackingPiece = board[i, j];
                        if (attackingPiece != null && attackingPiece.IsWhite == isOpponent)
                        {
                            if (attackingPiece.IsValidMove(board, i, j, x, y))
                            {
                                threats.Add((i, j));
                            }
                        }
                    }
                }

                return threats;
            }

            private List<Move> GetSafeMoves(ChessPiece[,] board, int startX, int startY)
            {
                List<Move> safeMoves = new List<Move>();

                foreach (var move in GetPossibleMovesForPiece(board, startX, startY))
                {
                    ChessPiece[,] simulatedBoard = CloneBoard(board);

                    // Simulate the move
                    simulatedBoard[move.endX, move.endY] = simulatedBoard[startX, startY];
                    simulatedBoard[startX, startY] = null;

                    // Check if the moved piece or other important pieces are safe
                    if (!IsAnyPieceThreatened(simulatedBoard, simulatedBoard[move.endX, move.endY].IsWhite))
                    {
                        safeMoves.Add(new Move(startX, startY, move.endX, move.endY, board[startX, startY]));
                    }
                }

                return safeMoves;
            }
            private bool IsAnyPieceThreatened(ChessPiece[,] board, bool isWhite)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null && piece.IsWhite == isWhite)
                        {
                            if (IsPieceThreatened(board, x, y, IsWhite))
                            {
                                return true; // A piece is under threat
                            }
                        }
                    }
                }
                return false; // No threats detected
            }


            private bool IsPieceThreatened(ChessPiece[,] board, int x, int y, bool isWhite)
            {
                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        ChessPiece attacker = board[i, j];
                        if (attacker != null && attacker.IsWhite != isWhite && attacker.IsValidMove(board, i, j, x, y))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }


            private int GetAttackersValue(int x, int y)
            {
                int value = 0;
                foreach (var attacker in GetPotentialAttackers(x, y, !IsWhite))
                {
                    value += GetPieceValue(attacker);
                }
                return value;
            }

            private int GetDefendersValue(int x, int y)
            {
                int value = 0;
                foreach (var defender in GetPotentialDefenders(x, y, IsWhite))
                {
                    value += GetPieceValue(defender);
                }
                return value;
            }

            private IEnumerable<ChessPiece> GetPotentialAttackers(int x, int y, bool isWhite)
            {
                for (int startX = 0; startX < 8; startX++)
                {
                    for (int startY = 0; startY < 8; startY++)
                    {
                        ChessPiece piece = Board.Board[startX, startY];
                        if (piece != null && piece.IsWhite == isWhite && piece.IsValidMove(Board.Board, startX, startY, x, y))
                        {
                            yield return piece;
                        }
                    }
                }
            }

            private IEnumerable<ChessPiece> GetPotentialDefenders(int x, int y, bool isWhite)
            {
                for (int startX = 0; startX < 8; startX++)
                {
                    for (int startY = 0; startY < 8; startY++)
                    {
                        ChessPiece piece = Board.Board[startX, startY];
                        if (piece != null && piece.IsWhite == isWhite && piece.IsValidMove(Board.Board, startX, startY, x, y))
                        {
                            yield return piece;
                        }
                    }
                }
            }


            private List<Move> GetMovesToDefend(ChessPiece[,] board, int targetX, int targetY, bool isWhite)
            {
                List<Move> defendMoves = new List<Move>();

                for (int x = 0; x < BoardSize; x++)
                {
                    for (int y = 0; y < BoardSize; y++)
                    {
                        ChessPiece piece = board[x, y];
                        if (piece != null && piece.IsWhite == isWhite)
                        {
                            if (piece.IsValidMove(board, x, y, targetX, targetY))
                            {
                                defendMoves.Add(new Move(x, y, targetX, targetY, piece));
                            }
                        }
                    }
                }

                return defendMoves;
            }



            private int EvaluateKingDefense(ChessBoard chessBoard, Position kingPosition, bool isWhite)
            {
                if (kingPosition == null)
                    return -100; // Penalize heavily if the king is missing (indicates an invalid state)

                int score = 0;

                // Define relative directions to check around the king
                var directions = new List<(int, int)>
                {
                    (-1, -1), (0, -1), (1, -1), // Above king
                    (-1, 0),          (1, 0),  // Left/Right of king
                    (-1, 1), (0, 1), (1, 1)    // Below king
                };

                foreach (var (dx, dy) in directions)
                {
                    int x = kingPosition.Row + dx;
                    int y = kingPosition.Col + dy;

                    if (IsValidSquare(x, y) && chessBoard.Board[x, y] is Pawn pawn && pawn.IsWhite == isWhite)
                    {
                        score += 10; // Reward pawns defending the king
                    }
                }

                return score;
            }

            private bool HasCastled(ChessBoard chessBoard, bool isWhite)
            {
                Position kingStart = isWhite ? new Position(7, 4) : new Position(0, 4);
                Position rookStartLeft = isWhite ? new Position(7, 0) : new Position(0, 0);
                Position rookStartRight = isWhite ? new Position(7, 7) : new Position(0, 7);

                ChessPiece king = chessBoard.Board[kingStart.Row, kingStart.Col];
                ChessPiece rookLeft = chessBoard.Board[rookStartLeft.Row, rookStartLeft.Col];
                ChessPiece rookRight = chessBoard.Board[rookStartRight.Row, rookStartRight.Col];

                if (king is King && !king.HasMoved)
                {
                    if (rookLeft is Rook && !rookLeft.HasMoved)
                    {
                        // Check for castling path being clear and not under attack
                        return IsCastlingPathClear(chessBoard, kingStart, rookStartLeft);
                    }

                    if (rookRight is Rook && !rookRight.HasMoved)
                    {
                        return IsCastlingPathClear(chessBoard, kingStart, rookStartRight);
                    }
                }

                return false;
            }

            private bool IsCastlingPathClear(ChessBoard chessBoard, Position kingPos, Position rookPos)
            {
                int row = kingPos.Row;
                int colStart = Math.Min(kingPos.Col, rookPos.Col);
                int colEnd = Math.Max(kingPos.Col, rookPos.Col);

                for (int col = colStart + 1; col < colEnd; col++)
                {
                    if (chessBoard.Board[row, col] != null)
                        return false;
                }

                return true;
            }





            private int EvaluatePosition(ChessPiece piece, int x, int y)
            {
                int penalty = 0;

                if (piece is Knight || piece is Bishop)
                {
                    // Penalize returning to starting positions
                    if ((piece.IsWhite && y == 7) || (!piece.IsWhite && y == 0))
                    {
                        penalty -= 10;
                    }
                }
                else if (piece is Pawn)
                {
                    // Reward advancing pawns toward the opponent's side
                    penalty += piece.IsWhite ? y : (7 - y);
                }

                return penalty;
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



        private static bool IsKingInCheck(ChessPiece[,] board, bool isWhite)
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
        private  int GetPieceValue(ChessPiece piece)
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
                    ChessBoardButtons[x, y] = button;
                    this.Controls.Add(button);
                }
            }
            this.ClientSize = new Size(BoardSize * tileSize + 200, BoardSize * tileSize); // Extra space for captured pieces
            AddGameplayButtons();
            UpdateBoardUI();
            InitializeCapturedPiecesUI();
        }

        private void DisableChessBoard()
        {
            foreach(Button button in ChessBoardButtons)
            {
                button.Enabled = false;
                button.Click -= new EventHandler(Button_Click);
            }
        }

        private void EnableChessBoard()
        {
            foreach (Button button in ChessBoardButtons)
            {
                button.Enabled = true;
                button.Click += new EventHandler(Button_Click);
            }
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
            bool isInCheck = chessBoard.IsInCheck(false);
            if (isInCheck)
            {
                // Get defensive moves that take the king out of check
                List<(int startX, int startY, int endX, int endY)> defensiveMoves = GetDefensiveMovesForAI();
                if (defensiveMoves.Count > 0)
                {
                    Random rand = new Random();
                    var defensiveMove = defensiveMoves[rand.Next(defensiveMoves.Count)];
                    chessBoard.MovePiece(defensiveMove.startX, defensiveMove.startY, defensiveMove.endX, defensiveMove.endY);
                    UpdateBoardUI();
                    return;
                }
            }
            DisableChessBoard();
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
            chessBoard.IsWhiteTurn = true;
            EnableChessBoard();

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
                    // Selecting a new piece
                    selectedX = x;
                    selectedY = y;
                    clickedButton.BackColor = Color.Yellow;  // Highlight the selected piece
                }
            }
            else
            {
                // Try to move the selected piece to the clicked position
                if (chessBoard.MovePiece(selectedX, selectedY, x, y))
                {
                    // Visual feedback for the move (origin and destination)
                    // Set origin square to dark yellow
                    ChessBoardButtons[selectedX, selectedY].BackColor = Color.DarkGoldenrod;

                    // Set destination square to light yellow
                    clickedButton.BackColor = Color.LightGoldenrodYellow;

                    // Update the board and check for game status
                    UpdateBoardUI();

                    // Check for check and checkmate
                    bool isCheck = chessBoard.IsInCheck(chessBoard.IsWhiteTurn);
                    bool isCheckmate = chessBoard.IsCheckmate(chessBoard.IsWhiteTurn);

                    if (isCheck)
                    {
                        if (isCheckmate)
                        {
                            MessageBox.Show($"{(!chessBoard.IsWhiteTurn ? "White" : "Black")} wins by checkmate!");
                            canMoveAI = false;
                            DisableChessBoard(); // Disable the board after checkmate
                            return;
                        }
                        else
                        {
                            MessageBox.Show($"{(!chessBoard.IsWhiteTurn ? "Black" : "White")} is in check!");
                        }
                    }

                    // If single-player mode, make the AI move
                    if (gameMode == GameMode.SinglePlayer)
                    {
                        MakeAIMove();
                    }

                    // Deselect the piece after moving
                    ChessBoardButtons[selectedX, selectedY].BackColor = (selectedX + selectedY) % 2 == 0 ? Color.White : Color.Gray;
                    selectedX = -1;
                    selectedY = -1;

                    // Update the board UI after the move
                    UpdateBoardUI();
                }
                else
                {
                    // If the move is invalid, reset the selection
                    ChessBoardButtons[selectedX, selectedY].BackColor = (selectedX + selectedY) % 2 == 0 ? Color.White : Color.Gray;
                    selectedX = -1;
                    selectedY = -1;
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
                    ChessBoardButtons[x, y].Text = piece != null ? GetPieceSymbol(piece) : "";
                    ChessBoardButtons[x, y].BackColor = (x + y) % 2 == 0 ? Color.White : Color.Gray;
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