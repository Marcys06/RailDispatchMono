// ============================================================
// DEBUGMANAGER.CS - CENTRALNY SYSTEM DEBUGOWANIA
// ============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RailDispatchMono.Core
{
    /// <summary>
    /// Centralny system debugowania - zarz¹dza logowaniem i wyœwietlaniem informacji
    /// </summary>
    public static class DebugManager
    {
        // ============================================================
        // KATEGORIE DEBUGOWANIA
        // ============================================================

        public enum DebugCategory
        {
            General,
            Block,
            Signal,
            Train,
            TrainMovement,
            Camera,
            Input,
            Render,
            Map,
            TrackBuilder,
            UI,
            Performance,
            Error,
            All
        }

        // ============================================================
        // KONFIGURACJA I W£AŒCIWOŒCI KOMPATYBILNOŒCI
        // ============================================================

        private static readonly Dictionary<DebugCategory, bool> _enabledCategories = new();
        private static bool _logToConsole = true;
        private static bool _logToFile = false;
        private static string _logFilePath = "debug_log.txt";
        private static bool _showTimestamps = true;
        private static bool _showCategory = true;
        private static int _maxLogEntries = 1000;
        private static readonly List<string> _logHistory = new();
        private static readonly object _lock = new();

        public static bool IsDebugEnabled { get; set; } = true;
        public static bool LogToConsole 
        { 
            get => _logToConsole; 
            set => _logToConsole = value; 
        }

        // ============================================================
        // INICJALIZACJA
        // ============================================================

        static DebugManager()
        {
            foreach (DebugCategory category in Enum.GetValues(typeof(DebugCategory)))
            {
                _enabledCategories[category] = true;
            }

            _enabledCategories[DebugCategory.Performance] = false;
            _enabledCategories[DebugCategory.Camera] = false;
            _enabledCategories[DebugCategory.TrackBuilder] = false;
            _enabledCategories[DebugCategory.UI] = false;
        }

        // ============================================================
        // KONFIGURACJA
        // ============================================================

        public static void EnableCategory(DebugCategory category)
        {
            _enabledCategories[category] = true;
            Log($"[DEBUG] Enabled: {category}");
        }

        public static void DisableCategory(DebugCategory category)
        {
            _enabledCategories[category] = false;
            Log($"[DEBUG] Disabled: {category}");
        }

        public static void ToggleCategory(DebugCategory category)
        {
            _enabledCategories[category] = !_enabledCategories[category];
            Log($"[DEBUG] {category}: {(_enabledCategories[category] ? "ON" : "OFF")}");
        }

        public static bool IsCategoryEnabled(DebugCategory category)
        {
            return _enabledCategories.TryGetValue(category, out bool enabled) && enabled;
        }

        public static void EnableAll()
        {
            foreach (var category in _enabledCategories.Keys.ToList())
            {
                _enabledCategories[category] = true;
            }
            Log("[DEBUG] All categories enabled");
        }

        public static void DisableAll()
        {
            foreach (var category in _enabledCategories.Keys.ToList())
            {
                _enabledCategories[category] = false;
            }
            Log("[DEBUG] All categories disabled");
        }

        public static void SetLogToConsole(bool enabled)
        {
            _logToConsole = enabled;
        }

        public static void SetLogToFile(bool enabled, string filePath = null)
        {
            _logToFile = enabled;
            if (filePath != null)
            {
                _logFilePath = filePath;
            }
            Log($"[DEBUG] Log to file: {(_logToFile ? "ON" : "OFF")} -> {_logFilePath}");
        }

        public static void SetShowTimestamps(bool enabled)
        {
            _showTimestamps = enabled;
        }

        public static void SetShowCategory(bool enabled)
        {
            _showCategory = enabled;
        }

        // ============================================================
        // G£ÓWNA METODA LOGOWANIA
        // ============================================================

        public static void Log(DebugCategory category, string message)
        {
            if (!IsDebugEnabled) return;

            if (!_enabledCategories.TryGetValue(category, out bool enabled) || !enabled)
                return;

            if (!_enabledCategories[DebugCategory.All] && category == DebugCategory.All)
                return;

            string formattedMessage = FormatMessage(category, message);

            lock (_lock)
            {
                _logHistory.Add(formattedMessage);
                if (_logHistory.Count > _maxLogEntries)
                {
                    _logHistory.RemoveAt(0);
                }

                if (_logToConsole)
                {
                    Console.WriteLine(formattedMessage);
                }

                if (_logToFile)
                {
                    try
                    {
                        File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Error saving to file: {ex.Message}");
                    }
                }
            }
        }

        public static void Log(string message) => Log(DebugCategory.General, message);
        public static void LogError(string message) => Log(DebugCategory.Error, $"[ERR] {message}");
        public static void LogWarning(string message) => Log(DebugCategory.General, $"[WARN] {message}");
        public static void LogSuccess(string message) => Log(DebugCategory.General, $"[OK] {message}");
        public static void LogInfo(string message) => Log(DebugCategory.General, $"[INFO] {message}");

        // ============================================================
        // METODY SKRÓTU DLA KA¯DEJ KATEGORII
        // ============================================================

        public static void Block(string message) => Log(DebugCategory.Block, message);
        public static void BlockSuccess(string message) => Log(DebugCategory.Block, $"[OK] {message}");
        public static void BlockError(string message) => Log(DebugCategory.Block, $"[ERR] {message}");
        public static void BlockWarning(string message) => Log(DebugCategory.Block, $"[WARN] {message}");

        public static void Signal(string message) => Log(DebugCategory.Signal, message);
        public static void SignalSuccess(string message) => Log(DebugCategory.Signal, $"[OK] {message}");
        public static void SignalError(string message) => Log(DebugCategory.Signal, $"[ERR] {message}");
        public static void SignalWarning(string message) => Log(DebugCategory.Signal, $"[WARN] {message}");

        public static void Train(string message) => Log(DebugCategory.Train, message);
        public static void TrainSuccess(string message) => Log(DebugCategory.Train, $"[OK] {message}");
        public static void TrainError(string message) => Log(DebugCategory.Train, $"[ERR] {message}");
        public static void TrainWarning(string message) => Log(DebugCategory.Train, $"[WARN] {message}");

        public static void TrainMovement(string message) => Log(DebugCategory.TrainMovement, message);
        public static void TrainMovementSuccess(string message) => Log(DebugCategory.TrainMovement, $"[OK] {message}");

        public static void Input(string message) => Log(DebugCategory.Input, message);
        public static void Render(string message) => Log(DebugCategory.Render, message);
        public static void Map(string message) => Log(DebugCategory.Map, message);
        public static void TrackBuilder(string message) => Log(DebugCategory.TrackBuilder, message);
        public static void UI(string message) => Log(DebugCategory.UI, message);
        public static void Performance(string message) => Log(DebugCategory.Performance, message);
        public static void Error(string message) => Log(DebugCategory.Error, message);

        // ============================================================
        // METODY SKRÓTU CYKLU GRY
        // ============================================================

        public static void Update(object gameTime = null) { }
        public static void Draw(object spriteBatch = null) { }

        // ============================================================
        // FORMATOWANIE
        // ============================================================

        private static string FormatMessage(DebugCategory category, string message)
        {
            var parts = new List<string>();

            if (_showTimestamps)
            {
                parts.Add($"[{DateTime.Now:HH:mm:ss.fff}]");
            }

            if (_showCategory && category != DebugCategory.All)
            {
                parts.Add($"[{category}]");
            }

            parts.Add(message);

            return string.Join(" ", parts);
        }

        // ============================================================
        // ZARZ¥DZANIE HISTORI¥
        // ============================================================

        public static List<string> GetLogHistory()
        {
            lock (_lock)
            {
                return new List<string>(_logHistory);
            }
        }

        public static List<string> GetLogHistory(DebugCategory category)
        {
            lock (_lock)
            {
                return _logHistory.Where(line => line.Contains($"[{category}]")).ToList();
            }
        }

        public static void ClearHistory()
        {
            lock (_lock)
            {
                _logHistory.Clear();
            }
            Log("[DEBUG] History cleared");
        }

        public static void SaveLogToFile(string filePath)
        {
            try
            {
                lock (_lock)
                {
                    File.WriteAllLines(filePath, _logHistory);
                }
                Log($"[DEBUG] Log saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Log($"[DEBUG] Error saving log: {ex.Message}");
            }
        }

        // ============================================================
        // STAN
        // ============================================================

        public static string GetStatus()
        {
            lock (_lock)
            {
                int enabledCount = _enabledCategories.Count(kv => kv.Value);
                return $"[DEBUG] Console={_logToConsole}, File={_logToFile}, History={_logHistory.Count}/{_maxLogEntries}, Categories={enabledCount}/{_enabledCategories.Count}";
            }
        }

        public static void PrintStatus()
        {
            Log(GetStatus());
            Log("[DEBUG] Active categories:");
            foreach (var kv in _enabledCategories)
            {
                if (kv.Value && kv.Key != DebugCategory.All)
                {
                    Log($"  - {kv.Key}");
                }
            }
        }
    }
}
