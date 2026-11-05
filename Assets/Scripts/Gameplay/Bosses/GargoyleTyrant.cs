// Implements: design/narrative/kich_ban_chuan.md section 4 (Wave 5), production/epics/micro-arena-survivor/story-006-bosses.md.

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>Wave 5 boss. 600 HP, hammer slam with a 1 s red circle (see <see cref="GargoyleBrain"/>). All numbers in <see cref="Config.BossConfig"/>.</summary>
    public sealed class GargoyleTyrant : BossBase
    {
        /// <inheritdoc />
        protected override float StartingHealth => Settings.GargoyleMaxHealth;

        /// <inheritdoc />
        protected override float MoveSpeed => Settings.GargoyleMoveSpeed;

        /// <inheritdoc />
        protected override BossBrain CreateBrain()
        {
            return new GargoyleBrain(Settings);
        }
    }
}
