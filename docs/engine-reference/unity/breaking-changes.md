# Unity 6.3 LTS — Breaking Changes

**Last verified:** 2026-02-13

This document tracks breaking API changes and behavioral differences between Unity 2022 LTS
(likely in model training) and Unity 6.3 LTS (current version). Organized by risk level.

## HIGH RISK — Will Break Existing Code

### Entities/DOTS API Complete Overhaul
**Versions:** Entities 1.0+ (Unity 6.0+)

```csharp
// ❌ OLD (pre-Unity 6, GameObjectEntity pattern)
public class HealthComponent : ComponentData {
    public float Value;
}

// ✅ NEW (Unity 6+, IComponentData)
public struct HealthComponent : IComponentData {
    public float Value;
}

// ❌ OLD: ComponentSystem
public class DamageSystem : ComponentSystem { }

// ✅ NEW: ISystem (unmanaged, Burst-compatible)
public partial struct DamageSystem : ISystem {
    public void OnCreate(ref SystemState state) { }
    public void OnUpdate(ref SystemState state) { }
}
```

**Migration:** Follow Unity's ECS migration guide. Major architectural changes required.

---

### Input System — Legacy Input Deprecated
**Versions:** Unity 6.0+

```csharp
// ❌ OLD: Input class (deprecated)
if (Input.GetKeyDown(KeyCode.Space)) { }

// ✅ NEW: Input System package
using UnityEngine.InputSystem;
if (Keyboard.current.spaceKey.wasPressedThisFrame) { }
```

**Migration:** Install Input System package, replace all `Input.*` calls with new API.

---

### URP/HDRP Renderer Feature API Changes
**Versions:** Unity 6.0+

```csharp
// ❌ OLD: ScriptableRenderPass.Execute signature
public override void Execute(ScriptableRenderContext context, ref RenderingData data)

// ✅ NEW: Uses RenderGraph API
public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
```

**Migration:** Update custom render passes to use RenderGraph API.

---

## MEDIUM RISK — Behavioral Changes

### Addressables — Asset Loading Returns
**Versions:** Unity 6.2+

Asset loading failures now throw exceptions by default instead of returning null.
Add proper exception handling or use `TryLoad` variants.

```csharp
// ❌ OLD: Silent null on failure
var handle = Addressables.LoadAssetAsync<Sprite>("key");
var sprite = handle.Result; // null if failed

// ✅ NEW: Throws on failure, use try/catch or TryLoad
try {
    var handle = Addressables.LoadAssetAsync<Sprite>("key");
    var sprite = await handle.Task;
} catch (Exception e) {
    Debug.LogError($"Failed to load: {e}");
}
```

---

### Physics — Default Solver Iterations Changed
**Versions:** Unity 6.0+

Default solver iterations increased for better stability.
Check `Physics.defaultSolverIterations` if you rely on old behavior.

---

## LOW RISK — Deprecations (Still Functional)

### UGUI (Legacy UI)
**Status:** Deprecated but supported
**Replacement:** UI Toolkit

UGUI still works but UI Toolkit is recommended for new projects.

---

### Legacy Particle System
**Status:** Deprecated
**Replacement:** Visual Effect Graph (VFX Graph)

---

### Old Animation System
**Status:** Deprecated
**Replacement:** Animator Controller (Mecanim)

---

## Platform-Specific Breaking Changes

### WebGL
- **Unity 6.0+**: WebGPU is now the default (WebGL 2.0 fallback available)
- Update shaders for WebGPU compatibility

### Android
- **Unity 6.0+**: Minimum API level raised to 24 (Android 7.0)

### iOS
- **Unity 6.0+**: Minimum deployment target raised to iOS 13

---

## Migration Checklist

When upgrading from 2022 LTS to Unity 6.3 LTS:

- [ ] Audit all DOTS/ECS code (complete rewrite likely needed)
- [ ] Replace `Input` class with Input System package
- [ ] Update custom render passes to RenderGraph API
- [ ] Add exception handling to Addressables calls
- [ ] Test physics behavior (solver iterations changed)
- [ ] Consider migrating UGUI to UI Toolkit for new UI
- [ ] Update WebGL shaders for WebGPU
- [ ] Verify minimum platform versions (Android/iOS)

---

## 6.3 → 6.6

**Last verified:** 2026-11-02. Every line below comes from Unity's upgrade guides
fetched that day (links under Sources). Anything a guide does not state is marked
NOT SOURCEABLE; do not fill it from memory.

### 6.4 (from 6.3)
- **URP Compatibility Mode removed** (custom render passes) and the `URP_COMPATIBILITY_MODE` define; use the render graph system.
- `IVolume` removed; use `VolumeGizmoDrawer`.
- `LightShadowCasterMode.NonLightmappedOnly` → `ShadowMask`, `Everything` → `DistanceShadowMask` (scripts auto-update).
- `Object.Destroy` now runs `OnDisable` across the whole descendant hierarchy (previously only the object and direct children); remove manual propagation workarounds to avoid duplicate calls.
- Asset Database: dependents reimport only when a dependency's import artifact changes; declare dependencies with `AssetImportContext.DependsOnSourceAsset` / `DependsOnArtifact`.
- SRP Core's dependency on `com.unity.modules.physics` is now soft.
- NOT SOURCEABLE: any `EntityId`/`InstanceID` change in the 6.4 guide (the page does not mention them).

