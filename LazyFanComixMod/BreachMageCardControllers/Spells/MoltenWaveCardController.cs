﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace LazyFanComix.BreachMage
{
    public class MoltenWaveCardController : BreachMageSharedSpellCardController
    {
        public MoltenWaveCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator ActivateCast()
        {
            IEnumerator coroutine;
            // Damage.
            coroutine = this.DealDamage(this.CharacterCard, (Card card) => !card.IsHero, 2, DamageType.Fire, false, false, null, null, null, false, null, null, false, false);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}