// Implements: design/game-brief.md MVP 6, production/epics/micro-arena-survivor/story-006-bosses.md.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>Everything a boss brain needs to know about the world for one tick. Filled by <see cref="BossBase"/>.</summary>
    public struct BossFrame
    {
        /// <summary>The boss's world position.</summary>
        public Vector3 Position;
        /// <summary>Flat unit direction to what the boss chases and aims at: Kaelen, or the decoy while it exists.</summary>
        public Vector3 DirectionToTarget;
        /// <summary>Flat distance to what the boss chases and aims at (Kaelen or the decoy).</summary>
        public float DistanceToTarget;
        /// <summary>Kaelen's world position. Telegraph hits are tested against this, never against the decoy.</summary>
        public Vector3 PlayerPosition;
        /// <summary>False once Kaelen is down: telegraphs that resolve then deal no damage.</summary>
        public bool PlayerAlive;
        /// <summary>Current HP / max HP.</summary>
        public float HealthFraction;
    }

    /// <summary>
    /// Plain-C# attack logic shared by both bosses: owns the pending <see cref="Telegraph"/>s, resolves them (damage only to
    /// Kaelen inside the area, tested at resolve time) and applies the minimum telegraph time. Subclasses add the
    /// attack sequencing in <see cref="Advance"/>. Time only moves through <see cref="Tick"/>, so the pause (delta 0)
    /// freezes everything, and tests drive it with fixed steps.
    /// </summary>
    public abstract class BossBrain
    {
        private readonly List<Telegraph> _telegraphs = new List<Telegraph>();
        private bool _stopped;

        /// <summary>Creates the brain.</summary>
        protected BossBrain(BossConfig config)
        {
            Config = config;
        }

        /// <summary>Raised when a telegraph is placed (the view draws it).</summary>
        public event Action<Telegraph> OnTelegraphPlaced;
        /// <summary>Raised when a telegraph resolved or was cleared (the view removes it).</summary>
        public event Action<Telegraph> OnTelegraphRemoved;

        /// <summary>Pending telegraphs.</summary>
        public IReadOnlyList<Telegraph> Telegraphs => _telegraphs;
        /// <summary>True while the boss should chase its target this tick (false while it stands still for an attack).</summary>
        public bool WantsToMove { get; protected set; }
        /// <summary>Displacement to add to Kaelen this tick (the Void Warden's pull). Zero otherwise.</summary>
        public Vector3 PullStep { get; protected set; }
        /// <summary>True while an attack sequence is running (telegraphs placed or about to be).</summary>
        public abstract bool IsAttacking { get; }
        /// <summary>Seconds left before the next attack may start.</summary>
        public float CooldownRemaining { get; protected set; }

        /// <summary>Tuning values.</summary>
        protected BossConfig Config { get; }

        /// <summary>Advances all telegraphs, resolves the ones that finished, then runs the boss's own sequencing.</summary>
        /// <param name="hitPlayer">Called with the damage when a resolving telegraph contains Kaelen.</param>
        public void Tick(float deltaTime, in BossFrame frame, Action<float> hitPlayer)
        {
            PullStep = Vector3.zero;
            if (_stopped)
            {
                return;
            }
            for (int i = _telegraphs.Count - 1; i >= 0; i--)
            {
                Telegraph telegraph = _telegraphs[i];
                if (!telegraph.Tick(deltaTime))
                {
                    continue;
                }
                _telegraphs.RemoveAt(i);
                OnTelegraphRemoved?.Invoke(telegraph);
                if (telegraph.Damage > 0f && frame.PlayerAlive && telegraph.Shape.Contains(frame.PlayerPosition))
                {
                    hitPlayer(telegraph.Damage);
                }
            }
            Advance(deltaTime, frame);
        }

        /// <summary>Drops every pending telegraph (no damage) and returns to the idle state.</summary>
        public void Clear()
        {
            foreach (Telegraph telegraph in _telegraphs)
            {
                telegraph.Cancel();
                OnTelegraphRemoved?.Invoke(telegraph);
            }
            _telegraphs.Clear();
            PullStep = Vector3.zero;
            WantsToMove = false;
            ResetSequence();
        }

        /// <summary>Clears everything and makes every later <see cref="Tick"/> a no-op (boss died or was removed).</summary>
        public void Stop()
        {
            Clear();
            _stopped = true;
        }

        /// <summary>Boss-specific sequencing, called after the telegraphs were resolved.</summary>
        protected abstract void Advance(float deltaTime, in BossFrame frame);

        /// <summary>Back to the idle state (called by <see cref="Clear"/>; telegraphs are already gone).</summary>
        protected abstract void ResetSequence();

        /// <summary>Places a telegraph; the time is raised to <see cref="BossConfig.MinTelegraphSeconds"/> when shorter.</summary>
        protected Telegraph Place(TelegraphShape shape, float damage, float seconds)
        {
            var telegraph = new Telegraph(shape, damage, Config.EffectiveTelegraph(seconds));
            _telegraphs.Add(telegraph);
            OnTelegraphPlaced?.Invoke(telegraph);
            return telegraph;
        }
    }
}
