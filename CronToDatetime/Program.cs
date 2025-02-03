using System;
using System.Collections.Generic;
using NCrontab;

class Program
{
    static void Main()
    {
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
                var schedule = CrontabSchedule.Parse(cronExpression);
                DateTime now = DateTime.Now;
                List<DateTime> nextRuns = new List<DateTime>();

                for (int i = 0; i < occurrences; i++)
                {
                    now = schedule.GetNextOccurrence(now);
                    nextRuns.Add(now);
                }

                Console.WriteLine("次の実行予定日時:");
                foreach (var runTime in nextRuns)
                {
                    Console.WriteLine(runTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cron式の解析に失敗しました: {ex.Message}");
            }
        }
    }
}
