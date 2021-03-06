﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LazyFanComix.Soulbinder
{
    public class KeystoneOfSpiritCardController : SoulbinderSharedYourTargetDamageCardController
    {
        public KeystoneOfSpiritCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            List<Card> targetList = new List<Card>();
            IEnumerator coroutine;

            // You may play a ritual or soulsplinter.
            coroutine = this.GameController.SelectAndPlayCardsFromHand(this.DecisionMaker, 1, false, 0, cardCriteria: new LinqCardCriteria((Card c) => c.DoKeywordsContain("ritual") || c.DoKeywordsContain("soulsplinter"), "ritual or soulsplinter"), cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Select target.
            coroutine = this.SelectYourTargetToDealDamage(targetList, 1, DamageType.Infernal);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (targetList.Count > 0)
            {
                // That target deals itself 1 damage.
                coroutine = this.GameController.DealDamage(this.DecisionMaker, targetList.FirstOrDefault(), ((Card c) => targetList.FirstOrDefault() == c), 1, DamageType.Infernal, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            // All others gain HP. If target was invalid somehow, it will be null, which will still be fine for this check.
            coroutine = this.GameController.GainHP(this.DecisionMaker, (Card c) => c.IsHero && c != targetList.FirstOrDefault(), 1, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            coroutine = this.GameController.SelectAndUsePower(this.DecisionMaker, true, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}