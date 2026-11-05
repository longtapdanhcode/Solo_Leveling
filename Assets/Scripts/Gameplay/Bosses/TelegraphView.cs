// Implements: production/epics/micro-arena-survivor/story-006-bosses.md (minimal red telegraph; story 007 refines the look).

using MicroArena.Gameplay.Config;
using UnityEngine;

namespace MicroArena.Gameplay.Bosses
{
    /// <summary>
    /// Draws one <see cref="Telegraph"/> as a flat red sector (a circle is a 360 degree sector) on the floor, using the
    /// telegraph's own <see cref="TelegraphShape"/> numbers. Alpha rises with the telegraph's progress.
    /// Owns its GameObject and mesh; call <see cref="Dispose"/> to remove both.
    /// </summary>
    public sealed class TelegraphView
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Telegraph _telegraph;
        private readonly BossConfig _config;
        private readonly GameObject _object;
        private readonly Mesh _mesh;
        private readonly Renderer _renderer;
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private readonly Color _color;

        /// <summary>The telegraph this view draws.</summary>
        public Telegraph Telegraph => _telegraph;

        /// <summary>Builds the view and places it on the floor.</summary>
        public TelegraphView(Telegraph telegraph, BossConfig config, Material material)
        {
            _telegraph = telegraph;
            _config = config;
            _color = material.color;
            TelegraphShape shape = telegraph.Shape;

            _mesh = BuildSectorMesh(shape.HalfAngleDegrees, config.TelegraphSegments);
            _object = new GameObject("BossTelegraph");
            _object.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = _object.AddComponent<MeshRenderer>();
            _renderer.sharedMaterial = material;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;

            Transform t = _object.transform;
            t.position = new Vector3(shape.Origin.x, config.TelegraphHeight, shape.Origin.z);
            t.rotation = Quaternion.LookRotation(shape.Direction, Vector3.up);
            t.localScale = new Vector3(shape.Radius, 1f, shape.Radius);
            Refresh();
        }

        /// <summary>Updates the alpha from the telegraph's progress.</summary>
        public void Refresh()
        {
            Color color = _color;
            color.a = Mathf.Lerp(_config.TelegraphStartAlpha, _config.TelegraphEndAlpha, _telegraph.Progress);
            _block.SetColor(BaseColorId, color);
            _renderer.SetPropertyBlock(_block);
        }

        /// <summary>Destroys the GameObject and the mesh.</summary>
        public void Dispose()
        {
            DestroyObject(_object);
            DestroyObject(_mesh);
        }

        // Unit-radius sector in the XZ plane, centred on +Z. Triangles run clockwise seen from above (front face up).
        private static Mesh BuildSectorMesh(float halfAngleDegrees, int segments)
        {
            var vertices = new Vector3[segments + 2];
            var triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;
            float start = -halfAngleDegrees * Mathf.Deg2Rad;
            float step = 2f * halfAngleDegrees * Mathf.Deg2Rad / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = start + step * i;
                vertices[i + 1] = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            }
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            var mesh = new Mesh { name = "TelegraphSector", vertices = vertices, triangles = triangles };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
