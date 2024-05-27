using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace Chess
{
    public partial class Form1 : Form
    {
        private const int BoardSize = 8;
        private Button[,] buttons = new Button[BoardSize, BoardSize];
        private ChessBoard chessBoard = new ChessBoard();
        private int selectedX = -1;
        private int selectedY = -1;
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


        private void MakeEasyAIMove()
        {
           
            List<(int startX, int startY, int endX, int endY)> availableMoves = GetAvailableMovesForAI();

            if (availableMoves.Count == 0)
            {
                return;
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

            // Find capturing moves with the highest value difference
            int maxDifference = int.MinValue;
            (int startX, int startY, int endX, int endY) bestMove = (0, 0, 0, 0);

            foreach (var move in availableMoves)
            {
                ChessPiece targetPiece = chessBoard.Board[move.endX, move.endY];
                if (targetPiece != null && targetPiece.IsWhite != chessBoard.IsWhiteTurn)
                {
                    int valueDifference = pieceValues[typeof(ChessPiece)] - pieceValues[targetPiece.GetType()];
                    if (valueDifference > maxDifference)
                    {
                        maxDifference = valueDifference;
                        bestMove = move;
                    }
                }
            }

            // If no capturing moves found, prioritize threatening opponent pieces
            if (maxDifference <= 0)
            {
                foreach (var move in availableMoves)
                {
                    ChessPiece targetPiece = chessBoard.Board[move.endX, move.endY];
                    if (targetPiece != null && targetPiece.IsWhite != chessBoard.IsWhiteTurn)
                    {
                        int valueDifference = pieceValues[typeof(ChessPiece)] - pieceValues[targetPiece.GetType()];
                        if (valueDifference >= 0)
                        {
                            maxDifference = valueDifference;
                            bestMove = move;
                            break; // Break after finding the first threatening move
                        }
                    }
                }
            }

            // Execute the best move found
            if (maxDifference > 0)
            {
                chessBoard.MovePiece(bestMove.startX, bestMove.startY, bestMove.endX, bestMove.endY);
            }
            else
            {
                // If no capturing or threatening moves available, move a random piece
                var randomMove = availableMoves[new Random().Next(availableMoves.Count)];
                chessBoard.MovePiece(randomMove.startX, randomMove.startY, randomMove.endX, randomMove.endY);
            }

            bool isInCheck = chessBoard.IsInCheck(false); // Check if AI's king is in check

            if (isInCheck)
            {
                List<(int startX, int startY, int endX, int endY)> defendMoves = GetDefensiveMovesForAI();

                if (defendMoves.Count > 0)
                {
                    // If there are defensive moves available, prioritize those
                    Random rand = new Random();
                    var move = defendMoves[rand.Next(defendMoves.Count)];

                    chessBoard.MovePiece(move.startX, move.startY, move.endX, move.endY);
                    UpdateBoardUI();
                    return;
                }
            }

            // If no defensive moves available or AI is not in check, proceed with normal move selection
            
            if (availableMoves.Count == 0)
            {


                UpdateBoardUI();
            }
        }
        private void MakeHardAIMove()
        {
            
        }


        private List<(int startX, int startY, int endX, int endY)> GetDefensiveMovesForAI()
        {
            List<(int startX, int startY, int endX, int endY)> defensiveMoves = new List<(int, int, int, int)>();

            // Iterate over the board to find defensive moves that protect the king
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    ChessPiece piece = chessBoard.Board[x, y];
                    if (piece != null && !piece.IsWhite)
                    {
                        // Check if the piece can defend the king by moving to a safe square
                        for (int endX = 0; endX < BoardSize; endX++)
                        {
                            for (int endY = 0; endY < BoardSize; endY++)
                            {
                                if (piece.IsValidMove(chessBoard.Board, x, y, endX, endY))
                                {
                                    // Simulate the move to check if it defends the king
                                    ChessPiece[,] simulatedBoard = CloneBoard(chessBoard.Board); // Create a clone of the board
                                    simulatedBoard[endX, endY] = piece;
                                    simulatedBoard[x, y] = null;

                                    bool isKingSafeAfterMove = !IsKingInCheck(simulatedBoard, true);
                                    if (isKingSafeAfterMove)
                                    {
                                        // If the move defends the king, add it to the list of defensive moves
                                        defensiveMoves.Add((x, y, endX, endY));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return defensiveMoves;
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


        private ChessPiece[,] CloneBoard(ChessPiece[,] originalBoard)
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


        private void BtnBack_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("went back");
            this.Controls.Clear();
            ShowModeSelectionMenu();
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
                Text = "White Captured:\n"
            };
            this.Controls.Add(lblWhiteCaptured);

            lblBlackCaptured = new Label
            {
                Location = new Point(this.ClientSize.Width - 175, 20),
                Size = new Size(180, 150),
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.TopLeft,
                Text = "Black Captured:\n"
            };
            this.Controls.Add(lblBlackCaptured);
        }

        private void Button_Click(object sender, EventArgs e)
        {
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
                bool canMoveAI = true;
                if (chessBoard.MovePiece(selectedX, selectedY, x, y))
                {
                    UpdateBoardUI();
                    Console.WriteLine($"{(!chessBoard.IsWhiteTurn ? "White" : "Black")} Moved piece from ({selectedX}, {selectedY}) to ({x}, {y})");

                    // Check for the opponent's check and checkmate
                    bool isCheck = chessBoard.IsInCheck(chessBoard.IsWhiteTurn);
                    bool isCheckmate = chessBoard.IsCheckmate(chessBoard.IsWhiteTurn);
                    

                    Debug.WriteLine($"isCheck: {isCheck}, isCheckmate: {isCheckmate}");

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
                }
               
                buttons[selectedX, selectedY].BackColor = (selectedX + selectedY) % 2 == 0 ? Color.White : Color.Gray;
                selectedX = -1;
                selectedY = -1;
                if (gameMode != GameMode.TwoPlayer && currentDifficultyLevel == DifficultyLevel.Easy && canMoveAI)
                {
                    Thread.Sleep(1000);
                    MakeEasyAIMove(); // Let the AI make a move
                }
                if (gameMode != GameMode.TwoPlayer && currentDifficultyLevel == DifficultyLevel.Medium && canMoveAI)
                {
                    Thread.Sleep(1000);
                    MakeMediumAIMove(); // Let the AI make a move
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
}