﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Soulbinder
{
    public class ClaySoulsplinterCardController : CardController
    {
        private readonly List<Card> _actedTargets = new List<Card>();

        public ClaySoulsplinterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // You may play a one-shot or a ritual.
            coroutine = this.GameController.SelectAndPlayCardsFromHand(this.DecisionMaker, 1, false, 0, cardCriteria: new LinqCardCriteria((Card c) => c.IsOneShot || c.DoKeywordsContain("ritual"), "one-shot or ritual"), cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Select target to deal damage to.
            List<int> numerals = new List<int>(){
                            this.GetPowerNumeral(0, 1),  // Number of Targets
                            this.GetPowerNumeral(1, 1)   // Damage.
            };
            SelectCardsDecision scdTarget = new SelectCardsDecision(this.GameController, this.DecisionMaker, (Card c) => !c.IsHero && c.IsTarget && c.IsInPlayAndNotUnderCard, SelectionType.SelectTarget, numerals[0], false, numerals[0], eliminateOptions: true, allAtOnce: true, cardSource: this.GetCardSource());
            IEnumerator coroutine = this.GameController.SelectCardsAndDoAction(scdTarget, (SelectCardDecision sc) => this.TargetSelectedResponse(sc, numerals[1]), null, null, this.GetCardSource(), null, false, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        private IEnumerator TargetSelectedResponse(SelectCardDecision scdTarget, int damageAmount)
        {
            // Each source deals damage to the selected target.
            SelectCardsDecision scdTargets = new SelectCardsDecision(this.GameController, this.DecisionMaker, (Card c) => c.IsTarget && c.IsInPlayAndNotUnderCard && c.Owner == this.HeroTurnTaker, SelectionType.CardToDealDamage, null, false, null, true, true, false, () => NumTargetsToDamage(scdTarget.SelectedCard), null, null, null, this.GetCardSource());
            IEnumerator coroutine = this.GameController.SelectCardsAndDoAction(scdTargets, (SelectCardDecision scd) => this.TargetDamageResponse(scd, scdTarget, damageAmount), null, null, this.GetCardSource(), null, false, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            _actedTargets.Clear();
        }

        private int NumTargetsToDamage(Card target)
        {
            // If we should ever stop, drop this to 0.
            if (!this.Card.IsIncapacitatedOrOutOfGame && !target.IsBeingDestroyed && target.Location.IsInPlay)
            {
                int num = this.GameController.FindCardsWhere((Card c) => c.IsTarget && c.IsInPlayAndNotUnderCard && c.Owner == this.HeroTurnTaker).Except(_actedTargets).Count<Card>();
                return _actedTargets.Count<Card>() + num;
            }
            return 0;
        }

        private IEnumerator TargetDamageResponse(SelectCardDecision scd, SelectCardDecision scdTarget, int damageAmount)
        {
            Card selectedCard = scd.SelectedCard;
            Card selectedTarget = scdTarget.SelectedCard;
            _actedTargets.Add(selectedCard);
            IEnumerator coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, selectedCard), selectedTarget, damageAmount, DamageType.Melee, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}