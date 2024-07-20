using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LudicWorlds
{
    public class DebugPanel : MonoBehaviour
    {
        private static Canvas   _canvas;
        private static Text     _debugText;
        private static Text     _fpsText;
        private static Text     _statusText; 
        private static Text     _memoryText;

        private float   _elapsedTime;
        private uint    _fpsSamples;
        private float   _sumFps;

        private const int MAX_LINES = 23;

        private Transform _cameraTransform;
        private Vector3 _dirToPlayer = Vector3.zero;

        private float _memoryUsage;
        private float _maxMemoryUsage;
        private float _averageMemoryUsage;
        private uint _memorySamples;

        private bool _showMemoryUsage = true; 

        void Awake()
        {
            AcquireObjects();
            _elapsedTime = 0;
            _fpsSamples = 0;
            _fpsText.text = "0";
            Application.logMessageReceived += HandleLog;
        }

        void Start()
        {
            _cameraTransform = Camera.main.transform;
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void AcquireObjects()
        {
            _canvas = this.gameObject.GetComponent<Canvas>();
            Transform ui = this.transform.Find("UI");

            _debugText = ui.Find("DebugText").GetComponent<Text>();
            _fpsText = ui.Find("FpsText").GetComponent<Text>();
            _statusText = ui.Find("StatusText").GetComponent<Text>();
            _memoryText = ui.Find("MemoryText").GetComponent<Text>(); 
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            _debugText.text += (message + "\n");
            TrimText();
        }

        void Update()
        {
            _elapsedTime += Time.deltaTime;

            if(_elapsedTime > 0.5f)
            {
                _fpsText.text = (Mathf.Round((_sumFps / _fpsSamples))).ToString();
                UpdateMemoryUsage();
                _elapsedTime = 0f;
                _sumFps = 0f;
                _fpsSamples = 0;
            }

            _sumFps += (1.0f / Time.smoothDeltaTime);
            _fpsSamples++;

            _dirToPlayer = (this.transform.position - _cameraTransform.position).normalized;
            _dirToPlayer.y = 0;
            this.transform.rotation = Quaternion.LookRotation(_dirToPlayer);
        }

        private void UpdateMemoryUsage()
        {
            _memoryUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f); // Convert bytes to MB
            _maxMemoryUsage = Mathf.Max(_maxMemoryUsage, _memoryUsage);
            _averageMemoryUsage = ((_averageMemoryUsage * _memorySamples) + _memoryUsage) / (_memorySamples + 1);
            _memorySamples++;

            if (_showMemoryUsage)
            {
                _memoryText.text = $"Memory: {_memoryUsage:F2} MB\nMax Memory: {_maxMemoryUsage:F2} MB\nAvg Memory: {_averageMemoryUsage:F2} MB";
            }
        }

        public static void Clear()
        {
            if (_debugText is null) return;
            _debugText.text = "";
        }

        public static void Show()
        {
            SetVisibility(true);
        }

        public static void Hide()
        {
            SetVisibility(false);
        }

        public static void SetVisibility(bool visible)
        {
            if (_canvas is null) return;
            _canvas.enabled = visible;
        }

        public static void ToggleVisibility()
        {
            if (_canvas is null) return;
            _canvas.enabled = !_canvas.enabled;
        }

        public static void SetStatus(string message)
        {
            if (_statusText is null) return;
            _statusText.text = (message);
        }

        private static void TrimText()
        {
            string[] lines = _debugText.text.Split('\n');

            if (lines.Length > MAX_LINES)
            {
                _debugText.text = string.Join("\n", lines, lines.Length - MAX_LINES, MAX_LINES);
            }
        }

        public void ToggleMemoryUsageDisplay()
        {
            _showMemoryUsage = !_showMemoryUsage;
            if (!_showMemoryUsage)
            {
                _memoryText.text = "";
            }
        }

        public static void LogCustomMessage(string message, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                default:
                    Debug.Log(message);
                    break;
            }
        }

        public static void ClearAllLogs()
        {
            if (_debugText is null) return;
            _debugText.text = "";
            _statusText.text = "";
        }

        public void ShowFpsGraph()
        {
            Debug.Log("FPS Graph feature not implemented yet.");
        }

        public static void LogPerformanceData()
        {
            float frameTime = Time.deltaTime * 1000f;
            float fps = 1.0f / Time.deltaTime;
            Debug.Log($"Frame Time: {frameTime:F2} ms, FPS: {fps:F2}");
        }

        public static void SaveLogsToFile(string fileName)
        {
            string path = $"{Application.persistentDataPath}/{fileName}.txt";
            System.IO.File.WriteAllText(path, _debugText.text);
            Debug.Log($"Logs saved to {path}");
        }

        public static void ClearMemoryStats()
        {
            if (_memoryText is null) return;
            _maxMemoryUsage = 0;
            _averageMemoryUsage = 0;
            _memorySamples = 0;
            _memoryText.text = "Memory stats cleared";
        }

        public static void UpdateStatusWithTimestamp(string message)
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _statusText.text = $"[{timestamp}] {message}";
        }
    }
}
