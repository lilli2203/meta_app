using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

namespace LudicWorlds
{
    public class DebugPanel : MonoBehaviour
    {
        private static Canvas _canvas;
        private static Text _debugText;
        private static Text _fpsText;
        private static Text _statusText;
        private static Text _memoryText;
        private static Text _cpuUsageText;

        private float _elapsedTime;
        private uint _fpsSamples;
        private float _sumFps;

        private const int MAX_LINES = 23;

        private Transform _cameraTransform;
        private Vector3 _dirToPlayer = Vector3.zero;

        private float _memoryUsage;
        private float _maxMemoryUsage;
        private float _averageMemoryUsage;
        private uint _memorySamples;

        private float _cpuUsage;
        private float _maxCpuUsage;
        private float _averageCpuUsage;
        private uint _cpuSamples;

        private bool _showMemoryUsage = true;
        private bool _showFpsGraph = false;
        private bool _showCpuUsage = true;

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
            _cpuUsageText = ui.Find("CpuUsageText").GetComponent<Text>();
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            _debugText.text += (message + "\n");
            TrimText();
        }

        void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > 0.5f)
            {
                _fpsText.text = (Mathf.Round((_sumFps / _fpsSamples))).ToString();
                UpdateMemoryUsage();
                UpdateCpuUsage();
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
            _memoryUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            _maxMemoryUsage = Mathf.Max(_maxMemoryUsage, _memoryUsage);
            _averageMemoryUsage = ((_averageMemoryUsage * _memorySamples) + _memoryUsage) / (_memorySamples + 1);
            _memorySamples++;

            if (_showMemoryUsage)
            {
                _memoryText.text = $"Memory: {_memoryUsage:F2} MB\nMax Memory: {_maxMemoryUsage:F2} MB\nAvg Memory: {_averageMemoryUsage:F2} MB";
            }
        }

        private void UpdateCpuUsage()
        {
            _cpuUsage = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            _maxCpuUsage = Mathf.Max(_maxCpuUsage, _cpuUsage);
            _averageCpuUsage = ((_averageCpuUsage * _cpuSamples) + _cpuUsage) / (_cpuSamples + 1);
            _cpuSamples++;

            if (_showCpuUsage)
            {
                _cpuUsageText.text = $"CPU: {_cpuUsage:F2} MB\nMax CPU: {_maxCpuUsage:F2} MB\nAvg CPU: {_averageCpuUsage:F2} MB";
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

        public void ToggleCpuUsageDisplay()
        {
            _showCpuUsage = !_showCpuUsage;
            if (!_showCpuUsage)
            {
                _cpuUsageText.text = "";
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
            if (_showFpsGraph)
            {
                Debug.Log("FPS Graph is already visible.");
                return;
            }
            _showFpsGraph = true;
            Debug.Log("FPS Graph feature activated.");
        }

        public void HideFpsGraph()
        {
            if (!_showFpsGraph)
            {
                Debug.Log("FPS Graph is not visible.");
                return;
            }
            _showFpsGraph = false;
            Debug.Log("FPS Graph feature deactivated.");
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

        public static void LogMemoryWarning(float thresholdMB)
        {
            if (_memoryUsage > thresholdMB)
            {
                Debug.LogWarning($"Memory usage exceeded threshold: {_memoryUsage:F2} MB");
            }
        }

        public static void LogFpsError(float thresholdFps)
        {
            float fps = 1.0f / Time.deltaTime;
            if (fps < thresholdFps)
            {
                Debug.LogError($"FPS dropped below threshold: {fps:F2} FPS");
            }
        }

        public static void DisplayPerformanceReport()
        {
            float frameTime = Time.deltaTime * 1000f;
            float fps = 1.0f / Time.deltaTime;
            string report = $"Performance Report:\nFrame Time: {frameTime:F2} ms\nFPS: {fps:F2}\nMemory: {_memoryUsage:F2} MB\nCPU: {_cpuUsage:F2} MB";
            Debug.Log(report);
        }

        public static void ResetPerformanceMetrics()
        {
            _sumFps = 0;
            _fpsSamples = 0;
            _memoryUsage = 0;
            _maxMemoryUsage = 0;
            _averageMemoryUsage = 0;
            _memorySamples = 0;
            _cpuUsage = 0;
            _maxCpuUsage = 0;
            _averageCpuUsage = 0;
            _cpuSamples = 0;
            Debug.Log("Performance metrics reset.");
        }

        public static void SavePerformanceMetrics(string fileName)
        {
            string path = $"{Application.persistentDataPath}/{fileName}.txt";
            string metrics = $"Frame Time: {Time.deltaTime * 1000f:F2} ms\nFPS: {1.0f / Time.deltaTime:F2}\nMemory: {_memoryUsage:F2} MB\nCPU: {_cpuUsage:F2} MB";
            System.IO.File.WriteAllText(path, metrics);
            Debug.Log($"Performance metrics saved to {path}");
        }

        public static void LogAndDisplayCustomMessage(string message, LogType type)
        {
            LogCustomMessage(message, type);
            SetStatus(message);
        }

        public static void LogAndSaveCustomMessage(string message, LogType type, string fileName)
        {
            LogCustomMessage(message, type);
            string path = $"{Application.persistentDataPath}/{fileName}.txt";
            System.IO.File.WriteAllText(path, message);
            Debug.Log($"Message logged and saved to {path}");
        }
    }
}
