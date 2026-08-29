// ============================================================
// SIGNAL.CS - KLASA SEMAFORA
// ============================================================

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway
{
    /// <summary>
    /// Semafor kolejowy - komunikacja z pociągami
    /// </summary>
    public class Signal
    {
        // IDENTYFIKACJA
        public Guid Id { get; }
        public string Name { get; set; }
        public MapPosition Position { get; }

        // KONFIGURACJA
        public TrackConnections Direction { get; }
        public SignalAspect Aspect { get; private set; }
        public List<SignalAspect> AvailableAspects { get; }
        public bool IsLocked { get; set; }

        // KOMUNIKACJA Z POCIĄGIEM
        public Train.Train? CurrentTrain { get; private set; }

        // ZDARZENIA
        public event EventHandler<SignalEventArgs>? AspectChanged;
        public event EventHandler<SignalEventArgs>? SignalLocked;
        public event EventHandler<SignalEventArgs>? SignalUnlocked;
        public event EventHandler<SignalEventArgs>? TrainApproached;
        public event EventHandler<SignalEventArgs>? TrainPassed;

        public class SignalEventArgs : EventArgs
        {
            public Signal Signal { get; }
            public SignalAspect OldAspect { get; }
            public SignalAspect NewAspect { get; }
            public Train.Train? Train { get; }

            public SignalEventArgs(Signal signal, SignalAspect oldAspect, SignalAspect newAspect, Train.Train? train = null)
            {
                Signal = signal;
                OldAspect = oldAspect;
                NewAspect = newAspect;
                Train = train;
            }
        }

        // KONSTRUKTOR
        public Signal(MapPosition position, TrackConnections direction, List<SignalAspect>? availableAspects = null)
        {
            Id = Guid.NewGuid();
            Position = position;
            Direction = direction;
            AvailableAspects = availableAspects ?? new List<SignalAspect>
            {
                SignalAspect.Stop,
                SignalAspect.StopStation,
                SignalAspect.Clear,
                SignalAspect.Warning,
                SignalAspect.Speed100,
                SignalAspect.Speed40,
                SignalAspect.Reserve1,
                SignalAspect.Reserve2,
                SignalAspect.Reserve3,
                SignalAspect.Reserve4
            };
            Aspect = SignalAspect.Stop;
            IsLocked = false;
            CurrentTrain = null;
        }

        // METODY
        public bool SetAspect(SignalAspect newAspect, Train.Train? train = null)
        {
            if (IsLocked)
                return false;

            if (!AvailableAspects.Contains(newAspect))
                return false;

            var oldAspect = Aspect;
            Aspect = newAspect;

            AspectChanged?.Invoke(this, new SignalEventArgs(this, oldAspect, newAspect, train));
            return true;
        }

        public bool CanTrainPass(Train.Train train)
        {
            if (IsLocked && CurrentTrain != train)
                return false;

            return Aspect switch
            {
                SignalAspect.Stop => false,
                SignalAspect.StopStation => false,
                _ => true
            };
        }

        public void Lock(Train.Train? train = null)
        {
            IsLocked = true;
            CurrentTrain = train;
            SignalLocked?.Invoke(this, new SignalEventArgs(this, Aspect, Aspect, train));
        }

        public void Unlock(Train.Train? train = null)
        {
            IsLocked = false;
            CurrentTrain = null;
            SignalUnlocked?.Invoke(this, new SignalEventArgs(this, Aspect, Aspect, train));
        }

        public void NotifyTrainApproach(Train.Train train)
        {
            CurrentTrain = train;
            TrainApproached?.Invoke(this, new SignalEventArgs(this, Aspect, Aspect, train));
        }

        public void NotifyTrainPassed(Train.Train train)
        {
            if (CurrentTrain == train)
            {
                CurrentTrain = null;
                TrainPassed?.Invoke(this, new SignalEventArgs(this, Aspect, Aspect, train));
            }
        }

        public string GetAspectName() => Aspect.GetName();
        public string GetAspectDescription() => Aspect.GetDescription();
        public float GetSpeedLimitKmh() => Aspect.GetSpeedLimit();

        public override string ToString()
        {
            return $"[Signal {Id.ToString()[..8]}] {GetAspectName()} at ({Position.X},{Position.Y})";
        }
    }
}