# Unity 6.3 LTS — Deprecated APIs

**Last verified:** 2026-02-13

Quick lookup table for deprecated APIs and their replacements.
Format: **Don't use X** → **Use Y instead**

---

## Input

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Input.GetKey()` | `Keyboard.current[Key.X].isPressed` | New Input System |
| `Input.GetKeyDown()` | `Keyboard.current[Key.X].wasPressedThisFrame` | New Input System |
| `Input.GetMouseButton()` | `Mouse.current.leftButton.isPressed` | New Input System |
| `Input.GetAxis()` | `InputAction` callbacks | New Input System |
| `Input.mousePosition` | `Mouse.current.position.ReadValue()` | New Input System |

**Migration:** Install `com.unity.inputsystem` package.

---

## UI

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Canvas` (UGUI) | `UIDocument` (UI Toolkit) | UI Toolkit is now production-ready |
| `Text` component | `TextMeshPro` or UI Toolkit `Label` | Better rendering, fewer draw calls |
| `Image` component | UI Toolkit `VisualElement` with background | More flexible styling |

**Migration:** UGUI still works, but UI Toolkit is recommended for new projects.

---

## DOTS/Entities

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `ComponentSystem` | `ISystem` (unmanaged) | Entities 1.0+ complete rewrite |
| `JobComponentSystem` | `ISystem` with `IJobEntity` | Burst-compatible |
| `GameObjectEntity` | Pure ECS workflow | No GameObject conversion |
| `EntityManager.CreateEntity()` (old signature) | `EntityManager.CreateEntity(EntityArchetype)` | Explicit archetype |
| `ComponentDataFromEntity<T>` | `ComponentLookup<T>` | Entities 1.0+ rename |

**Migration:** See Entities package migration guide. Major refactor required.

---

## Rendering

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `CommandBuffer.DrawMesh()` | RenderGraph API | URP/HDRP render passes |
| `OnPreRender()` / `OnPostRender()` | `RenderPipelineManager` callbacks | SRP compatibility |
| `Camera.SetReplacementShader()` | Custom render pass | Not supported in SRP |

---

## Physics

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Physics.RaycastAll()` | `Physics.RaycastNonAlloc()` | Avoid GC allocations |
| `Rigidbody.velocity` (direct write) | `Rigidbody.AddForce()` | Better physics stability |

---

## Asset Loading

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Resources.Load()` | Addressables | Better memory control, async loading |
| Synchronous asset loading | `Addressables.LoadAssetAsync()` | Non-blocking |

---

## Animation

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| Legacy Animation component | Animator Controller | Mecanim system |
| `Animation.Play()` | `Animator.Play()` | State machine control |

---

## Particles

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| Legacy Particle System | Visual Effect Graph | GPU-accelerated, more performant |

---

## Scripting

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `WWW` class | `UnityWebRequest` | Modern async networking |
| `Application.LoadLevel()` | `SceneManager.LoadScene()` | Scene management |

---

## Platform-Specific

### WebGL
| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| WebGL 1.0 | WebGL 2.0 or WebGPU | Unity 6+ defaults to WebGPU |

---

## Quick Migration Patterns

### Input Example
```csharp
// ❌ Deprecated
if (Input.GetKeyDown(KeyCode.Space)) {
    Jump();
}

// ✅ New Input System
using UnityEngine.InputSystem;
if (Keyboard.current.spaceKey.wasPressedThisFrame) {
    Jump();
}
```

### Asset Loading Example
```csharp
// ❌ Deprecated
var prefab = Resources.Load<GameObject>("Enemies/Goblin");

// ✅ Addressables
var handle = Addressables.LoadAssetAsync<GameObject>("Enemies/Goblin");
await handle.Task;
var prefab = handle.Result;
```

### UI Example
```csharp
// ❌ Deprecated (UGUI)
GetComponent<Text>().text = "Score: 100";

// ✅ TextMeshPro
GetComponent<TextMeshProUGUI>().text = "Score: 100";

// ✅ UI Toolkit
rootVisualElement.Q<Label>("score-label").text = "Score: 100";
```

---

## 6.3 → 6.6

**Last verified:** 2026-11-02. Sourced from the 6.4, 6.5 and 6.6 upgrade guides
(see breaking-changes.md `## 6.3 → 6.6` for context and URLs).

| Don't use | Use instead | Since |
|-----------|-------------|-------|
| `go.GetInstanceID()` and other `InstanceID` APIs | `go.GetEntityId()` / `EntityId` (8 bytes, never store in `int`) | 6.5 |
| `GameObject.rigidbody`, `.camera`, `.light`, `.renderer`, `.audio`, `.collider` and the other shortcut properties | `GetComponent<T>()` | 6.5 |
| `GameObject.AddComponent(string)` | `GameObject.AddComponent<T>()` | 6.5 |
| `LightShadowCasterMode.NonLightmappedOnly` / `.Everything` | `ShadowMask` / `DistanceShadowMask` | 6.4 |
| URP Compatibility Mode, `URP_COMPATIBILITY_MODE` | Render graph system | 6.4 |
| `LowLevelPhysics2D` (`UnityEngine.LowLevelPhysics2D`) | `PhysicsCore2D` (`Unity.U2D.Physics`) | 6.5 |
| `#if UNITY_64` | Runtime `IntPtr.Size` check | 6.6 (removed in 6.8) |
| `#if DEVELOPMENT_BUILD`, `[Conditional("DEVELOPMENT_BUILD")]` | `UNITY_ENABLE_CHECKS`, `UNITY_INCLUDE_INSTRUMENTATION`, or `Debug.isDebugBuild` | 6.6 (removed in 6.8) |
| `BuildOptions.ForceEnableAssertions` | `PlayerSettings.SetManagedCodeVariant(target, ManagedCodeVariant.Checked)` | 6.6 |
| Legacy UXML Factory/Traits | `UxmlElement` / `UxmlAttribute` | 6.6 |
| `UIToolkitInputConfiguration.SetRuntimeInputBackend` | Remove the call (no effect) | 6.6 |
| Built-In Render Pipeline | URP | deprecated 6.5 |
| Dynamic batching | Methods in "Choose a method for optimizing calls" | obsolete 6.6 |

**Sources:**
- https://docs.unity.com/en-us/engine/6000.6/manual/upgrade-guides/upgrade-guide-unity64
- https://docs.unity.com/en-us/engine/6000.6/manual/upgrade-guides/upgrade-guide-unity65
- https://docs.unity.com/en-us/engine/6000.6/manual/upgrade-guides/upgrade-guide-unity66
- https://docs.unity3d.com/6000.0/Documentation/Manual/deprecated-features.html
- https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/Migration.html
