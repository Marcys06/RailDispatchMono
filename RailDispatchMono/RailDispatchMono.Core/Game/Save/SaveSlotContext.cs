using System;
using System.IO;

namespace RailDispatchMono.Core.Game.Save;

/// <summary>Global save-slot selection shared by gameplay persistence services.</summary>
public static class SaveSlotContext
{
    private const string RootName = "RailDispatchMono";
    private const string SlotsName = "Saves";

    public static string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        RootName,
        SlotsName);

    public static string? ActiveSlotDirectory { get; private set; }

    public static void SetActiveSlot(string slotDirectory)
    {
        if (string.IsNullOrWhiteSpace(slotDirectory))
            throw new ArgumentException("Save slot directory is required.", nameof(slotDirectory));

        Directory.CreateDirectory(slotDirectory);
        ActiveSlotDirectory = Path.GetFullPath(slotDirectory);
    }

    public static void ClearActiveSlot() => ActiveSlotDirectory = null;
}
