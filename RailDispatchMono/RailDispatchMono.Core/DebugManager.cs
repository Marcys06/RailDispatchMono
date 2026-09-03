using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RailDispatchMono.Core
{
    /// <summary>Centralny system debugowania z globalnym limitem 30 komunikatów/s.</summary>
    public static class DebugManager
    {
        public enum DebugCategory { General, Block, Signal, Train, TrainMovement, Camera, Input, Render, Map, TrackBuilder, UI, Performance, Error, All }
        private static readonly Dictionary<DebugCategory, bool> _enabledCategories = new();
        private static readonly Queue<DateTime> _outputTimes = new();
        private static readonly List<string> _logHistory = new();
        private static readonly object _lock = new();
        private const int MaxOutputsPerSecond = 30;
        private static readonly TimeSpan OutputWindow = TimeSpan.FromSeconds(1);
        private static bool _logToConsole = true, _logToFile = true, _showTimestamps = true, _showCategory = true;
        private static string _logFilePath = "debug_log.txt";
        private static int _maxLogEntries = 1000;
        public static bool IsDebugEnabled { get; set; } = true;
        public static bool LogToConsole { get => _logToConsole; set => _logToConsole = value; }
        public static bool LogToFile { get => _logToFile; set => _logToFile = value; }
        public static string LogFilePath => _logFilePath;
        public static int MaxOutputsPerSecondLimit => MaxOutputsPerSecond;
        static DebugManager()
        {
            foreach (DebugCategory category in Enum.GetValues(typeof(DebugCategory))) _enabledCategories[category] = true;
            _enabledCategories[DebugCategory.Performance] = false; _enabledCategories[DebugCategory.Camera] = false;
            _enabledCategories[DebugCategory.TrackBuilder] = false; _enabledCategories[DebugCategory.UI] = false;
            try { string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); if (string.IsNullOrWhiteSpace(root)) root = AppContext.BaseDirectory; string dir = Path.Combine(root, "RailDispatchMono", "Logs"); Directory.CreateDirectory(dir); _logFilePath = Path.Combine(dir, "debug_log.txt"); }
            catch { _logFilePath = Path.Combine(AppContext.BaseDirectory, "debug_log.txt"); }
        }
        public static void EnableCategory(DebugCategory c) { _enabledCategories[c] = true; Log($"[DEBUG] Enabled: {c}"); }
        public static void DisableCategory(DebugCategory c) { _enabledCategories[c] = false; Log($"[DEBUG] Disabled: {c}"); }
        public static void ToggleCategory(DebugCategory c) { _enabledCategories[c] = !_enabledCategories[c]; Log($"[DEBUG] {c}: {(_enabledCategories[c] ? "ON" : "OFF")}"); }
        public static bool IsCategoryEnabled(DebugCategory c) => _enabledCategories.TryGetValue(c, out bool e) && e;
        public static void EnableAll() { foreach (var k in _enabledCategories.Keys.ToList()) _enabledCategories[k] = true; Log("[DEBUG] All categories enabled"); }
        public static void DisableAll() { foreach (var k in _enabledCategories.Keys.ToList()) _enabledCategories[k] = false; Log("[DEBUG] All categories disabled"); }
        public static void SetLogToConsole(bool e) => _logToConsole = e;
        public static void SetLogToFile(bool e, string filePath = null) { _logToFile = e; if (!string.IsNullOrWhiteSpace(filePath)) _logFilePath = filePath; Log($"[DEBUG] Log to file: {(_logToFile ? "ON" : "OFF")} -> {_logFilePath}"); }
        public static void SetShowTimestamps(bool e) => _showTimestamps = e;
        public static void SetShowCategory(bool e) => _showCategory = e;
        public static void Log(DebugCategory c, string message) { if (!IsDebugEnabled || !IsCategoryEnabled(c) || !TryAcquireOutputSlot()) return; WriteLog(c, message); }
        public static void Log(string message) { if (!IsDebugEnabled || !TryAcquireOutputSlot()) return; WriteLog(DebugCategory.General, message); }
        private static bool TryAcquireOutputSlot() { lock (_lock) { DateTime now = DateTime.UtcNow; while (_outputTimes.Count > 0 && now - _outputTimes.Peek() >= OutputWindow) _outputTimes.Dequeue(); if (_outputTimes.Count >= MaxOutputsPerSecond) return false; _outputTimes.Enqueue(now); return true; } }
        private static void WriteLog(DebugCategory c, string message) { string formatted = FormatMessage(c, message); lock (_lock) { _logHistory.Add(formatted); if (_logHistory.Count > _maxLogEntries) _logHistory.RemoveAt(0); try { if (_logToConsole) { Console.WriteLine(formatted); Debug.WriteLine(formatted); Trace.WriteLine(formatted); } } catch { } if (_logToFile) try { string dir = Path.GetDirectoryName(_logFilePath); if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir); File.AppendAllText(_logFilePath, formatted + Environment.NewLine); } catch (Exception ex) { Debug.WriteLine($"[DEBUG] Error saving log: {ex.Message}"); } } }
        public static void LogError(string m) => Log(DebugCategory.Error, $"[ERR] {m}"); public static void LogWarning(string m) => Log(DebugCategory.General, $"[WARN] {m}"); public static void LogSuccess(string m) => Log(DebugCategory.General, $"[OK] {m}"); public static void LogInfo(string m) => Log(DebugCategory.General, $"[INFO] {m}");
        public static void Block(string m) => Log(DebugCategory.Block, m); public static void BlockSuccess(string m) => Log(DebugCategory.Block, $"[OK] {m}"); public static void BlockError(string m) => Log(DebugCategory.Block, $"[ERR] {m}"); public static void BlockWarning(string m) => Log(DebugCategory.Block, $"[WARN] {m}");
        public static void Signal(string m) => Log(DebugCategory.Signal, m); public static void SignalSuccess(string m) => Log(DebugCategory.Signal, $"[OK] {m}"); public static void SignalError(string m) => Log(DebugCategory.Signal, $"[ERR] {m}"); public static void SignalWarning(string m) => Log(DebugCategory.Signal, $"[WARN] {m}");
        public static void Train(string m) => Log(DebugCategory.Train, m); public static void TrainSuccess(string m) => Log(DebugCategory.Train, $"[OK] {m}"); public static void TrainError(string m) => Log(DebugCategory.Train, $"[ERR] {m}"); public static void TrainWarning(string m) => Log(DebugCategory.Train, $"[WARN] {m}");
        public static void TrainMovement(string m) => Log(DebugCategory.TrainMovement, m); public static void TrainMovementSuccess(string m) => Log(DebugCategory.TrainMovement, $"[OK] {m}"); public static void Input(string m) => Log(DebugCategory.Input, m); public static void Render(string m) => Log(DebugCategory.Render, m); public static void Map(string m) => Log(DebugCategory.Map, m); public static void TrackBuilder(string m) => Log(DebugCategory.TrackBuilder, m); public static void UI(string m) => Log(DebugCategory.UI, m); public static void Performance(string m) => Log(DebugCategory.Performance, m); public static void Error(string m) => Log(DebugCategory.Error, m);
        public static void Update(object gameTime = null) { } public static void Draw(object spriteBatch = null) { }
        private static string FormatMessage(DebugCategory c, string m) { var p = new List<string>(); if (_showTimestamps) p.Add($"[{DateTime.Now:HH:mm:ss.fff}]"); if (_showCategory && c != DebugCategory.All) p.Add($"[{c}]"); p.Add(m); return string.Join(" ", p); }
        public static List<string> GetLogHistory() { lock (_lock) return new List<string>(_logHistory); } public static List<string> GetLogHistory(DebugCategory c) { lock (_lock) return _logHistory.Where(x => x.Contains($"[{c}]")).ToList(); }
        public static void ClearHistory() { lock (_lock) _logHistory.Clear(); Log("[DEBUG] History cleared"); }
        public static void SaveLogToFile(string path) { try { lock (_lock) File.WriteAllLines(path, _logHistory); Log($"[DEBUG] Log saved to: {path}"); } catch (Exception ex) { Debug.WriteLine($"[DEBUG] Error saving log: {ex.Message}"); } }
        public static string GetStatus() { lock (_lock) { int n = _enabledCategories.Count(x => x.Value); return $"[DEBUG] Console={_logToConsole}, File={_logToFile}, Path={_logFilePath}, History={_logHistory.Count}/{_maxLogEntries}, Categories={n}/{_enabledCategories.Count}, MaxOutput={MaxOutputsPerSecond}/s"; } }
        public static void PrintStatus() { Log(GetStatus()); Log("[DEBUG] Active categories:"); foreach (var x in _enabledCategories) if (x.Value && x.Key != DebugCategory.All) Log($"  - {x.Key}"); }
    }
}
