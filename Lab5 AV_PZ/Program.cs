using System;
using System.Collections.Generic;
using System.Linq;

// Клас для представлення потенційного заходу зі зменшення або усунення ризику (Етап 3)
public class MitigationAction
{
    public string Code { get; set; } // ev1, ev2, ...
    public string Description { get; set; } // Опис заходу
}

// Клас для представлення ризикової події (Етап 1.2)
public class RiskEvent
{
    public string Code { get; set; }
    public string Description { get; set; }
    public double Probability { get; set; } // Ймовірність настання (er_i^p)
    public double Loss { get; set; }        // Можливий збиток (lrer_i^p)
    public double VRER { get; private set; } // Величина ризику (vrer_i^p)

    // Показники після застосування заходу (Етап 4)
    public MitigationAction Action { get; set; }
    public double PostMitigationProbability { get; set; } // erper_i^p
    public double PostMitigationLoss { get; set; }        // elrer_i^p
    public double EVRER { get; private set; } // Величина ризику після заходу (evrer_i^p)
    public double Reduction { get; private set; } // Зменшення ризику

    public string Priority { get; set; } // Рівень пріоритету

    public RiskEvent(string code, string desc, double prob, double loss)
    {
        Code = code;
        Description = desc;
        Probability = prob;
        Loss = loss;
        // Обчислення початкової величини ризику
        VRER = Probability * Loss;
        // Ініціалізація значень після заходу
        PostMitigationProbability = Probability;
        PostMitigationLoss = Loss;
        EVRER = VRER;
    }

    public void EvaluateAfterMitigation()
    {
        // Обчислюємо EVRER, лише якщо був призначений захід та змінені оцінки
        if (Action != null && (PostMitigationProbability != Probability || PostMitigationLoss != Loss))
        {
            EVRER = PostMitigationProbability * PostMitigationLoss;
            Reduction = VRER - EVRER;
        }
        else
        {
            // Якщо захід не призначений або оцінки не змінені, ризик залишається незмінним
            EVRER = VRER;
            Reduction = 0;
        }
    }
}

public class RiskManager
{
    private List<RiskEvent> riskEvents;
    private List<MitigationAction> actions;

    public RiskManager()
    {
        // 1. Сформована множина заходів
        actions = new List<MitigationAction>
        {
            new MitigationAction { Code = "ev1", Description = "Попереднє навчання членів проектного колективу" },
            new MitigationAction { Code = "ev2", Description = "Створення груп в різних соціальних мережах і онлайн зустрічі" },
            new MitigationAction { Code = "ev3", Description = "Використання експертних систем оцінки/прогнозування" },
            new MitigationAction { Code = "ev8", Description = "Тренінг/дослідження з вивчення необхідних інструментів (профілювання Unity)" },
            new MitigationAction { Code = "ev16", Description = "Використання генератора програмного коду" },
            new MitigationAction { Code = "ev20", Description = "Деталізація графіку та буферизація часу" }
        };

        // 2. Ідентифікація та початкова оцінка ризиків
        riskEvents = new List<RiskEvent>
        {
            // Технічні та Планові ризики (були раніше)
            new RiskEvent("t8_R", "Неефективність програмного коду (AI/генерація)", 0.8, 0.9), // vrer=0.72
            new RiskEvent("t10_R", "Швидкість виявлення дефектів у програмному коді є нижчою від запланованих термінів", 0.7, 0.8), // vrer=0.56
            new RiskEvent("p2_R", "Порушення графіка виконання робіт у компанії-розробника ПЗ", 0.7, 0.9), // vrer=0.63
            new RiskEvent("p6_R", "Недооцінювання тривалості етапів (складність процедурної генерації)", 0.9, 0.6), // vrer=0.54
            new RiskEvent("m2_R", "Низька взаємодія між членами команди виконавців ПЗ", 0.6, 0.8), // vrer=0.48
            new RiskEvent("m9_R", "Неможливість організації необхідного навчання персоналу", 0.7, 0.7), // vrer=0.49
            new RiskEvent("m15_R", "Нереалістичне прогнозування результатів", 0.8, 0.7) // vrer=0.56
        };
    }

