using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RailDispatchMono.Core.Screens.UI;

/// <summary>
/// Single timetable editor exposed through the existing S workflow.
/// The UI is rendered and managed by Myra; the wagon remains the owner of the schedule.
/// </summary>
public sealed class WagonRouteMenu
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Desktop _desktop;

    private Wagon? _wagon;
    private StationController? _stations;
    private Window? _window;
    private VerticalStackPanel? _routePanel;
    private VerticalStackPanel? _availableStationsPanel;
    private VerticalStackPanel? _timetablePanel;
    private Label? _validationLabel;

    private readonly List<Guid> _baseRoute = new();
    private readonly List<TextBox> _arrivalBoxes = new();
    private readonly List<TextBox> _departureBoxes = new();
    private readonly List<string> _pendingArrivals = new();
    private readonly List<string> _pendingDepartures = new();
    private string _validationMessage = string.Empty;

    public bool IsOpen => _wagon != null && _stations != null && _window != null;
    public event Action<Wagon>? RouteChanged;

    public WagonRouteMenu(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _desktop = new Desktop
        {
            BoundsFetcher = () => new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height)
        };
    }

    public void SetFont(SpriteFont font) { }
    public void LoadContent() { }

    public void Open(Vector2 screenPosition, Wagon wagon, StationController stations)
    {
        _wagon = wagon;
        _stations = stations;
        _baseRoute.Clear();
        _arrivalBoxes.Clear();
        _departureBoxes.Clear();
        _pendingArrivals.Clear();
        _pendingDepartures.Clear();
        _validationMessage = string.Empty;

        if (wagon.Schedule?.BaseStationIds.Count >= 2)
        {
            _baseRoute.AddRange(wagon.Schedule.BaseStationIds);
            _pendingArrivals.AddRange(wagon.Schedule.Points.Select(p => FormatTime(p.ArrivalSeconds)));
            _pendingDepartures.AddRange(wagon.Schedule.Points.Select(p => FormatTime(p.DepartureSeconds)));
        }
        else
        {
            _baseRoute.AddRange(wagon.Route.StationIds);
        }

        BuildEditor();
    }

    public void Close()
    {
        _desktop.Root = null;
        _window = null;
        _routePanel = null;
        _availableStationsPanel = null;
        _timetablePanel = null;
        _validationLabel = null;
        _arrivalBoxes.Clear();
        _departureBoxes.Clear();
        _pendingArrivals.Clear();
        _pendingDepartures.Clear();
        _wagon = null;
        _stations = null;
    }

    public void Update(MouseState mouse)
    {
        if (!IsOpen)
            return;

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Close();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen)
            return;

        _desktop.Render();
    }

    private void BuildEditor()
    {
        if (_wagon == null || _stations == null)
            return;

        var root = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _window = new Window
        {
            Title = $"ROZKŁAD WAGONU — {_wagon.ShortName}",
            Width = Math.Min(1180, Math.Max(900, _graphicsDevice.Viewport.Width - 80)),
            Height = Math.Min(780, Math.Max(620, _graphicsDevice.Viewport.Height - 80)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var main = new Grid { RowSpacing = 10, ColumnSpacing = 14 };
        main.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 360));
        main.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        main.RowsProportions.Add(new Proportion(ProportionType.Part, 1));
        main.RowsProportions.Add(new Proportion(ProportionType.Auto));

        BuildRouteSection(main);
        BuildTimetableSection(main);
        BuildActions(main);

        _window.Content = main;
        root.Widgets.Add(_window);
        _desktop.Root = root;
    }

    private void BuildRouteSection(Grid main)
    {
        var section = new VerticalStackPanel { Spacing = 8 };
        section.Widgets.Add(new Label { Text = "TRASA" });
        section.Widgets.Add(new Label
        {
            Text = "Kolejność bazowa. System rozwija ją do pełnej pętli A-B-...-B-A.",
            Wrap = true
        });

        var routeScroll = new ScrollViewer
        {
            Height = 430,
            Content = new VerticalStackPanel { Spacing = 5 }
        };
        _routePanel = (VerticalStackPanel)routeScroll.Content;
        section.Widgets.Add(routeScroll);

        var addButton = CreateButton("DODAJ STACJĘ", 150);
        addButton.Click += (_, _) => RebuildAvailableStations();
        section.Widgets.Add(addButton);

        _availableStationsPanel = new VerticalStackPanel { Spacing = 4 };
        section.Widgets.Add(new ScrollViewer
        {
            Height = 180,
            Content = _availableStationsPanel
        });

        Grid.SetColumn(section, 0);
        Grid.SetRow(section, 0);
        main.Widgets.Add(section);
        RebuildRoutePanel();
    }

    private void BuildTimetableSection(Grid main)
    {
        var section = new VerticalStackPanel { Spacing = 8 };
        section.Widgets.Add(new Label { Text = "ROZKŁAD" });
        section.Widgets.Add(new Label
        {
            Text = "Każdy punkt ma niezależny PRZYJAZD i ODJAZD. Terminale A/D mogą mieć dłuższy postój.",
            Wrap = true
        });

        var header = new Grid { ColumnSpacing = 8 };
        header.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 42));
        header.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        header.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 105));
        header.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 105));
        AddGridLabel(header, "#", 0);
        AddGridLabel(header, "STACJA", 1);
        AddGridLabel(header, "PRZYJAZD", 2);
        AddGridLabel(header, "ODJAZD", 3);
        section.Widgets.Add(header);

        var scroll = new ScrollViewer
        {
            Height = 485,
            Content = new VerticalStackPanel { Spacing = 5 }
        };
        _timetablePanel = (VerticalStackPanel)scroll.Content;
        section.Widgets.Add(scroll);

        _validationLabel = new Label { Text = _validationMessage, Wrap = true };
        section.Widgets.Add(_validationLabel);

        Grid.SetColumn(section, 1);
        Grid.SetRow(section, 0);
        main.Widgets.Add(section);
        RebuildTimetablePanel();
    }

    private void BuildActions(Grid main)
    {
        var actions = new HorizontalStackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var save = CreateButton("ZAPISZ", 120);
        save.Click += (_, _) => SaveSchedule();
        actions.Widgets.Add(save);

        var cancel = CreateButton("ANULUJ", 120);
        cancel.Click += (_, _) => Close();
        actions.Widgets.Add(cancel);

        var delete = CreateButton("USUŃ ROZKŁAD", 150);
        delete.Click += (_, _) => DeleteSchedule();
        actions.Widgets.Add(delete);

        Grid.SetColumn(actions, 0);
        Grid.SetRow(actions, 1);
        Grid.SetColumnSpan(actions, 2);
        main.Widgets.Add(actions);
    }

    private void RebuildRoutePanel()
    {
        if (_routePanel == null || _stations == null)
            return;

        _routePanel.Widgets.Clear();
        for (int i = 0; i < _baseRoute.Count; i++)
        {
            int index = i;
            var row = new HorizontalStackPanel { Spacing = 4 };
            row.Widgets.Add(new Label { Text = $"{index + 1}.", Width = 28 });
            var station = FindStation(_baseRoute[index]);
            row.Widgets.Add(new Label { Text = station?.Name ?? "BRAK STACJI", Width = 165, Wrap = true });

            var up = CreateButton("GÓRA", 58);
            if (index > 0)
                up.Click += (_, _) => MoveRoute(index, -1);
            row.Widgets.Add(up);

            var down = CreateButton("DÓŁ", 58);
            if (index < _baseRoute.Count - 1)
                down.Click += (_, _) => MoveRoute(index, 1);
            row.Widgets.Add(down);

            var remove = CreateButton("USUŃ", 58);
            remove.Click += (_, _) => RemoveRouteStation(index);
            row.Widgets.Add(remove);
            _routePanel.Widgets.Add(row);
        }

        if (_baseRoute.Count == 0)
            _routePanel.Widgets.Add(new Label { Text = "Brak stacji. Kliknij DODAJ STACJĘ." });

        RebuildAvailableStations();
    }

    private void RebuildAvailableStations()
    {
        if (_availableStationsPanel == null || _stations == null)
            return;

        _availableStationsPanel.Widgets.Clear();
        foreach (var station in _stations.Stations)
        {
            if (_baseRoute.Contains(station.Id))
                continue;

            var button = CreateButton($"DODAJ: {station.Name}", 300);
            Guid id = station.Id;
            button.Click += (_, _) => AddRouteStation(id);
            _availableStationsPanel.Widgets.Add(button);
        }

        if (_availableStationsPanel.Widgets.Count == 0)
            _availableStationsPanel.Widgets.Add(new Label { Text = "Brak dostępnych stacji." });
    }

    private void AddRouteStation(Guid stationId)
    {
        if (_baseRoute.Contains(stationId))
            return;

        CaptureTimetableTexts();
        _baseRoute.Add(stationId);
        RebuildRouteAndTimetable();
    }

    private void RemoveRouteStation(int index)
    {
        if (index < 0 || index >= _baseRoute.Count)
            return;

        CaptureTimetableTexts();
        _baseRoute.RemoveAt(index);
        RebuildRouteAndTimetable();
    }

    private void MoveRoute(int index, int direction)
    {
        int target = index + direction;
        if (index < 0 || target < 0 || index >= _baseRoute.Count || target >= _baseRoute.Count)
            return;

        CaptureTimetableTexts();
        (_baseRoute[index], _baseRoute[target]) = (_baseRoute[target], _baseRoute[index]);
        RebuildRouteAndTimetable();
    }

    private void RebuildRouteAndTimetable()
    {
        RebuildRoutePanel();
        RebuildTimetablePanel();
    }

    private void RebuildTimetablePanel()
    {
        if (_timetablePanel == null || _stations == null)
            return;

        _timetablePanel.Widgets.Clear();
        _arrivalBoxes.Clear();
        _departureBoxes.Clear();

        var loop = _baseRoute.Concat(_baseRoute.Skip(1).Reverse()).ToList();
        int pointCount = loop.Count;

        for (int i = 0; i < pointCount; i++)
        {
            int index = i;
            var station = FindStation(loop[i]);
            bool terminal = i == 0 || i == pointCount - 1;

            var row = new Grid { ColumnSpacing = 8, RowSpacing = 2 };
            row.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 42));
            row.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
            row.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 105));
            row.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 105));

            AddGridLabel(row, (index + 1).ToString(CultureInfo.InvariantCulture), 0);
            AddGridLabel(row, terminal ? $"{station?.Name ?? "BRAK"} [TERMINAL]" : station?.Name ?? "BRAK", 1);

            var arrival = new TextBox { Text = GetPendingTime(_pendingArrivals, index), Width = 100 };
            Grid.SetColumn(arrival, 2);
            row.Widgets.Add(arrival);
            _arrivalBoxes.Add(arrival);

            var departure = new TextBox { Text = GetPendingTime(_pendingDepartures, index), Width = 100 };
            Grid.SetColumn(departure, 3);
            row.Widgets.Add(departure);
            _departureBoxes.Add(departure);
            _timetablePanel.Widgets.Add(row);
        }

        if (pointCount == 0)
            _timetablePanel.Widgets.Add(new Label { Text = "Dodaj co najmniej dwie stacje, aby utworzyć rozkład." });

        if (_validationLabel != null)
            _validationLabel.Text = _validationMessage;
    }

    private static Grid CreateTimetableGrid()
    {
        var grid = new Grid { ColumnSpacing = 8, RowSpacing = 2 };
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 42));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 1));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 105));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, 105));
        return grid;
    }

    private static void AddGridLabel(Grid grid, string text, int column)
    {
        var label = new Label { Text = text, Wrap = true };
        Grid.SetColumn(label, column);
        grid.Widgets.Add(label);
    }

    private void CaptureTimetableTexts()
    {
        if (_arrivalBoxes.Count == 0 && _departureBoxes.Count == 0)
            return;

        _pendingArrivals.Clear();
        _pendingDepartures.Clear();
        _pendingArrivals.AddRange(_arrivalBoxes.Select(x => x.Text ?? string.Empty));
        _pendingDepartures.AddRange(_departureBoxes.Select(x => x.Text ?? string.Empty));
    }

    private static string GetPendingTime(List<string> values, int index)
        => index >= 0 && index < values.Count && !string.IsNullOrWhiteSpace(values[index]) ? values[index] : "00:00";

    private void SaveSchedule()
    {
        if (_wagon == null)
            return;

        CaptureTimetableTexts();
        if (_baseRoute.Count < 2)
        {
            SetValidation("BŁĄD: rozkład wymaga co najmniej dwóch stacji.");
            return;
        }

        var schedule = new WagonSchedule
        {
            Id = _wagon.Schedule?.Id ?? Guid.NewGuid(),
            Name = _wagon.ShortName + " — rozkład",
            BaseStationIds = _baseRoute.ToList(),
            Enabled = true
        };
        schedule.BuildLoopFromBaseRoute();

        for (int i = 0; i < schedule.Points.Count; i++)
        {
            if (!TryParseTime(_pendingArrivals[i], out int arrival))
            {
                SetValidation($"BŁĄD: nieprawidłowy PRZYJAZD w punkcie {i + 1}. Użyj HH:MM.");
                return;
            }

            if (!TryParseTime(_pendingDepartures[i], out int departure))
            {
                SetValidation($"BŁĄD: nieprawidłowy ODJAZD w punkcie {i + 1}. Użyj HH:MM.");
                return;
            }

            schedule.Points[i].ArrivalSeconds = arrival;
            schedule.Points[i].DepartureSeconds = departure;
        }

        if (!schedule.IsValid(out string error))
        {
            SetValidation("BŁĄD: " + error);
            return;
        }

        _wagon.Route.Clear();
        foreach (Guid stationId in _baseRoute)
            _wagon.Route.AddStation(stationId);

        _wagon.SetSchedule(schedule);
        RouteChanged?.Invoke(_wagon);
        Close();
    }

    private void DeleteSchedule()
    {
        if (_wagon == null)
            return;

        _wagon.SetSchedule(null);
        RouteChanged?.Invoke(_wagon);
        Close();
    }

    private void SetValidation(string message)
    {
        _validationMessage = message;
        if (_validationLabel != null)
            _validationLabel.Text = message;
    }

    private Station? FindStation(Guid id)
        => _stations?.Stations.FirstOrDefault(s => s.Id == id);

    private static Button CreateButton(string text, int width)
        => new Button
        {
            Width = width,
            Height = 32,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content = new Label { Text = text }
        };

    private static string FormatTime(int seconds)
    {
        if (seconds < 0)
            seconds = 0;
        return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm", CultureInfo.InvariantCulture);
    }

    private static bool TryParseTime(string? text, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string value = text.Trim();
        string[] formats = { @"hh\:mm", @"h\:mm", "hhmm", "hmm" };
        if (!TimeSpan.TryParseExact(value, formats, CultureInfo.InvariantCulture, out TimeSpan time))
            return false;

        if (time < TimeSpan.Zero || time.TotalSeconds >= 24 * 60 * 60)
            return false;

        seconds = (int)time.TotalSeconds;
        return true;
    }
}
