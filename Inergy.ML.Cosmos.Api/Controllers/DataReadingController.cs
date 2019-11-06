using Inergy.ML.Service.Cosmos;
using Inergy.Tools.Architecture.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imergy.ML.Cosmos.Api.Controllers
{
    [ApiController]
    [Route("api/data_readings")]
    public class DataReadingController : ControllerBase
    {
        private readonly IDataReadingService dataReadingService;

        public DataReadingController(IDataReadingService dataReadingService)
        {
            this.dataReadingService = dataReadingService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<DataReading>> Get(string cups, DateTime dateBegin, DateTime dateEnd)
        {
            return this.dataReadingService.GetDataReadings(cups, dateBegin, dateEnd).ToList();
        }

        [HttpPost]
        public IActionResult Create(IEnumerable<DataReading> dataReadings)
        {
            this.dataReadingService.CreateDataReadings(dataReadings);

            return NoContent();
        }

        [HttpPut]
        public IActionResult Update(IEnumerable<DataReading> dataReadings)
        {
            this.dataReadingService.UpdateDataReadings(dataReadings);

            return NoContent();
        }

        [HttpDelete]
        public IActionResult Delete(string cups, DateTime dateBegin, DateTime dateEnd)
        {
            this.dataReadingService.DeleteDataReadings(cups, dateBegin, dateEnd);

            return NoContent();
        }
    }
}
