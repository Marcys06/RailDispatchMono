using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>F8 editor for infrastructure metadata of the track under the cursor.</summary>
internal sealed class MyraTrackInfrastructureView
{
    public Widget Root { get; }
    private readonly GameMap _map;
    private readonly TrackBuilder _builder;
    private readonly MapPosition _position;
    private readonly Action _close;
    private readonly VerticalStackPanel _content;

    public MyraTrackInfrastructureView(GameMap map, TrackBuilder builder, MapPosition position, Action close)
    {
        _map = map;
        _builder = builder;
        _position = position;
        _close = close;
        var root = new VerticalStackPanel { Width = 760, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Spacing = 7 };
        root.Widgets.Add(new Label { Text = "INFRASTRUKTURA TORU — F8", Wrap = true });
        root.Widgets.Add(new Label { Text = "Zmiana metadanych nie zmienia geometrii toru, połączeń, bloków ani sygnałów.", Wrap = true });
        _content = new VerticalStackPanel { Spacing = 5 };
        root.Widgets.Add(_content);
        var closeButton = new Button { Content = new Label { Text = "POWRÓT" }, Width = 140 };
        closeButton.Click += (_, _) => _close();
        root.Widgets.Add(closeButton);
        Root = root;
        Refresh();
    }

    private void Refresh()
    {
        _content.Widgets.Clear();
        if (!_map.TryGetTrack(_position, out var track) || track == null)
        {
            _content.Widgets.Add(new Label { Text = $"Brak toru na pozycji ({_position.X}, {_position.Y}).", Wrap = true });
            return;
        }

        _content.Widgets.Add(new Label
        {
            Text = $"Pozycja: ({_position.X}, {_position.Y}) • geometria={track.Geometry} • stan={track.ConditionPercent:0}%",
            Wrap = true
        });
        _content.Widgets.Add(CreateCycleRow("RODZAJ TORU", track.Type, Enum.GetValues<TrackType>(), value => _builder.SetTrackType(_position, value)));
        _content.Widgets.Add(CreateCycleRow("KLASA LINII", track.LineClass, Enum.GetValues<LineClass>(), value => _builder.SetLineClass(_position, value)));
        _content.Widgets.Add(CreateCycleRow("TRAKCJA", track.Traction, Enum.GetValues<TractionSystem>(), value => _builder.SetTraction(_position, value)));
        _content.Widgets.Add(new Label
        {
            Text = $"Parametry przygotowane: Vmax infrastruktury {track.InfrastructureMaxSpeedKmh:0} km/h • nacisk osi {track.InfrastructureMaxAxleLoadTons:0.0} t • zużycie {track.WearPercent:0}%",
            Wrap = true
        });
    }

    private HorizontalStackPanel CreateCycleRow<T>(string caption, T current, T[] values, Func<T, bool> apply)
        where T : struct, Enum
    {
        int index = Array.IndexOf(values, current);
        var row = new HorizontalStackPanel { Spacing = 6 };
        row.Widgets.Add(new Label { Text = $"{caption}: {current}", Width = 320, Wrap = true });
        var previous = new Button { Content = new Label { Text = "◀" }, Width = 55 };
        previous.Click += (_, _) =>
        {
            T value = values[(index - 1 + values.Length) % values.Length];
            if (apply(value)) Refresh();
        };
        var next = new Button { Content = new Label { Text = "▶" }, Width = 55 };
        next.Click += (_, _) =>
        {
            T value = values[(index + 1) % values.Length];
            if (apply(value)) Refresh();
        };
        row.Widgets.Add(previous);
        row.Widgets.Add(next);
        return row;
    }
}
