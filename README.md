## **Cron Scheduler - NCrontab & Cronos**
📅 **NCrontab & Cronos を使用して Cron スケジュールを解析し、次回の実行日時を取得する .NET コンソールアプリ**

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Language](https://img.shields.io/badge/language-C%23-blue.svg)

---

## **🛠️ 機能**
✅ **NCrontab / Cronos の両方をサポート**（好きなライブラリを選択）  
✅ **Cron 式の解析と説明を日本語で表示**（`CronExpressionDescriptor` を使用）  
✅ **指定した回数分の次回実行予定をリストアップ**  
✅ **簡単にカスタマイズ可能**

---

## **📦 インストール**
このプロジェクトをローカルにクローンし、必要なパッケージをインストールしてください。

```sh
git clone https://github.com/your-username/cron-scheduler.git
cd cron-scheduler
dotnet restore
```

**必要な NuGet パッケージ**
```sh
dotnet add package NCrontab
dotnet add package Cronos
dotnet add package CronExpressionDescriptor
```

---

## **🚀 使い方**
### **1️⃣ 実行方法**
プロジェクトをビルドして実行：
```sh
dotnet run
```

### **2️⃣ 使用方法**
1. **使用する Cron ライブラリを選択**
   - `1: NCrontab`（簡易スケジューリング）
   - `2: Cronos`（秒単位のスケジューリングが可能）
2. **Cron 式を入力**
3. **取得する実行回数を入力**
4. **次回の実行予定と日本語説明が表示される**

---

## **📜 サンプル実行結果**
### **✅ NCrontab の場合**
```
使用するCronライブラリを選択してください (1: NCrontab, 2: Cronos): 1
Cron式を入力してください (終了するには 'exit' を入力): 0 3 * * *
取得する実行日時の回数を入力してください: 3

===========================
Cron 式: 0 3 * * *
説明: 毎日 03:00 に
次の実行予定日時:
2025/02/05 03:00:00
2025/02/06 03:00:00
2025/02/07 03:00:00
===========================
```

### **✅ Cronos の場合**
```
使用するCronライブラリを選択してください (1: NCrontab, 2: Cronos): 2
Cron式を入力してください (終了するには 'exit' を入力): */15 * * * *
取得する実行日時の回数を入力してください: 3

===========================
Cron 式: */15 * * * *
説明: 毎 15 分ごとに
次の実行予定日時:
2025/02/04 12:15:00
2025/02/04 12:30:00
2025/02/04 12:45:00
===========================
```

---

## **📝 ソースコード**
```csharp
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
        DateTime now = DateTime.Now;
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
        DateTime now = DateTime.UtcNow;
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
                List<DateTime> nextRuns = scheduler.GetNextOccurrences(cronExpression, occurrences);
                string description = CronExpressionDescriptionProvider.GetDescription(cronExpression);

                Console.WriteLine("\n===========================");
                Console.WriteLine($"Cron 式: {cronExpression}");
                Console.WriteLine($"説明: {description}");
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
