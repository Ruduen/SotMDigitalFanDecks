﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Manually tested!

namespace LazyFanComix.Inquirer
{
    public class InquirerCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public InquirerCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<DestroyCardAction> storedResults = new List<DestroyCardAction>();

            // Draw a card.
            coroutine = this.DrawCard(this.HeroTurnTaker, false, null, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // You may destroy a distortion.
            coroutine = this.GameController.SelectAndDestroyCard(this.DecisionMaker,
                new LinqCardCriteria((Card c) => c.IsDistortion, "distortion", true, false, null, null, false),
                true, storedResults, null, this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // If you do, play a card.
            if (storedResults.Count<DestroyCardAction>() >= 1)
            {
                coroutine = this.SelectAndPlayCardFromHand(this.DecisionMaker, true, null, null, false, false, false, null);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            yield break;
        }

        // TODO: Replace with something more unique!
        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        coroutine = this.SelectHeroToPlayCard(this.DecisionMaker);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 1:
                    {
                        coroutine = this.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 2:
                    {
                        coroutine = this.GameController.SelectAndReturnCards(this.DecisionMaker, 1, new LinqCardCriteria((Card c) => c.IsHero && !c.IsCharacter, "non-Character"), true, false, false, null, cardSource: this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
            }
            yield break;
        }
    }
}