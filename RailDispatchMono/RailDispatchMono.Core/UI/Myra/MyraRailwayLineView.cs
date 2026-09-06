using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>F11 editor for named railway lines, multi-track selection and bulk infrastructure changes.</summary>
internal sealed class MyraRailwayLineView
{
    public Widget Root { get; }
    private readonly GameMap _map;
    private readonly RailwayLineManager _lines;
    private readonly Func<IReadOnlyCollection<MapPosition>> _getSelection;
    private readonly Action<IEnumerable<MapPosition>> _setSelection;
    private readonly Action _close;
    private readonly TextBox _nameBox;
    private readonly Label _status;
    private readonly VerticalStackPanel _linesPanel;
    private readonly Label _selectionLabel;
    private TrackType _trackType = TrackType.Mainline;
    private LineClass _lineClass = LineClass.Mainline;
    private TractionSystem _traction = TractionSystem.None;
    private Guid? _selectedLineId;

    public MyraRailwayLineView(GameMap map, RailwayLineManager lines, Func<IReadOnlyCollection<MapPosition>> getSelection, Action<IEnumerable<MapPosition>> setSelection, Action close)
    {
        _map = map;
        _lines = lines;
        _getSelection = getSelection;
        _setSelection = setSelection;
        _close = close;

        var root = new VerticalStackPanel
        {
            Width = 1200,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 6
        };

        root.Widgets.Add(new Label { Text = "LINIE KOLEJOWE — F11", Wrap = true });
        root.Widgets.Add(new Label
        {
            Text = "LPM na mapie: nowe zaznaczenie • Shift+LPM: dodaj tor • Ctrl+LPM: przełącz tor. F11 otwiera ten ekran.",
            Wrap = true
        });

        // Wiersz tworzenia i zarządzania linią
        var createRow = new HorizontalStackPanel { Spacing = 5 };
        _nameBox = new TextBox { Text = "Nowa linia", Width = 280 };
        createRow.Widgets.Add(_nameBox);
        createRow.Widgets.Add(CreateButton("UTWÓRZ Z ZAZNACZENIA", 220, CreateLine));
        createRow.Widgets.Add(CreateButton("ZMIEŃ NAZWĘ", 130, RenameCurrentLine));
        createRow.Widgets.Add(CreateButton("ZAZNACZ LINIĘ", 150, SelectCurrentLine));
        createRow.Widgets.Add(CreateButton("USUŃ LINIĘ", 130, DeleteCurrentLine));
        root.Widgets.Add(createRow);

        _selectionLabel = new Label { Wrap = true };
        root.Widgets.Add(_selectionLabel);

        // Sekcja masowych zmian
        var bulk = new VerticalStackPanel { Spacing = 4 };
        bulk.Widgets.Add(new Label { Text = "MASOWA ZMIANA PARAMETRÓW", Wrap = true });

        var bulkRow1 = new HorizontalStackPanel { Spacing = 4 };
        var trackTypeLabel = new Label { Text = "TrackType: " + _trackType, Width = 180 };
        bulkRow1.Widgets.Add(trackTypeLabel);
        bulkRow1.Widgets.Add(CreateButton("◀", 45, () => CycleTrackType(-1)));
        bulkRow1.Widgets.Add(CreateButton("▶", 45, () => CycleTrackType(1)));
        bulkRow1.Widgets.Add(CreateButton("ZASTOSUJ DO ZAZNACZENIA", 220, ApplyToSelection));
        bulkRow1.Widgets.Add(CreateButton("ZASTOSUJ DO LINII", 170, ApplyToLine));
        bulk.Widgets.Add(bulkRow1);

        var bulkRow2 = new HorizontalStackPanel { Spacing = 4 };
        var lineClassLabel = new Label { Text = "LineClass: " + _lineClass, Width = 180 };
        bulkRow2.Widgets.Add(lineClassLabel);
        bulkRow2.Widgets.Add(CreateButton("◀", 45, () => CycleLineClass(-1)));
        bulkRow2.Widgets.Add(CreateButton("▶", 45, () => CycleLineClass(1)));
        bulkRow2.Widgets.Add(CreateButton("DODAJ ZAZNACZENIE DO LINII", 220, AddSelectionToLine));
        bulkRow2.Widgets.Add(CreateButton("USUŃ ZAZNACZENIE Z LINII", 220, RemoveSelectionFromLine));
        bulk.Widgets.Add(bulkRow2);

        var bulkRow3 = new HorizontalStackPanel { Spacing = 4 };
        var tractionLabel = new Label { Text = "TractionSystem: " + _traction, Width = 180 };
        bulkRow3.Widgets.Add(tractionLabel);
        bulkRow3.Widgets.Add(CreateButton("◀", 45, () => CycleTraction(-1)));
        bulkRow3.Widgets.Add(CreateButton("▶", 45, () => CycleTraction(1)));
        bulkRow3.Widgets.Add(CreateButton("ZAZNACZ POŁĄCZONY OBSZAR", 220, SelectConnectedArea));
        bulkRow3.Widgets.Add(CreateButton("WYCZYŚĆ ZAZNACZENIE", 170, () => { _setSelection(Array.Empty<MapPosition>()); Refresh(); }));
        bulk.Widgets.Add(bulkRow3);
        root.Widgets.Add(bulk);

        // Lista zapisanych linii
        _linesPanel = new VerticalStackPanel { Spacing = 4 };
        root.Widgets.Add(new Label { Text = "ZAPISANE LINIE", Wrap = true });
        root.Widgets.Add(new ScrollViewer { Height = 520, Content = _linesPanel });

        _status = new Label { Wrap = true };
        root.Widgets.Add(_status);
        root.Widgets.Add(CreateButton("POWRÓT", 140, _close));

        Root = root;
        Refresh();
    }

