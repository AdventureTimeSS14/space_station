// using Content.Client.Corvax.TTS;
// using Robust.Client.UserInterface;
// using Robust.Shared.Random;

// namespace Content.Client.Lobby;

// public sealed partial class LobbyUIController
// {
//     [Dependency] private readonly IRobustRandom _rng = default!;
//     [UISystemDependency] private readonly TTSSystem _tts = default!;

//     private readonly List<string> _sampleText =
//     new()
//     {
//         "Это ведь трупы прошедших клонирование? Отлично. Ждите вкусных бургеров.",
//         "Вирус с галлюцинациями? Вирусолог играется? Массовый сход с ума? Это ловушка!",
//         "Я побуду в шкафчике. Минут через десять вылезу!",
//         "Идет ассистент по отбытию, видит шаттл горит - сел в него и сгорел."
//     };

//     public void PlayTTS()
//     {
//         // Test moment
//         if (_profile == null || _stateManager.CurrentState is not LobbyState)
//             return;

//         _tts.RequestGlobalTTS(_rng.Pick(_sampleText), _profile.Voice);
//     }
// }
