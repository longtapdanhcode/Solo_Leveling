// Implements: design/narrative/kich_ban_chuan.md sections 4 (Wave 10) and 5.4, production/epics/micro-arena-survivor/story-006-bosses.md.

using System;
using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>
    /// Void Warden. Phase 1 alternates two attacks (alternation is a [PLACEHOLDER]): the triple slash (three red fans one
    /// after the other, each re-aimed at the target when it appears) and the pull + ground burst (a wind-up circle, then
    /// Kaelen is pulled toward the centre, then a red circle at the centre bursts). Phase 2 starts ONCE when HP drops strictly
    /// below <c>PhaseTwoThreshold</c> and adds the meteor rain, on its own timer, running beside the attack sequence.
    /// <code>
    /// Transition table (attack sequence)
    ///   Chasing    --cooldown over, next = Slash, target within SlashRange--> Slashing   (fan placed)
    ///   Chasing    --cooldown over, next = Pull, target within PullRadius---> PullWindup (pull-area circle placed, centre = boss position)
    ///   Slashing   --fan resolved, slashes left----------------------------> SlashGap
    ///   Slashing   --fan resolved, last slash------------------------------> Chasing    (cooldown = AttackCooldown)
    ///   SlashGap   --gap over----------------------------------------------> Slashing   (next fan placed)
    ///   PullWindup --circle resolved (no damage)---------------------------> Pulling
    ///   Pulling    --pull duration over (burst circle placed at the centre)--> Bursting
    ///   Bursting   --circle resolved---------------------------------------> Chasing    (cooldown = AttackCooldown)
    /// Phase (latch): Phase 1 --HealthFraction &lt; threshold--> Phase 2 (never back).
    /// Meteor rain: in Phase 2 only, every MeteorIntervalSeconds, MeteorCount circles at random points.
    /// </code>
    /// A telegraph already placed never moves: the target (decoy or Kaelen) only decides where the NEXT one is aimed.
    /// </summary>
    public sealed class VoidWardenBrain : BossBrain
    {
        private enum State
        {
            Chasing,
            Slashing,
            SlashGap,
            PullWindup,
            Pulling,
            Bursting,
        }

        private readonly Func<float> _random01;
        private State _state = State.Chasing;
        private bool _nextIsPull;
        private Telegraph _current;
        private int _slashesDone;
        private float _gapRemaining;
        private float _pullRemaining;
        private Vector3 _centre;
        private float _meteorTimer;

        /// <summary>Creates the brain. <paramref name="random01"/> returns a value in [0, 1) (meteor points).</summary>
        public VoidWardenBrain(BossConfig config, Func<float> random01) : base(config)
        {
            _random01 = random01;
        }

        /// <summary>Raised once, when phase 2 begins.</summary>
        public event Action OnPhaseTwoStarted;

        /// <summary>True from the moment HP drops below the threshold. Latched.</summary>
        public bool IsPhaseTwo { get; private set; }
        /// <inheritdoc />
        public override bool IsAttacking => _state != State.Chasing;
        /// <summary>True when the next attack of the sequence is the pull + burst (else the triple slash).</summary>
        public bool NextIsPull => _nextIsPull;
        /// <summary>The fixed centre of the running pull + burst.</summary>
        public Vector3 PullCentre => _centre;

        /// <summary>
        /// One step of the pull: moves <paramref name="player"/> toward <paramref name="centre"/> by at most
        /// <paramref name="maxStep"/> metres, never past the centre, and not at all beyond <paramref name="radius"/>.
        /// Added on top of Kaelen's own movement, so a dash (far faster) always outpaces it.
        /// </summary>
        public static Vector3 ComputePullStep(Vector3 player, Vector3 centre, float maxStep, float radius)
        {
            Vector3 toCentre = centre - player;
            toCentre.y = 0f;
            float distance = toCentre.magnitude;
            if (distance <= 0f || distance > radius)
            {
                return Vector3.zero;
            }
            return toCentre / distance * Mathf.Min(maxStep, distance);
        }

        /// <inheritdoc />
        protected override void Advance(float deltaTime, in BossFrame frame)
        {
            if (!IsPhaseTwo && frame.HealthFraction < Config.PhaseTwoThreshold)
            {
                IsPhaseTwo = true;
                _meteorTimer = Config.MeteorIntervalSeconds;
                OnPhaseTwoStarted?.Invoke();
            }
            if (IsPhaseTwo)
            {
                _meteorTimer -= deltaTime;
                if (_meteorTimer <= 0f)
                {
                    _meteorTimer += Config.MeteorIntervalSeconds;
                    RainMeteors();
                }
            }

            switch (_state)
            {
                case State.Chasing:
                    CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
                    WantsToMove = frame.DistanceToTarget > Config.StopDistance;
                    if (CooldownRemaining <= 0f)
                    {
                        TryStartAttack(frame);
                    }
                    break;

                case State.Slashing:
                    WantsToMove = false;
                    if (_current.IsResolved)
                    {
                        _slashesDone++;
                        if (_slashesDone >= Config.SlashCount)
                        {
                            EndAttack();
                        }
                        else
                        {
                            _state = State.SlashGap;
                            _gapRemaining = Config.SlashGapSeconds;
                        }
                    }
                    break;

                case State.SlashGap:
                    WantsToMove = false;
                    _gapRemaining -= deltaTime;
                    if (_gapRemaining <= 0f)
                    {
                        PlaceSlash(frame);
                    }
                    break;

                case State.PullWindup:
                    WantsToMove = false;
                    if (_current.IsResolved)
                    {
                        _state = State.Pulling;
                        _pullRemaining = Config.PullDurationSeconds;
                    }
                    break;

                case State.Pulling:
                    WantsToMove = false;
                    float used = Mathf.Min(deltaTime, _pullRemaining);
                    if (frame.PlayerAlive)
                    {
                        PullStep = ComputePullStep(frame.PlayerPosition, _centre, Config.PullSpeed * used, Config.PullRadius);
                    }
                    _pullRemaining -= used;
                    if (_pullRemaining <= 0f)
                    {
                        _current = Place(TelegraphShape.Circle(_centre, Config.BurstRadius), Config.BurstDamage, Config.BurstTelegraphSeconds);
                        _state = State.Bursting;
                    }
                    break;

                case State.Bursting:
                    WantsToMove = false;
                    if (_current.IsResolved)
                    {
                        EndAttack();
                    }
                    break;
            }
        }

        /// <inheritdoc />
        protected override void ResetSequence()
        {
            _state = State.Chasing;
            _current = null;
            _slashesDone = 0;
            _pullRemaining = 0f;
        }

        private void TryStartAttack(in BossFrame frame)
        {
            if (_nextIsPull)
            {
                if (frame.DistanceToTarget > Config.PullRadius)
                {
                    return;
                }
                _centre = frame.Position;
                // Warning only (damage 0): shows the area the pull reaches before it starts.
                _current = Place(TelegraphShape.Circle(_centre, Config.PullRadius), 0f, Config.PullWindupSeconds);
                _state = State.PullWindup;
            }
            else
            {
                if (frame.DistanceToTarget > Config.SlashRange)
                {
                    return;
                }
                _slashesDone = 0;
                PlaceSlash(frame);
            }
            WantsToMove = false;
        }

        private void PlaceSlash(in BossFrame frame)
        {
            _current = Place(
                TelegraphShape.Fan(frame.Position, frame.DirectionToTarget, Config.SlashRange, Config.SlashAngleDegrees),
                Config.SlashDamage, Config.SlashTelegraphSeconds);
            _state = State.Slashing;
        }

        private void EndAttack()
        {
            _state = State.Chasing;
            CooldownRemaining = Config.AttackCooldown;
            _nextIsPull = !_nextIsPull;
        }

        private void RainMeteors()
        {
            float extent = Config.MeteorAreaHalfExtent;
            for (int i = 0; i < Config.MeteorCount; i++)
            {
                var point = new Vector3((_random01() * 2f - 1f) * extent, 0f, (_random01() * 2f - 1f) * extent);
                Place(TelegraphShape.Circle(point, Config.MeteorRadius), Config.MeteorDamage, Config.MeteorTelegraphSeconds);
            }
        }
    }
}
