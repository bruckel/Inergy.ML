using Inergy.ML.Data;
using Inergy.Tools.Architecture.Data.Mongo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Service.Cosmos
{
    public class DataReadingService : IDataReadingService
    {
        private readonly IDataReadingRepository dataReadingRepository;

        public DataReadingService(string connectionString, string database) : this(new DataReadingRepository(new MongoContext(connectionString, database), "LoadCurves"))
        {

        }

        public DataReadingService(IDataReadingRepository dataReadingRepository)
        {
            this.dataReadingRepository = dataReadingRepository;
        }


    }
}
