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

/// <summary>Creates and enumerates versioned save slots.</summary>
public static class SaveSlotService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

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
        return directory;
    }

    public static void Activate(string directory)
    {
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
        if (!File.Exists(Path.Combine(directory, "metadata.json"))) throw new InvalidDataException("metadata.json is missing.");
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
