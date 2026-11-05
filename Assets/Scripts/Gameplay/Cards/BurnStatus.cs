// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (Vết Chém Hỏa Ngục: 10 DMG/s for 3 s,
// stacks up to 3), design/narrative/kich_ban_chuan.md section 6.2.

namespace MicroArena.Gameplay.Cards
{
    /// <summary>
    /// Plain-C# burn on one enemy.
    /// <code>
    /// Transition table
    ///   Idle    --AddStack-----------------------> Burning (stacks = 1, remaining = duration)
    ///   Burning --AddStack-----------------------> Burning (stacks = min(stacks + 1, max), remaining = duration)
    ///   Burning --Tick, remaining reaches 0------> Idle    (stacks = 0)
    /// </code>
    /// </summary>
    public sealed class BurnStatus
    {
        /// <summary>Current stacks.</summary>
        public int Stacks { get; private set; }
        /// <summary>Seconds left.</summary>
        public float Remaining { get; private set; }
        /// <summary>True while burning.</summary>
        public bool IsActive => Stacks > 0 && Remaining > 0f;

        /// <summary>Adds one stack (capped at <paramref name="maxStacks"/>) and restarts the timer.</summary>
        public void AddStack(int maxStacks, float duration)
        {
            if (Stacks < maxStacks)
            {
                Stacks++;
            }
            Remaining = duration;
        }

        /// <summary>Advances the burn.</summary>
        /// <returns>Damage dealt during this step: stacks * DMG/s * burning time (never past the timer).</returns>
        public float Tick(float deltaTime, float damagePerSecondPerStack)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return 0f;
            }
            float step = deltaTime < Remaining ? deltaTime : Remaining;
            Remaining -= step;
            float damage = Stacks * damagePerSecondPerStack * step;
            if (Remaining <= 0f)
            {
                Remaining = 0f;
                Stacks = 0;
            }
            return damage;
        }
    }
}
