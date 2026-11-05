// Implements: production/epics/micro-arena-survivor/story-004-upgrade-cards.md (card pick after waves 1-9, cards kept
// for the run and lost on restart), design/game-brief.md MVP 4.

using System;
using System.Collections.Generic;
using MicroArena.Gameplay.Combat;
using MicroArena.Gameplay.Config;
using MicroArena.Gameplay.Player;
using MicroArena.Gameplay.Waves;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MicroArena.Gameplay.Cards
{
    /// <summary>
    /// Owns the run's <see cref="ModifierStack"/> and drives the card pick hand-off of <see cref="WaveDirector"/>.
    /// <code>
    /// Transition table
    ///   Idle    --OnCardPickRequested, cards drawn--------> Picking (view shown, game paused by the view)
    ///   Idle    --OnCardPickRequested, no eligible card----> Idle    (ContinueFromCardPick at once)
    ///   Picking --card chosen-----------------------------> Idle    (apply, hide, ContinueFromCardPick)
    ///   Picking --phase leaves CardPick (death)-----------> Idle    (hide, no card)
    ///   (any)   --OnRunReset------------------------------> Idle    (hide, stack cleared)
    /// </code>
    /// </summary>
    /// <remarks>Development builds only: <c>-arenaForceCards id,id,id</c> (<see cref="ForceCardsArgument"/>) shows those cards
    /// first at every pick; the normal draw fills the rest.</remarks>
    public sealed class CardController : MonoBehaviour
    {
        /// <summary>Launch option <c>-arenaForceCards &lt;id,id,...&gt;</c>: CardConfig ids shown first at every pick. Development builds only.</summary>
        public const string ForceCardsArgument = "-arenaForceCards";

        [SerializeField] private WaveDirector _director;
        [SerializeField] private CardPoolConfig _pool;
        [SerializeField] private PlayerController _player;
        [SerializeField] private AutoAttack _autoAttack;
        [SerializeField] private Health _health;
        [Tooltip("Component implementing ICardPickView (CardPickScreen).")]
        [SerializeField] private MonoBehaviour _view;
        [Tooltip("Runs the Rare/Legendary effects. Optional (commons only without it).")]
        [SerializeField] private CardEffectRunner _effects;

        private readonly ModifierStack _stack = new ModifierStack();
        private ICardPickView _pickView;
        private RunState _runState;
        private Func<float> _roll = () => Random.value;
        private List<CardConfig> _offered;
        private List<CardConfig> _forced = new List<CardConfig>();

        /// <summary>The run's modifiers (read-only use outside this class).</summary>
        public ModifierStack Modifiers => _stack;

        /// <summary>Injects dependencies when building the scene. Call before the object is activated.</summary>
        public void Configure(WaveDirector director, CardPoolConfig pool, PlayerController player, AutoAttack autoAttack,
            Health health, MonoBehaviour view, CardEffectRunner effects = null)
        {
            _effects = effects;
            _director = director;
            _pool = pool;
            _player = player;
            _autoAttack = autoAttack;
            _health = health;
            _view = view;
        }

        /// <summary>Replaces the draw random source (0..1) so draws can be deterministic.</summary>
        public void SetRandomSource(Func<float> roll)
        {
            _roll = roll;
        }

        private void Awake()
        {
            _pickView = _view as ICardPickView;
            if (_pickView == null)
            {
                Debug.LogError("CardController: the view does not implement ICardPickView; card picks will be skipped.", this);
            }
            _runState = _director.Run;
            _player.SetModifiers(_stack);
            _autoAttack.SetModifiers(_stack);
            if (_effects != null)
            {
                _effects.SetModifiers(_stack);
            }
            // Debug switches never reach a release player: Debug.isDebugBuild is true only in the Editor and development builds.
            if (Debug.isDebugBuild)
            {
                List<string> ids = ForcedCards.ParseIds(Environment.GetCommandLineArgs(), ForceCardsArgument);
                if (ids.Count > 0)
                {
                    _forced = ForcedCards.Resolve(_pool, ids, message => Debug.LogWarning(message, this));
                }
            }
        }

        private void OnEnable()
        {
            _director.OnCardPickRequested += HandlePickRequested;
            _director.OnRunReset += HandleRunReset;
            _runState.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            _director.OnCardPickRequested -= HandlePickRequested;
            _director.OnRunReset -= HandleRunReset;
            _runState.OnPhaseChanged -= HandlePhaseChanged;
            CloseView();
        }

        private void HandlePickRequested(int clearedWave)
        {
            List<CardConfig> forced = ForcedCards.Eligible(_forced, _stack, _pool.CardsPerPick);
            _offered = CardPool.Draw(_pool, _stack, _roll, forced);
            if (_offered.Count == 0 || _pickView == null)
            {
                _offered = null;
                _director.ContinueFromCardPick();
                return;
            }
            _pickView.Show(_offered, HandlePicked);
        }

        private void HandlePicked(int index)
        {
            if (_offered == null || index < 0 || index >= _offered.Count || _runState.Phase != RunPhase.CardPick)
            {
                return;
            }
            CardConfig card = _offered[index];
            CardEffects.Apply(card, _stack, _health);
            CloseView();
            _director.ContinueFromCardPick();
        }

        private void HandlePhaseChanged(RunPhase previous, RunPhase next)
        {
            if (previous == RunPhase.CardPick && next != RunPhase.Playing)
            {
                CloseView(); // a stray bolt killed Kaelen during the pick
            }
        }

        // Health already dropped its max-HP bonus in PlayerController.ResetForNewRun, which runs before this event.
        private void HandleRunReset()
        {
            CloseView();
            _stack.Reset();
            if (_effects != null)
            {
                _effects.ResetForNewRun();
            }
        }

        private void CloseView()
        {
            _offered = null;
            if (_pickView != null && _pickView.IsShown)
            {
                _pickView.Hide();
            }
        }
    }
}
