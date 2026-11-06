// TEMPORARY batchmode rule check for story 007 (HUD, end screens, screen shake, FeelConfig, telegraph colours, TMP fonts).
// Kept in tools/hud-feel-check/ (outside Assets/, never compiled); see the README there to copy it in, run it, remove it.
//   Unity -batchmode -quit -projectPath <project> -executeMethod MicroArena.Gameplay.Editor.HudFeelCheck.Run
// Needs the assets made by ArenaSceneBuilder.Build (Arena.unity, FeelConfig.asset, the Be Vietnam Pro SDF assets, prefabs).

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MicroArena.Gameplay.Arena;
using MicroArena.Gameplay.Bosses;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Feel;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using MicroArena.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MicroArena.Gameplay.Editor
{
    /// <summary>Batchmode rule check for story 007. Prints PASS/FAIL lines and exits 1 on any failure.</summary>
    public static class HudFeelCheck
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string VietnameseSample = "ẮẰẲẴẶắằẳẵặĐđƠơƯưỆệ";
        private const string RegularFontPath = "Assets/Fonts/BeVietnamPro/BeVietnamPro-Regular SDF.asset";
        private const string BoldFontPath = "Assets/Fonts/BeVietnamPro/BeVietnamPro-Bold SDF.asset";

        private static int _failures;
        private static readonly List<Object> Created = new List<Object>();

        // Rig (real components, Awake/OnEnable invoked by reflection: edit mode does not run them).
        private static TMP_FontAsset _regular;
        private static TMP_FontAsset _bold;
        private static FeelConfig _feel;
        private static Health _health;
        private static Dash _dash;
        private static AutoAttack _attack;
        private static SoulBurst _soulBurst;
        private static RunState _runState;
        private static WaveDirector _director;
        private static HudController _hud;
        private static EndScreens _ends;
        private static FollowCamera _follow;
        private static ScreenShake _shake;
        private static int _runResets;

        /// <summary>Runs every check, prints PASS/FAIL lines, exits 1 on any failure.</summary>
        public static void Run()
        {
            _failures = 0;
            try
            {
                CheckSceneAndAssets();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                CheckShakeModel();
                CheckOverlayGate();
                CheckTelegraphColours();
                BuildRig();
                CheckHudPlayer();
                CheckHudWaves();
                CheckHudStateless();
                CheckShake();
                CheckEndScreens();
                CheckFontsOnBuiltUi();
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
            Debug.Log(_failures == 0 ? "HudFeelCheck: ALL PASS" : $"HudFeelCheck: {_failures} FAILURE(S)");
            EditorApplication.Exit(_failures == 0 ? 0 : 1);
        }

        // ---------- assets and the saved scene ----------

        private static void CheckSceneAndAssets()
        {
            _regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath);
            _bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);
            Check("fonts: Be Vietnam Pro Regular and Bold TMP font assets exist",
                _regular != null && _bold != null && _regular.faceInfo.familyName == "Be Vietnam Pro" && _bold.faceInfo.familyName == "Be Vietnam Pro");
            foreach (TMP_FontAsset font in new[] { _regular, _bold })
            {
                Check($"fonts: {font.name} already holds every glyph of \"{VietnameseSample}\" (HasCharacters, no add)",
                    font.HasCharacters(VietnameseSample, out List<char> missing) && missing.Count == 0);
                Check($"fonts: {font.name} holds the whole builder set (ASCII + all Vietnamese letters)",
                    font.HasCharacters(ArenaSceneBuilder.FontCharacters()));
                var so = new SerializedObject(font);
                Check($"fonts: {font.name} is Dynamic, keeps its source .ttf for player builds, ClearDynamicDataOnBuild off",
                    font.atlasPopulationMode == AtlasPopulationMode.Dynamic && font.sourceFontFile != null
                    && AssetDatabase.GetAssetPath(font.sourceFontFile).EndsWith(".ttf") && !so.FindProperty("m_ClearDynamicDataOnBuild").boolValue);
                Check($"fonts: {font.name} atlas texture and material are sub-assets, material uses a TextMeshPro shader",
                    AssetDatabase.GetAssetPath(font.atlasTexture) == AssetDatabase.GetAssetPath(font)
                    && AssetDatabase.GetAssetPath(font.material) == AssetDatabase.GetAssetPath(font)
                    && font.material.shader != null && font.material.shader.name.StartsWith("TextMeshPro/"));
            }
            Check("tmp: TMP Essential Resources imported (TMP Settings in a Resources folder)",
                AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset") != null);

            var feelAsset = AssetDatabase.LoadAssetAtPath<FeelConfig>("Assets/Data/FeelConfig.asset");
            Check("feel: Assets/Data/FeelConfig.asset exists (shake on combo step 3)", feelAsset != null && feelAsset.ComboShakeStep == 3);

            EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity", OpenSceneMode.Single);
            var hud = Object.FindAnyObjectByType<HudController>(FindObjectsInactive.Include);
            var ends = Object.FindAnyObjectByType<EndScreens>(FindObjectsInactive.Include);
            var shake = Object.FindAnyObjectByType<ScreenShake>(FindObjectsInactive.Include);
            var cards = Object.FindAnyObjectByType<CardPickScreen>(FindObjectsInactive.Include);
            Check("scene: Arena has HudController, EndScreens, ScreenShake (on the camera), CardPickScreen",
                hud != null && ends != null && shake != null && shake.GetComponent<FollowCamera>() != null && cards != null);
            bool fontsWired = true;
            foreach (MonoBehaviour ui in new MonoBehaviour[] { hud, ends, cards })
            {
                var so = new SerializedObject(ui);
                fontsWired &= so.FindProperty("_font").objectReferenceValue == _regular && so.FindProperty("_boldFont").objectReferenceValue == _bold;
            }
            Check("scene: HUD, end screens and card pick are wired to the Be Vietnam Pro font assets", fontsWired);
            Check("scene: HUD, end screens, shake are wired to FeelConfig.asset",
                new SerializedObject(hud).FindProperty("_feel").objectReferenceValue == feelAsset
                && new SerializedObject(ends).FindProperty("_feel").objectReferenceValue == feelAsset
                && new SerializedObject(shake).FindProperty("_config").objectReferenceValue == feelAsset);
            Check("scene: no legacy UnityEngine.UI.Text in Arena.unity",
                Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0);
            bool noLegacySource = true;
            foreach (string file in new[] { "Assets/Scripts/UI/HudController.cs", "Assets/Scripts/UI/EndScreens.cs", "Assets/Scripts/UI/CardPickScreen.cs", "Assets/Scripts/UI/UiKit.cs" })
            {
                string source = File.ReadAllText(file);
                noLegacySource &= !source.Contains("LegacyRuntime") && !source.Contains("AddComponent<Text>") && !source.Contains("GetBuiltinResource<Font>");
            }
            Check("source: UI scripts never create legacy Text or load LegacyRuntime.ttf", noLegacySource);
            Check("scene: the defeat no longer auto-restarts (no _autoRestartOnDefeat on WaveDirector)",
                typeof(WaveDirector).GetField("_autoRestartOnDefeat", Private) == null
                && new SerializedObject(Object.FindAnyObjectByType<WaveDirector>()).FindProperty("_autoRestartOnDefeat") == null);
        }

        // ---------- pure shake maths ----------

        private static void CheckShakeModel()
        {
            var model = new ShakeModel();
            Check("shake model: idle offset is zero", model.Offset == Vector3.zero && !model.IsShaking);
            model.Trigger(1f, 1f);
            model.Tick(0.5f, 30f);
            Near("shake model: strength fades linearly (1 -> 0.5 after half the duration)", model.CurrentStrength, 0.5f);
            Check("shake model: each axis bounded by the remaining strength",
                Mathf.Abs(model.Offset.x) <= 0.5f + 1e-4f && Mathf.Abs(model.Offset.z) <= 0.5f + 1e-4f && model.Offset.y == 0f);
            model.Trigger(0.2f, 1f);
            Near("shake model: a weaker shake does not cut a stronger one short", model.CurrentStrength, 0.5f);
            model.Trigger(0.8f, 0.4f);
            Near("shake model: a stronger shake restarts it", model.CurrentStrength, 0.8f);
            model.Tick(0.5f, 30f);
            model.Tick(0f, 30f);
            Check("shake model: after its duration the offset is zero", !model.IsShaking && model.Offset == Vector3.zero);
            model.Trigger(1f, 1f);
            model.Tick(0.1f, 30f);
            model.Reset();
            Check("shake model: Reset zeroes the offset at once", !model.IsShaking && model.Offset == Vector3.zero);
            model.Trigger(0f, 1f);
            model.Trigger(1f, 0f);
            Check("shake model: non-positive amplitude or duration is ignored", !model.IsShaking);
        }

        // ---------- debug overlay ----------

        private static void CheckOverlayGate()
        {
            Check("overlay: WaveDebugOverlay.IsAllowed == Debug.isDebugBuild", WaveDebugOverlay.IsAllowed == Debug.isDebugBuild);
            var overlay = Make("Overlay").AddComponent<WaveDebugOverlay>();
            typeof(WaveDebugOverlay).GetMethod("Awake", Private).Invoke(overlay, null);
            Check("overlay: Awake leaves it enabled exactly when Debug.isDebugBuild (editor: true)", overlay.enabled == Debug.isDebugBuild);
            string source = File.ReadAllText("Assets/Scripts/Gameplay/Waves/WaveDebugOverlay.cs");
            Check("overlay: source gates Awake and OnGUI on IsAllowed (=> Debug.isDebugBuild), drawn at the bottom-left",
                source.Contains("IsAllowed => Debug.isDebugBuild") && source.Contains("enabled = IsAllowed")
                && source.Contains("if (!IsAllowed)") && source.Contains("Screen.height - Height - Margin"));
        }

        // ---------- telegraph colours ----------

        private static void CheckTelegraphColours()
        {
            var config = AssetDatabase.LoadAssetAtPath<BossConfig>("Assets/Data/BossConfig.asset");
            Color damage = config.TelegraphColor(30f);
            Color warning = config.TelegraphColor(0f);
            Check($"telegraph: damage colour is red ({damage})", damage.r > 0.8f && damage.g < 0.3f && damage.b < 0.3f);
            Check($"telegraph: warning-only colour is purple ({warning})", warning.b > 0.7f && warning.r > 0.4f && warning.g < 0.4f && warning.r < warning.b);

            var material = Track(new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.white });
            var warden = Make("Warden", PrimitiveType.Capsule).AddComponent<VoidWarden>();
            warden.ConfigureBoss(config, null);
            warden.SetTelegraphMaterial(material);
            typeof(BossBase).GetMethod("Awake", Private).Invoke(warden, null);
            var views = (List<TelegraphView>)typeof(BossBase).GetField("_views", Private).GetValue(warden);
            var seen = new List<(float damage, Color color)>();
            var frame = new BossFrame
            {
                Position = Vector3.zero, DirectionToTarget = Vector3.forward, DistanceToTarget = 2f,
                PlayerPosition = new Vector3(100f, 0f, 100f), PlayerAlive = true, HealthFraction = 1f,
            };
            bool sawPull = false;
            for (int i = 0; i < 2000 && !sawPull; i++)
            {
                warden.Brain.Tick(0.02f, frame, _ => { });
                foreach (TelegraphView view in views)
                {
                    seen.Add((view.Telegraph.Damage, view.Color));
                    sawPull |= view.Telegraph.Damage == 0f;
                }
            }
            bool slashRed = false;
            bool pullPurple = false;
            bool allConsistent = seen.Count > 0;
            foreach ((float dmg, Color color) in seen)
            {
                bool expected = color == config.TelegraphColor(dmg);
                allConsistent &= expected;
                slashRed |= dmg > 0f && color == damage;
                pullPurple |= dmg == 0f && color == warning;
            }
            Check("telegraph: Void Warden slash fans are drawn red", slashRed);
            Check("telegraph: Void Warden pull wind-up circle (0 damage) is drawn purple", sawPull && pullPurple);
            Check("telegraph: every drawn telegraph uses red when it deals damage, purple when it does not", allConsistent);
            warden.Brain.Stop();
        }

        // ---------- rig ----------

        private static void BuildRig()
        {
            // A FeelConfig with values different from the asset, so the checks prove the component reads its config.
            _feel = Track(ScriptableObject.CreateInstance<FeelConfig>());
            var so = new SerializedObject(_feel);
            so.FindProperty("_comboShakeAmplitude").floatValue = 0.37f;
            so.FindProperty("_comboShakeDuration").floatValue = 0.5f;
            so.FindProperty("_explosionShakeAmplitude").floatValue = 0.61f;
            so.FindProperty("_explosionShakeDuration").floatValue = 0.7f;
            so.FindProperty("_lowHealthFraction").floatValue = 0.45f;
            so.FindProperty("_replayInputDelay").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var kaelenPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Kaelen.prefab");
            var kaelen = (GameObject)PrefabUtility.InstantiatePrefab(kaelenPrefab);
            Created.Add(kaelen);
            _health = kaelen.GetComponent<Health>();
            _dash = kaelen.GetComponent<Dash>();
            _attack = kaelen.GetComponent<AutoAttack>();
            var controller = kaelen.GetComponent<PlayerController>();
            Invoke(_health, "Awake");
            Invoke(_dash, "Awake");
            Invoke(_attack, "Awake");
            Invoke(controller, "Awake");

            _runState = Make("RunState").AddComponent<RunState>();
            var trap = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RotatingTrap.prefab"))).GetComponent<RotatingTrap>();
            Created.Add(trap.gameObject);
            _director = Make("WaveDirector").AddComponent<WaveDirector>();
            _director.Configure(AssetDatabase.LoadAssetAtPath<WaveConfig>("Assets/Data/WaveConfig.asset"), _runState, _health, controller, trap, null);
            Invoke(_director, "Awake");
            Invoke(_director, "OnEnable");
            _director.OnRunReset += () => _runResets++;

            _soulBurst = kaelen.AddComponent<SoulBurst>();
            _soulBurst.Configure(AssetDatabase.LoadAssetAtPath<SoulBurstConfig>("Assets/Data/SoulBurstConfig.asset"),
                AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"), _health, controller, _runState,
                _director, ~0, null, AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SoulBurstRing.mat"));
            Invoke(_soulBurst, "Awake");
            Invoke(_soulBurst, "OnEnable");

            var cameraObject = Make("Camera");
            _follow = cameraObject.AddComponent<FollowCamera>();
            _follow.Configure(kaelen.transform);
            _shake = cameraObject.AddComponent<ScreenShake>();
            _shake.Configure(_feel, _follow, _attack, _director);
            Invoke(_shake, "OnEnable");

            var ui = Make("UI");
            _hud = ui.AddComponent<HudController>();
            _hud.Configure(_feel, _director, _health, _dash, _soulBurst, _regular, _bold);
            Invoke(_hud, "Awake");
            Invoke(_hud, "OnEnable");
            Invoke(_hud, "Start");
            _ends = ui.AddComponent<EndScreens>();
            _ends.Configure(_feel, _runState, _director, _regular, _bold);
            Invoke(_ends, "Awake");
            Invoke(_ends, "OnEnable");

            _director.RestartRun(); // the real start of a run: every HUD value arrives by event from here
        }

        // ---------- HUD: player ----------

        private static void CheckHudPlayer()
        {
            float max = _health.Max;
            Check($"hud hp: full at run start ({Text("HpText")})", Near(Fill("HpBar"), 1f) && Text("HpText") == Hp(max, max));
            _health.TakeDamage(new DamageInfo { Amount = 30f });
            Check($"hud hp: after 30 damage the bar and label follow OnHealthChanged ({Text("HpText")})",
                Near(Fill("HpBar"), (max - 30f) / max) && Text("HpText") == Hp(max - 30f, max));
            _health.TakeDamage(new DamageInfo { Amount = max * 0.6f });
            Check("hud hp: at or below FeelConfig.LowHealthFraction (0.45 here) the bar turns to the low colour",
                Fill("HpBar") <= 0.45f && FieldColor(_hud, "_hpLowColor") == Find<Image>("HpBar/Fill").color);

            List<Image> pips = Pips();
            Check("hud dash: 2 pips, both bright at run start", pips.Count == 2 && pips[0].color == FieldColor(_hud, "_pipReadyColor") && pips[1].color == FieldColor(_hud, "_pipReadyColor"));
            var model = (DashModel)typeof(Dash).GetField("_model", Private).GetValue(_dash);
            model.TryStart();
            pips = Pips();
            Check("hud dash: after a dash one pip bright, one dim", pips[0].color == FieldColor(_hud, "_pipReadyColor") && pips[1].color == FieldColor(_hud, "_pipSpentColor"));
            var playerConfig = AssetDatabase.LoadAssetAtPath<PlayerConfig>("Assets/Data/PlayerConfig.asset");
            model.Tick(playerConfig.DashDuration + 0.01f); // the first dash ends (recharge has started)
            model.TryStart();
            pips = Pips();
            Check("hud dash: after the second dash both dim", pips[0].color == FieldColor(_hud, "_pipSpentColor") && pips[1].color == FieldColor(_hud, "_pipSpentColor"));
            model.Tick(playerConfig.DashRechargeTime - playerConfig.DashDuration);
            pips = Pips();
            Check("hud dash: a recharge brightens a pip (OnChargesChanged)", pips[0].color == FieldColor(_hud, "_pipReadyColor") && pips[1].color == FieldColor(_hud, "_pipSpentColor"));

            for (int i = 0; i < 5; i++)
            {
                _soulBurst.RegisterKill(default);
            }
            Check($"hud rage: bar and label follow OnRageChanged ({Text("RageText")})",
                Near(Fill("RageBar"), _soulBurst.Rage / _soulBurst.MaxRage) && Text("RageText") == string.Format(FieldString(_hud, "_rageFormat"), _soulBurst.Rage, _soulBurst.MaxRage));
            for (int i = 0; i < 40; i++)
            {
                _soulBurst.RegisterKill(default);
            }
            Check($"hud rage: full bar shows the Q-ready text ({Text("RageText")})",
                Near(Fill("RageBar"), 1f) && Text("RageText") == FieldString(_hud, "_rageReadyText"));

            // Revive: real lethal damage with the revive available.
            _health.TakeDamage(new DamageInfo { Amount = _health.Max * 10f });
            Check("hud revive: the revive shows the flash and the message", _health.IsAlive && !_health.ReviveAvailable
                && Find<Image>("ReviveFlash").gameObject.activeSelf && Find<TextMeshProUGUI>("ReviveMessage").gameObject.activeSelf
                && Text("ReviveMessage") == FieldString(_hud, "_reviveText"));
        }

        // ---------- HUD: waves and bosses ----------

        private static void CheckHudWaves()
        {
            Check($"hud wave: run start shows {Text("WaveText")} and 15/15 remaining",
                Text("WaveText") == "WAVE 01/10" && Text("RemainingText") == string.Format(FieldString(_hud, "_remainingFormat"), 15, 15) && Near(Fill("RemainingBar"), 1f));
            Check("hud boss: no boss bar on a normal wave", !BossPanel().activeSelf);

            // Wave 5: the real StartWave spawns the Gargoyle Tyrant and raises OnBossSpawned.
            _runState.StartRun(5);
            Invoke(_director, "StartWave");
            var gargoyle = (EnemyBase)typeof(WaveDirector).GetField("_boss", Private).GetValue(_director);
            Created.Add(gargoyle.gameObject);
            Check($"hud wave 5: {Text("WaveText")}, boss bar shown with {Text("BossName")}",
                Text("WaveText") == "WAVE 05/10" && BossPanel().activeSelf && Text("BossName") == "GARGOYLE BẠO QUÂN");
            Check($"hud wave 5: target = boss + 4 escorts ({Text("RemainingText")})", Text("RemainingText") == Remaining(5, 5));
            typeof(BossBase).GetMethod("Awake", Private).Invoke(gargoyle, null);
            gargoyle.TakeDamage(new DamageInfo { Amount = 60f });
            Check("hud boss: a hit on the Gargoyle moves the boss bar (600 -> 540)", Near(Fill("BossBar"), 0.9f));

            // Two escorts spawned, two not yet: both kinds count while the Gargoyle lives.
            var waveConfig = AssetDatabase.LoadAssetAtPath<WaveConfig>("Assets/Data/WaveConfig.asset");
            WaveSpawnEntry entry = waveConfig.GetWave(5).SpawnPool[0];
            MethodInfo spawn = typeof(WaveDirector).GetMethod("Spawn", Private);
            var escorts = new List<EnemyBase>();
            for (int i = 0; i < 2; i++)
            {
                var escort = (EnemyBase)spawn.Invoke(_director, new object[] { entry.Prefab, entry.Config, null });
                Created.Add(escort.gameObject);
                escorts.Add(escort);
            }
            typeof(WaveDirector).GetField("_poolSpawned", Private).SetValue(_director, 2);
            Check("wave 5: Remaining counts boss + 2 alive escorts + 2 unspawned = 5/5", _director.Remaining == 5 && _director.RemainingTarget == 5);
            Died(escorts[0]);
            Check($"hud wave 5: an escort kill -> {Text("RemainingText")}", Text("RemainingText") == Remaining(4, 5));
            Died(gargoyle);
            Check("hud boss: the Gargoyle's death hides the boss bar (OnBossCleared)", !BossPanel().activeSelf);
            Check($"hud wave 5: Gargoyle dead, 1 escort alive, no more spawns -> {Text("RemainingText")} (not 0)",
                Text("RemainingText") == Remaining(1, 5) && _runState.Phase == RunPhase.Playing);
            Died(escorts[1]);
            Check($"hud wave 5: last escort dies -> {Text("RemainingText")} and the wave ends", Text("RemainingText") == Remaining(0, 5) && _runState.Phase == RunPhase.CardPick);

            // Wave 5 again, all 4 escorts alive when the Gargoyle dies (the exact story-006 deviation).
            _runState.StartRun(5);
            Invoke(_director, "StartWave");
            gargoyle = (EnemyBase)typeof(WaveDirector).GetField("_boss", Private).GetValue(_director);
            Created.Add(gargoyle.gameObject);
            escorts.Clear();
            for (int i = 0; i < 4; i++)
            {
                var escort = (EnemyBase)spawn.Invoke(_director, new object[] { entry.Prefab, entry.Config, null });
                Created.Add(escort.gameObject);
                escorts.Add(escort);
            }
            typeof(WaveDirector).GetField("_poolSpawned", Private).SetValue(_director, 4);
            Died(gargoyle);
            Check($"hud wave 5: Gargoyle dead with 4 escorts alive -> {Text("RemainingText")} (was 0/1 before story 007)", Text("RemainingText") == Remaining(4, 5));
            foreach (EnemyBase escort in escorts)
            {
                Died(escort);
            }
            Check("hud wave 5: the 4 escorts dead -> 0/5, wave over", Text("RemainingText") == Remaining(0, 5) && _runState.Phase == RunPhase.CardPick);

            // A boss removed without dying (run reset / wave end path) also hides the bar.
            _runState.StartRun(5);
            Invoke(_director, "StartWave");
            gargoyle = (EnemyBase)typeof(WaveDirector).GetField("_boss", Private).GetValue(_director);
            Created.Add(gargoyle.gameObject);
            ((List<EnemyBase>)typeof(WaveDirector).GetField("_alive", Private).GetValue(_director)).Clear(); // no Destroy in edit mode
            Check("hud boss: bar shown again on a new Wave 5", BossPanel().activeSelf);
            Invoke(_director, "DespawnAllEnemies");
            Check("hud boss: a despawned boss (reset / wave end) hides the bar", !BossPanel().activeSelf);

            // Wave 10: Void Warden bar, its death is the victory.
            _runState.StartRun(10);
            Invoke(_director, "StartWave");
            var warden = (EnemyBase)typeof(WaveDirector).GetField("_boss", Private).GetValue(_director);
            Created.Add(warden.gameObject);
            Check($"hud wave 10: {Text("WaveText")}, boss bar with {Text("BossName")}, target 1",
                Text("WaveText") == "WAVE 10/10" && BossPanel().activeSelf && Text("BossName") == "CHÚA TỂ HƯ VÔ" && Text("RemainingText") == Remaining(1, 1));
            Died(warden);
            Check("hud wave 10: Void Warden death hides the bar and gives Victory", !BossPanel().activeSelf && _runState.Phase == RunPhase.Victory);
        }

        // ---------- HUD holds no game state ----------

        private static void CheckHudStateless()
        {
            // Raise Health's own event with values Health does not hold: the HUD shows the event's values.
            var raise = (Action<float, float>)typeof(Health).GetField("OnHealthChanged", Private).GetValue(_health);
            raise(37f, 140f);
            Check($"hud stateless: shows the values of the event, not of Health ({Text("HpText")} while Health is {_health.Current:0}/{_health.Max:0})",
                Text("HpText") == Hp(37f, 140f) && Near(Fill("HpBar"), 37f / 140f));
            var remaining = (Action<int, int>)typeof(WaveDirector).GetField("OnRemainingChanged", Private).GetValue(_director);
            remaining(7, 9);
            Check("hud stateless: remaining label comes from OnRemainingChanged", Text("RemainingText") == Remaining(7, 9));

            var offending = new List<string>();
            foreach (FieldInfo field in typeof(HudController).GetFields(Private))
            {
                if (field.GetCustomAttribute<SerializeField>() != null)
                {
                    continue; // config: references, labels, colours, layout
                }
                Type type = field.FieldType;
                bool allowed = typeof(Object).IsAssignableFrom(type) || type == typeof(Coroutine) || type == typeof(List<Image>);
                if (!allowed)
                {
                    offending.Add($"{field.Name}:{type.Name}");
                }
            }
            Check($"hud stateless: runtime fields are only UI objects (no numbers, flags or strings) [{string.Join(", ", offending)}]", offending.Count == 0);
        }

        // ---------- screen shake ----------

        private static void CheckShake()
        {
            Transform kaelen = _health.transform;
            Vector3 kaelenBefore = kaelen.position;
            var offset = (Vector3)typeof(FollowCamera).GetField("_offset", Private).GetValue(_follow);
            Invoke(_shake, "Update");
            Invoke(_follow, "LateUpdate");
            Check("shake: idle camera sits exactly at Kaelen + offset", Near(_follow.transform.position, kaelenBefore + offset) && _follow.ShakeOffset == Vector3.zero);

            var swing = (Action<int, IReadOnlyList<IDamageable>>)typeof(AutoAttack).GetField("OnSwingResolved", Private).GetValue(_attack);
            var struck = new List<IDamageable> { _health };
            swing(0, struck);
            Check("shake: combo step 1 does not shake", Strength() == 0f);
            swing(2, new List<IDamageable>());
            Check("shake: a 3rd slash that hits nothing does not shake", Strength() == 0f);
            swing(2, struck);
            Near("shake: the 3rd slash that hits shakes with FeelConfig.ComboShakeAmplitude (0.37 here)", Strength(), 0.37f);
            Invoke(_shake, "Update");
            Invoke(_follow, "LateUpdate");
            Vector3 shakeOffset = _follow.ShakeOffset;
            Check($"shake: the camera moves by the shake offset ({shakeOffset}), bounded by the amplitude",
                shakeOffset != Vector3.zero && shakeOffset == _shake.Offset && Mathf.Abs(shakeOffset.x) <= 0.37f + 1e-4f && Mathf.Abs(shakeOffset.z) <= 0.37f + 1e-4f
                && Near(_follow.transform.position, kaelen.position + offset + shakeOffset));
            Check("shake: Kaelen (gameplay) is not moved by the shake", kaelen.position == kaelenBefore);
            Check("shake: the view translates without tilting (looks at Kaelen + shake)",
                Vector3.Angle(_follow.transform.forward, -offset) < 0.01f);

            GamePause.Acquire(_hud);
            Invoke(_shake, "Update");
            Check("shake: a pause (GamePause) resets the shake and the camera offset to zero", Strength() == 0f && _follow.ShakeOffset == Vector3.zero);
            swing(2, struck);
            Check("shake: no shake starts while paused", Strength() == 0f);
            GamePause.Release(_hud);

            // Volatile Husk explosion: the husk's own OnExploded -> WaveDirector.OnEnemyExploded -> ScreenShake.
            var waveConfig = AssetDatabase.LoadAssetAtPath<WaveConfig>("Assets/Data/WaveConfig.asset");
            WaveSpawnEntry huskEntry = Array.Find(waveConfig.GetWave(6).SpawnPool, e => e.Prefab is VolatileHusk);
            var husk = (VolatileHusk)typeof(WaveDirector).GetMethod("Spawn", Private).Invoke(_director, new object[] { huskEntry.Prefab, huskEntry.Config, null });
            Created.Add(husk.gameObject);
            var exploded = (Action<Vector3>)typeof(VolatileHusk).GetField("OnExploded", Private).GetValue(husk);
            exploded(husk.transform.position);
            Near("shake: a Volatile Husk explosion shakes with FeelConfig.ExplosionShakeAmplitude (0.61 here)", Strength(), 0.61f);
            ((List<EnemyBase>)typeof(WaveDirector).GetField("_alive", Private).GetValue(_director)).Clear();
            _director.RestartRun();
            Check("shake: a run reset stops the shake (offset zero)", Strength() == 0f && _follow.ShakeOffset == Vector3.zero);
            Check("shake: the run reset put Kaelen back at the spawn (gameplay untouched by the camera)", kaelen.position == kaelenBefore);
        }

        // ---------- end screens ----------

        private static void CheckEndScreens()
        {
            GameObject root = Find<RectTransform>("EndScreenCanvas").gameObject;
            Check("end: hidden during play", !root.activeSelf && !_ends.IsShown);

            // Victory: the real path (Void Warden dies in Wave 10) schedules the screen; Show is what the delay then calls.
            _runState.StartRun(10);
            Invoke(_director, "StartWave");
            var warden = (EnemyBase)typeof(WaveDirector).GetField("_boss", Private).GetValue(_director);
            Created.Add(warden.gameObject);
            Died(warden);
            Check("end: Void Warden death -> Victory phase, the screen is scheduled (real-time delay)",
                _runState.Phase == RunPhase.Victory && typeof(EndScreens).GetField("_pending", Private).GetValue(_ends) != null);
            _ends.Show(RunPhase.Victory);
            Check($"end victory: shown with {Text("Title")}, game paused (timeScale {Time.timeScale})",
                root.activeSelf && Text("Title") == FieldString(_ends, "_victoryTitle") && GamePause.IsPaused && Time.timeScale == 0f);
            int resets = _runResets;
            Find<Button>("ReplayButton").onClick.Invoke(); // the mouse path
            Check("end victory: clicking CHƠI LẠI at timeScale 0 restarts the run (Wave 1, Playing, pause released)",
                !root.activeSelf && _runResets == resets + 1 && _runState.Phase == RunPhase.Playing && _runState.WaveNumber == 1
                && !GamePause.IsPaused && Time.timeScale == 1f);

            // Defeat: revive spent, then lethal damage (real Health -> WaveDirector -> RunState).
            _health.TakeDamage(new DamageInfo { Amount = _health.Max * 10f }); // uses the revive
            ((HealthModel)typeof(Health).GetField("_model", Private).GetValue(_health)).Tick(100f); // revive invulnerability over
            _health.TakeDamage(new DamageInfo { Amount = _health.Max * 10f });
            Check("end defeat: out of revives -> Defeat phase", !_health.IsAlive && _runState.Phase == RunPhase.Defeat);
            Invoke(_director, "Update");
            Check("end defeat: no automatic restart is scheduled",
                _runState.Phase == RunPhase.Defeat && typeof(WaveDirector).GetField("_pending", Private).GetValue(_director).ToString() == "None");
            _ends.Show(RunPhase.Defeat);
            Check($"end defeat: shown with {Text("Title")}, game paused", root.activeSelf && Text("Title") == FieldString(_ends, "_defeatTitle") && Time.timeScale == 0f);

            // Keyboard path. Edit mode cannot produce wasPressedThisFrame (input goes to editor updates), so the check
            // confirms Update maps the replay keys to Replay() and then calls Replay() itself at time scale 0.
            string source = File.ReadAllText("Assets/Scripts/UI/EndScreens.cs");
            Check("end: Update calls Replay() when a replay key wasPressedThisFrame, at any time scale (no scaled time used)",
                source.Contains("keyboard[key].wasPressedThisFrame") && source.Contains("Replay();") && !source.Contains("Time.deltaTime")
                && source.Contains("WaitForSecondsRealtime"));
            resets = _runResets;
            _ends.Replay();
            Check("end defeat: the replay (Enter / R) at timeScale 0 restarts the run (Kaelen alive, full HP, pause released)",
                !root.activeSelf && _runResets == resets + 1 && _runState.Phase == RunPhase.Playing && _health.IsAlive
                && Mathf.Approximately(_health.Current, _health.Max) && Time.timeScale == 1f);

            _ends.Show(RunPhase.Defeat);
            _director.RestartRun();
            Check("end: a run reset from elsewhere also hides the screen and releases the pause", !root.activeSelf && !GamePause.IsPaused);
            Check("end: replay keys are Enter / keypad Enter / R; Space (Dash) excluded",
                new List<Key>((Key[])typeof(EndScreens).GetField("ReplayKeys", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null))
                    is var keys && keys.Contains(Key.Enter) && keys.Contains(Key.R) && !keys.Contains(Key.Space));
        }

        // ---------- every TMP text built at runtime ----------

        private static void CheckFontsOnBuiltUi()
        {
            var cards = Make("Cards").AddComponent<CardPickScreen>();
            cards.Configure(_regular, _bold);
            var pool = AssetDatabase.LoadAssetAtPath<CardPoolConfig>("Assets/Data/CardPoolConfig.asset");
            var all = new List<CardConfig>(pool.Cards);
            cards.Show(all, _ => { });
            Check("cards: the card pick screen shows every pool card", cards.IsShown && Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length >= all.Count);

            _ends.Show(RunPhase.Victory); // make the end screen texts current too
            TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int wrongFont = 0;
            int missingGlyphs = 0;
            var missingList = new List<string>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.font != _regular && text.font != _bold)
                {
                    wrongFont++;
                }
                else if (!string.IsNullOrEmpty(text.text) && !text.font.HasCharacters(text.text, out List<char> missing))
                {
                    missingGlyphs++;
                    missingList.Add($"{text.name}:{new string(missing.ToArray())}");
                }
            }
            Check($"fonts: all {texts.Length} TMP texts (HUD, end screens, card pick) use Be Vietnam Pro Regular/Bold", texts.Length > 30 && wrongFont == 0);
            Check($"fonts: every shown string (card names like \"Vết Chém Hỏa Ngục\", \"Cuồng Nộ Hư Vô\") has all its glyphs [{string.Join(" ", missingList)}]", missingGlyphs == 0);
            Check("fonts: no legacy UnityEngine.UI.Text was built at runtime",
                Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0);
            cards.Hide();
            _ends.Replay();
        }

        // ---------- helpers ----------

        private static string Hp(float current, float max) => string.Format(FieldString(_hud, "_hpFormat"), Mathf.Ceil(current), max);

        private static string Remaining(int remaining, int target) => string.Format(FieldString(_hud, "_remainingFormat"), remaining, target);

        private static float Strength()
        {
            var model = (ShakeModel)typeof(ScreenShake).GetField("_model", Private).GetValue(_shake);
            return model.CurrentStrength;
        }

        private static void Died(EnemyBase enemy)
        {
            typeof(WaveDirector).GetMethod("HandleEnemyDied", Private).Invoke(_director,
                new object[] { enemy, new EnemyDied { Kind = enemy.Kind, Position = enemy.transform.position } });
        }

        private static GameObject BossPanel() => Find<RectTransform>("BossPanel").gameObject;

        private static List<Image> Pips()
        {
            var pips = new List<Image>();
            foreach (Transform child in Find<RectTransform>("DashPips"))
            {
                pips.Add(child.GetComponent<Image>());
            }
            return pips;
        }

        private static float Fill(string bar) => UiKit.GetFill(Find<Image>($"{bar}/Fill"));

        private static string Text(string name) => Find<TextMeshProUGUI>(name).text;

        // Finds a component by object name (or "Parent/Child") under the HUD / end screen objects, inactive included.
        private static T Find<T>(string path) where T : Component
        {
            string[] parts = path.Split('/');
            foreach (T candidate in _hud.GetComponentsInChildren<T>(true))
            {
                if (candidate.name == parts[parts.Length - 1] && (parts.Length == 1 || candidate.transform.parent.name == parts[0]))
                {
                    return candidate;
                }
            }
            throw new InvalidOperationException($"UI object {path} not found");
        }

        private static string FieldString(object target, string field) => (string)target.GetType().GetField(field, Private).GetValue(target);

        private static Color FieldColor(object target, string field) => (Color)target.GetType().GetField(field, Private).GetValue(target);

        private static void Invoke(object target, string method)
        {
            Type type = target.GetType();
            MethodInfo info = null;
            while (info == null && type != null)
            {
                info = type.GetMethod(method, Private | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }
            info.Invoke(target, null);
        }

        private static GameObject Make(string name, PrimitiveType? shape = null)
        {
            GameObject go = shape.HasValue ? GameObject.CreatePrimitive(shape.Value) : new GameObject();
            go.name = name;
            Created.Add(go);
            return go;
        }

        private static T Track<T>(T asset) where T : Object
        {
            Created.Add(asset);
            return asset;
        }

        private static bool Near(float a, float b) => Mathf.Abs(a - b) < 1e-3f;

        private static bool Near(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < 1e-6f;

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
