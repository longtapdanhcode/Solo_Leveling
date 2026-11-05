// TEMPORARY batchmode rule check for story 006 (bosses: BossConfig values, telegraphs, Gargoyle, Void Warden, wave rules).
// Kept in tools/boss-check/ (outside Assets/, never compiled); see the README there to copy it in, run it, remove it.
//   Unity -batchmode -quit -projectPath <project> -executeMethod MicroArena.Gameplay.Editor.BossCheck.Run
// Needs the assets made by ArenaSceneBuilder.Build (WaveConfig.asset, BossConfig.asset, boss prefabs).

using System;
using System.Collections.Generic;
using System.Reflection;
using MicroArena.Gameplay.Arena;
using MicroArena.Gameplay.Bosses;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MicroArena.Gameplay.Editor
{
    /// <summary>Batchmode rule check for story 006. Prints PASS/FAIL lines and exits 1 on any failure.</summary>
    public static class BossCheck
    {
        private const float Step = 0.02f; // fixed tick, seconds
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static int _failures;
        private static readonly List<Object> Created = new List<Object>();

        /// <summary>Runs every check, prints PASS/FAIL lines, exits 1 on any failure.</summary>
        public static void Run()
        {
            _failures = 0;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            try
            {
                CheckConfigDefaults();
                CheckShapes();
                CheckGargoyle();
                CheckVoidWardenSlashAndDecoy();
                CheckPull();
                CheckPhaseTwoAndMeteors();
                CheckPause();
                CheckBossComponents();
                CheckWaves();
            }
            catch (Exception e)
            {
                Fail($"exception: {e}");
            }
            GamePause.Reset();
            foreach (Object o in Created)
            {
                if (o != null)
                {
                    Object.DestroyImmediate(o);
                }
            }
            Debug.Log(_failures == 0 ? "BossCheck: ALL PASS" : $"BossCheck: {_failures} FAILURE(S)");
            EditorApplication.Exit(_failures == 0 ? 0 : 1);
        }

        private static void CheckConfigDefaults()
        {
            var c = Track(ScriptableObject.CreateInstance<BossConfig>());
            Near("config: MinTelegraphSeconds == 1", c.MinTelegraphSeconds, 1f);
            Near("config: AttackCooldown == 3", c.AttackCooldown, 3f);
            Near("config: Gargoyle HP == 600", c.GargoyleMaxHealth, 600f);
            Near("config: SlamDamage == 25", c.SlamDamage, 25f);
            Near("config: SlamRadius == 4", c.SlamRadius, 4f);
            Near("config: SlamTelegraphSeconds == 1", c.SlamTelegraphSeconds, 1f);
            Near("config: Void Warden HP == 2000", c.WardenMaxHealth, 2000f);
            Near("config: PhaseTwoThreshold == 0.5", c.PhaseTwoThreshold, 0.5f);
            Near("config: SlashDamage == 30", c.SlashDamage, 30f);
            Check("config: SlashCount == 3", c.SlashCount == 3);
            Near("config: BurstDamage == 40", c.BurstDamage, 40f);
            Near("config: BurstRadius == 4", c.BurstRadius, 4f);
            Near("config: MeteorDamage == 20", c.MeteorDamage, 20f);
            Check("config: MeteorCount == 5", c.MeteorCount == 5);
            Near("config: MeteorIntervalSeconds == 8", c.MeteorIntervalSeconds, 8f);
            Near("config: MeteorRadius == 2", c.MeteorRadius, 2f);
            Check("config: every attack telegraph >= 1 s",
                c.EffectiveTelegraph(c.SlamTelegraphSeconds) >= 1f && c.EffectiveTelegraph(c.SlashTelegraphSeconds) >= 1f
                && c.EffectiveTelegraph(c.PullWindupSeconds) >= 1f && c.EffectiveTelegraph(c.BurstTelegraphSeconds) >= 1f
                && c.EffectiveTelegraph(c.MeteorTelegraphSeconds) >= 1f);

            var asset = AssetDatabase.LoadAssetAtPath<BossConfig>("Assets/Data/BossConfig.asset");
            Check("asset: Assets/Data/BossConfig.asset exists with 600 / 2000 HP, 25 / 30 / 40 / 20 DMG",
                asset != null && asset.GargoyleMaxHealth == 600f && asset.WardenMaxHealth == 2000f && asset.SlamDamage == 25f
                && asset.SlashDamage == 30f && asset.BurstDamage == 40f && asset.MeteorDamage == 20f);

            // The minimum is enforced in code: a designer value below 1 s is raised to 1 s.
            BossConfig shortWarning = Config(("_slamTelegraphSeconds", 0.3f));
            var brain = new GargoyleBrain(shortWarning);
            brain.Tick(Step, Frame(Vector3.zero, Vector3.forward, 2f, Vector3.forward * 2f), _ => { });
            Check("telegraph: a 0.3 s slam warning is raised to MinTelegraphSeconds (1 s)",
                brain.Telegraphs.Count == 1 && Mathf.Abs(brain.Telegraphs[0].Duration - 1f) < 1e-4f);
        }

        private static void CheckShapes()
        {
            TelegraphShape circle = TelegraphShape.Circle(new Vector3(10f, 0f, 10f), 4f);
            Check("circle: point 3.9 m from the centre is inside", circle.Contains(new Vector3(13.9f, 1f, 10f)));
            Check("circle: point exactly on the 4 m edge is inside", circle.Contains(new Vector3(10f, 1f, 14f)));
            Check("circle: point 4.1 m away is outside", !circle.Contains(new Vector3(14.1f, 1f, 10f)));
            Check("circle: height is ignored (flat test)", circle.Contains(new Vector3(10f, 50f, 10f)));

            TelegraphShape fan = TelegraphShape.Fan(Vector3.zero, Vector3.forward, 8f, 90f);
            Check("fan: straight ahead at 5 m is inside", fan.Contains(new Vector3(0f, 0f, 5f)));
            Check("fan: 40 degrees off-axis at 4.6 m is inside", fan.Contains(new Vector3(3f, 0f, 3.5f)));
            Check("fan: 79 degrees off-axis is outside (half angle 45)", !fan.Contains(new Vector3(5f, 0f, 1f)));
            Check("fan: behind the boss is outside", !fan.Contains(new Vector3(0f, 0f, -2f)));
            Check("fan: beyond the 8 m length is outside", !fan.Contains(new Vector3(0f, 0f, 8.1f)));

            // Drawing uses the same numbers: every rim vertex of the mesh sits on the shape's edge.
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            var material = Track(new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.red });
            TelegraphShape aimed = TelegraphShape.Fan(new Vector3(2f, 0f, -3f), new Vector3(1f, 0f, 1f), 8f, 90f);
            var view = new TelegraphView(new Telegraph(aimed, 30f, 1f), config, material);
            GameObject drawn = GameObject.Find("BossTelegraph");
            bool rimOnEdge = drawn != null;
            if (drawn != null)
            {
                Vector3[] vertices = drawn.GetComponent<MeshFilter>().sharedMesh.vertices;
                float widest = 0f;
                for (int i = 1; i < vertices.Length; i++)
                {
                    Vector3 offset = drawn.transform.TransformPoint(vertices[i]) - aimed.Origin;
                    offset.y = 0f;
                    float angle = Vector3.Angle(aimed.Direction, offset);
                    widest = Mathf.Max(widest, angle);
                    rimOnEdge &= Mathf.Abs(offset.magnitude - aimed.Radius) < 1e-3f && angle <= aimed.HalfAngleDegrees + 1e-2f;
                }
                rimOnEdge &= Mathf.Abs(widest - aimed.HalfAngleDegrees) < 1e-2f;
            }
            Check("view: fan mesh rim matches the hit-test shape (same origin, direction, length, angle)", rimOnEdge);
            view.Dispose();
            Check("view: Dispose removes the drawing", GameObject.Find("BossTelegraph") == null);
        }

        private static void CheckGargoyle()
        {
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            var brain = new GargoyleBrain(config);
            var hits = new List<float>();
            Vector3 player = new Vector3(0f, 1f, 2f);
            BossFrame frame = Frame(Vector3.zero, Vector3.forward, 2f, player);
            brain.Tick(Step, frame, hits.Add);
            Check("gargoyle: target at 2 m starts the slam (one red circle, boss stops)",
                brain.Telegraphs.Count == 1 && brain.IsAttacking && !brain.WantsToMove);
            Telegraph slam = brain.Telegraphs[0];
            Check("gargoyle: slam circle is centred on the boss, radius 4, 25 DMG",
                slam.Shape.IsCircle && slam.Shape.Radius == 4f && slam.Shape.Origin == Vector3.zero && slam.Damage == 25f);
            float t = Advance(brain, frame, 0.96f, hits);
            Check($"gargoyle: no damage before the telegraph resolves (after {t:0.00} s)", hits.Count == 0 && !slam.IsResolved);
            while (!slam.IsResolved && t < 3f)
            {
                brain.Tick(Step, frame, hits.Add);
                t += Step;
            }
            Check($"gargoyle: slam lands after 1 s (got {t:0.00} s) for exactly 25 to Kaelen inside",
                Mathf.Abs(t - 1f) <= Step + 1e-4f && hits.Count == 1 && hits[0] == 25f);
            Check("gargoyle: telegraph removed after resolving", brain.Telegraphs.Count == 0);

            // 3 s cooldown between attacks, measured from the slam landing to the next circle.
            float wait = 0f;
            while (brain.Telegraphs.Count == 0 && wait < 10f)
            {
                brain.Tick(Step, frame, hits.Add);
                wait += Step;
            }
            Check($"gargoyle: next slam starts 3 s after the last one landed (got {wait:0.00} s)", Mathf.Abs(wait - 3f) <= Step + 1e-4f);

            // Membership is tested at resolve time: Kaelen leaves the circle before it lands.
            hits.Clear();
            BossFrame escaped = Frame(Vector3.zero, Vector3.forward, 2f, new Vector3(0f, 1f, 4.5f));
            Advance(brain, escaped, 1.1f, hits);
            Check("gargoyle: Kaelen outside the circle at resolve time takes no damage", hits.Count == 0 && brain.Telegraphs.Count == 0);

            // Out of range: the boss chases and never attacks.
            var far = new GargoyleBrain(config);
            Advance(far, Frame(Vector3.zero, Vector3.forward, 10f, new Vector3(0f, 1f, 10f)), 5f, hits);
            Check("gargoyle: target at 10 m: chases, no telegraph", far.Telegraphs.Count == 0 && far.WantsToMove && hits.Count == 0);
        }

        private static void CheckVoidWardenSlashAndDecoy()
        {
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            var brain = new VoidWardenBrain(config, () => 0.5f);
            var placed = new List<Telegraph>();
            brain.OnTelegraphPlaced += placed.Add;
            var hits = new List<float>();
            // Kaelen 3 m in front (+Z); the target point (Kaelen or decoy) decides the aim.
            Vector3 kaelen = new Vector3(0f, 1f, 3f);
            BossFrame toKaelen = Frame(Vector3.zero, Vector3.forward, 3f, kaelen);
            brain.Tick(Step, toKaelen, hits.Add);
            Check("warden: first attack is the triple slash (one red fan placed)", placed.Count == 1 && !placed[0].Shape.IsCircle);
            Telegraph first = placed[0];
            Check("warden: fan aimed at the target, length 8, 90 degrees, 30 DMG",
                Vector3.Dot(first.Shape.Direction, Vector3.forward) > 0.999f && first.Shape.Radius == 8f
                && Mathf.Abs(first.Shape.HalfAngleDegrees - 45f) < 1e-4f && first.Damage == 30f);

            // A decoy appears to the right (+X) while the first fan is telegraphing: the placed fan does not move.
            BossFrame toDecoy = Frame(Vector3.zero, Vector3.right, 3f, kaelen);
            Advance(brain, toDecoy, 0.5f, hits);
            Check("decoy: the fan already placed keeps its direction", Vector3.Dot(first.Shape.Direction, Vector3.forward) > 0.999f && !first.IsResolved);
            Advance(brain, toDecoy, 0.55f, hits);
            Check("decoy: the placed fan still resolves normally and hits Kaelen inside it (30)", first.IsResolved && hits.Count == 1 && hits[0] == 30f);
            Advance(brain, toDecoy, 1.6f, hits);
            Check("decoy: the next fan is aimed at the decoy (+X), Kaelen (+Z) is outside it",
                placed.Count >= 2 && Vector3.Dot(placed[1].Shape.Direction, Vector3.right) > 0.999f && hits.Count == 1);

            // Back on Kaelen: three slashes in total, then the cooldown.
            Advance(brain, toKaelen, 3f, hits);
            int fans = placed.FindAll(p => !p.Shape.IsCircle).Count;
            Check($"warden: exactly 3 fans in one triple slash (got {fans}), each resolving for 30", fans == 3 && hits.TrueForAll(h => h == 30f));
            Check("warden: after the triple slash the next attack is the pull", brain.NextIsPull && !brain.IsAttacking);
        }

        private static void CheckPull()
        {
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            var playerConfig = Track(ScriptableObject.CreateInstance<PlayerConfig>());
            var brain = new VoidWardenBrain(config, () => 0.5f);
            typeof(VoidWardenBrain).GetField("_nextIsPull", Private).SetValue(brain, true);
            var placed = new List<Telegraph>();
            brain.OnTelegraphPlaced += placed.Add;
            var hits = new List<float>();
            Vector3 centre = new Vector3(1f, 0f, 1f);
            Vector3 kaelen = new Vector3(1f, 1f, 9f); // 8 m from the centre, inside the 14 m pull radius
            brain.Tick(Step, Frame(centre, Vector3.forward, 8f, kaelen), hits.Add);
            Check("pull: starts with a wind-up circle (radius 14, damage 0, >= 1 s) before any pull",
                placed.Count == 1 && placed[0].Shape.IsCircle && placed[0].Shape.Radius == 14f && placed[0].Damage == 0f
                && placed[0].Duration >= 1f && brain.PullStep == Vector3.zero);
            float windup = 0f;
            while (brain.PullStep == Vector3.zero && windup < 5f)
            {
                brain.Tick(Step, Frame(centre, Vector3.forward, 8f, kaelen), hits.Add);
                windup += Step;
            }
            Check($"pull: no pull during the wind-up; first pull step after {windup:0.00} s (>= 1 s)", windup >= 1f - 1e-4f && hits.Count == 0);

            // While pulling: bounded per-tick step toward the centre.
            bool bounded = true;
            bool towardCentre = true;
            float pullTime = 0f;
            Vector3 position = kaelen;
            while (brain.IsAttacking && placed.Count == 1 && pullTime < 10f)
            {
                Vector3 step = brain.PullStep;
                bounded &= step.magnitude <= config.PullSpeed * Step + 1e-4f;
                Vector3 toCentre = centre - position;
                toCentre.y = 0f;
                towardCentre &= step == Vector3.zero || Vector3.Dot(step.normalized, toCentre.normalized) > 0.999f;
                position += step;
                brain.Tick(Step, Frame(centre, Vector3.forward, 8f, position), hits.Add);
                pullTime += Step;
            }
            Check("pull: each step <= PullSpeed * dt and points at the centre", bounded && towardCentre);
            Check($"pull: lasts PullDurationSeconds (2 s, got {pullTime:0.00})", Mathf.Abs(pullTime - config.PullDurationSeconds) <= 2f * Step);
            Check("pull: ComputePullStep never overshoots the centre", Mathf.Abs(VoidWardenBrain.ComputePullStep(new Vector3(0.1f, 0f, 0f), Vector3.zero, 1f, 14f).magnitude - 0.1f) < 1e-4f);
            Check("pull: no pull beyond the pull radius", VoidWardenBrain.ComputePullStep(new Vector3(15f, 0f, 0f), Vector3.zero, 1f, 14f) == Vector3.zero);

            // Dash outpaces the pull (the pull is added to Kaelen's movement, never replacing it).
            float dashSpeed = playerConfig.DashDistance / playerConfig.DashDuration;
            float pullOverDash = config.PullSpeed * playerConfig.DashDuration;
            Check($"pull: dash speed {dashSpeed:0.#} m/s > pull {config.PullSpeed} m/s and > walk; pull during one dash {pullOverDash:0.##} m < dash {playerConfig.DashDistance} m",
                dashSpeed > config.PullSpeed && playerConfig.MoveSpeed > config.PullSpeed && pullOverDash < playerConfig.DashDistance);
            Vector3 dasher = new Vector3(4f, 0f, 0f);
            for (float d = 0f; d < playerConfig.DashDuration - 1e-5f; d += Step)
            {
                dasher += Vector3.right * (dashSpeed * Step); // the dash's own travel, untouched
                dasher += VoidWardenBrain.ComputePullStep(dasher, Vector3.zero, config.PullSpeed * Step, config.PullRadius);
            }
            Check($"pull: a dash away from 4 m ends outside the 4 m burst circle (at {dasher.x:0.00} m)", dasher.x > config.BurstRadius + 3f);

            // Then the ground burst: red circle at the fixed centre, 40 DMG.
            Check("burst: circle at the pull centre, radius 4, 40 DMG", placed.Count == 2 && placed[1].Shape.IsCircle
                && placed[1].Shape.Origin == centre && placed[1].Shape.Radius == 4f && placed[1].Damage == 40f && placed[1].Duration >= 1f);
            Vector3 inside = centre + new Vector3(1f, 1f, 0f);
            Advance(brain, Frame(centre, Vector3.forward, 1f, inside), 0.9f, hits);
            Check("burst: no damage before the circle resolves", hits.Count == 0);
            Advance(brain, Frame(centre, Vector3.forward, 1f, inside), 0.2f, hits);
            Check("burst: Kaelen inside takes exactly 40", hits.Count == 1 && hits[0] == 40f);
            Check($"warden: 3 s cooldown running after the burst (left {brain.CooldownRemaining:0.00}), next attack is the slash",
                !brain.IsAttacking && brain.CooldownRemaining <= 3f && brain.CooldownRemaining >= 3f - 0.2f - Step && !brain.NextIsPull);
        }

        private static void CheckPhaseTwoAndMeteors()
        {
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            float r = 0.1f;
            var brain = new VoidWardenBrain(config, () => { r = (r + 0.37f) % 1f; return r; });
            int phaseEvents = 0;
            brain.OnPhaseTwoStarted += () => phaseEvents++;
            var meteors = new List<Telegraph>();
            brain.OnTelegraphPlaced += t => { if (t.Shape.Radius == config.MeteorRadius && t.Damage == config.MeteorDamage) meteors.Add(t); };
            var hits = new List<float>();
            // Target 100 m away: no slash / pull, so only meteors can appear.
            BossFrame full = Frame(Vector3.zero, Vector3.forward, 100f, new Vector3(0f, 1f, 100f));
            Advance(brain, full, 20f, hits);
            Check("phase 1: no meteors in 20 s at full HP", meteors.Count == 0 && !brain.IsPhaseTwo);
            BossFrame half = full;
            half.HealthFraction = 0.5f;
            Advance(brain, half, 10f, hits);
            Check("phase: exactly 50% HP is still phase 1 (rule: strictly below 50%)", !brain.IsPhaseTwo && meteors.Count == 0);
            BossFrame below = full;
            below.HealthFraction = 0.4995f;
            brain.Tick(Step, below, hits.Add);
            Check("phase: HP below 50% enters phase 2", brain.IsPhaseTwo && phaseEvents == 1);
            BossFrame healed = full; // HealthFraction 1
            Advance(brain, healed, 7.9f, hits);
            Check("phase: latched (stays phase 2 even at full HP), event raised once, no meteors before 8 s", brain.IsPhaseTwo && phaseEvents == 1 && meteors.Count == 0);
            Advance(brain, healed, 0.2f, hits);
            Check($"meteors: 5 red circles (radius 2, 20 DMG) after 8 s of phase 2 (got {meteors.Count})", meteors.Count == 5);
            bool inArena = meteors.TrueForAll(m => Mathf.Abs(m.Shape.Origin.x) <= config.MeteorAreaHalfExtent && Mathf.Abs(m.Shape.Origin.z) <= config.MeteorAreaHalfExtent);
            Check("meteors: points inside the scatter square, warning >= 1 s", inArena && meteors.TrueForAll(m => m.Duration >= 1f));
            Advance(brain, healed, 8f, hits);
            Check($"meteors: 5 more after another 8 s (total {meteors.Count})", meteors.Count == 10);
            Check("meteors: Kaelen far away took no damage", hits.Count == 0);

            // A meteor landing on Kaelen deals 20.
            Telegraph pending = brain.Telegraphs[brain.Telegraphs.Count - 1];
            BossFrame onMeteor = Frame(Vector3.zero, Vector3.forward, 100f, pending.Shape.Origin + Vector3.up);
            Advance(brain, onMeteor, pending.Duration, hits);
            Check("meteors: Kaelen under a meteor takes 20", hits.Count >= 1 && hits.TrueForAll(h => h == 20f));

            // Death: everything pending is dropped, nothing ever deals damage afterwards.
            for (float wait = 0f; brain.Telegraphs.Count == 0 && wait < 10f; wait += Step)
            {
                brain.Tick(Step, healed, hits.Add);
            }
            int pendingBefore = brain.Telegraphs.Count;
            hits.Clear();
            brain.Stop();
            Advance(brain, onMeteor, 30f, hits);
            Check($"stop: {pendingBefore} pending meteors cleared, no damage and no new telegraph in 30 s", pendingBefore > 0 && brain.Telegraphs.Count == 0 && hits.Count == 0);
        }

        private static void CheckPause()
        {
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            var brain = new GargoyleBrain(config);
            var hits = new List<float>();
            BossFrame frame = Frame(Vector3.zero, Vector3.forward, 2f, new Vector3(0f, 1f, 2f));
            brain.Tick(Step, frame, hits.Add);
            Telegraph slam = brain.Telegraphs[0];
            var owner = new object();
            GamePause.Acquire(owner);
            Check("pause: GamePause.Acquire sets Time.timeScale 0 (bosses tick on Time.deltaTime)", Time.timeScale == 0f);
            float before = slam.Progress;
            for (int i = 0; i < 1000; i++)
            {
                brain.Tick(Time.timeScale * Step, frame, hits.Add); // what Time.deltaTime is while paused
            }
            Check("pause: no telegraph progress and no damage while paused", slam.Progress == before && !slam.IsResolved && hits.Count == 0);
            GamePause.Release(owner);
            Check("pause: released -> Time.timeScale 1", Time.timeScale == 1f);
        }

        private static void CheckBossComponents()
        {
            var config = AssetDatabase.LoadAssetAtPath<BossConfig>("Assets/Data/BossConfig.asset");
            var material = Track(new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.red });
            GargoyleTyrant gargoyle = Boss<GargoyleTyrant>("Gargoyle", config, material);
            VoidWarden warden = Boss<VoidWarden>("Warden", config, material);
            Check("boss: Gargoyle spawns with 600 HP, Kind Boss", gargoyle.CurrentHealth == 600f && gargoyle.Kind == EnemyKind.Boss);
            Check("boss: Void Warden spawns with 2000 HP", warden.CurrentHealth == 2000f);

            // Damage packets as sent by the combo (stun on the 3rd hit), Soul Burst, the meteor card and the revive shock.
            var combo = new DamageInfo { Amount = 20f, KnockbackDirection = Vector3.forward, KnockbackDistance = 1.5f, StunDuration = 0.5f };
            var soulBurst = new DamageInfo { Amount = 120f, KnockbackDirection = Vector3.right, KnockbackDistance = 4f };
            var meteorCard = new DamageInfo { Amount = 80f, StunDuration = 1f };
            var reviveShock = new DamageInfo { Amount = 0f, KnockbackDirection = Vector3.back, KnockbackDistance = 3f };
            foreach (BossBase boss in new BossBase[] { gargoyle, warden })
            {
                float hp = boss.CurrentHealth;
                boss.TakeDamage(combo);
                boss.TakeDamage(soulBurst);
                boss.TakeDamage(meteorCard);
                boss.TakeDamage(reviveShock);
                Vector3 slide = (Vector3)typeof(EnemyBase).GetField("_slideVelocity", Private).GetValue(boss);
                Check($"immune: {boss.name} lost exactly 220 HP but was not stunned or pushed",
                    Mathf.Abs(hp - boss.CurrentHealth - 220f) < 1e-3f && !boss.IsStunned && slide == Vector3.zero);
            }

            // Telegraphs on screen, then the Void Warden dies: everything is cleared at once.
            var hits = new List<float>();
            warden.Brain.Tick(Step, Frame(warden.transform.position, Vector3.forward, 3f, warden.transform.position + Vector3.forward * 3f), hits.Add);
            int drawn = CountTelegraphObjects();
            Check("warden: an attack draws its telegraph on the floor", warden.Brain.Telegraphs.Count == 1 && drawn == 1);
            // Same order as EnemyBase.Die (whose Destroy cannot run in edit mode): OnDying, then OnDied.
            int telegraphsSeenByListener = -1;
            warden.OnDied += _ => telegraphsSeenByListener = warden.Brain.Telegraphs.Count + CountTelegraphObjects();
            typeof(EnemyBase).GetMethod("OnDying", Private).Invoke(warden, null);
            var onDied = (Action<EnemyDied>)typeof(EnemyBase).GetField("OnDied", Private).GetValue(warden);
            onDied(new EnemyDied { Kind = EnemyKind.Boss, Position = warden.transform.position });
            Check("death: telegraphs are already gone when OnDied listeners (Victory) run", telegraphsSeenByListener == 0);
            Advance(warden.Brain, Frame(Vector3.zero, Vector3.forward, 1f, warden.transform.position), 10f, hits);
            Check("death: Void Warden death clears every telegraph and drawing; no damage afterwards",
                warden.Brain.Telegraphs.Count == 0 && CountTelegraphObjects() == 0 && hits.Count == 0);

            // Run reset / despawn destroys the boss: OnDestroy clears its telegraphs.
            gargoyle.Brain.Tick(Step, Frame(gargoyle.transform.position, Vector3.forward, 2f, gargoyle.transform.position), hits.Add);
            Check("reset: Gargoyle slam telegraph drawn", gargoyle.Brain.Telegraphs.Count == 1 && CountTelegraphObjects() == 1);
            typeof(BossBase).GetMethod("OnDestroy", Private).Invoke(gargoyle, null);
            Advance(gargoyle.Brain, Frame(Vector3.zero, Vector3.forward, 1f, gargoyle.transform.position), 10f, hits);
            Check("reset: destroying the boss (run reset / despawn) clears its telegraphs; no damage afterwards",
                gargoyle.Brain.Telegraphs.Count == 0 && CountTelegraphObjects() == 0 && hits.Count == 0);
        }

        private static void CheckWaves()
        {
            var waveConfig = AssetDatabase.LoadAssetAtPath<WaveConfig>("Assets/Data/WaveConfig.asset");
            WaveDefinition five = waveConfig.GetWave(5);
            WaveDefinition ten = waveConfig.GetWave(10);
            Check("waves: Wave 5 = Gargoyle Tyrant + BossConfig, 4 Crawler escorts",
                five.Goal == WaveGoal.Boss && five.BossPrefab is GargoyleTyrant && five.BossConfig != null && five.EscortCount == 4
                && five.SpawnPool.Length == 1 && five.SpawnPool[0].Prefab is Crawler);
            Check("waves: Wave 10 = Void Warden + BossConfig, no escorts",
                ten.Goal == WaveGoal.Boss && ten.BossPrefab is VoidWarden && ten.BossConfig != null && ten.EscortCount == 0);
            Check("waves: stand-in boss assets are gone",
                AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/MiniBossStandIn.prefab") == null
                && AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/FinalBossStandIn.prefab") == null
                && AssetDatabase.LoadAssetAtPath<Object>("Assets/Data/EnemyConfig_MiniBossStandIn.asset") == null);

            // Wave 5: boss dies first, the wave waits for the last escort.
            var runState = Make("RunState").AddComponent<RunState>();
            var trap = Make("Trap").AddComponent<RotatingTrap>();
            WaveDirector director = Director(waveConfig, runState, trap);
            runState.StartRun(5);
            var boss = Make("Gargoyle", PrimitiveType.Cube).AddComponent<GargoyleTyrant>();
            var escorts = new List<EnemyBase>();
            for (int i = 0; i < 4; i++)
            {
                escorts.Add(Make($"Escort{i}", PrimitiveType.Cube).AddComponent<Crawler>());
            }
            int ended = 0;
            director.OnWaveEnded += _ => ended++;
            StartBossWave(director, five, boss, escorts);
            Check("waves: IsBoss(Gargoyle) is true (Soul Burst's boss predicate stays consistent)", director.IsBoss(boss) && !director.IsBoss(escorts[0]));
            Died(director, boss);
            Check("wave 5: Gargoyle dead with 4 escorts alive -> wave NOT complete", runState.Phase == RunPhase.Playing && ended == 0);
            Died(director, escorts[0]);
            Died(director, escorts[1]);
            Died(director, escorts[2]);
            Check("wave 5: 1 escort left -> still not complete", runState.Phase == RunPhase.Playing && ended == 0);
            Died(director, escorts[3]);
            Check("wave 5: last escort dies -> wave complete (card pick)", runState.Phase == RunPhase.CardPick && ended == 1);

            // Escorts first, boss last: also completes on the boss death.
            runState.StartRun(5);
            var boss2 = Make("Gargoyle2", PrimitiveType.Cube).AddComponent<GargoyleTyrant>();
            var escort = Make("Escort", PrimitiveType.Cube).AddComponent<Crawler>();
            StartBossWave(director, five, boss2, new List<EnemyBase> { escort });
            Died(director, escort);
            Check("wave 5: escorts dead, Gargoyle alive -> not complete", runState.Phase == RunPhase.Playing);
            Died(director, boss2);
            Check("wave 5: Gargoyle dies last -> complete", runState.Phase == RunPhase.CardPick && ended == 2);

            // Wave 10: Void Warden dies -> Victory.
            runState.StartRun(10);
            var warden = Make("Warden", PrimitiveType.Capsule).AddComponent<VoidWarden>();
            StartBossWave(director, ten, warden, new List<EnemyBase>());
            Died(director, warden);
            Check("wave 10: Void Warden death -> Victory", runState.Phase == RunPhase.Victory && ended == 3);
        }

        // ---------- fixtures ----------

        private static WaveDirector Director(WaveConfig config, RunState runState, RotatingTrap trap)
        {
            var director = Make("Director").AddComponent<WaveDirector>(); // Awake/Start do not run in edit mode
            typeof(WaveDirector).GetField("_config", Private).SetValue(director, config);
            typeof(WaveDirector).GetField("_runState", Private).SetValue(director, runState);
            typeof(WaveDirector).GetField("_trap", Private).SetValue(director, trap);
            return director;
        }

        private static void StartBossWave(WaveDirector director, WaveDefinition wave, EnemyBase boss, List<EnemyBase> escorts)
        {
            Type type = typeof(WaveDirector);
            type.GetField("_wave", Private).SetValue(director, wave);
            type.GetField("_waveActive", Private).SetValue(director, true);
            type.GetField("_kills", Private).SetValue(director, 0);
            type.GetField("_boss", Private).SetValue(director, boss);
            type.GetField("_bossDefeated", Private).SetValue(director, false);
            var alive = (List<EnemyBase>)type.GetField("_alive", Private).GetValue(director);
            alive.Clear();
            alive.Add(boss);
            alive.AddRange(escorts);
        }

        private static void Died(WaveDirector director, EnemyBase enemy)
        {
            typeof(WaveDirector).GetMethod("HandleEnemyDied", Private).Invoke(director,
                new object[] { enemy, new EnemyDied { Kind = enemy is BossBase ? EnemyKind.Boss : EnemyKind.Crawler } });
        }

        private static T Boss<T>(string name, BossConfig config, Material material) where T : BossBase
        {
            GameObject go = Make(name, PrimitiveType.Cube, new Vector3(30f, 1f, 30f));
            var boss = go.AddComponent<T>();
            boss.ConfigureBoss(config, null);
            boss.SetTelegraphMaterial(material);
            typeof(BossBase).GetMethod("Awake", Private).Invoke(boss, null);
            return boss;
        }

        private static int CountTelegraphObjects()
        {
            int count = 0;
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == "BossTelegraph")
                {
                    count++;
                }
            }
            return count;
        }

        private static BossConfig Config(params (string field, float value)[] overrides)
        {
            var config = Track(ScriptableObject.CreateInstance<BossConfig>());
            var so = new SerializedObject(config);
            foreach ((string field, float value) in overrides)
            {
                so.FindProperty(field).floatValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static BossFrame Frame(Vector3 boss, Vector3 directionToTarget, float distanceToTarget, Vector3 player)
        {
            return new BossFrame
            {
                Position = boss,
                DirectionToTarget = directionToTarget,
                DistanceToTarget = distanceToTarget,
                PlayerPosition = player,
                PlayerAlive = true,
                HealthFraction = 1f,
            };
        }

        private static float Advance(BossBrain brain, BossFrame frame, float seconds, List<float> hits)
        {
            int steps = Mathf.RoundToInt(seconds / Step);
            for (int i = 0; i < steps; i++)
            {
                brain.Tick(Step, frame, hits.Add);
            }
            return steps * Step;
        }

        private static GameObject Make(string name, PrimitiveType? shape = null, Vector3 position = default)
        {
            GameObject go = shape.HasValue ? GameObject.CreatePrimitive(shape.Value) : new GameObject();
            go.name = name;
            go.transform.position = position;
            Created.Add(go);
            return go;
        }

        private static T Track<T>(T asset) where T : Object
        {
            Created.Add(asset);
            return asset;
        }

        private static void Near(string name, float actual, float expected)
        {
            Check($"{name} (got {actual:0.#####})", Mathf.Abs(actual - expected) < 1e-3f);
        }

        private static void Check(string name, bool ok)
        {
            if (ok)
            {
                Debug.Log($"PASS {name}");
            }
            else
            {
                Fail(name);
            }
        }

        private static void Fail(string name)
        {
            _failures++;
            Debug.LogError($"FAIL {name}");
        }
    }
}
