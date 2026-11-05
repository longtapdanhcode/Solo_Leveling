// TEMPORARY batchmode rule check for story 005 (Soul Burst: rage, Q gating, shockwave damage/knockback, boss rule).
// Kept in tools/soul-burst-check/ (outside Assets/, never compiled); see the README there to copy it in, run it, remove it.
//   Unity -batchmode -quit -projectPath <project> -executeMethod MicroArena.Gameplay.Editor.SoulBurstCheck.Run

using System;
using System.Collections.Generic;
using System.Reflection;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace MicroArena.Gameplay.Editor
{
    /// <summary>Batchmode rule check for story 005. Prints PASS/FAIL lines and exits 1 on any failure.</summary>
    public static class SoulBurstCheck
    {
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const float EnemyHealth = 1000f;
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static int _failures;
        private static readonly List<Object> Created = new List<Object>();

        /// <summary>Runs every check, prints PASS/FAIL lines, exits 1 on any failure.</summary>
        public static void Run()
        {
            _failures = 0;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // no stray damageables in the overlap tests
            try
            {
                CheckConfigDefaults();
                CheckModel();
                CheckGatingAndShockwave();
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
            Debug.Log(_failures == 0 ? "SoulBurstCheck: ALL PASS" : $"SoulBurstCheck: {_failures} FAILURE(S)");
            EditorApplication.Exit(_failures == 0 ? 0 : 1);
        }

        private static void CheckConfigDefaults()
        {
            var config = Track(ScriptableObject.CreateInstance<SoulBurstConfig>());
            Near("config: RagePerKill == 5", config.RagePerKill, 5f);
            Near("config: MaxRage == 100", config.MaxRage, 100f);
            Near("config: Damage == 120", config.Damage, 120f);
            Near("config: ShockwaveRadius == 6", config.ShockwaveRadius, 6f);
            Near("config: KnockbackDistance == 4", config.KnockbackDistance, 4f);
        }

        private static void CheckModel()
        {
            var model = new SoulBurstModel(5f, 100f);
            var events = new List<float>();
            model.OnRageChanged += (current, max) => events.Add(current);
            Near("model: rage 0 at start", model.Rage, 0f);
            model.AddKill();
            Near("model: +5 per kill", model.Rage, 5f);
            for (int i = 0; i < 30; i++)
            {
                model.AddKill();
            }
            Near("model: capped at 100 after 31 kills", model.Rage, 100f);
            Check("model: OnRageChanged raised 20 times (no event at the cap), last value 100", events.Count == 20 && events[19] == 100f);
            model.Reset();
            Near("model: Reset -> 0", model.Rage, 0f);
            model.AddKill();
            Check("model: TryConsume below 100 -> false, rage kept", !model.TryConsume() && model.Rage == 5f);
        }

        private static void CheckGatingAndShockwave()
        {
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Check("input: SoulBurst action exists in the Player map", actions != null && actions.FindActionMap("Player")?.FindAction("SoulBurst") != null);
            bool boundToQ = false;
            foreach (InputBinding binding in actions.FindActionMap("Player").FindAction("SoulBurst").bindings)
            {
                boundToQ |= binding.path == "<Keyboard>/q";
            }
            Check("input: SoulBurst is bound to <Keyboard>/q", boundToQ);

            var playerConfig = Track(ScriptableObject.CreateInstance<PlayerConfig>());
            var so = new SerializedObject(playerConfig);
            so.FindProperty("_reviveInvulnerability").floatValue = 0f; // lets two big hits kill Kaelen without ticking time
            so.ApplyModifiedPropertiesWithoutUndo();
            var burstConfig = Track(ScriptableObject.CreateInstance<SoulBurstConfig>());

            GameObject kaelenObject = Make("Kaelen", PrimitiveType.Capsule, new Vector3(0f, 1f, 0f));
            var health = kaelenObject.AddComponent<Health>();
            health.Configure(playerConfig, ~0);
            CallAwake(health);
            var controller = kaelenObject.AddComponent<PlayerController>(); // facing defaults to +Z without Awake
            var runState = Make("RunState").AddComponent<RunState>();
            var director = Make("Director").AddComponent<WaveDirector>(); // Awake/OnEnable do not run in edit mode
            var burst = kaelenObject.AddComponent<SoulBurst>();
            var ringMaterial = Track(new Material(Shader.Find("Universal Render Pipeline/Lit")));
            burst.Configure(burstConfig, actions, health, controller, runState, director, ~0, null, ringMaterial);
            CallAwake(burst);
            burst.GetType().GetMethod("OnEnable", Private).Invoke(burst, null); // the real director subscriptions

            Near("burst: rage 0 at start", burst.Rage, 0f);
            // Wiring: rage comes from the director's OnEnemyKilled and resets on its OnRunReset.
            ((Action<EnemyDied>)typeof(WaveDirector).GetField("OnEnemyKilled", Private).GetValue(director))?.Invoke(new EnemyDied { Kind = EnemyKind.VolatileHusk });
            Near("wiring: WaveDirector.OnEnemyKilled (Husk) gives +5", burst.Rage, 5f);
            ((Action)typeof(WaveDirector).GetField("OnRunReset", Private).GetValue(director))?.Invoke();
            Near("wiring: WaveDirector.OnRunReset empties the bar", burst.Rage, 0f);
            Check("burst: Q at 0 does nothing", !burst.TryActivate() && burst.Rage == 0f);
            Kills(burst, 1, EnemyKind.VolatileHusk);
            Near("burst: a Volatile Husk EnemyDied gives +5", burst.Rage, 5f);
            Kills(burst, 18, EnemyKind.Crawler);
            Near("burst: 19 kills -> 95", burst.Rage, 95f);
            Check("burst: Q at 95 does nothing and keeps the rage", !burst.TryActivate() && burst.Rage == 95f);
            Kills(burst, 10, EnemyKind.VoidCaster);
            Near("burst: capped at 100 after 29 kills", burst.Rage, 100f);

            // Blocked contexts keep the rage at 100.
            var owner = new object();
            GamePause.Acquire(owner);
            Check("gate: blocked while GamePause paused (card pick / pause)", !burst.TryActivate() && burst.Rage == 100f);
            GamePause.Release(owner);
            runState.StartRun(5);
            runState.CompleteWave(10);
            Check("gate: blocked in CardPick phase", runState.Phase == RunPhase.CardPick && !burst.TryActivate() && burst.Rage == 100f);
            runState.Fail();
            Check("gate: blocked in Defeat phase", runState.Phase == RunPhase.Defeat && !burst.TryActivate() && burst.Rage == 100f);
            runState.StartRun(10);
            runState.CompleteWave(10);
            Check("gate: blocked in Victory phase", runState.Phase == RunPhase.Victory && !burst.TryActivate() && burst.Rage == 100f);
            runState.StartRun(1);

            // Allowed during hit-stop only, with enemies around: the physics part.
            var enemies = new List<Crawler>();
            Crawler near = Enemy(enemies, "Near", new Vector3(3f, 1f, 0f));
            Crawler diagonal = Enemy(enemies, "Diagonal", new Vector3(-3f, 1f, -3f));
            Crawler edge = Enemy(enemies, "Edge", new Vector3(0f, 1f, 5f));
            Crawler far = Enemy(enemies, "Far", new Vector3(7f, 1f, 0f));
            Crawler boss = Enemy(enemies, "Boss", new Vector3(0f, 1f, -4f));
            typeof(WaveDirector).GetField("_boss", Private).SetValue(director, boss); // the real WaveDirector.IsBoss does the check

            int reportedHits = -1;
            burst.OnBurst += hits => reportedHits = hits;
            float hpBefore = health.Current;
            GamePause.SetHitStop(true);
            Check("gate: allowed during hit-stop alone (IsFrozen, not IsPaused)", GamePause.IsFrozen && !GamePause.IsPaused && burst.TryActivate());
            GamePause.SetHitStop(false);
            Near("burst: rage 0 after firing", burst.Rage, 0f);
            Check("burst: OnBurst reported 4 hits, LastBurstHits == 4", reportedHits == 4 && burst.LastBurstHits == 4);
            Check("burst: second Q right after does nothing", !burst.TryActivate());

            float max = EnemyHealth;
            Created.Insert(0, ((Component)typeof(SoulBurst).GetField("_ring", Private).GetValue(burst)).gameObject); // destroyed before Kaelen
            Near("burst: enemy at 3 m took exactly 120", max - near.CurrentHealth, 120f);
            Near("burst: enemy at 4.2 m (diagonal) took exactly 120", max - diagonal.CurrentHealth, 120f);
            Near("burst: enemy at 5 m took exactly 120", max - edge.CurrentHealth, 120f);
            Near("burst: boss took exactly 120", max - boss.CurrentHealth, 120f);
            Check("burst: enemy at 7 m untouched (HP == max)", far.CurrentHealth == max);
            Near("burst: Kaelen's HP unchanged", health.Current, hpBefore);

            Near("knockback: near enemy pushed 4 m", Knockback(near), 4f);
            Near("knockback: diagonal enemy pushed 4 m", Knockback(diagonal), 4f);
            Check("knockback: near enemy pushed outward (+X)", Vector3.Dot(SlideDirection(near), Vector3.right) > 0.999f);
            Check("knockback: diagonal enemy pushed outward (-X,-Z)", Vector3.Dot(SlideDirection(diagonal), new Vector3(-1f, 0f, -1f).normalized) > 0.999f);
            Near("knockback: boss pushed 0 m", Knockback(boss), 0f);
            Near("knockback: unreached enemy pushed 0 m", Knockback(far), 0f);

            // Reset on run reset; persists otherwise.
            Kills(burst, 4, EnemyKind.Crawler);
            burst.ResetForNewRun();
            Near("reset: run reset empties the bar", burst.Rage, 0f);

            // Kaelen dead: blocked, rage kept.
            Kills(burst, 20, EnemyKind.Crawler);
            health.TakeDamage(new DamageInfo { Amount = 1_000_000f }); // spends the revive (no invulnerability in this config)
            health.TakeDamage(new DamageInfo { Amount = 1_000_000f });
            Check("gate: blocked while Kaelen is dead", !health.IsAlive && !burst.TryActivate() && burst.Rage == 100f);
            burst.GetType().GetMethod("OnDisable", Private).Invoke(burst, null);
        }

        // ---------- fixtures ----------

        private static void Kills(SoulBurst burst, int count, EnemyKind kind)
        {
            for (int i = 0; i < count; i++)
            {
                burst.RegisterKill(new EnemyDied { Kind = kind });
            }
        }

        // Real Crawler with a big HP pool so nothing dies (Destroy is not allowed in edit mode).
        private static Crawler Enemy(List<Crawler> list, string name, Vector3 position)
        {
            GameObject go = Make(name, PrimitiveType.Cube, position);
            var crawler = go.AddComponent<Crawler>();
            EnemyConfig config = Track(EnemyConfig.CreateDefault(EnemyKind.Crawler));
            var so = new SerializedObject(config);
            so.FindProperty("_maxHealth").floatValue = EnemyHealth;
            so.ApplyModifiedPropertiesWithoutUndo();
            crawler.Configure(config, null);
            typeof(EnemyBase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(crawler, null);
            list.Add(crawler);
            return crawler;
        }

        private static float Knockback(EnemyBase enemy)
        {
            float seconds = (float)typeof(EnemyBase).GetField("_knockbackSlideSeconds", Private).GetValue(enemy);
            return ((Vector3)typeof(EnemyBase).GetField("_slideVelocity", Private).GetValue(enemy)).magnitude * seconds;
        }

        private static Vector3 SlideDirection(EnemyBase enemy)
        {
            return ((Vector3)typeof(EnemyBase).GetField("_slideVelocity", Private).GetValue(enemy)).normalized;
        }

        private static void CallAwake(Component component)
        {
            component.GetType().GetMethod("Awake", Private).Invoke(component, null);
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
