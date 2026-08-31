using System;
using System.IO;
using System.Text.Json;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Small persistence layer for schedule JSON files.</summary>
public static class ScheduleStorage
{
    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "schedules");

    public static string Save(TrainSchedule schedule)
    {
        if (schedule == null) throw new ArgumentNullException(nameof(schedule));
        Directory.CreateDirectory(DirectoryPath);
        string path = Path.Combine(DirectoryPath, $"{schedule.TrainId:N}.json");
        string json = JsonSerializer.Serialize(schedule, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        return path;
    }

    public static TrainSchedule? Load(Guid trainId)
    {
        string path = Path.Combine(DirectoryPath, $"{trainId:N}.json");
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<TrainSchedule>(File.ReadAllText(path));
    }
}