    // Етап 2.4: Встановлення рівня пріоритету та ранжування ризиків
    public void PrioritizeRisks()
    {
        Console.WriteLine("\n=== АНАЛІЗ РИЗИКІВ: ПРІОРИТИЗАЦІЯ ===");

        // Знаходження min та max
        double min = riskEvents.Min(e => e.VRER);
        double max = riskEvents.Max(e => e.VRER);
        double range = max - min;

        // Використовуємо 3 рівні пріоритету, ділячи діапазон на 3 частини.
        double mpr = range / 3.0;

        double boundaryLow = min + mpr;
        double boundaryMedium = min + 2 * mpr;

        Console.WriteLine($"[I] Мінімальна величина ризику (min): {min:F2}");
        Console.WriteLine($"[I] Максимальна величина ризику (max): {max:F2}");
        Console.WriteLine($"[I] Крок пріоритету (mpr): {mpr:F2}");
        Console.WriteLine($"[I] Інтервали: Низький (VRER < {boundaryLow:F2}), Середній ({boundaryLow:F2} <= VRER < {boundaryMedium:F2}), Високий (VRER >= {boundaryMedium:F2})");

        foreach (var e in riskEvents.OrderByDescending(r => r.VRER))
        {
            if (e.VRER >= boundaryMedium)
                e.Priority = "Високий (HER)";
            else if (e.VRER >= boundaryLow)
                e.Priority = "Середній (MER)";
            else
                e.Priority = "Низький (LER)";

            Console.WriteLine($"- {e.Code} ({e.Description}): VRER={e.VRER:F2} -> {e.Priority}");
        }
    }

    // Етап 3: Планування ризиків (призначення заходу)
    public void PlanMitigation()
    {
        Console.WriteLine("\n=== ПЛАНУВАННЯ РИЗИКІВ: ПРИЗНАЧЕННЯ ЗАХОДІВ ===");

        // Призначаємо заходи, забезпечуючи, щоб Action не був null
        riskEvents.First(e => e.Code == "t8_R").Action = actions.First(a => a.Code == "ev16"); // Код
        riskEvents.First(e => e.Code == "t10_R").Action = actions.First(a => a.Code == "ev8"); // Дефекти
        riskEvents.First(e => e.Code == "p2_R").Action = actions.First(a => a.Code == "ev20"); // Графік
        riskEvents.First(e => e.Code == "p6_R").Action = actions.First(a => a.Code == "ev1"); // Тривалість
        riskEvents.First(e => e.Code == "m2_R").Action = actions.First(a => a.Code == "ev2"); // Взаємодія
        riskEvents.First(e => e.Code == "m9_R").Action = actions.First(a => a.Code == "ev1"); // Навчання
        riskEvents.First(e => e.Code == "m15_R").Action = actions.First(a => a.Code == "ev3"); // Прогнозування

        // Виведення результатів призначення заходів
        foreach (var e in riskEvents.OrderByDescending(r => r.VRER))
        {
            Console.WriteLine($"[+] Ризик {e.Code} ({e.Priority}): Захід: {e.Action.Code} - {e.Action.Description}");
        }
    }

