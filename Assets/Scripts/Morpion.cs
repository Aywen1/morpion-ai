using System.Linq;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Random = UnityEngine.Random;

public class TicTacToeState : NetworkBehaviour {
    [SerializeField] private UIManager uIManager;
    [SerializeField] private OdinAI odinAI;
    [SerializeField] private AIBufferVisualizer _bufferVisualizer;

    public readonly SyncList<int> _board = new(Enumerable.Repeat(0, 25).ToList());
    private readonly SyncVar<int> _currentPlayer = new(1);
    private readonly SyncVar<int> _gameState = new(0);

    public int CurrentPlayer => _currentPlayer.Value;

    private void Awake() {
        _bufferVisualizer.OnAIFinished += move => {
            if (IsServerStarted)
                PlayMoveServerRpc(move);
        };
    }

    private void Update() {
        if (InstanceFinder.IsServerStarted && Input.GetKeyDown(KeyCode.R)) {
            ResetGame();
            return;
        }

        if (!InstanceFinder.IsServerStarted || !Input.GetKeyDown(KeyCode.K)) {
            return;
        }

        if (_gameState.Value == 0 && _currentPlayer.Value == 1) {
            int[] boardCopy = _board.ToArray();
            _bufferVisualizer.StartAI(boardCopy, _currentPlayer.Value);
        }
    }

    private void OnDestroy() {
        _bufferVisualizer.OnAIFinished -= null;
    }

    public override void OnStartClient() {
        _board.OnChange += (op, index, item, newItem, server) => {
            uIManager.RefreshBoard(_board.ToArray());
        };

        _gameState.OnChange += (prev, next, server) => {
            uIManager.RefreshStatus(next, _currentPlayer.Value);
        };

        _currentPlayer.OnChange += (prev, next, server) => {
            uIManager.RefreshStatus(_gameState.Value, next);
        };
    }

    public override void OnStartServer() {
        base.OnStartServer();
        ResetGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayMoveServerRpc(int index) {
        if (_gameState.Value != 0) {
            return;
        }

        if (index < 0 || index > 25) {
            return;
        }

        if (_board[index] != 0) {
            return;
        }

        _board[index] = _currentPlayer.Value;
        CheckGameState();
        _currentPlayer.Value = _currentPlayer.Value == 1 ? 2 : 1;
    }

    private void CheckGameState() {
        var size = 5;
        var alignToWin = 4;

        bool CheckDirection(int startX, int startY, int dx, int dy, int player) {
            for (var i = 0; i < alignToWin; i++) {
                int x = startX + i * dx;
                int y = startY + i * dy;
                if (x < 0 || x >= size || y < 0 || y >= size) return false;
                if (_board[y * size + x] != player) return false;
            }
            return true;
        }

        for (var y = 0; y < size; y++) {
            for (var x = 0; x < size; x++) {
                int player = _board[y * size + x];
                if (player == 0) continue;

                if (CheckDirection(x, y, 1, 0, player) ||
                    CheckDirection(x, y, 0, 1, player) ||
                    CheckDirection(x, y, 1, 1, player) ||
                    CheckDirection(x, y, 1, -1, player)) {
                    _gameState.Value = player;
                    return;
                }
            }
        }

        if (_board.All(cell => cell != 0)) {
            _gameState.Value = 3;
        }
    }

    private void ResetGame() {
        for (var i = 0; i < 25; i++) {
            _board[i] = 0;
        }
        _currentPlayer.Value = Random.Range(1, 3);
        _gameState.Value = 0;
    }
}