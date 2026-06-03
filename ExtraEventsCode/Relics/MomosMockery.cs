using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class MomosMockery : ModRelicTemplate
{
    private const int CurseCount = 5;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: Res.RelicIcon<MomosMockery>(),
        IconOutlinePath: Res.RelicOutline<MomosMockery>(),
        BigIconPath: Res.RelicBigIcon<MomosMockery>()
    );

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != Owner)
            return;

        Flash();

        var availableCurses = ModelDb.CardPool<CurseCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.CanBeGeneratedByModifiers)
            .ToList();

        var results = new List<CardPileAddResult>();
        for (int i = 0; i < CurseCount; i++)
        {
            var curse = Owner.RunState.Rng.Niche.NextItem(availableCurses);
            if (curse == null) continue;
            availableCurses.Remove(curse);
            var card = shuffler.Creature.CombatState.CreateCard(curse, Owner);
            results.Add(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random));
        }

        CardCmd.PreviewCardPileAdd(results, 2f);
        await Cmd.Wait(0.75f);
    }
}
