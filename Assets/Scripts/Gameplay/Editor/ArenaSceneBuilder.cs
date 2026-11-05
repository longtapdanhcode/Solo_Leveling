// Implements: production/epics/micro-arena-survivor/story-003-arena-waves.md (Arena scene), design/game-brief.md MVP 2,
// design/narrative/kich_ban_chuan.md section 4 (Bang 1 wave values).
//
// Editor only (this folder compiles into Assembly-CSharp-Editor). Generates the URP materials, prefabs, config assets,
// Assets/Scenes/Arena.unity and the Scene List entry. Batchmode:
//   Unity -batchmode -quit -projectPath <project> -executeMethod MicroArena.Gameplay.Editor.ArenaSceneBuilder.Build
// Idempotent: materials, configs and prefabs are created only when missing (designer edits survive a re-run; delete an
// asset to regenerate it), materials are always re-pointed to URP Lit, the scene is rebuilt every run.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Arena;
using MicroArena.Gameplay.Cards;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using MicroArena.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MicroArena.Gameplay.Editor
{
    /// <summary>Builds the story-003 arena scene and every asset it references.</summary>
    public static class ArenaSceneBuilder
    {
        private const string MaterialsFolder = "Assets/Materials";
        private const string PrefabsFolder = "Assets/Prefabs";
        private const string DataFolder = "Assets/Data";
        private const string ScenePath = "Assets/Scenes/Arena.unity";
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string SandboxScenePath = "Assets/Scenes/PlayerSandbox.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string LitShaderPath = "Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader";
        private const string LitShaderName = "Universal Render Pipeline/Lit";

        // Visual only: floor slab thickness and bolt size (same as VoidCaster's runtime-built bolt).
        private const float FloorThickness = 0.2f;
        private const float ProjectileScale = 0.4f;

        /// <summary>Entry point for <c>-executeMethod</c> and the menu. Throws (non-zero exit in batchmode) on failure.</summary>
        [MenuItem("Micro Arena Survivor/Build Arena Scene")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Data");
            EnsureFolder("Assets", "Scenes");

            // A fresh empty scene also hosts the temporary objects that prefabs are saved from.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Shader lit = ResolveLitShader();
            Material floorMat = GetOrCreateMaterial("Floor", new Color(0.25f, 0.27f, 0.3f), lit);
            Material wallMat = GetOrCreateMaterial("Wall", new Color(0.42f, 0.4f, 0.48f), lit);
            Material trapMat = GetOrCreateMaterial("Trap", new Color(0.85f, 0.15f, 0.15f), lit);
            Material kaelenMat = GetOrCreateMaterial("Kaelen", new Color(0.2f, 0.5f, 1f), lit);
            Material kaelenFacingMat = GetOrCreateMaterial("KaelenFacing", Color.white, lit);
            Material crawlerMat = GetOrCreateMaterial("Crawler", new Color(0.45f, 0.75f, 0.2f), lit);
            Material casterMat = GetOrCreateMaterial("VoidCaster", new Color(0.6f, 0.2f, 0.9f), lit);
            Material huskMat = GetOrCreateMaterial("VolatileHusk", new Color(1f, 0.55f, 0.1f), lit);
            Material miniBossMat = GetOrCreateMaterial("MiniBossStandIn", new Color(0.55f, 0.35f, 0.15f), lit);
            Material finalBossMat = GetOrCreateMaterial("FinalBossStandIn", new Color(0.2f, 0.05f, 0.35f), lit);
            Material projectileMat = GetOrCreateMaterial("Projectile", new Color(0.85f, 0.45f, 1f), lit);

            PlayerConfig playerConfig = GetOrCreateAsset($"{DataFolder}/PlayerConfig.asset", () => ScriptableObject.CreateInstance<PlayerConfig>());
            CombatQueryConfig queryConfig = GetOrCreateAsset($"{DataFolder}/CombatQueryConfig.asset", () => ScriptableObject.CreateInstance<CombatQueryConfig>());
            EnemyConfig crawlerConfig = GetOrCreateAsset($"{DataFolder}/EnemyConfig_Crawler.asset", () => EnemyConfig.CreateDefault(EnemyKind.Crawler));
            EnemyConfig casterConfig = GetOrCreateAsset($"{DataFolder}/EnemyConfig_VoidCaster.asset", () => EnemyConfig.CreateDefault(EnemyKind.VoidCaster));
            EnemyConfig huskConfig = GetOrCreateAsset($"{DataFolder}/EnemyConfig_VolatileHusk.asset", () => EnemyConfig.CreateDefault(EnemyKind.VolatileHusk));
            // [PLACEHOLDER] Boss stand-ins until story 006: tougher Crawlers (HP, damage, speed, bite reach for the bigger body).
            EnemyConfig miniBossConfig = GetOrCreateAsset($"{DataFolder}/EnemyConfig_MiniBossStandIn.asset", () => CreateBossStandIn(600f, 20f, 3f, 2.2f));
            EnemyConfig finalBossConfig = GetOrCreateAsset($"{DataFolder}/EnemyConfig_FinalBossStandIn.asset", () => CreateBossStandIn(1200f, 25f, 3f, 2.6f));

            Projectile projectilePrefab = GetOrCreatePrefab<Projectile>($"{PrefabsFolder}/Projectile.prefab", () => BuildProjectile(projectileMat));
            Crawler crawlerPrefab = GetOrCreatePrefab<Crawler>($"{PrefabsFolder}/Crawler.prefab",
                () => BuildEnemy<Crawler>("Crawler", PrimitiveType.Cube, new Vector3(0.9f, 0.6f, 0.9f), crawlerMat, crawlerConfig, queryConfig));
            VoidCaster casterPrefab = GetOrCreatePrefab<VoidCaster>($"{PrefabsFolder}/VoidCaster.prefab", () =>
            {
                GameObject go = BuildEnemy<VoidCaster>("VoidCaster", PrimitiveType.Capsule, new Vector3(0.8f, 1f, 0.8f), casterMat, casterConfig, queryConfig);
                var so = new SerializedObject(go.GetComponent<VoidCaster>());
                so.FindProperty("_projectilePrefab").objectReferenceValue = projectilePrefab;
                so.ApplyModifiedPropertiesWithoutUndo();
                return go;
            });
            VolatileHusk huskPrefab = GetOrCreatePrefab<VolatileHusk>($"{PrefabsFolder}/VolatileHusk.prefab",
                () => BuildEnemy<VolatileHusk>("VolatileHusk", PrimitiveType.Sphere, Vector3.one * 0.8f, huskMat, huskConfig, queryConfig));
            Crawler miniBossPrefab = GetOrCreatePrefab<Crawler>($"{PrefabsFolder}/MiniBossStandIn.prefab",
                () => BuildEnemy<Crawler>("MiniBossStandIn", PrimitiveType.Cube, new Vector3(1.8f, 1.2f, 1.8f), miniBossMat, miniBossConfig, queryConfig));
            Crawler finalBossPrefab = GetOrCreatePrefab<Crawler>($"{PrefabsFolder}/FinalBossStandIn.prefab",
                () => BuildEnemy<Crawler>("FinalBossStandIn", PrimitiveType.Cube, new Vector3(2.4f, 1.6f, 2.4f), finalBossMat, finalBossConfig, queryConfig));

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                throw new InvalidOperationException($"ArenaSceneBuilder: input actions not found at {InputActionsPath}.");
            }
            PlayerController kaelenPrefab = GetOrCreatePrefab<PlayerController>($"{PrefabsFolder}/Kaelen.prefab",
                () => BuildKaelen(kaelenMat, kaelenFacingMat, playerConfig, queryConfig, actions));

            var pools = new PoolPrefabs
            {
                Crawler = crawlerPrefab, CrawlerConfig = crawlerConfig,
                Caster = casterPrefab, CasterConfig = casterConfig,
                Husk = huskPrefab, HuskConfig = huskConfig,
            };
            WaveConfig waveConfig = GetOrCreateAsset($"{DataFolder}/WaveConfig.asset",
                () => CreateWaveConfig(pools, miniBossPrefab, miniBossConfig, finalBossPrefab, finalBossConfig));
            RotatingTrap trapPrefab = GetOrCreatePrefab<RotatingTrap>($"{PrefabsFolder}/RotatingTrap.prefab", () => BuildTrap(trapMat, waveConfig));

            // Story 004: the 9 cards (kich_ban_chuan.md section 6; descriptions are the script wording). Created only when missing.
            var newCards = new List<CardConfig>();
            CardConfig[] cards =
            {
                GetOrCreateCard("Card_SucManhTho", newCards, () => CreateCard("suc_manh_tho", "Sức Mạnh Thô",
                    "Tăng 15% sát thương tấn công cơ bản.", CardRarity.Common, CardEffectKind.DamagePercent, percent: 15f)),
                GetOrCreateCard("Card_GiayBao", newCards, () => CreateCard("giay_bao", "Giày Bão",
                    "Tăng 12% tốc độ di chuyển.", CardRarity.Common, CardEffectKind.MoveSpeedPercent, percent: 12f)),
                GetOrCreateCard("Card_HuyetNhu", newCards, () => CreateCard("huyet_nhu", "Huyết Nhũ",
                    "Hồi phục ngay 30 HP và tăng thêm 20 HP tối đa.", CardRarity.Common, CardEffectKind.HealAndMaxHealth,
                    healAmount: 30f, maxHealthBonus: 20f)),
                GetOrCreateCard("Card_VetChemHoaNguc", newCards, () => CreateCard("hoa_nguc", "Vết Chém Hỏa Ngục",
                    "Đòn đánh thường gây hiệu ứng thiêu đốt (gây 10 DMG/s trong 3s, cộng dồn 3 lần).", CardRarity.Rare,
                    CardEffectKind.Burn, amount: 10f)),
                GetOrCreateCard("Card_LoiChan", newCards, () => CreateCard("loi_chan", "Lôi Chấn",
                    "Đòn thứ 3 của combo phóng ra tia sét giật lan 3 mục tiêu xung quanh (25 DMG điện).", CardRarity.Rare,
                    CardEffectKind.ChainLightning, amount: 25f)),
                GetOrCreateCard("Card_DichChuyenPhanThan", newCards, () => CreateCard("phan_than", "Dịch Chuyển Phân Thân",
                    "Khi Dash để lại một ảo ảnh thu hút quái đánh trong 1.5 giây.", CardRarity.Rare,
                    CardEffectKind.Decoy, amount: 1.5f)),
                GetOrCreateCard("Card_VongXoayHuyetThan", newCards, () => CreateCard("vong_xoay", "Vòng Xoáy Huyết Thần",
                    "Tăng 30% bán kính vung kiếm. Tiêu diệt 1 quái hồi 4% máu tối đa.", CardRarity.Legendary,
                    CardEffectKind.BloodVortex, percent: 30f, healPercentPerKill: 4f)),
                GetOrCreateCard("Card_SaoBangGiangTran", newCards, () => CreateCard("sao_bang", "Sao Băng Giáng Trần",
                    "Sau cú Dash, một thiên thạch rơi ngay vào vị trí cũ gây 80 DMG diện rộng và làm choáng 1s.", CardRarity.Legendary,
                    CardEffectKind.Meteor, amount: 80f)),
                GetOrCreateCard("Card_CuongNoHuVo", newCards, () => CreateCard("cuong_no", "Cuồng Nộ Hư Vô",
                    "Khi HP xuống dưới 30%, sát thương tăng 100% và nhận thêm 50% tỉ lệ chí mạng.", CardRarity.Legendary,
                    CardEffectKind.VoidRage, percent: 100f, critChanceBonus: 0.5f)),
            };
            CardPoolConfig cardPool = GetOrCreateAsset($"{DataFolder}/CardPoolConfig.asset", () => CreateCardPool(cards));
            AddToPool(cardPool, newCards); // an existing pool gains only cards created in this run (designer removals survive)

            Material decoyMat = GetOrCreateMaterial("Decoy", new Color(0.55f, 0.8f, 1f), lit);
            Material meteorMat = GetOrCreateMaterial("MeteorMarker", new Color(1f, 0.35f, 0.05f), lit);
            CardEffectConfig cardEffects = GetOrCreateAsset($"{DataFolder}/CardEffectConfig.asset", () =>
            {
                var effects = ScriptableObject.CreateInstance<CardEffectConfig>();
                var so = new SerializedObject(effects);
                so.FindProperty("_decoyMaterial").objectReferenceValue = decoyMat;
                so.FindProperty("_meteorMaterial").objectReferenceValue = meteorMat;
                so.ApplyModifiedPropertiesWithoutUndo();
                return effects;
            });

            BuildSceneContents(waveConfig, floorMat, wallMat, kaelenPrefab, trapPrefab, queryConfig, cardPool, cardEffects);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"ArenaSceneBuilder: could not save {ScenePath}.");
            }
            UpdateSceneList();
            AssetDatabase.SaveAssets();
            Debug.Log($"ArenaSceneBuilder: built {ScenePath} (shader '{lit.name}'), Scene List: {string.Join(", ", Array.ConvertAll(EditorBuildSettings.scenes, s => s.path))}");
        }

        private struct PoolPrefabs
        {
            public EnemyBase Crawler;
            public EnemyConfig CrawlerConfig;
            public EnemyBase Caster;
            public EnemyConfig CasterConfig;
            public EnemyBase Husk;
            public EnemyConfig HuskConfig;
        }

        private static Shader ResolveLitShader()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(LitShaderPath);
            if (shader == null)
            {
                RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
                Material fallback = pipeline != null ? pipeline.defaultMaterial : null;
                shader = fallback != null ? fallback.shader : null;
            }
            if (shader == null || shader.name != LitShaderName)
            {
                throw new InvalidOperationException(
                    $"ArenaSceneBuilder: expected shader '{LitShaderName}', got '{(shader != null ? shader.name : "none")}'. Is URP installed and assigned?");
            }
            return shader;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Material GetOrCreateMaterial(string name, Color color, Shader shader)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { color = color }; // URP Lit: color maps to _BaseColor ([MainColor])
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
            return material;
        }

        private static T GetOrCreateAsset<T>(string path, Func<T> create) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }
            asset = create();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static T GetOrCreatePrefab<T>(string path, Func<GameObject> build) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                GameObject temp = build();
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
                Object.DestroyImmediate(temp);
                if (prefab == null)
                {
                    throw new InvalidOperationException($"ArenaSceneBuilder: could not save prefab {path}.");
                }
            }
            return prefab.GetComponent<T>();
        }

        private static EnemyConfig CreateBossStandIn(float maxHealth, float damage, float moveSpeed, float biteRange)
        {
            EnemyConfig config = EnemyConfig.CreateDefault(EnemyKind.Crawler);
            var so = new SerializedObject(config);
            so.FindProperty("_maxHealth").floatValue = maxHealth;
            so.FindProperty("_damage").floatValue = damage;
            so.FindProperty("_moveSpeed").floatValue = moveSpeed;
            so.FindProperty("_biteRange").floatValue = biteRange;
            so.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static CardConfig GetOrCreateCard(string fileName, List<CardConfig> created, Func<CardConfig> create)
        {
            string path = $"{DataFolder}/{fileName}.asset";
            bool existed = AssetDatabase.LoadAssetAtPath<CardConfig>(path) != null;
            CardConfig card = GetOrCreateAsset(path, create);
            if (!existed)
            {
                created.Add(card);
            }
            return card;
        }

        private static CardConfig CreateCard(string id, string displayName, string description, CardRarity rarity,
            CardEffectKind effect, float percent = 0f, float healAmount = 0f, float maxHealthBonus = 0f, float amount = 0f,
            float healPercentPerKill = 0f, float critChanceBonus = 0f)
        {
            var card = ScriptableObject.CreateInstance<CardConfig>();
            var so = new SerializedObject(card);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_effect").enumValueIndex = (int)effect;
            so.FindProperty("_percent").floatValue = percent;
            so.FindProperty("_healAmount").floatValue = healAmount;
            so.FindProperty("_maxHealthBonus").floatValue = maxHealthBonus;
            so.FindProperty("_amount").floatValue = amount;
            so.FindProperty("_healPercentPerKill").floatValue = healPercentPerKill;
            so.FindProperty("_critChanceBonus").floatValue = critChanceBonus;
            so.FindProperty("_maxStacks").intValue = 0; // repeats allowed and stacking (decision 2026-11-05)
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        private static void AddToPool(CardPoolConfig pool, List<CardConfig> cards)
        {
            var so = new SerializedObject(pool);
            SerializedProperty list = so.FindProperty("_cards");
            bool changed = false;
            foreach (CardConfig card in cards)
            {
                bool present = false;
                for (int i = 0; i < list.arraySize; i++)
                {
                    present |= list.GetArrayElementAtIndex(i).objectReferenceValue == card;
                }
                if (!present)
                {
                    list.arraySize++;
                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = card;
                    changed = true;
                }
            }
            if (changed)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pool);
            }
        }

        // Weights and CardsPerPick keep the CardPoolConfig defaults (60/30/10, 3).
        private static CardPoolConfig CreateCardPool(params CardConfig[] cards)
        {
            var pool = ScriptableObject.CreateInstance<CardPoolConfig>();
            var so = new SerializedObject(pool);
            SerializedProperty list = so.FindProperty("_cards");
            list.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static GameObject BuildProjectile(Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Projectile";
            Object.DestroyImmediate(go.GetComponent<Collider>()); // hit test is a distance check; keep it out of overlap queries
            go.transform.localScale = Vector3.one * ProjectileScale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.AddComponent<Projectile>();
            return go;
        }

        // Enemies stay on the Default layer, which Kaelen's "everything" masks include. The root position rests the
        // body on the floor; WaveDirector reuses that height at spawn.
        private static GameObject BuildEnemy<T>(string name, PrimitiveType shape, Vector3 scale, Material material,
            EnemyConfig config, CombatQueryConfig queryConfig) where T : EnemyBase
        {
            GameObject go = GameObject.CreatePrimitive(shape);
            go.name = name;
            float restHeight = shape == PrimitiveType.Capsule ? scale.y : scale.y * 0.5f; // capsule is 2 units tall
            go.transform.position = new Vector3(0f, restHeight, 0f);
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            go.AddComponent<T>().Configure(config, null, queryConfig); // Kaelen is injected by WaveDirector at spawn
            return go;
        }

        private static GameObject BuildKaelen(Material body, Material facing, PlayerConfig config, CombatQueryConfig queryConfig,
            InputActionAsset actions)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Kaelen";
            go.transform.position = new Vector3(0f, 1f, 0f);
            go.GetComponent<Renderer>().sharedMaterial = body;

            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "FacingMarker";
            nose.transform.SetParent(go.transform, false);
            nose.transform.localPosition = new Vector3(0f, 0.3f, 0.6f);
            nose.transform.localScale = new Vector3(0.3f, 0.3f, 0.5f);
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            nose.GetComponent<Renderer>().sharedMaterial = facing;

            var health = go.AddComponent<Health>();
            var dash = go.AddComponent<Dash>();
            var attack = go.AddComponent<AutoAttack>();
            var controller = go.AddComponent<PlayerController>();
            var hitStop = go.AddComponent<HitStop>();

            LayerMask everything = ~0;
            health.Configure(config, everything, queryConfig);
            attack.Configure(config, controller, health, hitStop, everything, queryConfig);
            dash.Configure(config, actions, controller, health);
            controller.Configure(config, actions, health, dash, attack);
            return go;
        }

        // Root stays at scale 1 (RotatingTrap tests contact in its local space); the blade is a child without collider.
        private static GameObject BuildTrap(Material material, WaveConfig config)
        {
            var go = new GameObject("RotatingTrap");
            go.AddComponent<RotatingTrap>().Configure(config, null); // Kaelen is set on the scene instance

            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(go.transform, false);
            blade.transform.localPosition = new Vector3(0f, config.TrapHeight * 0.5f, 0f);
            blade.transform.localScale = new Vector3(config.TrapWidth, config.TrapHeight, config.TrapLength);
            Object.DestroyImmediate(blade.GetComponent<Collider>());
            blade.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        // Bang 1. [PLACEHOLDER] pool weights (mix ratios are not specified).
        private static WaveConfig CreateWaveConfig(PoolPrefabs p, EnemyBase miniBoss, EnemyConfig miniBossConfig,
            EnemyBase finalBoss, EnemyConfig finalBossConfig)
        {
            var config = ScriptableObject.CreateInstance<WaveConfig>();
            var so = new SerializedObject(config);
            SerializedProperty waves = so.FindProperty("_waves");
            waves.arraySize = 10;

            var crawlerOnly = new[] { (p.Crawler, p.CrawlerConfig, 1) };
            var crawlerCaster = new[] { (p.Crawler, p.CrawlerConfig, 2), (p.Caster, p.CasterConfig, 1) };
            var crawlerHusk = new[] { (p.Crawler, p.CrawlerConfig, 2), (p.Husk, p.HuskConfig, 1) };
            var everyKind = new[] { (p.Crawler, p.CrawlerConfig, 2), (p.Caster, p.CasterConfig, 1), (p.Husk, p.HuskConfig, 1) };
            var none = Array.Empty<(EnemyBase, EnemyConfig, int)>();

            SetWave(waves.GetArrayElementAtIndex(0), WaveGoal.Kill, 15, 0f, crawlerOnly, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(1), WaveGoal.Kill, 15, 0f, crawlerOnly, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(2), WaveGoal.Kill, 25, 0f, crawlerCaster, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(3), WaveGoal.Kill, 25, 0f, crawlerCaster, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(4), WaveGoal.Boss, 1, 0f, crawlerOnly, miniBoss, miniBossConfig, 4);
            SetWave(waves.GetArrayElementAtIndex(5), WaveGoal.SurviveOrKill, 40, 60f, crawlerHusk, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(6), WaveGoal.SurviveOrKill, 40, 60f, crawlerHusk, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(7), WaveGoal.Kill, 50, 0f, everyKind, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(8), WaveGoal.Kill, 50, 0f, everyKind, null, null, 0);
            SetWave(waves.GetArrayElementAtIndex(9), WaveGoal.Boss, 1, 0f, none, finalBoss, finalBossConfig, 0);

            so.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static void SetWave(SerializedProperty wave, WaveGoal goal, int killTarget, float surviveSeconds,
            IReadOnlyList<(EnemyBase prefab, EnemyConfig config, int weight)> pool, EnemyBase bossPrefab, EnemyConfig bossConfig,
            int escortCount)
        {
            wave.FindPropertyRelative("_goal").enumValueIndex = (int)goal;
            wave.FindPropertyRelative("_killTarget").intValue = killTarget;
            wave.FindPropertyRelative("_surviveSeconds").floatValue = surviveSeconds;
            wave.FindPropertyRelative("_bossPrefab").objectReferenceValue = bossPrefab;
            wave.FindPropertyRelative("_bossConfig").objectReferenceValue = bossConfig;
            wave.FindPropertyRelative("_escortCount").intValue = escortCount;
            SerializedProperty entries = wave.FindPropertyRelative("_spawnPool");
            entries.arraySize = pool.Count;
            for (int i = 0; i < pool.Count; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_prefab").objectReferenceValue = pool[i].prefab;
                entry.FindPropertyRelative("_config").objectReferenceValue = pool[i].config;
                entry.FindPropertyRelative("_weight").intValue = pool[i].weight;
            }
        }

        private static void BuildSceneContents(WaveConfig config, Material floorMat, Material wallMat, PlayerController kaelenPrefab,
            RotatingTrap trapPrefab, CombatQueryConfig queryConfig, CardPoolConfig cardPool, CardEffectConfig cardEffects)
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.4f);

            float size = config.ArenaSize;
            float half = size * 0.5f;
            float height = config.WallHeight;
            float thickness = config.WallThickness;
            CreateBox("Floor", new Vector3(0f, -FloorThickness * 0.5f, 0f), new Vector3(size, FloorThickness, size), floorMat);
            CreateBox("Wall North", new Vector3(0f, height * 0.5f, half), new Vector3(size + thickness, height, thickness), wallMat);
            CreateBox("Wall South", new Vector3(0f, height * 0.5f, -half), new Vector3(size + thickness, height, thickness), wallMat);
            CreateBox("Wall East", new Vector3(half, height * 0.5f, 0f), new Vector3(thickness, height, size + thickness), wallMat);
            CreateBox("Wall West", new Vector3(-half, height * 0.5f, 0f), new Vector3(thickness, height, size + thickness), wallMat);

            var kaelenObject = (GameObject)PrefabUtility.InstantiatePrefab(kaelenPrefab.gameObject);
            kaelenObject.transform.position = new Vector3(0f, kaelenPrefab.transform.position.y, 0f); // arena centre
            var controller = kaelenObject.GetComponent<PlayerController>();
            var health = kaelenObject.GetComponent<Health>();

            var trapObject = (GameObject)PrefabUtility.InstantiatePrefab(trapPrefab.gameObject);
            trapObject.transform.position = Vector3.zero;
            var trap = trapObject.GetComponent<RotatingTrap>();
            trap.Configure(config, health);
            PrefabUtility.RecordPrefabInstancePropertyModifications(trap);
            trapObject.SetActive(false); // WaveDirector arms it from TrapStartWave
            PrefabUtility.RecordPrefabInstancePropertyModifications(trapObject);

            var runState = new GameObject("RunState").AddComponent<RunState>();

            var directorObject = new GameObject("WaveDirector");
            var director = directorObject.AddComponent<WaveDirector>();
            director.Configure(config, runState, health, controller, trap, queryConfig);
            director.SetAutoContinueCardPick(false); // story 004: CardController continues after the pick
            directorObject.AddComponent<WaveDebugOverlay>().Configure(director, health);

            var cardObject = new GameObject("CardController");
            var screen = cardObject.AddComponent<CardPickScreen>();
            var attack = kaelenObject.GetComponent<AutoAttack>();
            var runner = cardObject.AddComponent<CardEffectRunner>();
            runner.Configure(cardEffects, director, attack, kaelenObject.GetComponent<Dash>(), health, ~0, queryConfig);
            cardObject.AddComponent<CardController>().Configure(director, cardPool, controller, attack, health, screen, runner);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<FollowCamera>();
            follow.Configure(kaelenObject.transform);
            cameraObject.transform.position = kaelenObject.transform.position + new Vector3(0f, 14f, -5f); // matches FollowCamera default offset
            cameraObject.transform.LookAt(kaelenObject.transform.position);
        }

        private static void CreateBox(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
        }

        // Arena first so the player build starts in it; SampleScene and PlayerSandbox kept; any other entry kept after them.
        private static void UpdateSceneList()
        {
            var ordered = new List<EditorBuildSettingsScene>();
            var seen = new HashSet<string>();
            foreach (string path in new[] { ScenePath, SampleScenePath, SandboxScenePath })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null && seen.Add(path))
                {
                    ordered.Add(new EditorBuildSettingsScene(path, true));
                }
            }
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (seen.Add(existing.path))
                {
                    ordered.Add(existing);
                }
            }
            EditorBuildSettings.scenes = ordered.ToArray();
        }
    }
}
