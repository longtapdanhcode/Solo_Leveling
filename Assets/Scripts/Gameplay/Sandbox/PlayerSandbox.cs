// VERIFICATION SCAFFOLDING for story-001 (production/epics/micro-arena-survivor/story-001-kaelen-core-combat.md,
// design/game-brief.md MVP 1). Not a game feature; delete or replace once the arena scene (story 003) exists.
//
// Put this on an empty GameObject in a scene. At runtime it builds a ground plane, Kaelen (capsule with all
// story-001 components), a top-down follow camera and a few TargetDummy targets, plus a debug overlay.
// Test keys: H = Kaelen takes damage (to see the revive and run end), R = reset the run.
// Story 002 (production/epics/micro-arena-survivor/story-002-enemies.md) adds: 1 = Crawler, 2 = Void Caster,
// 3 = Volatile Husk (spawned a few metres in front of Kaelen at a random side angle), 0 or C = clear all enemies.

using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Enemies;
using MicroArena.Gameplay.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MicroArena.Gameplay.Sandbox
{
    /// <summary>Builds a minimal playable test area for Kaelen at runtime.</summary>
    public sealed class PlayerSandbox : MonoBehaviour
    {
        [Tooltip("Optional. A fresh instance with the default values is created when empty.")]
        [SerializeField] private PlayerConfig _config;
        [Tooltip("Assets/InputSystem_Actions.inputactions. When empty: project-wide InputSystem.actions, then (editor only) the asset path.")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 14f, -5f);
        [SerializeField] private float _groundSize = 40f;
        [SerializeField] private Vector3[] _dummyPositions =
        {
            new Vector3(2f, 1f, 0f), new Vector3(-2f, 1f, 2f), new Vector3(0f, 1f, 6f), new Vector3(5f, 1f, 5f),
        };
        [Tooltip("Damage dealt to Kaelen by the H test key.")]
        [SerializeField] private float _testDamage = 30f;
        [Tooltip("Optional. A fresh instance with the default values is created when empty.")]
        [SerializeField] private CombatQueryConfig _queryConfig;
        [Tooltip("Optional, indexed by EnemyKind (Crawler, VoidCaster, VolatileHusk). Empty slots use EnemyConfig.CreateDefault.")]
        [SerializeField] private EnemyConfig[] _enemyConfigs = new EnemyConfig[3];
        [Tooltip("Distance (m) from Kaelen at which test enemies spawn.")]
        [SerializeField] private float _enemySpawnDistance = 6f;
        [Tooltip("Spawn direction is Kaelen's facing rotated by a random angle within plus/minus this many degrees.")]
        [SerializeField] private float _enemySpawnArcDegrees = 60f;

        private readonly List<EnemyBase> _enemies = new List<EnemyBase>();
        private readonly Material[] _enemyMaterials = new Material[3]; // one shared material per kind, reused by every spawn
        private Transform _kaelen;
        private Health _health;
        private Dash _dash;
        private AutoAttack _attack;
        private PlayerController _controller;
        private Camera _camera;
        private string _lastEvent = "-";
        private string _lastEnemyDied = "-";

        private void Start()
        {
            if (_inputActions == null)
            {
                _inputActions = InputSystem.actions; // project-wide actions, also available in player builds
            }
#if UNITY_EDITOR
            if (_inputActions == null)
            {
                _inputActions = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            }
#endif
            if (_inputActions == null)
            {
                Debug.LogError("PlayerSandbox: assign the InputSystem_Actions asset.", this);
                enabled = false;
                return;
            }
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<PlayerConfig>();
            }
            if (_queryConfig == null)
            {
                _queryConfig = ScriptableObject.CreateInstance<CombatQueryConfig>();
            }
            System.Array.Resize(ref _enemyConfigs, 3);
            for (int i = 0; i < _enemyConfigs.Length; i++)
            {
                if (_enemyConfigs[i] == null)
                {
                    _enemyConfigs[i] = EnemyConfig.CreateDefault((EnemyKind)i);
                }
            }

            BuildGround();
            BuildKaelen();
            BuildCamera();
            foreach (Vector3 position in _dummyPositions)
            {
                BuildDummy(position);
            }
        }

        private void BuildGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(_groundSize / 10f, 1f, _groundSize / 10f); // plane is 10 m per scale unit
            ground.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(0.25f, 0.27f, 0.3f));
        }

        private void BuildKaelen()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Kaelen";
            go.transform.position = new Vector3(0f, 1f, 0f);
            go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(0.2f, 0.5f, 1f));
            go.SetActive(false); // configure before Awake runs

            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "FacingMarker";
            nose.transform.SetParent(go.transform, false);
            nose.transform.localPosition = new Vector3(0f, 0.3f, 0.6f);
            nose.transform.localScale = new Vector3(0.3f, 0.3f, 0.5f);
            Destroy(nose.GetComponent<Collider>());
            nose.GetComponent<Renderer>().sharedMaterial = MakeMaterial(Color.white);

            _health = go.AddComponent<Health>();
            _dash = go.AddComponent<Dash>();
            _attack = go.AddComponent<AutoAttack>();
            _controller = go.AddComponent<PlayerController>();
            var hitStop = go.AddComponent<HitStop>();

            LayerMask everything = ~0;
            _health.Configure(_config, everything, _queryConfig);
            _attack.Configure(_config, _controller, _health, hitStop, everything, _queryConfig);
            _dash.Configure(_config, _inputActions, _controller, _health);
            _controller.Configure(_config, _inputActions, _health, _dash, _attack);

            _health.OnRevived += () => _lastEvent = "Revived";
            _health.OnRunEnded += () => _lastEvent = "Run ended";
            _attack.OnHitLanded += (target, info) => _lastEvent = $"Hit {info.Amount}{(info.IsCritical ? " CRIT" : "")}";

            go.SetActive(true);
            _kaelen = go.transform;
        }

        private void BuildCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                _camera = cameraObject.AddComponent<Camera>();
            }
            if (FindAnyObjectByType<Light>() == null)
            {
                var lightObject = new GameObject("Directional Light");
                lightObject.AddComponent<Light>().type = LightType.Directional;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        private void BuildDummy(Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Dummy";
            go.transform.position = position;
            go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(0.8f, 0.7f, 0.2f));
            go.AddComponent<TargetDummy>();
        }

        // Enemies stay on the Default layer, which the "everything" masks given to AutoAttack and Health include.
        private void SpawnEnemy(EnemyKind kind)
        {
            PrimitiveType shape;
            Vector3 scale;
            Color color;
            switch (kind)
            {
                case EnemyKind.Crawler:
                    shape = PrimitiveType.Cube;
                    scale = new Vector3(0.9f, 0.6f, 0.9f);
                    color = new Color(0.45f, 0.75f, 0.2f);
                    break;
                case EnemyKind.VoidCaster:
                    shape = PrimitiveType.Capsule;
                    scale = new Vector3(0.8f, 1f, 0.8f);
                    color = new Color(0.6f, 0.2f, 0.9f);
                    break;
                default:
                    shape = PrimitiveType.Sphere;
                    scale = Vector3.one * 0.8f;
                    color = new Color(1f, 0.55f, 0.1f);
                    break;
            }

            Vector3 direction = Quaternion.AngleAxis(Random.Range(-_enemySpawnArcDegrees, _enemySpawnArcDegrees), Vector3.up) * _kaelen.forward;
            Vector3 position = _kaelen.position + direction * _enemySpawnDistance;
            position.y = shape == PrimitiveType.Capsule ? scale.y : scale.y * 0.5f; // rest on the ground (capsule is 2 units tall)

            GameObject go = GameObject.CreatePrimitive(shape);
            go.name = kind.ToString();
            go.transform.position = position;
            go.transform.localScale = scale;
            if (_enemyMaterials[(int)kind] == null)
            {
                _enemyMaterials[(int)kind] = MakeMaterial(color);
            }
            go.GetComponent<Renderer>().sharedMaterial = _enemyMaterials[(int)kind];
            go.SetActive(false); // configure before Awake runs

            EnemyBase enemy;
            switch (kind)
            {
                case EnemyKind.Crawler:
                    enemy = go.AddComponent<Crawler>();
                    break;
                case EnemyKind.VoidCaster:
                    enemy = go.AddComponent<VoidCaster>();
                    break;
                default:
                    enemy = go.AddComponent<VolatileHusk>();
                    break;
            }
            enemy.Configure(_enemyConfigs[(int)kind], _health, _queryConfig);
            enemy.OnDied += died =>
            {
                _enemies.Remove(enemy);
                _lastEnemyDied = $"{died.Kind} at ({died.Position.x:0.0}, {died.Position.z:0.0})";
            };
            _enemies.Add(enemy);
            go.SetActive(true);
        }

        private void ClearEnemies()
        {
            foreach (EnemyBase enemy in _enemies)
            {
                Destroy(enemy.gameObject);
            }
            _enemies.Clear();
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            return new Material(shader) { color = color };
        }

        private void LateUpdate()
        {
            if (_kaelen == null)
            {
                return;
            }
            _camera.transform.position = _kaelen.position + _cameraOffset;
            _camera.transform.LookAt(_kaelen.position);
        }

        private void Update()
        {
            if (_kaelen == null || Keyboard.current == null)
            {
                return;
            }
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                _health.TakeDamage(new DamageInfo { Amount = _testDamage });
            }
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                _controller.ResetForNewRun();
                _lastEvent = "Run reset";
            }
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SpawnEnemy(EnemyKind.Crawler);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SpawnEnemy(EnemyKind.VoidCaster);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                SpawnEnemy(EnemyKind.VolatileHusk);
            }
            if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame)
            {
                ClearEnemies();
            }
        }

        private void OnGUI()
        {
            if (_kaelen == null)
            {
                return;
            }
            GUILayout.BeginArea(new Rect(10f, 10f, 520f, 260f));
            GUILayout.Label($"HP {_health.Current:0}/{_health.Max:0}   invulnerable: {_health.IsInvulnerable}   revive left: {_health.ReviveAvailable}   alive: {_health.IsAlive}");
            GUILayout.Label($"Dash charges {_dash.Charges}/{_dash.MaxCharges}   next: {_dash.RechargeProgress:P0}   dashing: {_dash.IsDashing}");
            GUILayout.Label($"Combo step {_attack.CurrentStep + 1}   state: {_attack.State}");
            GUILayout.Label($"Last event: {_lastEvent}");
            GUILayout.Label($"Enemies alive: {_enemies.Count}   last EnemyDied: {_lastEnemyDied}");
            GUILayout.Label("WASD move, Space dash, H hurt self, R reset run");
            GUILayout.Label("1 Crawler, 2 Void Caster, 3 Volatile Husk, 0 / C clear enemies");
            GUILayout.EndArea();
        }
    }
}
