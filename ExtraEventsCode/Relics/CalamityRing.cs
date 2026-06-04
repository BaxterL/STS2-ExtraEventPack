using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ExtraEvents.ExtraEventsCode.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class CalamityRing : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: Res.RelicIcon<CalamityRing>(),
        IconOutlinePath: Res.RelicOutline<CalamityRing>(),
        BigIconPath: Res.RelicBigIcon<CalamityRing>()
    );

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var source = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c != null && c.IsTransformable)
            .ToList();

        var transformations = source.Select(c =>
            new CardTransformation(c, CardFactory.CreateRandomCardForTransform(c, false, Owner.RunState.Rng.Niche)));

        var results = (await CardCmd.Transform(transformations, null, CardPreviewStyle.None)).ToList();
        if (results.Count > 0 && LocalContext.IsMe(Owner))
            NSimpleCardsViewScreen.ShowScreen(results, new LocString("relics", "EXTRA_EVENTS_RELIC_CALAMITY_RING.infoText"));
    }
}
