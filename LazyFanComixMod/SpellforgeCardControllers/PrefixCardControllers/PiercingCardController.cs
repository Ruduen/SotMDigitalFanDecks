﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace LazyFanComix.Spellforge
{
    // TODO: TEST!
    public class PiercingCardController : SpellforgeSharedModifierCardController
    {
        public PiercingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Power.
            coroutine = this.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        protected override ITrigger AddModifierTriggerOverride(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger.
            ITrigger trigger = null;

            bool damageCriteria(DealDamageAction dd) => dd.CardSource.CardController == cardSource.CardController; // Only if the action sources of this play and the damage are an exact match, AKA the triggering step is the same.

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd),
                (DealDamageAction dd) => this.TrackOriginalTargetsAndRunResponse(dd, cardSource),
                new TriggerType[]
                {
                    TriggerType.MakeDamageIrreducible
                },
                TriggerTiming.Before);

            return trigger;
        }

        protected override IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            IEnumerator coroutine;

            // Deal damage response.
            coroutine = this.GameController.MakeDamageIrreducible(dd, cardSource: cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}