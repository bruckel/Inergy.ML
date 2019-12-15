using Inergy.ML.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Inergy.ML.Service
{
    public class ApiService : IApiService
    {
        private readonly ApiContext apiContext;

        public ApiService(ApiContext apiContext)
        {
            this.apiContext = apiContext;
        }

        public IEnumerable<string> GetCups()
        {
            return this.apiContext.DataReadingMigrationConfigs.GroupBy(d => d.Cups).Select(d => d.Key);
        }
    }
}