    // Етап 4: Моніторинг та оцінювання після застосування заходів
    public void MonitorAndEvaluate()
    {
        Console.WriteLine("\n=== МОНІТОРИНГ РИЗИКІВ: ОЦІНКА ЕФЕКТИВНОСТІ ===");

        // Імітація експертних оцінок після впровадження заходів

        // Ризики, що були раніше
        riskEvents.First(e => e.Code == "t8_R").PostMitigationProbability = 0.4; // 0.8 -> 0.4
        riskEvents.First(e => e.Code == "t8_R").PostMitigationLoss = 0.5;        // 0.9 -> 0.5

        riskEvents.First(e => e.Code == "t10_R").PostMitigationProbability = 0.5; // 0.7 -> 0.5
        riskEvents.First(e => e.Code == "t10_R").PostMitigationLoss = 0.7;        // 0.8 -> 0.7

        riskEvents.First(e => e.Code == "p2_R").PostMitigationProbability = 0.5; // 0.7 -> 0.5
        riskEvents.First(e => e.Code == "p2_R").PostMitigationLoss = 0.6;        // 0.9 -> 0.6

        riskEvents.First(e => e.Code == "p6_R").PostMitigationProbability = 0.6; // 0.9 -> 0.6
        riskEvents.First(e => e.Code == "p6_R").PostMitigationLoss = 0.4;        // 0.6 -> 0.4

        riskEvents.First(e => e.Code == "m2_R").PostMitigationProbability = 0.3; // 0.6 -> 0.3
        riskEvents.First(e => e.Code == "m2_R").PostMitigationLoss = 0.6;        // 0.8 -> 0.6 (Зменшення: 0.48 - 0.18 = 0.30)

        riskEvents.First(e => e.Code == "m9_R").PostMitigationProbability = 0.4; // 0.7 -> 0.4
        riskEvents.First(e => e.Code == "m9_R").PostMitigationLoss = 0.5;        // 0.7 -> 0.5 (Зменшення: 0.49 - 0.20 = 0.29)

        riskEvents.First(e => e.Code == "m15_R").PostMitigationProbability = 0.5; // 0.8 -> 0.5
        riskEvents.First(e => e.Code == "m15_R").PostMitigationLoss = 0.5;        // 0.7 -> 0.5 (Зменшення: 0.56 - 0.25 = 0.31)


        Console.WriteLine("| Код | VRER (До) | Пріоритет | Захід | PostProb | PostLoss | EVRER (Після) | Зменшення | Ефективність |");
        Console.WriteLine("|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|");

        foreach (var e in riskEvents.OrderByDescending(r => r.Reduction)) // Сортуємо за зменшенням
        {
            e.EvaluateAfterMitigation(); // Обчислення EVRER та Reduction

            // Якщо захід не призначено, виводимо відповідне повідомлення
            string actionCode = e.Action?.Code ?? "N/A";
            string effectiveness = e.Reduction > 0 ? "Ефективний" : "Не вплинув";

            // Якщо для ризику не було змінено оцінок, але захід призначено, виводимо "Нейтральний"
            if (e.Reduction == 0 && e.Action != null) effectiveness = "Нейтральний (0)";

            // Форматуємо вивід
            Console.WriteLine($"| {e.Code} | {e.VRER:F2} | {e.Priority} | {actionCode} | {e.PostMitigationProbability:F2} | {e.PostMitigationLoss:F2} | {e.EVRER:F2} | {e.Reduction:F2} | {effectiveness} |");
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Встановлення кодування для коректного відображення українських символів
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=================================================");
        Console.WriteLine("     ПЗ \"УПРАВЛІННЯ РИЗИКАМИ РОЗРОБЛЕННЯ ПЗ\"");
        Console.WriteLine("      (Для 3D Rogue-like гри на Unity/C#)");
        Console.WriteLine("=================================================");

        try
        {
            var manager = new RiskManager();

            // 1. Аналіз та пріоритизація
            manager.PrioritizeRisks();

            // 2. Планування заходів
            manager.PlanMitigation();

            // 3. Оцінка ефективності заходів
            manager.MonitorAndEvaluate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ПОМИЛКА ВИКОНАННЯ]: Сталася непередбачена помилка: {ex.Message}");
            Console.WriteLine("Перевірте, чи всі коди ризиків та заходів збігаються з тими, що були ініціалізовані.");
        }
    }
}

//КОМЕНТАР ДЛЯ ДЕМОНСТРАЦІЇ ДОДАВАННЯ ЗМІН