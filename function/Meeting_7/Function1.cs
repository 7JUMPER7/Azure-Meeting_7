using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;

namespace Meeting_7
{
    public static class Function1
    {
        [FunctionName("Set")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Table("urls", "1", "Key", Take = 1)] UrlKey keyTable,
            [Table("urls")] CloudTable outTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string href = req.Query["href"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            href = href ?? data?.href;
            if (keyTable == null)
            {
                keyTable = new UrlKey { PartitionKey = "1", RowKey = "Key", Id = 1024 };
                TableOperation insertKey = TableOperation.Insert(keyTable);
                await outTable.ExecuteAsync(insertKey);
            }
            string s = String.Empty;
            int idx = keyTable.Id;
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            while (idx > 0)
            {
                s += alphabet[idx % alphabet.Length];
                idx /= alphabet.Length;
            }
            string shortUrl = string.Join(string.Empty, s.Reverse());
            UrlData row = new UrlData { PartitionKey = $"{shortUrl[0]}", RowKey = shortUrl, Url = href, Count = 1 };
            TableOperation operation = TableOperation.Insert(row);
            await outTable.ExecuteAsync(operation);
            keyTable.Id++;
            operation = TableOperation.Replace(keyTable);
            await outTable.ExecuteAsync(operation);
            return !string.IsNullOrEmpty(href)
                ? (ActionResult)new OkObjectResult(new { status = 0, shortLink = shortUrl })
                : new BadRequestObjectResult(new { status = 1, message = "Need href"});
        }

        [FunctionName("Go")]
        public static async Task<IActionResult> Go(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "go/{shortUrl}")] HttpRequest req,
            [Table("urls")] CloudTable cloudTable,
            string shortUrl,
            [Queue("counts")] IAsyncCollector<string> queue)
        {
            if (string.IsNullOrEmpty(shortUrl))
            {
                return new BadRequestObjectResult(new { status = 1, message = "Route doesn't found" });
            }
            shortUrl = shortUrl.ToUpper();
            TableOperation operation = TableOperation.Retrieve<UrlData>($"{shortUrl[0]}", shortUrl);
            TableResult result = await cloudTable.ExecuteAsync(operation);
            if (result.Result != null && result.Result is UrlData data)
            {
                await queue.AddAsync(data.RowKey);
                return new RedirectResult(data.Url);
            }
            return new RedirectResult("https://www.google.com.ua/webhp?authuser=0");
        }

        [FunctionName("Counter")]
        public static async Task Counter(
            [QueueTrigger("counts")]string queue,
            [Table("urls")]CloudTable cloudTable)
        {
            TableOperation operation = TableOperation.Retrieve<UrlData>($"{queue[0]}", queue);
            TableResult result = await cloudTable.ExecuteAsync(operation);
            if (result.Result != null && result.Result is UrlData data)
            {
                data.Count++;
                operation = TableOperation.Replace(data);
                await cloudTable.ExecuteAsync(operation);
            }
        }
    }
}
