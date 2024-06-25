using System.Text.RegularExpressions;
using Content.Server.Speech.Components;


namespace Content.Server.Speech.EntitySystems;

public sealed class DeutschAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeutschAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, DeutschAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // Changed By Дубик :3 the beginning of German speech
        
        message = Regex.Replace(message, "Что", "Was");
        message = Regex.Replace(message, "что", "was");

        message = Regex.Replace(message, "Здравствуйте", "Guten tag");
        message = Regex.Replace(message, "здравствуйте", "guten tag");

        message = Regex.Replace(message, "Почему", "Warum");
        message = Regex.Replace(message, "почему", "warum");

        message = Regex.Replace(message, "Как", "Wie");
        message = Regex.Replace(message, "как", "wie");

        message = Regex.Replace(message, "Пожалуйста", "Bitte sehr");
        message = Regex.Replace(message, "пожалуйста", "bitte sehr");

        message = Regex.Replace(message, "Капитан", "Führer");
        message = Regex.Replace(message, "капитан", "führer");

        message = Regex.Replace(message, "Хорошо", "Gut");
        message = Regex.Replace(message, "хорошо", "gut");
        
        message = Regex.Replace(message, "Мой", "Mein");
        message = Regex.Replace(message, "мой", "mein");

        message = Regex.Replace(message, "Да", "Ja");
        message = Regex.Replace(message, "да", "ja");
        
        message = Regex.Replace(message, "Нет", "Nein");
        message = Regex.Replace(message, "нет", "nein");

        message = Regex.Replace(message, "Отлично", "Ausgezeichnet");
        message = Regex.Replace(message, "отлично", "ausgezeichnet");

        message = Regex.Replace(message, "Восхитительно", "Wunderschoen");
        message = Regex.Replace(message, "восхитительно", "wunderschoen");

        message = Regex.Replace(message, "Прекрасно", "Schon");
        message = Regex.Replace(message, "прекрасно", "schon");

        message = Regex.Replace(message, "Очень", "Sehr");
        message = Regex.Replace(message, "очень", "sehr");

        message = Regex.Replace(message, "Ассистент", "Assistent");
        message = Regex.Replace(message, "ассистент", "assistent");

        message = Regex.Replace(message, "Свинья", "Schwein");
        message = Regex.Replace(message, "свинья", "schwein");

        message = Regex.Replace(message, "Ты", "Du");
        message = Regex.Replace(message, "ты", "du");

        message = Regex.Replace(message, "Спасибо", "Danke");
        message = Regex.Replace(message, "спасибо", "danke");

        message = Regex.Replace(message, "Женщина", "Frau");
        message = Regex.Replace(message, "женщина", "frau");

        message = Regex.Replace(message, "Человек", "Mensch");
        message = Regex.Replace(message, "человек", "mensch");

        message = Regex.Replace(message, "Привет", "Hallo");
        message = Regex.Replace(message, "привет", "hallo");

        message = Regex.Replace(message, "Сб", "Polizei");  
        message = Regex.Replace(message, "СБ", "POLIZEI");  

        message = Regex.Replace(message, "Си", "Chief");  
        message = Regex.Replace(message, "СИ", "Chief");  

        message = Regex.Replace(message, "ГВ", "Chefarzt");  
        message = Regex.Replace(message, "Гв", "Chefarzt");  
 
        message = Regex.Replace(message, "НР", "Doktorvater");  
        message = Regex.Replace(message, "Нр", "Doktorvater");  

        message = Regex.Replace(message, "Капитан", "Führer");
        message = Regex.Replace(message, "капитан", "führer");

        message = Regex.Replace(message, "Капитана", "Führer'a");
        message = Regex.Replace(message, "капитана", "führer'a");

        message = Regex.Replace(message, "Кеп", "Führer");
        message = Regex.Replace(message, "кеп", "führer");

        message = Regex.Replace(message, "Кепа", "Führer'a");
        message = Regex.Replace(message, "кепа", "führer'a");

        message = Regex.Replace(message, "Мы", "Wir");
        message = Regex.Replace(message, "мы", "wir");

        message = Regex.Replace(message, "Пиво", "Bier");
        message = Regex.Replace(message, "пиво", "bier");

        message = Regex.Replace(message, "Пива", "Bier");
        message = Regex.Replace(message, "пива", "bier");

        message = Regex.Replace(message, "Блять", "Scheibe");
        message = Regex.Replace(message, "блять", "scheibe");

        message = Regex.Replace(message, "Бля", "Scheibe");
        message = Regex.Replace(message, "бля", "scheibe");

        message = Regex.Replace(message, "Сука", "Huяndin");
        message = Regex.Replace(message, "сука", "huяndin");

        message = Regex.Replace(message, "Идиот", "Dummkopf");
        message = Regex.Replace(message, "идиот", "dummkopf");

        message = Regex.Replace(message, "Идиоты", "Dummkopf");
        message = Regex.Replace(message, "идиоты", "Dummkopf");

        message = Regex.Replace(message, "Вода", "Wsser");
        message = Regex.Replace(message, "вода", "wsser");

        message = Regex.Replace(message, "Воды", "Wasser");
        message = Regex.Replace(message, "воды", "wasser");

        message = Regex.Replace(message, "ГП", "Leiter des Personals");
        message = Regex.Replace(message, "Гп", "Leiter des Personals");

        message = Regex.Replace(message, "ГСБ", "Leiter des Sicherheitsdienstes");
        message = Regex.Replace(message, "Глава Службы Безопасности", "Leiter des Sicherheitsdienstes");

        message = Regex.Replace(message, "КМ", "Quartiermeister");
        message = Regex.Replace(message, "Км", "Quartiermeister");
        message = Regex.Replace(message, "Квартирмейстер", "Quartiermeister");

        message = Regex.Replace(message, "ЯО", "Terroristen");
        message = Regex.Replace(message, "Яо", "Terroristen");
        message = Regex.Replace(message, "Ядерные оперативники", "Terroristen");

        message = Regex.Replace(message, "Похуй", "Scheib");
        message = Regex.Replace(message, "Похую", "Scheib");

        message = Regex.Replace(message, "Пошел нахуй", "Leck mich");
        message = Regex.Replace(message, "Пошли нахуй", "Leck mich");

        // Changed By Дубик :3 the stop of German speech
        
        args.Message = message;
    }
}