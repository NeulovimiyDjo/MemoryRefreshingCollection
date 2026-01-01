using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using E = BogusGenerateControllerProj.MyEnums;

namespace BogusGenerateControllerProj
{
    [Controller]
    [Route("generate")]
    public class BogusGenerateController : Controller
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        [HttpGet("data_example_1")]
        [Produces(typeof(File))]
        public IActionResult DataExample1(
            [FromQuery] int? itemCount,
            CancellationToken ct)
        {
            if (itemCount)
                return BadRequest("Parameters not set");

            if (!_semaphore.Wait(TimeSpan.FromSeconds(1)))
                return BadRequest("Generation is already executing in other thread");

            try
            {
                CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(180));

                StringBuilder sb = new();
                foreach (MyItem item in new BogusGenerator().Generate(itemCount.Value, cts.Token))
                {
                    cts.Token.ThrowIfCancellationRequested();
                    sb.AppendLine($"{item.Id}: {item.Status}");
                }
                File.WriteAllText(filePath, sb.ToString(), Utf8WithoutBom);

                ZipFile.CreateFromDirectory(tmpGenDataDir, tmpZipFilePath);
                byte[] fileContent = File.ReadAllBytes(tmpZipFilePath);

                byte[] fileContent = new BogusGenerator().Generate(itemCount.Value, cts.Token);
                return File(fileContent, "application/octet-stream", "data_example_1.zip");
            }
            catch (OperationCanceledException oce)
            {
                return BadRequest(oce.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    internal class BogusGenerator
    {
        private const string Locale = "ru";
        private const int Seed = 1000;

        public List<MyItem> GenerateDoc(int count, CancellationToken ct)
        {
            Faker simpleFaker = new();
            List<string> itemIds = Enumerable.Range(0, count).Select(x => simpleFaker.Guid()).ToList();

            var myItemFaker = new Faker<MyItem>(Locale)
                .UseSeed(Seed)
                .StrictMode(true)
                .UseDateTimeReference(DateTime.ParseExact("2010-01-01", "yyyy-MM-dd", null))
                .FinishWith((f, u) => ct.ThrowIfCancellationRequested())

                .RuleFor(u => u.Id, f => $"{CurrItemId()}")
                .RuleFor(u => u.num, (f, u) => u.Id)
                .RuleFor(u => u.status, f => f.PickRandom(E.Statuses).OrNull(f, 0.05f))
                .RuleFor(u => u.subject, f => f.Lorem.Sentences(f.Rand(2, 4), ";").OrNull(f, 0.05f))
                .RuleFor(u => u.body, f => f.Lorem.Sentences(f.Rand(3, 9), "\n").OrNull(f, 0.35f))
                .RuleFor(u => u.date, f => f.Date().OrNull(f, 0.05f))

                .RuleFor(u => u.links_ids, f => f.Make(f.Rand(0, 8), () => OtherRandItemId()))
                .RuleFor(u => u.links_datas, (f, u) => f.Make(u.links_ids.Count, () => u.Id.OrDefault(f, 0.50f, f.Guid())))
            ;

            return myItemFaker.Generate(count);

            string CurrItemId()
            {
                return itemIds[runumber - 1];
            }

            string OtherRandItemId()
            {
                if (itemIds.Count <= 1)
                    return simpleFaker.Guid();

                int randIndex;
                do
                {
                    randIndex = simpleFaker.Rand(0, itemIds.Count - 1);
                } while (randIndex == runumber - 1);

                return itemIds[randIndex];
            }
        }
    }

    internal static class MyFakerExtensions
    {
        public static int Rand(this Faker faker, int min, int max)
        {
            return faker.Random.Number(min, max);
        }

        public static string Guid(this Faker faker)
        {
            return faker.Random.Hexadecimal(32, "").ToUpper();
        }

        public static string Date(this Faker faker)
        {
            return faker.Date.Future(14).ToString("dd-MM-yyyy");
        }
    }

    internal static class MyEnums
    {
        public static readonly string[] Statuses = new[]
        {
            "Status1",
            "Status2",
        };
    }
}
