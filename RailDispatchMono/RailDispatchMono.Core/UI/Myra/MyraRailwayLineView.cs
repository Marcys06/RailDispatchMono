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
    private readonly Label _trackTypeLabel;
    private readonly Label _lineClassLabel;
    private readonly Label _tractionLabel;
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
            Width = 1160,
            Height = 700,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5
        };

        root.Widgets.Add(new Label { Text = "LINIE KOLEJOWE — F11", Wrap = true });
        root.Widgets.Add(new Label
        {
            Text = "Zarządzanie nazwanymi liniami i masowe parametry infrastruktury. LPM: nowe zaznaczenie • Shift+LPM: dodaj • Ctrl+LPM: przełącz.",
            Wrap = true
        });

        var content = new VerticalStackPanel { Spacing = 7 };

        // 1. Zaznaczenie i podstawowe operacje na linii.
        content.Widgets.Add(new Label { Text = "AKTYWNE ZAZNACZENIE", Wrap = true });
        _selectionLabel = new Label { Wrap = true };
        content.Widgets.Add(_selectionLabel);

        var selectionRow = new HorizontalStackPanel { Spacing = 5 };
        selectionRow.Widgets.Add(CreateButton("ZAZNACZ POŁĄCZONY OBSZAR", 220, SelectConnectedArea));
        selectionRow.Widgets.Add(CreateButton("WYCZYŚĆ ZAZNACZENIE", 190, () =>
        {
            _setSelection(Array.Empty<MapPosition>());
            _status.Text = "Zaznaczenie wyczyszczone.";
            Refresh();
        }));
        content.Widgets.Add(selectionRow);

        // 2. Tworzenie, wybór, zmiana nazwy i usuwanie.
        content.Widgets.Add(new Label { Text = "ZARZĄDZANIE LINIĄ", Wrap = true });
        var nameRow = new HorizontalStackPanel { Spacing = 5 };
        _nameBox = new TextBox { Text = "Nowa linia", Width = 270 };
        nameRow.Widgets.Add(_nameBox);
        nameRow.Widgets.Add(CreateButton("UTWÓRZ Z ZAZNACZENIA", 210, CreateLine));
        nameRow.Widgets.Add(CreateButton("ZMIEŃ NAZWĘ", 145, RenameCurrentLine));
        nameRow.Widgets.Add(CreateButton("ZAZNACZ LINIĘ", 145, SelectCurrentLine));
        nameRow.Widgets.Add(CreateButton("USUŃ LINIĘ", 125, DeleteCurrentLine));
        content.Widgets.Add(nameRow);

        // 3. Masowa zmiana parametrów — każdy parametr ma niezależny wybór i zastosowanie.
        content.Widgets.Add(new Label { Text = "MASOWA ZMIANA PARAMETRÓW INFRASTRUKTURY", Wrap = true });

        var trackTypeRow = new HorizontalStackPanel { Spacing = 5 };
        _trackTypeLabel = new Label { Text = "TrackType: " + _trackType, Width = 210, Wrap = true };
        trackTypeRow.Widgets.Add(_trackTypeLabel);
        trackTypeRow.Widgets.Add(CreateButton("◀", 45, () => CycleTrackType(-1)));
        trackTypeRow.Widgets.Add(CreateButton("▶", 45, () => CycleTrackType(1)));
        trackTypeRow.Widgets.Add(CreateButton("ZASTOSUJ DO ZAZNACZENIA", 230, ApplyToSelection));
        trackTypeRow.Widgets.Add(CreateButton("ZASTOSUJ DO WYBRANEJ LINII", 235, ApplyToLine));
        content.Widgets.Add(trackTypeRow);

        var lineClassRow = new HorizontalStackPanel { Spacing = 5 };
        _lineClassLabel = new Label { Text = "LineClass: " + _lineClass, Width = 210, Wrap = true };
        lineClassRow.Widgets.Add(_lineClassLabel);
        lineClassRow.Widgets.Add(CreateButton("◀", 45, () => CycleLineClass(-1)));
        lineClassRow.Widgets.Add(CreateButton("▶", 45, () => CycleLineClass(1)));
        lineClassRow.Widgets.Add(CreateButton("ZASTOSUJ DO ZAZNACZENIA", 230, ApplyToSelection));
        lineClassRow.Widgets.Add(CreateButton("ZASTOSUJ DO WYBRANEJ LINII", 235, ApplyToLine));
        content.Widgets.Add(lineClassRow);

        var tractionRow = new HorizontalStackPanel { Spacing = 5 };
        _tractionLabel = new Label { Text = "TractionSystem: " + _traction, Width = 210, Wrap = true };
        tractionRow.Widgets.Add(_tractionLabel);
        tractionRow.Widgets.Add(CreateButton("◀", 45, () => CycleTraction(-1)));
        tractionRow.Widgets.Add(CreateButton("▶", 45, () => CycleTraction(1)));
        tractionRow.Widgets.Add(CreateButton("ZASTOSUJ DO ZAZNACZENIA", 230, ApplyToSelection));
        tractionRow.Widgets.Add(CreateButton("ZASTOSUJ DO WYBRANEJ LINII", 235, ApplyToLine));
        content.Widgets.Add(tractionRow);

        // 4. Operacje członkostwa linii są osobno, żeby nie znikały poza szerokością okna.
        content.Widgets.Add(new Label { Text = "CZŁONKOSTWO W WYBRANEJ LINII", Wrap = true });
        var membershipRow = new HorizontalStackPanel { Spacing = 5 };
        membershipRow.Widgets.Add(CreateButton("DODAJ ZAZNACZENIE DO LINII", 250, AddSelectionToLine));
        membershipRow.Widgets.Add(CreateButton("USUŃ ZAZNACZENIE Z LINII", 250, RemoveSelectionFromLine));
        membershipRow.Widgets.Add(new Label
        {
            Text = "Operacje nie usuwają fizycznego toru.",
            Width = 300,
            Wrap = true
        });
        content.Widgets.Add(membershipRow);

        // 5. Lista linii ma własny scroll, ale cały ekran również jest przewijalny.
        content.Widgets.Add(new Label { Text = "ZAPISANE LINIE", Wrap = true });
        _linesPanel = new VerticalStackPanel { Spacing = 4 };
        content.Widgets.Add(new ScrollViewer { Height = 260, Content = _linesPanel });

        _status = new Label { Wrap = true };
        content.Widgets.Add(_status);
        content.Widgets.Add(CreateButton("POWRÓT", 140, _close));

        root.Widgets.Add(new ScrollViewer { Height = 635, Content = content });
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
        Refresh();
    }

    private void ApplyToLine()
    {
        if (CurrentLine is not RailwayLine line)
        {
            _status.Text = "Najpierw wybierz linię.";
            return;
        }

        int count = _lines.ApplyInfrastructure(line, _trackType, _lineClass, _traction);
        _status.Text = $"Zmieniono parametry {count} segmentów linii „{line.Name}”.";
        Refresh();
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
                               (current == null ? "" : $" • wybrana linia: {current.Name} ({current.TrackPositions.Count} segmentów)");
        _trackTypeLabel.Text = "TrackType: " + _trackType;
        _lineClassLabel.Text = "LineClass: " + _lineClass;
        _tractionLabel.Text = "TractionSystem: " + _traction;

        _linesPanel.Widgets.Clear();

        foreach (var line in _lines.Lines)
        {
            var local = line;
            var row = new HorizontalStackPanel { Spacing = 4 };

            row.Widgets.Add(new Label
            {
                Text = $"{line.Name} • {line.TrackPositions.Count} torów • kolor {line.ColorIndex + 1}",
                Width = 500,
                Wrap = true
            });

            row.Widgets.Add(CreateButton("WYBIERZ", 95, () =>
            {
                _selectedLineId = local.Id;
                _nameBox.Text = local.Name;
                Refresh();
            }));
            row.Widgets.Add(CreateButton("ZAZNACZ", 95, () =>
            {
                _selectedLineId = local.Id;
                SelectCurrentLine();
            }));
            row.Widgets.Add(CreateButton("+ ZAZNACZENIE", 125, () =>
            {
                _selectedLineId = local.Id;
                AddSelectionToLine();
            }));
            row.Widgets.Add(CreateButton("− ZAZNACZENIE", 125, () =>
            {
                _selectedLineId = local.Id;
                RemoveSelectionFromLine();
            }));
            row.Widgets.Add(CreateButton("USUŃ", 75, () =>
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
