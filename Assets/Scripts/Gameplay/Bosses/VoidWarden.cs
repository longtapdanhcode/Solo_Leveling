// Implements: design/narrative/kich_ban_chuan.md sections 4 (Wave 10) and 5.4, production/epics/micro-arena-survivor/story-006-bosses.md.

using System;
using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>
    /// Wave 10 boss. 2000 HP, two phases (see <see cref="VoidWardenBrain"/>). Its death is the run's victory: the wave director
    /// ends Wave 10 when it dies. All numbers in <see cref="Config.BossConfig"/>.
    /// </summary>
    public sealed class VoidWarden : BossBase
    {
        private VoidWardenBrain _wardenBrain;

        /// <summary>Raised once when phase 2 begins (story 007 hooks its effect here).</summary>
        public event Action OnPhaseTwoStarted;

        /// <summary>True once HP has dropped below the phase 2 threshold.</summary>
        public bool IsPhaseTwo => _wardenBrain.IsPhaseTwo;

        /// <inheritdoc />
        protected override float StartingHealth => Settings.WardenMaxHealth;

        /// <inheritdoc />
        protected override float MoveSpeed => Settings.WardenMoveSpeed;

        /// <inheritdoc />
        protected override BossBrain CreateBrain()
        {
            _wardenBrain = new VoidWardenBrain(Settings, () => UnityEngine.Random.value);
            _wardenBrain.OnPhaseTwoStarted += () => OnPhaseTwoStarted?.Invoke();
            return _wardenBrain;
        }
    }
}
