using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIBufferVisualizer : MonoBehaviour {
    public OdinAI ai;

    [Range(0f, 1f)] public float sampleRate = 0.1f;

    public bool onlyRootDepth = true;
    public int rootTopK = 4;
    public float stepDelay = 0.1f;
    private readonly List<StepInfo> _rootSteps = new();

    private readonly Queue<StepInfo> _stepQueue = new();

    private void OnEnable() => ai.StepEvaluated += EnqueueStep;

    private void OnDisable() => ai.StepEvaluated -= EnqueueStep;

    public event Action<int, int, bool, int> OnCellHighlight;

    public event Action<int> OnAIFinished;

    private void EnqueueStep(int[] board, int index, int score, bool isMaximizing, int depth) {
        if (onlyRootDepth) {
            if (depth == ai.maxDepth) {
                _rootSteps.Add(new StepInfo(index, score, isMaximizing, depth));
            }
        } else {
            if (Random.value <= sampleRate) {
                _stepQueue.Enqueue(new StepInfo(index, score, isMaximizing, depth));
            }
        }
    }

    public void StartAI(int[] board, int currentPlayer) {
        StartCoroutine(RunAIAndVisualize(board, currentPlayer));
    }

    private IEnumerator RunAIAndVisualize(int[] board, int currentPlayer) {
        _stepQueue.Clear();
        _rootSteps.Clear();

        var aiTask = Task.Run(() => ai.GetBestMove(board, currentPlayer));
        while (!aiTask.IsCompleted) {
            yield return null;
        }
        int bestMove = aiTask.Result;

        switch (onlyRootDepth) {
            case true when rootTopK > 0:
            {
                _rootSteps.Sort((a, b) => b.Score.CompareTo(a.Score));
                foreach (var step in _rootSteps.GetRange(0, Mathf.Min(rootTopK, _rootSteps.Count))) {
                    _stepQueue.Enqueue(step);
                }
                break;
            }
            case true:
            {
                foreach (var step in _rootSteps) {
                    _stepQueue.Enqueue(step);
                }
                break;
            }
        }

        while (_stepQueue.Count > 0) {
            var s = _stepQueue.Dequeue();
            OnCellHighlight?.Invoke(s.Index, s.Score, s.IsMaximizing, s.Depth);
            yield return new WaitForSeconds(stepDelay);
        }

        OnAIFinished?.Invoke(bestMove);
    }

    private struct StepInfo {
        public readonly int Index;
        public readonly int Score;
        public readonly bool IsMaximizing;
        public readonly int Depth;

        public StepInfo(int i, int s, bool m, int d) {
            Index = i;
            Score = s;
            IsMaximizing = m;
            Depth = d;
        }
    }
}