    private void CreateLine()
    {
        var selection = _getSelection().Where(_map.HasTrack).Distinct().ToList();
        if (selection.Count == 0)
        {
            _status.Text = "BŁĄD: zaznacz co najmniej jeden tor na mapie.";
            return;
        }

        var line = _lines.Create(_nameBox.Text, selection);
        _selectedLineId = line.Id;
        _status.Text = $"Utworzono „{line.Name}” — segmentów: {line.TrackPositions.Count}.";
        Refresh();
    }

    private RailwayLine? CurrentLine => _selectedLineId.HasValue ? _lines.Find(_selectedLineId.Value) : null;

    private void RenameCurrentLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            _status.Text = "Nazwa nie może być pusta.";
            return;
        }

        line.Rename(_nameBox.Text);
        _status.Text = $"Linia ma teraz nazwę „{line.Name}”.";
        Refresh();
    }

    private void SelectCurrentLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        _setSelection(line.TrackPositions.ToList());
        _nameBox.Text = line.Name;
        _status.Text = $"Zaznaczono linię „{line.Name}”.";
        Refresh();
    }

    private void DeleteCurrentLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        _lines.Delete(line.Id);
        _selectedLineId = null;
        _status.Text = "Linia usunięta. Tory pozostają bez zmian.";
        Refresh();
    }

    private void AddSelectionToLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        _lines.AddPositions(line, _getSelection());
        _status.Text = $"Linia „{line.Name}”: {line.TrackPositions.Count} segmentów.";
        Refresh();
    }

    private void RemoveSelectionFromLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        _lines.RemovePositions(line, _getSelection());
        _status.Text = $"Linia „{line.Name}”: {line.TrackPositions.Count} segmentów.";
        Refresh();
    }

    private void ApplyToSelection()
    {
        int count = _lines.ApplyInfrastructure(_getSelection(), _trackType, _lineClass, _traction);
        _status.Text = $"Zmieniono parametry {count} zaznaczonych segmentów.";
    }

    private void ApplyToLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        int count = _lines.ApplyInfrastructure(line, _trackType, _lineClass, _traction);
        _status.Text = $"Zmieniono parametry {count} segmentów linii „{line.Name}”."; // Poprawiono: dodano cudzysłów zamykający
    }

    private void SelectConnectedArea()
    {
        var first = _getSelection().FirstOrDefault();
        if (!_map.HasTrack(first))
        {
            _status.Text = "Zaznacz najpierw jeden tor startowy.";
            return;
        }

        _setSelection(RailwayLineManager.CollectConnectedTracks(_map, first));
        _status.Text = "Zaznaczono cały połączony obszar torów.";
        Refresh();
    }

    private void CycleTrackType(int delta)
    {
        var values = Enum.GetValues<TrackType>();
        _trackType = values[(Array.IndexOf(values, _trackType) + delta + values.Length) % values.Length];
        Refresh();
    }

    private void CycleLineClass(int delta)
    {
        var values = Enum.GetValues<LineClass>();
        _lineClass = values[(Array.IndexOf(values, _lineClass) + delta + values.Length) % values.Length];
        Refresh();
    }

    private void CycleTraction(int delta)
    {
        var values = Enum.GetValues<TractionSystem>();
        _traction = values[(Array.IndexOf(values, _traction) + delta + values.Length) % values.Length];
        Refresh();
    }

    private void Refresh()
    {
        var current = CurrentLine;
        _selectionLabel.Text = $"Zaznaczenie: {_getSelection().Count} torów" +
                               (current == null ? "" : $" • linia: {current.Name} ({current.TrackPositions.Count} segmentów)");

        _linesPanel.Widgets.Clear();

        foreach (var line in _lines.Lines)
        {
            var local = line;
            var row = new HorizontalStackPanel { Spacing = 4 };

            row.Widgets.Add(new Label
            {
                Text = $"{line.Name} • {line.TrackPositions.Count} torów • kolor {line.ColorIndex + 1}",
                Width = 520,
                Wrap = true
            });

            row.Widgets.Add(CreateButton("WYBIERZ", 100, () =>
            {
                _selectedLineId = local.Id;
                _nameBox.Text = local.Name;
                Refresh();
            }));

            row.Widgets.Add(CreateButton("ZAZNACZ", 100, () =>
            {
                _selectedLineId = local.Id;
                SelectCurrentLine();
            }));

            row.Widgets.Add(CreateButton("+ ZAZNACZENIE", 130, () =>
            {
                _selectedLineId = local.Id;
                AddSelectionToLine();
            }));

            row.Widgets.Add(CreateButton("− ZAZNACZENIE", 130, () =>
            {
                _selectedLineId = local.Id;
                RemoveSelectionFromLine();
            }));

            row.Widgets.Add(CreateButton("USUŃ", 80, () =>
            {
                _selectedLineId = local.Id;
                DeleteCurrentLine();
            }));

            _linesPanel.Widgets.Add(row);
        }
    }

    private static Button CreateButton(string text, int width, Action action)
    {
        var button = new Button
        {
            Content = new Label { Text = text, Wrap = true },
            Width = width,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        button.Click += (_, _) => action();
        return button;
    }
}