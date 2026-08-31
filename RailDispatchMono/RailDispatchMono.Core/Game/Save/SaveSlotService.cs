using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RailDispatchMono.Core.Game.Save;

public sealed class SaveSlotMetadata
{
    public int SchemaVersion { get; set; } = 1;
    public string GameVersion { get; set; } = "0.0.16";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime LastSavedAt { get; set; }
    [JsonIgnore] public string DirectoryPath { get; internal set; } = "";
}

public static class SaveSlotService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] RequiredFiles = { "metadata.json", "map.json", "trains.json", "schedules.json", "passengers.json", "economy.json" };

    public static IReadOnlyList<SaveSlotMetadata> GetSlots()
    {
        if (!Directory.Exists(SaveSlotContext.RootDirectory)) return Array.Empty<SaveSlotMetadata>();
        var result = new List<SaveSlotMetadata>();
        foreach (string directory in Directory.GetDirectories(SaveSlotContext.RootDirectory))
        {
            string metadataPath = Path.Combine(directory, "metadata.json");
            try
            {
                if (!File.Exists(metadataPath)) continue;
                var metadata = JsonSerializer.Deserialize<SaveSlotMetadata>(File.ReadAllText(metadataPath), Options);
                if (metadata != null)
                {
                    metadata.DirectoryPath = directory;
                    result.Add(metadata);
                }
            }
            catch { }
        }
        return result.OrderByDescending(x => x.LastSavedAt).ToList();
    }

    public static string CreateSlot()
    {
        Directory.CreateDirectory(SaveSlotContext.RootDirectory);
        DateTime now = DateTime.Now;
        string displayName = now.ToString("d.M.yyyy.HH:mm:ss.fff");
        string directoryName = now.ToString("d.M.yyyy.HH-mm-ss.fff");
        string directory = Path.Combine(SaveSlotContext.RootDirectory, directoryName);
        int suffix = 2;
        while (Directory.Exists(directory)) directory = Path.Combine(SaveSlotContext.RootDirectory, directoryName + "-" + suffix++);

        Directory.CreateDirectory(directory);
        SaveSlotContext.SetActiveSlot(directory);
        WriteMetadata(new SaveSlotMetadata { Name = displayName, DirectoryPath = directory, CreatedAt = now, LastSavedAt = now });
        InitializeEmptyDocuments();
        return directory;
    }

    public static void ValidateSlot(string directory)
    {
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
        foreach (string file in RequiredFiles)
        {
            string path = Path.Combine(directory, file);
            if (!File.Exists(path)) throw new InvalidDataException($"Brak pliku {file}.");
            try
            {
                using JsonDocument _ = JsonDocument.Parse(File.ReadAllText(path));
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Plik {file} jest uszkodzony.", ex);
            }
        }

        var metadata = JsonSerializer.Deserialize<SaveSlotMetadata>(File.ReadAllText(Path.Combine(directory, "metadata.json")), Options)
            ?? throw new InvalidDataException("metadata.json jest pusty lub niepoprawny.");
        if (metadata.SchemaVersion != 1) throw new InvalidDataException($"Nieobsługiwana wersja save: {metadata.SchemaVersion}.");
    }

    private static void InitializeEmptyDocuments()
    {
        WriteJson("map.json", new { schemaVersion = 1, gameVersion = "0.0.16", map = new { width = 100, height = 100 }, tracks = Array.Empty<object>(), signals = Array.Empty<object>(), stations = Array.Empty<object>(), depots = Array.Empty<object>() });
        WriteJson("trains.json", new { schemaVersion = 1, gameVersion = "0.0.16", gameDay = 1, gameTimeSeconds = 0d, trains = Array.Empty<object>() });
        WriteJson("schedules.json", new { schemaVersion = 1, schedules = Array.Empty<object>() });
        WriteJson("passengers.json", new { schemaVersion = 1, passengers = Array.Empty<object>() });
        WriteJson("economy.json", new { schemaVersion = 1, money = 0, income = 0, expenses = 0 });
    }

    private static void WriteJson(string fileName, object value)
        => File.WriteAllText(Path.Combine(SaveSlotContext.ActiveSlotDirectory!, fileName), JsonSerializer.Serialize(value, Options));

    public static void Activate(string directory)
    {
        ValidateSlot(directory);
        SaveSlotContext.SetActiveSlot(directory);
    }

    public static void Touch()
    {
        if (SaveSlotContext.ActiveSlotDirectory == null) return;
        string path = Path.Combine(SaveSlotContext.ActiveSlotDirectory, "metadata.json");
        if (!File.Exists(path)) return;
        var metadata = JsonSerializer.Deserialize<SaveSlotMetadata>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("metadata.json is invalid.");
        metadata.LastSavedAt = DateTime.Now;
        metadata.DirectoryPath = SaveSlotContext.ActiveSlotDirectory;
        WriteMetadata(metadata);
    }

    private static void WriteMetadata(SaveSlotMetadata metadata)
    {
        string directory = SaveSlotContext.ActiveSlotDirectory ?? throw new InvalidOperationException("No active save slot.");
        File.WriteAllText(Path.Combine(directory, "metadata.json"), JsonSerializer.Serialize(metadata, Options));
    }
}
