// Implements: design/game-brief.md MVP 6 ("moi don nguy hiem deu co telegraph tren san"),
// production/epics/micro-arena-survivor/story-006-bosses.md.

using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>
    /// Flat (XZ) red area of a boss attack: a circle or a fan. The SAME numbers drive the drawing (<see cref="TelegraphView"/>)
    /// and the hit test (<see cref="Contains"/>), so what the player sees is exactly what hurts.
    /// </summary>
    public readonly struct TelegraphShape
    {
        /// <summary>Centre of the circle / apex of the fan (world position; Y ignored).</summary>
        public readonly Vector3 Origin;
        /// <summary>Flat unit direction the fan points to (a circle ignores it).</summary>
        public readonly Vector3 Direction;
        /// <summary>Radius of the circle / length of the fan, metres.</summary>
        public readonly float Radius;
        /// <summary>Half the opening angle in degrees; 180 means a full circle.</summary>
        public readonly float HalfAngleDegrees;

        private TelegraphShape(Vector3 origin, Vector3 direction, float radius, float halfAngleDegrees)
        {
            Origin = origin;
            Direction = direction;
            Radius = radius;
            HalfAngleDegrees = halfAngleDegrees;
        }

        /// <summary>True for a full circle.</summary>
        public bool IsCircle => HalfAngleDegrees >= 180f;

        /// <summary>A red circle.</summary>
        public static TelegraphShape Circle(Vector3 origin, float radius)
        {
            return new TelegraphShape(origin, Vector3.forward, radius, 180f);
        }

        /// <summary>A red fan (cone) opening <paramref name="angleDegrees"/> around <paramref name="direction"/>.</summary>
        public static TelegraphShape Fan(Vector3 origin, Vector3 direction, float range, float angleDegrees)
        {
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            return new TelegraphShape(origin, direction, range, Mathf.Clamp(angleDegrees * 0.5f, 0f, 180f));
        }

        /// <summary>True when <paramref name="point"/> (flat) is inside the area. The edge counts as inside.</summary>
        public bool Contains(Vector3 point)
        {
            Vector3 offset = point - Origin;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > Radius)
            {
                return false;
            }
            if (IsCircle || distance <= 0f)
            {
                return true;
            }
            return Vector3.Angle(Direction, offset) <= HalfAngleDegrees;
        }
    }

    /// <summary>
    /// One pending boss hit: a red area that waits <see cref="Duration"/> seconds, then resolves. It never deals damage
    /// by itself; the owner tests the player against <see cref="Shape"/> at the moment <see cref="Tick"/> returns true.
    /// <para>Transitions: Pending --Tick, elapsed reaches Duration--> Resolved (once). Pending/Resolved --Cancel--> Resolved without a hit.</para>
    /// </summary>
    public sealed class Telegraph
    {
        private float _elapsed;

        /// <summary>The red area (fixed when the telegraph was placed).</summary>
        public TelegraphShape Shape { get; }
        /// <summary>Damage dealt to Kaelen inside the area at resolve time. 0 = a warning only (the pull).</summary>
        public float Damage { get; }
        /// <summary>Seconds from placing to resolving.</summary>
        public float Duration { get; }
        /// <summary>True once resolved or cancelled.</summary>
        public bool IsResolved { get; private set; }
        /// <summary>0..1 share of <see cref="Duration"/> that has passed.</summary>
        public float Progress => Duration > 0f ? Mathf.Clamp01(_elapsed / Duration) : 1f;

        /// <summary>Places a telegraph. <paramref name="duration"/> is used as given (the brain applies the minimum).</summary>
        public Telegraph(TelegraphShape shape, float damage, float duration)
        {
            Shape = shape;
            Damage = damage;
            Duration = duration;
        }

        /// <summary>Advances the timer.</summary>
        /// <returns>True on the one tick that resolves it.</returns>
        public bool Tick(float deltaTime)
        {
            if (IsResolved)
            {
                return false;
            }
            _elapsed += deltaTime;
            if (_elapsed >= Duration)
            {
                IsResolved = true;
                return true;
            }
            return false;
        }

        /// <summary>Drops the telegraph without a hit.</summary>
        public void Cancel()
        {
            IsResolved = true;
        }
    }
}
