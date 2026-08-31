using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RailDispatchMono.Core.Game.Save;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Persists all train schedules into the active save slot.</summary>
public static class ScheduleStorage
{
    private sealed class ScheduleDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public List<TrainSchedule> Schedules { get; set; } = new();
    }

    public static string DirectoryPath => SaveSlotContext.ActiveSlotDirectory ?? Path.Combine(AppContext.BaseDirectory, "schedules");
    public static string FilePath => SaveSlotContext.ActiveSlotDirectory != null
        ? Path.Combine(SaveSlotContext.ActiveSlotDirectory, "schedules.json")
        : Path.Combine(DirectoryPath, "schedules.json");

    public static string Save(TrainSchedule schedule)
    {
        if (schedule == null) throw new ArgumentNullException(nameof(schedule));
        Directory.CreateDirectory(DirectoryPath);
        var document = LoadDocument();
        document.Schedules.RemoveAll(x => x.TrainId == schedule.TrainId);
        document.Schedules.Add(schedule);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }));
        return FilePath;
    }

    public static TrainSchedule? Load(Guid trainId)
        => LoadDocument().Schedules.FirstOrDefault(x => x.TrainId == trainId);

    public static IReadOnlyList<TrainSchedule> LoadAll() => LoadDocument().Schedules;

    private static ScheduleDocument LoadDocument()
    {
        if (!File.Exists(FilePath)) return new ScheduleDocument();
        try
        {
            return JsonSerializer.Deserialize<ScheduleDocument>(File.ReadAllText(FilePath)) ?? new ScheduleDocument();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("schedules.json is invalid.", ex);
        }
    }
}
