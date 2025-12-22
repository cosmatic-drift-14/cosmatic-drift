// using Content.Shared._CD.Species;
// using Content.Shared.GameTicking;
//
// namespace Content.Server._CD.Species;
//
// public sealed class CustomSpeciesNameSystem : SharedCustomSpeciesNameSystem
// {
//     public override void Initialize()
//     {
//         base.Initialize();
//
//         SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
//     }
//
//     private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
//     {
//         var player = args.Mob;
//         var name = args.Profile.CDCustomSpeciesName;
//         if (name == null)
//             return;
//         AddComp(player, new CustomSpeciesNameComponent { NewSpeciesName = name });
//     }
// }
