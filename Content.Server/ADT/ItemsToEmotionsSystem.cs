///// <summary>
///// by ModerN, mailto:modern-nm@yandex.by or https://github.com/modern-nm. Discord: modern.df
///// </summary>

//using Content.Server.Actions;
//using Content.Server.Chat.Systems;
//using Content.Server.Chemistry.ReagentEffects;
//using Content.Shared.ADT;
//using Content.Shared.Damage;
//using Content.Shared.Emoting;
//using Content.Shared.Interaction;
//using Content.Shared.Interaction.Events;
//using Content.Shared.Item;
//using Content.Shared.Timing;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Content.Server.ADT
//{
//    internal sealed class ItemsToEmotionsSystem : EntitySystem
//    {
//        public override void Initialize()
//        {
//            base.Initialize();
//            SubscribeLocalEvent<ItemComponent, UseInHandEvent>(OnItemUsed);
//            //SubscribeNetworkEvent<ActionOnInteractComponent, InteractUsingEvent>(OnItemUsed);

//        }

//        private void OnItemUsed(EntityUid uid, ItemComponent component, UseInHandEvent args)
//        {
//            TryPrototype(uid, out var prototype);
//            if (prototype != null)
//                if (prototype.ID.ToLower() == "adtdumbbell")
//                {
//                    EntityManager.TrySystem<ChatSystem>(out var chatSystem);
//                    if (chatSystem != null)
//                    {
//                        chatSystem.TryEmoteWithChat(args.User, "Scream");
//                    }
//                }
//        }

//        //public void OnItemUsed(EntityUid uid, ActionOnInteractComponent component, InteractUsingEvent args)
//        //{
//        //    if (args.Target.GetType().Name.ToLower() == "foodbanana")
//        //    {
//        //        EntityManager.TrySystem<ChatSystem>(out var chatSystem);
//        //        if (chatSystem != null)
//        //        {
//        //            chatSystem.TryEmoteWithChat(uid, "Scream", default, false, default);
//        //        }
//        //    }

//        //    //var chatSys = TrySystem<ChatSystem>();
//        //    //if (ShowInChat)
//        //    //    chatSys.TryEmoteWithChat(args.SolutionEntity, EmoteId, ChatTransmitRange.GhostRangeLimit);
//        //    //else
//        //    //    chatSys.TryEmoteWithoutChat(args.SolutionEntity, EmoteId);
//        //}
//    }
//}
