using System;
using System.Linq;
using UnityEngine;

public class OdinAI : MonoBehaviour {
    private const int BoardSize = 5;
    private const int AlignToWin = 4;
    public int maxDepth = 3;

    public int aiPlayer;
    private int _humanPlayer;

    public Action<int[], int, int, bool, int> StepEvaluated;

    public int GetBestMove(int[] board, int currentPlayer) {
        aiPlayer = currentPlayer;
        _humanPlayer = currentPlayer == 1 ? 2 : 1;

        var bestScore = int.MinValue;
        int bestMove = -1;

        for (var i = 0; i < board.Length; i++) {
            if (board[i] != 0) {
                continue;
            }

            board[i] = aiPlayer;
            int score = Minimax(board, maxDepth, false, int.MinValue, int.MaxValue);
            board[i] = 0;

            if (score <= bestScore) {
                continue;
            }

            bestScore = score;
            bestMove = i;
        }

        return bestMove;
    }

    private int Minimax(int[] board, int depth, bool maximizing, int alpha, int beta) {
        int winner = EvaluateWinner(board);
        if (winner == aiPlayer) {
            return 10000 + depth;
        }

        if (winner == _humanPlayer) {
            return -10000 - depth;
        }

        if (IsBoardFull(board) || depth == 0) {
            return EvaluateBoard(board);
        }

        int bestScore = maximizing ? int.MinValue : int.MaxValue;

        for (var i = 0; i < board.Length; i++) {
            if (board[i] != 0) {
                continue;
            }

            board[i] = maximizing ? aiPlayer : _humanPlayer;
            int score = Minimax(board, depth - 1, !maximizing, alpha, beta);
            board[i] = 0;

            StepEvaluated?.Invoke((int[])board.Clone(), i, score, maximizing, depth);

            if (maximizing) {
                bestScore = Math.Max(score, bestScore);
                alpha = Math.Max(alpha, bestScore);
            } else {
                bestScore = Math.Min(score, bestScore);
                beta = Math.Min(beta, bestScore);
            }

            if (beta <= alpha) {
                break;
            }
        }

        return bestScore;
    }

    private static bool IsBoardFull(int[] board) => board.All(cell => cell != 0);

    private int EvaluateWinner(int[] board) {
        for (var y = 0; y < BoardSize; y++) {
            for (var x = 0; x < BoardSize; x++) {
                int player = board[y * BoardSize + x];
                if (player == 0) {
                    continue;
                }

                if (CheckDirection(board, x, y, 1, 0, player) ||
                    CheckDirection(board, x, y, 0, 1, player) ||
                    CheckDirection(board, x, y, 1, 1, player) ||
                    CheckDirection(board, x, y, 1, -1, player)) {
                    return player;
                }
            }
        }
        return 0;
    }

    private static bool CheckDirection(int[] board, int x, int y, int dx, int dy, int player) {
        for (var i = 0; i < AlignToWin; i++) {
            int nx = x + i * dx;
            int ny = y + i * dy;
            if (nx < 0 || nx >= BoardSize || ny < 0 || ny >= BoardSize) {
                return false;
            }

            if (board[ny * BoardSize + nx] != player) {
                return false;
            }
        }
        return true;
    }

    private int EvaluateBoard(int[] board) {
        var score = 0;
        score += EvaluateLines(board, aiPlayer) * 10;
        score -= EvaluateLines(board, _humanPlayer) * 10;
        return score;
    }

    private int EvaluateLines(int[] board, int player) {
        var count = 0;

        for (var y = 0; y < BoardSize; y++) {
            for (var x = 0; x < BoardSize; x++) {
                count += CountPattern(board, x, y, 1, 0, player);
                count += CountPattern(board, x, y, 0, 1, player);
                count += CountPattern(board, x, y, 1, 1, player);
                count += CountPattern(board, x, y, 1, -1, player);
            }
        }

        return count;
    }

    private int CountPattern(int[] board, int x, int y, int dx, int dy, int player) {
        int consecutive = 0, blanks = 0;

        for (var i = 0; i < AlignToWin; i++) {
            int nx = x + i * dx;
            int ny = y + i * dy;

            if (nx < 0 || nx >= BoardSize || ny < 0 || ny >= BoardSize)
                return 0;

            int val = board[ny * BoardSize + nx];
            if (val == player) {
                consecutive++;
            } else if (val == 0) {
                blanks++;
            } else {
                return 0;
            }
        }

        if (consecutive == 4) {
            return 100;
        }

        if (consecutive == 3 && blanks == 1) {
            return 10;
        }

        if (consecutive == 2 && blanks >= 1) {
            return 5;
        }
        return 0;
    }
}