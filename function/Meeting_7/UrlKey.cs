//using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace Meeting_7
{
    public class UrlKey : TableEntity
    {
        public int Id { get; set; }
    }
    public class UrlData : TableEntity
    {
        public string Url { get; set; }

        public int Count { get; set; }
    }
}
