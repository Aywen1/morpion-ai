using System.Collections;
using FishNet;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [Header("Grille TicTacToe")] [SerializeField]
    private GameObject grid;

    [SerializeField] private Sprite x;
    [SerializeField] private Sprite o;
    [SerializeField] private Sprite none;

    [Header("IA")] [SerializeField] private OdinAI ai;

    [SerializeField] private AIBufferVisualizer bufferVisualizer;
    public Text statusText;

    [SerializeField] private TicTacToeState gameState;

    private Button[] _cells;

    private void Awake() {
        _cells = grid.GetComponentsInChildren<Button>();
        for (var i = 0; i < _cells.Length; i++) {
            int idx = i;
            _cells[i].onClick.AddListener(() => OnCellClicked(idx));
        }

        bufferVisualizer.OnCellHighlight += HighlightCell;
    }

    private void OnDestroy() {
        bufferVisualizer.OnCellHighlight -= HighlightCell;
    }

    private void HighlightCell(int index, int score, bool isMaximizing, int depth) {
        var img = _cells[index].GetComponent<Image>(); // <- la ram ? je connais pas
        img.color = isMaximizing ? Color.green : Color.red;

        StartCoroutine(ResetColorAfterDelay(index, bufferVisualizer.stepDelay)); // <- c'est pas bo
    }

    private IEnumerator ResetColorAfterDelay(int index, float delay) {
        yield return new WaitForSeconds(delay);
        _cells[index].GetComponent<Image>().color = Color.white; //  <- Je suis pas opti-man ajd
    }

    public void RefreshBoard(int[] newBoard) {
        for (var i = 0; i < _cells.Length; i++) {
            var childImage = _cells[i].transform.Find("Value")?.GetComponent<Image>(); // <- Pas le time d'opti lol
            childImage.sprite = newBoard[i] == 1 ? x
                : newBoard[i] == 2 ? o
                : none;
        }
    }

    // Vive le hardcode
    public void RefreshStatus(int gameStateValue, int currentPlayer) {
        switch (gameStateValue) {
            case 0:
                statusText.text = "À " + (currentPlayer == 1 ? "O" : "X") + " de jouer !";
                break;
            case 1:
                statusText.text = "O a gagné !";
                break;
            case 2:
                statusText.text = "X a gagné !";
                break;
            case 3:
                statusText.text = "Égalité !";
                break;
        }
    }

    private void OnCellClicked(int index) {
        if (gameState.CurrentPlayer == (InstanceFinder.IsServerStarted ? 1 : 2)) {
            gameState.PlayMoveServerRpc(index);
        }
    }
}