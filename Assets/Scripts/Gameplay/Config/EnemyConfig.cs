// Implements: design/narrative/kich_ban_chuan.md sections 5.1-5.3, design/game-brief.md MVP 3,
// production/epics/micro-arena-survivor/story-002-enemies.md.
//
// One asset per enemy kind. Fields of the other kinds are ignored. Speeds are relative to Kaelen's 7 m/s;
// every value is initial and expected to be rebalanced after playtests.

using UnityEngine;

namespace MicroArena.Gameplay.Config
{
    /// <summary>The regular enemy kinds of story 002.</summary>
    public enum EnemyKind
    {
        /// <summary>Melee bug: runs in, winds up, bites.</summary>
        Crawler,
        /// <summary>Ranged caster: keeps its distance and fires straight bolts.</summary>
        VoidCaster,
        /// <summary>Suicide runner: flashes red near Kaelen, then explodes.</summary>
        VolatileHusk,
    }

    /// <summary>
    /// Tuning data for one enemy kind. Create with the asset menu, or use <see cref="CreateDefault"/> for the
    /// design defaults of a kind.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Micro Arena Survivor/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Common")]
        [SerializeField] private EnemyKind _kind = EnemyKind.Crawler;
        [SerializeField, Min(1f)] private float _maxHealth = 40f;
        [Tooltip("Bite damage (Crawler), bolt damage (Void Caster) or explosion damage (Volatile Husk).")]
        [SerializeField, Min(0f)] private float _damage = 10f;
        // [PLACEHOLDER] Design says medium / slow / very fast only. Crawler 4, Caster 2.5, Husk 9 (Kaelen runs at 7).
        [Tooltip("[PLACEHOLDER] Run speed in metres per second.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 4f;

        [Header("Crawler")]
        [Tooltip("Seconds the Crawler stands still winding up before it bites.")]
        [SerializeField, Min(0f)] private float _lungeWindup = 0.3f;
        // [PLACEHOLDER] Reach of the bite is not specified.
        [Tooltip("[PLACEHOLDER] Distance (m, centre to centre) at which the windup starts and the bite connects.")]
        [SerializeField, Min(0f)] private float _biteRange = 1.5f;
        // [PLACEHOLDER] Pause between bites is not specified.
        [Tooltip("[PLACEHOLDER] Seconds after a bite before the Crawler chases again.")]
        [SerializeField, Min(0f)] private float _biteCooldown = 1f;

        [Header("Void Caster")]
        [Tooltip("Distance (m) the Caster tries to keep from Kaelen.")]
        [SerializeField, Min(0f)] private float _keepDistance = 8f;
        // [PLACEHOLDER] How strictly the Caster holds its distance is not specified.
        [Tooltip("[PLACEHOLDER] The Caster holds still while within this many metres of the keep distance.")]
        [SerializeField, Min(0f)] private float _keepDistanceTolerance = 1f;
        [Tooltip("Seconds between bolts.")]
        [SerializeField, Min(0.01f)] private float _fireInterval = 2.5f;
        // [PLACEHOLDER] Bolt speed is not specified; slower than Kaelen's dash so it can be dodged.
        [Tooltip("[PLACEHOLDER] Bolt speed in metres per second.")]
        [SerializeField, Min(0f)] private float _projectileSpeed = 10f;
        // [PLACEHOLDER] Bolt lifetime is not specified (3 s at 10 m/s = 30 m, about the arena width).
        [Tooltip("[PLACEHOLDER] Seconds before a bolt that hit nothing disappears.")]
        [SerializeField, Min(0f)] private float _projectileLifetime = 3f;
        // [PLACEHOLDER] Bolt hit radius is not specified.
        [Tooltip("[PLACEHOLDER] Flat distance (m) from Kaelen's centre at which a bolt hits.")]
        [SerializeField, Min(0f)] private float _projectileHitRadius = 0.6f;

        [Header("Volatile Husk")]
        [Tooltip("Distance (m) to Kaelen at which the Husk stops and starts flashing.")]
        [SerializeField, Min(0f)] private float _fuseTriggerDistance = 2f;
        [Tooltip("Seconds of red flashing before the explosion.")]
        [SerializeField, Min(0f)] private float _fuseDuration = 0.8f;
        // [PLACEHOLDER] Explosion radius is not specified; larger than the trigger distance so Kaelen must move or dash.
        [Tooltip("[PLACEHOLDER] Explosion radius in metres.")]
        [SerializeField, Min(0f)] private float _explosionRadius = 3f;
        // [PLACEHOLDER] Blink rate of the warning flash is not specified.
        [Tooltip("[PLACEHOLDER] Seconds per on/off step of the red blink.")]
        [SerializeField, Min(0.01f)] private float _blinkInterval = 0.1f;

        /// <summary>Which enemy this config describes.</summary>
        public EnemyKind Kind => _kind;
        /// <summary>Starting and max HP.</summary>
        public float MaxHealth => _maxHealth;
        /// <summary>Damage of the kind's attack (bite, bolt or explosion).</summary>
        public float Damage => _damage;
        /// <summary>Run speed in metres per second.</summary>
        public float MoveSpeed => _moveSpeed;
        /// <summary>Crawler: seconds of windup before a bite.</summary>
        public float LungeWindup => _lungeWindup;
        /// <summary>Crawler: distance at which the windup starts and the bite connects.</summary>
        public float BiteRange => _biteRange;
        /// <summary>Crawler: seconds after a bite before chasing again.</summary>
        public float BiteCooldown => _biteCooldown;
        /// <summary>Void Caster: preferred distance to Kaelen.</summary>
        public float KeepDistance => _keepDistance;
        /// <summary>Void Caster: dead zone around <see cref="KeepDistance"/>.</summary>
        public float KeepDistanceTolerance => _keepDistanceTolerance;
        /// <summary>Void Caster: seconds between bolts.</summary>
        public float FireInterval => _fireInterval;
        /// <summary>Void Caster: bolt speed in metres per second.</summary>
        public float ProjectileSpeed => _projectileSpeed;
        /// <summary>Void Caster: bolt lifetime in seconds.</summary>
        public float ProjectileLifetime => _projectileLifetime;
        /// <summary>Void Caster: bolt hit radius in metres.</summary>
        public float ProjectileHitRadius => _projectileHitRadius;
        /// <summary>Volatile Husk: distance that starts the fuse.</summary>
        public float FuseTriggerDistance => _fuseTriggerDistance;
        /// <summary>Volatile Husk: seconds of flashing before the explosion.</summary>
        public float FuseDuration => _fuseDuration;
        /// <summary>Volatile Husk: explosion radius in metres.</summary>
        public float ExplosionRadius => _explosionRadius;
        /// <summary>Volatile Husk: seconds per blink step.</summary>
        public float BlinkInterval => _blinkInterval;

        /// <summary>Creates a runtime config holding the design defaults of <paramref name="kind"/>.</summary>
        public static EnemyConfig CreateDefault(EnemyKind kind)
        {
            var config = CreateInstance<EnemyConfig>();
            config.name = $"{kind}Config (default)";
            config._kind = kind;
            switch (kind)
            {
                case EnemyKind.VoidCaster:
                    config._maxHealth = 60f;
                    config._damage = 15f;
                    config._moveSpeed = 2.5f; // [PLACEHOLDER] "slow"
                    break;
                case EnemyKind.VolatileHusk:
                    config._maxHealth = 25f;
                    config._damage = 50f;
                    config._moveSpeed = 9f; // [PLACEHOLDER] "very fast"
                    break;
            }
            return config;
        }
    }
}
