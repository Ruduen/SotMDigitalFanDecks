﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenWorkshop.Soulbinder;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuduenModsTest
{
    [TestFixture]
    public class SoulbinderTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("RuduenWorkshop", Assembly.GetAssembly(typeof(SoulbinderCharacterCardController))); // replace with your own namespace
        }

        protected HeroTurnTakerController Soulbinder { get { return FindHero("Soulbinder"); } }

        protected Card[] Soulshards { get { return new Card[] { GetCard("SoulshardOfLightningCharacter"), GetCard("SoulshardOfMercuryCharacter"), GetCard("SoulshardOfIronCharacter") }; } }

        protected Card SoulbinderMortal { get { return GetCard("SoulbinderMortalFormCharacter"); } }

        protected Card SoulbinderInstruction { get { return GetCard("SoulbinderCharacter"); } }

        [Test(Description = "Basic Setup and Health")]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(Soulbinder);
            Assert.IsInstanceOf(typeof(HeroTurnTakerController), Soulbinder);
            Assert.IsInstanceOf(typeof(SoulbinderCharacterCardController), Soulbinder.CharacterCardController);

            AssertNumberOfCardsInDeck(Soulbinder, 36);
            AssertNumberOfCardsInHand(Soulbinder, 4);
        }

        #region Basic Setup Tests

        [Test]
        public void TestSetupBasic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Megalopolis");

            DecisionSelectCard = Soulshards[0];

            ResetDecisions();
            StartGame(false);

            AssertNotInPlay(SoulbinderMortal);
            AssertIsInPlay(Soulshards);

            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            Assert.IsTrue(Soulbinder.HasMultipleCharacterCards);
        }

        [Test]
        public void TestSetupMortal()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder/SoulbinderMortalCharacter", "Megalopolis");

            ResetDecisions();
            DecisionSelectCard = Soulshards[0];

            StartGame(false);

            AssertIsInPlay(SoulbinderMortal);
            AssertIsInPlay(Soulshards);

            AssertIsInPlay(Soulshards);
            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);

            AssertNotIncapacitatedOrOutOfGame(Soulbinder);
        }

        #endregion Basic Setup Tests

        #region Multi-Character and Incap Tests

        [Test]
        public void TestIncapBasicAll()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Megalopolis");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            AssertIsInPlay(Soulshards);
            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            // First is destroyed, second flips to take its place.
            DestroyCard(Soulshards[0]);
            AssertFlipped(Soulshards[0]);
            AssertIsInPlay(Soulshards[1]);

            DestroyCard(Soulshards[1]);
            AssertFlipped(Soulshards[1]);
            AssertIsInPlay(Soulshards[2]);

            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            DestroyCard(Soulshards[2]);
            AssertFlipped(Soulshards[2]);
            AssertIsInPlay(Soulshards[2]);

            AssertIncapacitated(Soulbinder);

            GoToUseIncapacitatedAbilityPhase(Soulbinder);
        }

        [Test]
        public void TestIncapBasicOneRemoved()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            Card card = PlayCard("UnforgivingWasteland");
            DealDamage(card, Soulshards[0], 100, DamageType.Melee);
            AssertOutOfGame(Soulshards[0]);

            //AssertUnderCard(SoulbinderInstruction, Soulshards[1]);

            AssertIncapacitated(Soulbinder);

            GoToStartOfTurn(Soulbinder);
            EnterNextTurnPhase();
            AssertCurrentTurnPhase(Soulbinder, Phase.End);
        }

        [Test]
        public void TestIncapMortalAll()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder/SoulbinderMortalCharacter", "Legacy", "Megalopolis");

            ResetDecisions();
            List<Card> SoulshardsDecision = new List<Card>();
            SoulshardsDecision.AddRange(Soulshards);
            SoulshardsDecision.AddRange(Soulshards);
            DecisionSelectCards = SoulshardsDecision;

            StartGame(false);

            AssertIsInPlay(SoulbinderMortal);
            AssertIsInPlay(Soulshards);
            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);

            // First is destroyed, second flips to take its place.
            DestroyCard(Soulshards[0]);
            AssertFlipped(Soulshards[0]);
            AssertIsInPlayAndNotUnderCard(Soulshards[1]);

            DestroyCard(Soulshards[1]);
            AssertFlipped(Soulshards[1]);
            AssertIsInPlayAndNotUnderCard(Soulshards[2]);

            DestroyCard(Soulshards[2]);
            AssertIsInPlayAndNotUnderCard(Soulshards[2]);
            AssertNotFlipped(SoulbinderMortal);

            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            DestroyCard(SoulbinderMortal);
            AssertOutOfGame(Soulshards);
            AssertIncapacitated(Soulbinder);

            GoToUseIncapacitatedAbilityPhase(Soulbinder);
        }

        [Test]
        public void TestIncapMortalFirst()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder/SoulbinderMortalCharacter", "Legacy", "Megalopolis");

            ResetDecisions();
            List<Card> SoulshardsDecision = new List<Card>
            {
                Soulshards[0]
            };
            SoulshardsDecision.AddRange(Soulshards);
            DecisionSelectCards = SoulshardsDecision;

            StartGame(false);

            AssertIsInPlay(SoulbinderMortal);
            AssertIsInPlay(Soulshards);
            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            DestroyCard(SoulbinderMortal);

            AssertOutOfGame(Soulshards);
            AssertIsInPlay(SoulbinderMortal);

            AssertIncapacitated(Soulbinder);

            GoToUseIncapacitatedAbilityPhase(Soulbinder);
        }

        [Test]
        public void TestIncapMortalRemoved()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder/SoulbinderMortalCharacter", "Legacy", "TheFinalWasteland");

            ResetDecisions();

            DecisionSelectCards = Soulshards;

            StartGame(false);

            AssertIsInPlay(SoulbinderMortal);
            AssertIsInPlay(Soulshards);
            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[1]);
            AssertUnderCard(SoulbinderInstruction, Soulshards[2]);
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            Card card = PlayCard("UnforgivingWasteland");
            DealDamage(card, SoulbinderMortal, 100, DamageType.Melee);
            AssertOutOfGame(SoulbinderMortal);

            // Still in play - one soulshard left!
            AssertNotIncapacitatedOrOutOfGame(Soulbinder);

            // First is destroyed, second flips to take its place.
            DestroyCard(Soulshards[0]);
            AssertFlipped(Soulshards[0]);
            AssertIsInPlayAndNotUnderCard(Soulshards[1]);

            DestroyCard(Soulshards[1]);
            AssertFlipped(Soulshards[1]);
            AssertIsInPlayAndNotUnderCard(Soulshards[2]);

            DestroyCard(Soulshards[2]);
            AssertFlipped(Soulshards[2]);

            AssertIncapacitated(Soulbinder);

            GoToUseIncapacitatedAbilityPhase(Soulbinder);
        }

        #endregion Multi-Character and Incap Tests

        #region Character Triggers

        [Test]
        public void TestTriggerLightningBasic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[0];

            StartGame(false);

            Card mdp = FindCardInPlay("MobileDefensePlatform");

            QuickHPStorage(mdp);
            DealDamage(Soulshards[0], mdp, 3, DamageType.Melee);
            QuickHPCheck(-4);
        }

        [Test]
        public void TestTriggerMercuryBasic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            DestroyCard(Soulshards[0]);
            ResetDecisions();
            Card discard = GetCardFromHand(Soulbinder);
            DecisionSelectCard = discard;
            QuickHandStorage(Soulbinder);
            GoToEndOfTurn(Soulbinder);
            QuickHandCheck(0);
            AssertInTrash(discard);

        }

        [Test]
        public void TestTriggerIronBasic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[2];

            StartGame(false);

            QuickHPStorage(Soulshards[2]);
            DealDamage(Soulshards[2], Soulshards[2], 3, DamageType.Melee);
            QuickHPCheck(-2);
        }

        #endregion Character Triggers

        #region Powers Cards

        [Test]
        public void TestPowerBasic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[1];

            StartGame(false);
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(Soulshards[1], mdp);
            UsePower(SoulbinderInstruction);
            QuickHPCheck(-1, -3);
        }

        [Test]
        public void TestPowerMortalNoRitual()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder/SoulbinderMortalCharacter", "Legacy", "TheFinalWasteland");

            ResetDecisions();

            StartGame(false);

            AssertNextMessage("There are no rituals with Ritual Tokens in play.");

            QuickHandStorage(Soulbinder);
            UsePower(SoulbinderMortal);
            QuickHandCheck(1);

            AssertExpectedMessageWasShown();
        }

        [Test]
        public void TestPowerMortalRitual()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder/SoulbinderMortalCharacter", "Legacy", "TheFinalWasteland");

            ResetDecisions();

            StartGame(false);

            Card card = PlayCard("RitualOfCatastrophe");

            QuickHandStorage(Soulbinder);
            UsePower(SoulbinderMortal);
            QuickHandCheck(1);

            AssertTokenPoolCount(card.TokenPools.FirstOrDefault(), 2); // One removed from 4.
        }

        [Test]
        public void TestPowerClay()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = new Card[] { Soulshards[1] };

            StartGame(false);

            ResetDecisions();
            DiscardAllCards(Soulbinder);
            Card clay = PutInHand("ClayDoll");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCards = new Card[] { mdp, clay };
            DecisionSelectFunction = 1;
            QuickHandStorage(Soulbinder);
            QuickHPStorage(mdp);
            PlayCard(clay);
            UsePower(clay);
            QuickHandCheck(-1); // 1 Played,
            QuickHPCheck(-2);
        }

        [Test]
        public void TestPowerWood()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = new Card[] { Soulshards[1] };

            StartGame(false);

            ResetDecisions();

            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card wood = PutInHand("WoodenPuppet");

            DecisionSelectFunction = 1;
            DecisionSelectCards = new Card[] { null };

            QuickHandStorage(Soulbinder);
            PlayCard(wood);
            QuickHandCheck(-1); // 1 Played.

            DealDamage(wood, wood, 3, DamageType.Melee);

            ResetDecisions();
            QuickHPStorage(wood, mdp);
            UsePower(wood);
            QuickHPCheck(1, -1);
        }

        [Test]
        public void TestPowerStraw()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = new Card[] { Soulshards[1] };

            StartGame(false);

            ResetDecisions();
            DiscardAllCards(Soulbinder);
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card straw = PutInHand("StrawEffigy");

            DecisionSelectCards = new Card[] { straw, mdp };
            DecisionSelectFunction = 1;

            QuickHandStorage(Soulbinder);
            PlayCard(straw);
            QuickHandCheck(-1); // 1 Played.
            DealDamage(straw, straw, 3, DamageType.Melee);

            QuickHPStorage(straw, mdp);
            UsePower(straw);
            QuickHPCheck(2, -2);
        }

        #endregion Powers Cards

        #region Rituals

        [Test]
        public void TestCardRitualEndRemoved()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfCatastrophe");

            GoToEndOfTurn(Soulbinder);
            AssertTokenPoolCount(ritual.TokenPools.FirstOrDefault(), 2);
        }

        [Test]
        public void TestCardRitualOfCatastrophe()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfCatastrophe");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            QuickHPStorage(mdp);
            RemoveTokensFromPool(ritual.TokenPools[0], 3);
            QuickHPCheck(-4);
            AssertInTrash(ritual);
        }

        [Test]
        public void TestCardRitualOfCausality()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfCausality");
            Card[] plays = new Card[] { PutInHand("RitualOfCatastrophe"), PutInHand("RitualOfTransferrence") };
            DecisionSelectCards = plays;

            QuickHandStorage(Soulbinder);
            RemoveTokensFromPool(ritual.TokenPools[0], 4);
            QuickHandCheck(1); // Draw 3, play 2
            AssertIsInPlay(plays);
            AssertInTrash(ritual);
        }

        [Test]
        public void TestCardRitualOfTransferrence()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfTransferrence");
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card[] targets = new Card[] { Soulshards[0], legacy.CharacterCard, ra.CharacterCard, mdp };
            DealDamage(baron.CharacterCard, targets, 3, DamageType.Melee);

            DecisionSelectCards = targets;
            QuickHPStorage(targets);
            RemoveTokensFromPool(ritual.TokenPools[0], 4);
            QuickHPCheck(2, 2, 2, -6);
            AssertInTrash(ritual);
        }

        [Test]
        public void TestCardRitualOfSalvation()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfSalvation");
            Card[] targets = new Card[] { Soulshards[0], legacy.CharacterCard, ra.CharacterCard };
            DealDamage(baron.CharacterCard, targets, 4, DamageType.Melee);

            DecisionSelectCards = targets;
            QuickHPStorage(targets);
            QuickHandStorage(Soulbinder, legacy, ra);
            RemoveTokensFromPool(ritual.TokenPools[0], 4);
            QuickHPCheck(3, 3, 3);
            QuickHandCheck(1, 1, 1);
            AssertInTrash(ritual);
        }

        [Test]
        public void TestCardRitualComponents()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card card = PlayCard("RitualComponents");

            Card ritualA = PlayCard("RitualOfSalvation");
            UsePower(card);
            AssertTokenPoolCount(ritualA.TokenPools[0], 2);

            DecisionSelectCards = new Card[] { null };

            PlayCard("ClayDoll");
            PlayCard("StrawEffigy");
            PutInHand(card);
            PlayCard(card);
            PutInHand(ritualA);
            PlayCard(ritualA);

            Card ritualB = PlayCard("RitualOfCatastrophe");
            ResetDecisions();

            DecisionSelectCard = ritualA;

            UsePower(card);
            AssertTokenPoolCount(ritualA.TokenPools[0], 1); // Selected and then reduced by all
            AssertTokenPoolCount(ritualB.TokenPools[0], 2); // Only reduced by all.
        }

        [Test]
        public void TestCardRitualComponentsTriggered()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();
            DecisionSelectCards = new Card[] { null };

            PlayCard("ClayDoll");
            PlayCard("StrawEffigy");

            ResetDecisions();

            Card[] playRituals = new Card[] { PutInHand("RitualOfCatastrophe"), PutInHand("RitualOfSalvation") };

            Card card = PlayCard("RitualComponents");
            Card ritual = PlayCard("RitualOfCausality");
            RemoveTokensFromPool(ritual.TokenPools[0], 1);

            DecisionSelectCards = new Card[] { playRituals[0], playRituals[1], playRituals[0] };
            UsePower(card);
            AssertInTrash(ritual);
            AssertIsInPlay(playRituals);
            // Power removed 1 again.
            AssertTokenPoolCount(playRituals[0].TokenPools[0], 2);
            AssertTokenPoolCount(playRituals[1].TokenPools[0], 2);
        }

        [Test]
        public void TestCardRepeatedRitesOutsideTrigger()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfCausality");
            Card card = PlayCard("RepeatedRites");

            DecisionYesNo = true;

            DestroyCard(ritual);

            AssertInTrash(card);
            AssertIsInPlay(ritual);
            // Check tokens added.
            AssertTokenPoolCount(ritual.TokenPools.FirstOrDefault(), 5);
        }

        [Test]
        public void TestCardRepeatedRitesCompleteTrigger()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            ResetDecisions();
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Card ritual = PlayCard("RitualOfCatastrophe");
            Card card = PlayCard("RepeatedRites");

            DecisionYesNo = true;

            QuickHPStorage(mdp);
            RemoveTokensFromPool(ritual.TokenPools.FirstOrDefault(), 4);

            AssertInTrash(card);
            AssertIsInPlay(ritual);

            RemoveTokensFromPool(ritual.TokenPools.FirstOrDefault(), 2);
            AssertInTrash(ritual);
            QuickHPCheck(-8);
        }

        #endregion Rituals

        #region Other Cards

        [Test]
        public void TestCardReliquary()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCards = Soulshards;

            StartGame(false);

            DecisionYesNo = true;

            QuickHandStorage(Soulbinder);
            PlayCard("Reliquary");

            AssertIsInPlayAndNotUnderCard(Soulshards[0]);
            AssertIsInPlayAndNotUnderCard(Soulshards[1]);

            DestroyCard(Soulshards[0]);
            AssertNotFlipped(Soulshards[0]);
        }

        [Test]
        public void TestCardArcaneDetonation()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[1];

            StartGame(false);

            ResetDecisions();

            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCards = new Card[] { mdp };

            QuickHPStorage(Soulshards[1], mdp);
            PlayCard("ArcaneDetonation");
            QuickHPCheck(-3, -3);
        }

        [Test]
        public void TestCardSacrificialRite()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[1];

            StartGame(false);

            ResetDecisions();

            Card ritual = PlayCard("RitualOfCatastrophe");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCards = new Card[] { mdp };

            QuickHPStorage(Soulshards[1], mdp);
            PlayCard("SacrificialRite");
            QuickHPCheck(-2, -2);
            AssertTokenPoolCount(ritual.TokenPools.FirstOrDefault(), 2);
        }

        [Test]
        public void TestCardKeystoneOfSpirit()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "Ra", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[1];

            StartGame(false);

            ResetDecisions();

            Card[] targets = new Card[] { Soulshards[1], ra.CharacterCard };
            DealDamage(baron.CharacterCard, targets, 3, DamageType.Melee);

            QuickHPStorage(Soulshards[1], ra.CharacterCard);

            PlayCard("KeystoneOfSpirit");

            QuickHPCheck(0, 1);
            AssertNotUsablePower(Soulbinder, SoulbinderInstruction);
        }

        [Test]
        public void TestCardFinalEruption()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[1];

            StartGame(false);

            ResetDecisions();

            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCards = new Card[] { mdp, Soulshards[0] };

            SetHitPoints(Soulshards[1], 5);
            QuickHPStorage(mdp);
            PlayCard("FinalEruption");
            QuickHPCheck(-7);
            AssertFlipped(Soulshards[1]);
        }

        [Test]
        public void TestCardBlindBeckoning()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Soulbinder", "Legacy", "TheFinalWasteland");

            ResetDecisions();
            DecisionSelectCard = Soulshards[1];

            StartGame(false);

            ResetDecisions();

            DecisionSelectCards = new Card[] { null };

            QuickHPStorage(Soulshards[1]);
            PlayCard("BlindBeckoning");
            QuickHPCheck(-1);
            AssertNumberOfCardsInPlay((Card c) => c.DoKeywordsContain("ritual"), 1);
        }

        #endregion Other Cards
    }
}