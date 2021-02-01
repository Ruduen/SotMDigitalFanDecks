﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Trailblazer
{
    public class SupplyPackCardController : CardController
    {
        private const string _FirstPositionPlayedThisTurn = "FirstPositionPlayedThisTurn";
        public SupplyPackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
        public override void AddTriggers()
        {
            this.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => !this.IsPropertyTrue(_FirstPositionPlayedThisTurn) && cepa.CardEnteringPlay.IsPosition && cepa.CardEnteringPlay.Owner == this.HeroTurnTaker, ResponseAction, TriggerType.UsePower, TriggerTiming.After);
        }

        protected IEnumerator ResponseAction(CardEntersPlayAction cepa)
        {

            this.SetCardPropertyToTrueIfRealAction(_FirstPositionPlayedThisTurn);

            // All of the relevant ownership means the positions should only have 1 power, so we can skip more complex handling.
            if (cepa.CardEnteringPlay.HasPowers)
            {
                IEnumerator coroutine = this.GameController.UsePower(cepa.CardEnteringPlay, 0, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}