### 6.5 (from 6.4)
- **`InstanceID` APIs obsolete → `EntityId`** (`go.GetInstanceID()` → `go.GetEntityId()`). `EntityId` is **8 bytes**: do not store it in an `int`, do not parse its string with `int.Parse`, do not use `EntityId.GetHashCode()` as a dictionary key.
- Removed `GameObject`/`Component` shortcut properties: `rigidbody`, `rigidbody2D`, `camera`, `light`, `animation`, `constantForce`, `renderer`, `audio`, `collider`, `collider2d`, `hingeJoint`, `particleSystem`, `networkView` → use `GetComponent<T>()`. `GameObject.AddComponent(string)` → `AddComponent<T>()`.
- Entities: `Entities.ForEach`, `Job.WithCode`, `IAspect` removed → `IJobEntity`, `SystemAPI.Query`, `IJob`.
- `com.unity.ugui` is no longer a dependency of Render Pipeline Core; add it explicitly if relied on indirectly.
- Legacy Render Graph API obsolete (URP/HDRP with native render pass support unaffected). Built-In Render Pipeline deprecated (supported through the 6.7 LTS lifecycle).
- `LowLevelPhysics2D` → `PhysicsCore2D`, namespace `Unity.U2D.Physics` (2D only).
- Editor log is project-local by default (`ProjectName/Logs/Editor.log`); CI that collects the old global path must change.
- Base Class Library: Unity ships `System.Text.Json`, `System.Collections.Immutable`, `System.Reflection.Metadata`, `System.Runtime.CompilerServices.Unsafe`; user-supplied copies are dropped from builds.

### 6.6 (from 6.5)
- **`UNITY_64` and `DEVELOPMENT_BUILD` deprecated** (warnings `UAC0008` / `UAC0009`; errors under warnings-as-errors); the guide states both are fully removed in Unity 6.8. Use `UNITY_ENABLE_CHECKS`, `UNITY_INCLUDE_INSTRUMENTATION`, `Debug.isDebugBuild`, or a runtime `IntPtr.Size` check. New per-platform **Managed Code Variant** setting defaults to Release (neither define set).
- `BuildOptions.ForceEnableAssertions` obsolete → `PlayerSettings.SetManagedCodeVariant(..., ManagedCodeVariant.Checked)`. `CoreBuildData.developmentBuild` obsolete and always `false`.
- **Play Mode: domain reload is off by default for new projects** (Enter Play Mode Settings = Reload Scene only), so static state is no longer reset between runs. Whether existing projects are affected: NOT SOURCEABLE (the what's-new page says new projects).
- **Cinemachine 3 is a core package**; Cinemachine 2 projects auto-upgrade (different API and data format).
- `[SerializeField]` on `Dictionary<TKey, TValue>` is now serialized by Unity; existing wrapper types keep working, migrating a field is manual.
- Dynamic batching obsolete. Progressive CPU Lightmapper deprecated (use Unity Compute Light Baker or Progressive GPU).
- Render Pipeline Core no longer depends on `com.unity.modules.terrain`; add the Terrain built-in package if Terrain is relied on.
- Legacy Rendering Debugger `DebugState` types and legacy `DebugUIDrawer` removed.
- UI Toolkit: `DefaultEventSystem.LegacyInputProcessor` removed, `UIToolkitInputConfiguration.SetRuntimeInputBackend` now a compile error; Legacy UXML Factory/Traits removed → `UxmlElement` / `UxmlAttribute`.
- Mesh Read/Write is no longer enabled at build time; builds fail when a Particle System Shape module, Terrain Detail Mesh or Mesh Collider needs a readable mesh that has it disabled.
- YAML word wrapping disabled; "Reduce Version Control Noise" removed (expect unrelated YAML diffs).
- Input System, URP and built-in physics: **NOT SOURCEABLE**, the 6.6 upgrade guide does not mention them.

**Sources:**
- https://docs.unity.com/en-us/engine/6000.6/manual/upgrade-guides/upgrade-guide-unity64
- https://docs.unity.com/en-us/engine/6000.6/manual/upgrade-guides/upgrade-guide-unity65
- https://docs.unity.com/en-us/engine/6000.6/manual/upgrade-guides/upgrade-guide-unity66
- https://docs.unity.com/en-us/engine/6000.6/manual/whats-new/unity66
- https://docs.unity3d.com/6000.0/Documentation/Manual/upgrade-guides.html
- https://docs.unity3d.com/Packages/com.unity.entities@1.3/manual/upgrade-guide.html
