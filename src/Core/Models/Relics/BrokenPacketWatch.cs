using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreEvent.Patches;

namespace MoreEvent.Relics;

/// <summary>
/// 当玩家在1分钟内结束普通怪物战斗时(随机1-6：1.将卡牌奖励中一张卡升级；2.获得2生命值上限；3.获得30~40金币；4.随机升级一张卡；5.获得一瓶随机药水；6.额外获得一次卡牌奖励)；
/// 当玩家在1分钟内结束精英怪物战斗时(随机7-12：1.卡牌奖励是全部升级的；2.获得5生命值上限；3.获得62~80金币；4.随机升级两张卡；5.获得一瓶随机罕见药水；6.额外获得一次卡牌奖励)；
/// 当玩家在1分钟内结束BOSS怪物战斗时(包含13：1.卡牌奖励是全部升级的；2.获得3生命值上限；3.升级牌组中一张卡；4.获得一瓶随机稀有药水)
/// 当玩家超过5分钟结束战斗时，结束后获得惩罚（随机14-18：1.随机一张卡牌降级；2.失去3生命上限；3.本次战斗奖励中不包含金币；4.什么都不会发生；5.当前的游戏时间计时 +1min）
/// </summary>
public sealed class BrokenPacketWatch : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => false;

    private long CombatStartRunTime = -1L;

    private int ResultChoice = 0;

    private long TimeAward = 60L;

    private long TimePunish = 300L;

    private long TimeStart;

    private long TimeEnd;

    private long TimeCost;

    public override Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (CombatStartRunTime < 0L)
        {
            CombatStartRunTime = RunManager.Instance.RunTime;
        }

        return Task.CompletedTask;
    }
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        TimeEnd = RunManager.Instance.RunTime;
        TimeStart = CombatStartRunTime;
        CombatStartRunTime = -1L;
        if (TimeStart < 0L)
        {
            return;
        }
        TimeCost = Math.Max(0L, TimeEnd - TimeStart);
        if(TimeCost <= TimeAward && room.RoomType.IsCombatRoom())
        {
            Flash();
            await ApplyAward(isFinish: true, room.RoomType);
        }
        else if(TimeCost > TimePunish && room.RoomType.IsCombatRoom())
        {
            Flash();
            await ApplyAward(isFinish: false, room.RoomType);
        }
    }
    public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != base.Owner || room is not CombatRoom)
        {
            return false;
        }

        switch (ResultChoice)
        {
            case 1:
                UpgradeCardsInReward(isAll: false, rewards, player);
                break;
            case 2:
                MaxHpFix(player, 2);
                break;
            case 3:
                GoinAddToAward(rewards, player, minimum: 30, maximum: 40);
                break;
            case 4:
                UpgradeRandomCards(1);
                break;
            case 5:
                AddPotionReward(rewards, player, potionRarity: PotionRarity.Common);
                break;
            case 6:
                AddCardReward(rewards, player, roomType: RoomType.Monster);
                break;
            case 7:
                UpgradeCardsInReward(isAll: true, rewards, player);
                break;
            case 8:
                MaxHpFix(player, 5);
                break;
            case 9:
                GoinAddToAward(rewards, player, minimum: 62, maximum: 80);
                break;
            case 10:
                UpgradeRandomCards(2);
                break;
            case 11:
                AddPotionReward(rewards, player, potionRarity: PotionRarity.Uncommon);
                break;
            case 12:
                AddCardReward(rewards, player, roomType: RoomType.Elite);
                break;
            case 13:
                UpgradeCardsInReward(isAll: true, rewards, player);
                MaxHpFix(player, 3);
                UpdateCardSelect(player);
                break;
            case 14:
                DowngradeRandomCard();
                break;
            case 15:
                MaxHpFix(player, -3);
                break;
            case 16:
                rewards.RemoveAll((Reward reward) => reward is GoldReward);
                break;
            case 18:
                RunTimeHelper.AdjustRunTime(60L);
                break;

        }

        return true;
    }
    private Task ApplyAward(bool isFinish, RoomType roomType)
    {
        if (isFinish && roomType == RoomType.Monster)
        {
            ResultChoice = base.Owner.RunState.Rng.Niche.NextInt(6) + 1;        //对应1-6
        }
        else if (isFinish && roomType == RoomType.Elite)
        {
            ResultChoice = base.Owner.RunState.Rng.Niche.NextInt(6) + 7;        //对应7-12
        }
        else if (isFinish && roomType == RoomType.Boss)
        {
            ResultChoice = 13;        //对应13
        }
        else if (!isFinish)
        {
            ResultChoice = base.Owner.RunState.Rng.Niche.NextInt(4) + 14;       //对应14-17
        }
        return Task.CompletedTask;
    }
    private static void UpgradeCardsInReward(bool isAll, List<Reward> rewards, Player player)
    {
        if (isAll) 
        {
            List<CardReward> cardRewards = rewards.OfType<CardReward>().ToList();
            foreach (CardReward cardReward in cardRewards)
            {
                CardCmd.Upgrade(cardReward.Cards, CardPreviewStyle.None);
                cardReward.AfterGenerated += () => CardCmd.Upgrade(cardReward.Cards, CardPreviewStyle.None);
            }
        }
        else
        {
            CardReward? cardReward = rewards.OfType<CardReward>().FirstOrDefault();
            List<CardModel> candidates = cardReward.Cards
            .Where((CardModel card) => card.IsUpgradable)
            .ToList();

            CardModel? card = player.RunState.Rng.Niche.NextItem(candidates);
            if (card != null)
            {
                CardCmd.Upgrade(card, CardPreviewStyle.None);
                cardReward.AfterGenerated += () => CardCmd.Upgrade(card, CardPreviewStyle.None);
            }
        }
    }
    private static async void MaxHpFix(Player player, int amount)
    {
        if (amount > 0)
        {
            await CreatureCmd.GainMaxHp(player.Creature, amount);
        }
        else 
        {
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), player.Creature, amount, isFromCard: false);
        }
    }
    private static void GoinAddToAward(List<Reward> rewards, Player player, int minimum, int maximum)
    {
        rewards.Add(new GoldReward(minimum, maximum, player));
    }
    private void UpgradeRandomCards(int count)
    {
        List<CardModel> candidates = PileType.Deck.GetPile(base.Owner).Cards
            .Where((CardModel card) => card.IsUpgradable)
            .ToList();

        base.Owner.RunState.Rng.Niche.Shuffle(candidates);
        CardCmd.Upgrade(candidates.Take(count), CardPreviewStyle.HorizontalLayout);
    }
    private void DowngradeRandomCard()
    {
        List<CardModel> candidates = PileType.Deck.GetPile(base.Owner).Cards
            .Where((CardModel card) => card.IsUpgraded)
            .ToList();

        CardModel? card = base.Owner.RunState.Rng.Niche.NextItem(candidates);
        if (card != null)
        {
            CardCmd.Downgrade(card);
        }
    }
    private static bool AddPotionReward(List<Reward> rewards, Player player, PotionRarity potionRarity)
    {
        PotionModel? potion = player.PlayerRng.Rewards.NextItem(
            PotionFactory.GetPotionOptions(player, Array.Empty<PotionModel>())
                .Where((PotionModel candidate) => candidate.Rarity == potionRarity));

        if (potion == null)
        {
            return false;
        }

        rewards.Add(new PotionReward(potion.ToMutable(), player));
        return true;
    }
    private static bool AddCardReward(List<Reward> rewards, Player player, RoomType roomType)
    {
        rewards.Add(new CardReward(CardCreationOptions.ForRoom(player, roomType), 3, player));
        return true;
    }
    private static async Task UpdateCardSelect(Player player)
    {
        CardModel cardModel = (await CardSelectCmd.FromDeckForUpgrade(player, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
        if (cardModel != null)
        {
            CardCmd.Upgrade(cardModel);
        }
    }
}