// TEMPORARY batchmode rule check for story 004 (cards, modifiers, GamePause, -arenaForceCards).
// Kept in tools/card-rule-check/ (outside Assets/, never compiled); see the README there to copy it in, run it, remove it.
//   Unity -batchmode -quit -projectPath <project> -executeMethod MicroArena.Gameplay.Editor.CardRuleCheck.Run

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Cards;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MicroArena.Gameplay.Editor
{
    /// <summary>Batchmode rule check for story 004. Prints PASS/FAIL lines and exits 1 on any failure.</summary>
    public static class CardRuleCheck
    {
        private const float Threshold = 0.3f;
        private static int _failures;
        private static readonly List<Object> Created = new List<Object>();

        /// <summary>Runs every check, prints PASS/FAIL lines, exits 1 on any failure.</summary>
        public static void Run()
        {
            _failures = 0;
            try
            {
                CheckSwingRadius();
                CheckCommonStacking();
                CheckRepeats();
                CheckRage();
                CheckCrit();
                CheckBurn();
                CheckChain();
                CheckPool();
                CheckForced();
                CheckPause();
                CheckReset();
            }
            catch (Exception e)
            {
                Fail($"exception: {e}");
            }
            foreach (Object o in Created)
            {
                if (o != null)
                {
                    Object.DestroyImmediate(o);
                }
            }
            Debug.Log(_failures == 0 ? "CardRuleCheck: ALL PASS" : $"CardRuleCheck: {_failures} FAILURE(S)");
            EditorApplication.Exit(_failures == 0 ? 0 : 1);
        }

        private static void CheckSwingRadius()
        {
            var stack = new ModifierStack();
            stack.Apply(Card("vong_xoay", CardRarity.Legendary, CardEffectKind.BloodVortex, percent: 30f, healPct: 4f));
            Near("Vong Xoay: SwingRadius(3) == 3.9", stack.SwingRadius(3f), 3.9f);
            Near("Vong Xoay: DetectionRange(3) == 3", stack.DetectionRange(3f), 3f);
        }

        private static void CheckCommonStacking()
        {
            var stack = new ModifierStack();
            CardConfig dmg = Card("d", CardRarity.Common, CardEffectKind.DamagePercent, percent: 15f);
            stack.Apply(dmg);
            stack.Apply(dmg);
            stack.Apply(Card("s", CardRarity.Common, CardEffectKind.MoveSpeedPercent, percent: 12f));
            stack.Apply(Card("h", CardRarity.Common, CardEffectKind.HealAndMaxHealth, heal: 30f, maxHp: 20f));
            Near("commons: Suc Manh Tho x2 -> damage x1.30", stack.DamageMultiplier, 1.3f);
            Near("commons: Giay Bao -> move x1.12", stack.MoveSpeedMultiplier, 1.12f);
            Near("commons: Huyet Nhu -> max HP +20", stack.MaxHealthBonus, 20f);
            Check("commons: StackCount(d) == 2", stack.StackCount("d") == 2);
        }

        private static void CheckRepeats()
        {
            var stack = new ModifierStack();
            stack.SetRageThreshold(Threshold);
            CardConfig burn = Card("hoa_nguc", CardRarity.Rare, CardEffectKind.Burn, amount: 10f);
            CardConfig chain = Card("loi_chan", CardRarity.Rare, CardEffectKind.ChainLightning, amount: 25f);
            CardConfig decoy = Card("phan_than", CardRarity.Rare, CardEffectKind.Decoy, amount: 1.5f);
            CardConfig vortex = Card("vong_xoay", CardRarity.Legendary, CardEffectKind.BloodVortex, percent: 30f, healPct: 4f);
            CardConfig meteor = Card("sao_bang", CardRarity.Legendary, CardEffectKind.Meteor, amount: 80f);
            CardConfig rage = Card("cuong_no", CardRarity.Legendary, CardEffectKind.VoidRage, percent: 100f, crit: 0.5f);
            foreach (CardConfig card in new[] { burn, chain, decoy, vortex, meteor, rage })
            {
                stack.Apply(card);
                stack.Apply(card);
            }
            Near("repeat: Hoa Nguc x2 -> 20 DMG/s per stack", stack.BurnDamagePerSecond, 20f);
            Near("repeat: Loi Chan x2 -> 50 chain DMG", stack.ChainDamage, 50f);
            Near("repeat: Phan Than x2 -> 3 s decoy", stack.DecoySeconds, 3f);
            Near("repeat: Vong Xoay x2 -> SwingRadius(3) = 4.8", stack.SwingRadius(3f), 4.8f);
            Near("repeat: Vong Xoay x2 -> detection still 3", stack.DetectionRange(3f), 3f);
            Near("repeat: Vong Xoay x2 -> 8% heal per kill (KillHeal(120) = 9.6)", stack.KillHeal(120f), 9.6f);
            Near("repeat: Sao Bang x2 -> 160 meteor DMG", stack.MeteorDamage, 160f);
            Near("repeat: Cuong No x2 below 30% -> damage x3", stack.DamageMultiplierAt(0.2f), 3f);
            Near("repeat: Cuong No x2 below 30% -> crit 0.05 + 1.0 clamps to 1", stack.CritChanceAt(0.05f, 0.2f), 1f);
            Check("repeat: StackCount(sao_bang) == 2", stack.StackCount("sao_bang") == 2);
        }

        private static void CheckRage()
        {
            var stack = new ModifierStack();
            stack.SetRageThreshold(Threshold);
            Near("rage: not owned, HP 10% -> damage x1", stack.DamageMultiplierAt(0.1f), 1f);
            Near("rage: not owned, HP 10% -> crit 0.05", stack.CritChanceAt(0.05f, 0.1f), 0.05f);
            stack.Apply(Card("cuong_no", CardRarity.Legendary, CardEffectKind.VoidRage, percent: 100f, crit: 0.5f));
            Near("rage: HP 50% -> damage x1", stack.DamageMultiplierAt(0.5f), 1f);
            Near("rage: HP exactly 30% -> damage x1 (strictly below)", stack.DamageMultiplierAt(0.3f), 1f);
            Near("rage: HP 29% -> damage x2", stack.DamageMultiplierAt(0.29f), 2f);
            Near("rage: HP 50% -> crit 0.05", stack.CritChanceAt(0.05f, 0.5f), 0.05f);
            Near("rage: HP 29% -> crit 0.05 + 0.50 = 0.55", stack.CritChanceAt(0.05f, 0.29f), 0.55f);
            stack.Apply(Card("d", CardRarity.Common, CardEffectKind.DamagePercent, percent: 15f));
            Near("rage: additive with Suc Manh Tho below 30% -> x2.15", stack.DamageMultiplierAt(0.29f), 2.15f);
            Near("rage: Suc Manh Tho alone above 30% -> x1.15", stack.DamageMultiplierAt(0.9f), 1.15f);
        }

        private static void CheckCrit()
        {
            var stack = new ModifierStack();
            Near("crit: no bonus -> 0.05", stack.CritChance(0.05f), 0.05f);
            stack.AddCritChanceBonus(0.5f);
            Near("crit: 0.05 + 0.50 -> 0.55", stack.CritChance(0.05f), 0.55f);
            stack.AddCritChanceBonus(0.5f);
            Near("crit: 0.05 + 1.00 clamps to 1", stack.CritChance(0.05f), 1f);
        }

        private static void CheckBurn()
        {
            var burn = new BurnStatus();
            burn.AddStack(3, 3f);
            Near("burn: 1 stack, 1 s at 10 DMG/s -> 10", burn.Tick(1f, 10f), 10f);
            burn.AddStack(3, 3f);
            Check("burn: 2nd hit -> 2 stacks", burn.Stacks == 2);
            Near("burn: 2nd hit refreshes timer to 3 s", burn.Remaining, 3f);
            burn.AddStack(3, 3f);
            burn.AddStack(3, 3f);
            Check("burn: 4 hits capped at 3 stacks", burn.Stacks == 3);
            Near("burn: 3 stacks, 0.5 s -> 15 (max 30 DMG/s)", burn.Tick(0.5f, 10f), 15f);
            Near("burn: remaining 2.5 s, tick 5 s only burns 2.5 s -> 75", burn.Tick(5f, 10f), 75f);
            Check("burn: expired -> inactive, 0 stacks", !burn.IsActive && burn.Stacks == 0);
            Near("burn: expired tick -> 0", burn.Tick(1f, 10f), 0f);
        }

        private static void CheckChain()
        {
            Fake source = MakeFake(0f, true);
            Fake a = MakeFake(1f, true);
            Fake b = MakeFake(2f, true);
            Fake c = MakeFake(3f, true);
            Fake d = MakeFake(3.5f, true);
            Fake far = MakeFake(5f, true);
            Fake dead = MakeFake(0.5f, false);
            var candidates = new List<IDamageable> { far, d, source, c, dead, b, a };
            var results = new List<IDamageable>();
            ChainTargets.Select(source, source.Transform.position, candidates, 4f, 3, results);
            Check("chain: picks the 3 nearest others in order (1 m, 2 m, 3 m)",
                results.Count == 3 && results[0] == a && results[1] == b && results[2] == c);
            Check("chain: excludes the source", !results.Contains(source));
            Check("chain: excludes dead and out-of-range (5 m) enemies", !results.Contains(dead) && !results.Contains(far));
            ChainTargets.Select(source, source.Transform.position, new List<IDamageable> { source, far, a }, 4f, 3, results);
            Check("chain: fewer in range -> only those (1)", results.Count == 1 && results[0] == a);
        }

        private static void CheckPool()
        {
            CardConfig a = Card("a", CardRarity.Common, CardEffectKind.DamagePercent, percent: 15f);
            CardConfig b = Card("b", CardRarity.Common, CardEffectKind.MoveSpeedPercent, percent: 12f);
            CardConfig c = Card("c", CardRarity.Common, CardEffectKind.HealAndMaxHealth, heal: 30f, maxHp: 20f);
            CardPoolConfig commons = Pool(a, b, c);
            bool allThree = true;
            for (int seed = 0; seed < 200; seed++)
            {
                List<CardConfig> drawn = CardPool.Draw(commons, new ModifierStack(), Sequence(seed));
                allThree &= drawn.Count == 3 && drawn.Contains(a) && drawn.Contains(b) && drawn.Contains(c);
            }
            Check("pool: only commons -> all 3 offered (200 draws)", allThree);

            CardPoolConfig full = FullPool();
            bool distinct = true;
            for (int seed = 0; seed < 300; seed++)
            {
                List<CardConfig> drawn = CardPool.Draw(full, new ModifierStack(), Sequence(seed));
                distinct &= drawn.Count == 3 && new HashSet<CardConfig>(drawn).Count == 3;
            }
            Check("pool: 9 cards -> 3 distinct (300 draws)", distinct);
            List<CardConfig> rare = CardPool.Draw(full, null, Constant(0.65f, 0f));
            Check("pool: roll 0.65 picks a Rare first", rare.Count > 0 && rare[0].Rarity == CardRarity.Rare);
            List<CardConfig> legend = CardPool.Draw(full, null, Constant(0.95f, 0f));
            Check("pool: roll 0.95 picks a Legendary first", legend.Count > 0 && legend[0].Rarity == CardRarity.Legendary);

            CardConfig capped = Card("cap", CardRarity.Common, CardEffectKind.DamagePercent, percent: 15f, maxStacks: 1);
            CardPoolConfig withCap = Pool(capped, b, c);
            var stack = new ModifierStack();
            stack.Apply(capped);
            bool excluded = true;
            for (int seed = 0; seed < 50; seed++)
            {
                List<CardConfig> drawn = CardPool.Draw(withCap, stack, Sequence(seed));
                excluded &= !drawn.Contains(capped) && drawn.Count == 2;
            }
            Check("pool: card at MaxStacks is excluded (2 offered)", excluded);
        }

        private static void CheckForced()
        {
            const string arg = "-arenaForceCards";
            List<string> ids = ForcedCards.ParseIds(new[] { "Unity", arg, "vong_xoay, cuong_no,,sao_bang", "-other" }, arg);
            Check("force: parse 'vong_xoay, cuong_no,,sao_bang' -> 3 trimmed ids",
                ids.Count == 3 && ids[0] == "vong_xoay" && ids[1] == "cuong_no" && ids[2] == "sao_bang");
            Check("force: argument absent -> none", ForcedCards.ParseIds(new[] { "Unity", "-batchmode" }, arg).Count == 0);
            Check("force: argument last (no value) -> none", ForcedCards.ParseIds(new[] { "Unity", arg }, arg).Count == 0);
            Check("force: next token is an option -> none", ForcedCards.ParseIds(new[] { arg, "-arenaStartWave", "3" }, arg).Count == 0);

            CardPoolConfig full = FullPool();
            var warnings = new List<string>();
            List<CardConfig> resolved = ForcedCards.Resolve(full, new List<string> { "nope", "sao_bang", "sao_bang", "loi_chan" }, warnings.Add);
            Check("force: unknown id skipped with one warning", warnings.Count == 1 && warnings[0].Contains("nope"));
            Check("force: duplicates dropped, order kept (sao_bang, loi_chan)",
                resolved.Count == 2 && resolved[0].Id == "sao_bang" && resolved[1].Id == "loi_chan");

            List<CardConfig> drawn = CardPool.Draw(full, new ModifierStack(), Sequence(7), ForcedCards.Eligible(resolved, null, 3));
            Check("force: 2 forced shown first, 1 normal fill, all distinct",
                drawn.Count == 3 && drawn[0].Id == "sao_bang" && drawn[1].Id == "loi_chan" && new HashSet<CardConfig>(drawn).Count == 3);

            List<CardConfig> many = ForcedCards.Resolve(full, new List<string> { "hoa_nguc", "loi_chan", "phan_than", "vong_xoay" }, null);
            List<CardConfig> eligible = ForcedCards.Eligible(many, null, 3);
            Check("force: more than CardsPerPick -> first 3", eligible.Count == 3 && eligible[2].Id == "phan_than");
            List<CardConfig> exact = CardPool.Draw(full, null, Sequence(1), eligible);
            Check("force: 3 forced -> exactly those 3", exact.Count == 3 && exact[0].Id == "hoa_nguc" && exact[2].Id == "phan_than");

            CardConfig cap = Card("capped", CardRarity.Rare, CardEffectKind.Burn, amount: 10f, maxStacks: 1);
            var stack = new ModifierStack();
            stack.Apply(cap);
            List<CardConfig> notCapped = ForcedCards.Eligible(new List<CardConfig> { cap, many[0] }, stack, 3);
            Check("force: capped forced card is skipped", notCapped.Count == 1 && notCapped[0].Id == "hoa_nguc");
        }

        private static void CheckPause()
        {
            var owner = new object();
            var other = new object();
            GamePause.Reset();
            Near("pause: Reset -> timeScale 1", Time.timeScale, 1f);
            GamePause.Acquire(owner);
            GamePause.SetHitStop(true);
            GamePause.SetHitStop(false);
            Near("pause: hit-stop ends while paused -> timeScale stays 0", Time.timeScale, 0f);
            GamePause.Release(owner);
            Near("pause: Release -> timeScale 1", Time.timeScale, 1f);
            GamePause.Acquire(owner);
            GamePause.Acquire(owner);
            GamePause.Release(owner);
            Near("pause: Acquire twice + Release once -> timeScale 1 (idempotent)", Time.timeScale, 1f);
            GamePause.Release(other);
            Near("pause: Release of unknown owner -> timeScale 1", Time.timeScale, 1f);
            GamePause.Reset();
        }

        private static void CheckReset()
        {
            var stack = new ModifierStack();
            stack.SetRageThreshold(Threshold);
            foreach (CardConfig card in FullPool().Cards)
            {
                stack.Apply(card);
            }
            stack.Reset();
            Check("reset: every multiplier back to 1",
                stack.DamageMultiplierAt(0.1f) == 1f && stack.MoveSpeedMultiplier == 1f && stack.SwingRadiusMultiplier == 1f);
            Check("reset: every card number back to 0",
                stack.MaxHealthBonus == 0f && stack.BurnDamagePerSecond == 0f && stack.ChainDamage == 0f && stack.DecoySeconds == 0f
                && stack.KillHealPercent == 0f && stack.MeteorDamage == 0f && stack.RageDamagePercent == 0f && stack.RageCritChanceBonus == 0f);
            Near("reset: crit at HP 10% back to 0.05", stack.CritChanceAt(0.05f, 0.1f), 0.05f);
            Check("reset: stack counts cleared", stack.StackCount("cuong_no") == 0);
        }

        // ---------- fixtures ----------

        private sealed class Fake : IDamageable
        {
            public Transform Transform { get; set; }
            public bool IsAlive { get; set; }
            public bool TakeDamage(in DamageInfo info) => true;
        }

        private static Fake MakeFake(float x, bool alive)
        {
            var go = new GameObject($"Fake{x}");
            go.transform.position = new Vector3(x, 1f, 0f);
            Created.Add(go);
            return new Fake { Transform = go.transform, IsAlive = alive };
        }

        private static CardPoolConfig FullPool()
        {
            return Pool(
                Card("suc_manh_tho", CardRarity.Common, CardEffectKind.DamagePercent, percent: 15f),
                Card("giay_bao", CardRarity.Common, CardEffectKind.MoveSpeedPercent, percent: 12f),
                Card("huyet_nhu", CardRarity.Common, CardEffectKind.HealAndMaxHealth, heal: 30f, maxHp: 20f),
                Card("hoa_nguc", CardRarity.Rare, CardEffectKind.Burn, amount: 10f),
                Card("loi_chan", CardRarity.Rare, CardEffectKind.ChainLightning, amount: 25f),
                Card("phan_than", CardRarity.Rare, CardEffectKind.Decoy, amount: 1.5f),
                Card("vong_xoay", CardRarity.Legendary, CardEffectKind.BloodVortex, percent: 30f, healPct: 4f),
                Card("sao_bang", CardRarity.Legendary, CardEffectKind.Meteor, amount: 80f),
                Card("cuong_no", CardRarity.Legendary, CardEffectKind.VoidRage, percent: 100f, crit: 0.5f));
        }

        private static Func<float> Sequence(int seed)
        {
            var random = new System.Random(seed);
            return () => (float)random.NextDouble();
        }

        private static Func<float> Constant(float first, float rest)
        {
            bool used = false;
            return () =>
            {
                if (used)
                {
                    return rest;
                }
                used = true;
                return first;
            };
        }

        private static CardConfig Card(string id, CardRarity rarity, CardEffectKind effect, float percent = 0f, float heal = 0f,
            float maxHp = 0f, float amount = 0f, float healPct = 0f, float crit = 0f, int maxStacks = 0)
        {
            var card = ScriptableObject.CreateInstance<CardConfig>();
            Created.Add(card);
            var so = new SerializedObject(card);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.FindProperty("_effect").enumValueIndex = (int)effect;
            so.FindProperty("_percent").floatValue = percent;
            so.FindProperty("_healAmount").floatValue = heal;
            so.FindProperty("_maxHealthBonus").floatValue = maxHp;
            so.FindProperty("_amount").floatValue = amount;
            so.FindProperty("_healPercentPerKill").floatValue = healPct;
            so.FindProperty("_critChanceBonus").floatValue = crit;
            so.FindProperty("_maxStacks").intValue = maxStacks;
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        private static CardPoolConfig Pool(params CardConfig[] cards)
        {
            var pool = ScriptableObject.CreateInstance<CardPoolConfig>();
            Created.Add(pool);
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

        private static void Near(string name, float actual, float expected)
        {
            Check($"{name} (got {actual:0.#####})", Mathf.Abs(actual - expected) < 1e-4f);
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
