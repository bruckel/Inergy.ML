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
        public ActionResult<IEnumerable<DataReading>> Get(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone = "Europe/Madrid")
        {
            try
            {
                return this.dataReadingService.GetDataReadings(cups, dateBegin, dateEnd, timeZone).ToList();
            }
            catch
            {
                return this.Problem("Error");
            }
            
        }

        [HttpPost]
        public IActionResult Create(IEnumerable<DataReading> dataReadings, string timeZone = "Europe/Madrid")
        {
            try
            {
                this.dataReadingService.CreateDataReadings(dataReadings, timeZone);

                return this.Content("Success");
            }
            catch
            {
                return this.Problem("Error");
            }
        }

        [HttpPut]
        public IActionResult Update(IEnumerable<DataReading> dataReadings, string timeZone = "Europe/Madrid")
        {
            try
            {
                this.dataReadingService.UpdateDataReadings(dataReadings, timeZone);

                return this.Content("Success");
            }
            catch
            {
                return this.Problem("Error");
            }
        }

        [HttpDelete]
        public IActionResult Delete(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone = "Europe/Madrid")
        {
            try
            {
                this.dataReadingService.DeleteDataReadings(cups, dateBegin, dateEnd, timeZone);

                return this.Content("Success");
            }
            catch
            {
                return this.Problem("Error");
            }
        }
    }
}
