using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NCrontab;
using Cronos;
using CronExpressionDescriptor;

interface ICronScheduler
{
    List<DateTime> GetNextOccurrences(string cronExpression, int count);
}

class NCrontabScheduler : ICronScheduler
{
    public List<DateTime> GetNextOccurrences(string cronExpression, int count)
    {
        var schedule = CrontabSchedule.Parse(cronExpression);
        DateTime now = DateTime.Now;  // ローカル時間
        List<DateTime> nextRuns = new List<DateTime>();

        for (int i = 0; i < count; i++)
        {
            now = schedule.GetNextOccurrence(now);
            nextRuns.Add(now);
        }

        return nextRuns;
    }
}

class CronosScheduler : ICronScheduler
{
    public List<DateTime> GetNextOccurrences(string cronExpression, int count)
    {
        var cronSchedule = CronExpression.Parse(cronExpression, CronFormat.Standard);
        DateTime now = DateTime.UtcNow; // CronosはUTCベース
        List<DateTime> nextRuns = new List<DateTime>();

        for (int i = 0; i < count; i++)
        {
            now = cronSchedule.GetNextOccurrence(now, TimeZoneInfo.Utc) 
                ?? throw new Exception("次の実行時刻が見つかりません");
            nextRuns.Add(now);
        }

        return nextRuns;
    }
}

// Cron 表記の説明を提供するクラス
class CronExpressionDescriptionProvider
{
    public static string GetDescription(string cronExpression, string locale = "ja")
    {
        try
        {
            return ExpressionDescriptor.GetDescription(cronExpression, new Options { Locale = locale });
        }
        catch (Exception ex)
        {
            return $"Cron式の説明を生成できません: {ex.Message}";
        }
    }
}

class Program
{
    static async Task Main()
    {
        Console.Write("使用するCronライブラリを選択してください (1: NCrontab, 2: Cronos): ");
        string choice = Console.ReadLine();
        ICronScheduler scheduler = choice == "1" ? new NCrontabScheduler() : new CronosScheduler();

        while (true)
        {
            Console.Write("Cron式を入力してください (終了するには 'exit' を入力): ");
            string cronExpression = Console.ReadLine();

            if (cronExpression.ToLower() == "exit")
            {
                break;
            }

            Console.Write("取得する実行日時の回数を入力してください: ");
            if (!int.TryParse(Console.ReadLine(), out int occurrences) || occurrences <= 0)
            {
                Console.WriteLine("無効な入力です。正の整数を入力してください。");
                continue;
            }

            try
            {
                // 次回の実行日時を取得
                List<DateTime> nextRuns = scheduler.GetNextOccurrences(cronExpression, occurrences);

                // Cron式の説明を取得
                string description = CronExpressionDescriptionProvider.GetDescription(cronExpression);

                Console.WriteLine("\n===========================");
                Console.WriteLine("Cron 式: " + cronExpression);
                Console.WriteLine("説明: " + description);
                Console.WriteLine("次の実行予定日時:");

                foreach (var runTime in nextRuns)
                {
                    Console.WriteLine(runTime);
                }
                Console.WriteLine("===========================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cron式の解析に失敗しました: {ex.Message}");
            }
        }
    }
}
