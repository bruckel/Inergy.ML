using Inergy.ML.Service.Cosmos;
using Inergy.Tools.Architecture.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imergy.ML.Cosmos.Api.Controllers
{
    [ApiController]
    [Route("api/data_readings")]
    [Authorize]
    public class DataReadingController : ControllerBase
    {
        private readonly IDataReadingService dataReadingService;

        public DataReadingController(IDataReadingService dataReadingService)
        {
            this.dataReadingService = dataReadingService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<DataReading>> Get()
        {
            //try
            //{
            //    return Enumerable.Range(0, 24).Select(i => new DataReading
            //    {
            //        Cups = "X12798739123123T",
            //        IdEnergySource = 0,
            //        Type = 0,
            //        TimeStamp = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, i, 0, 0),
            //        Unit = "kWh",
            //        Value = 34
            //    }).ToList();
            //}
            //catch
            //{
            //    return this.Problem("Error");
            //}

            return this.Problem("Error");
        }

        //[HttpGet]
        //public ActionResult<IEnumerable<DataReading>> Get(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone = "Europe/Madrid")
        //{
        //    try
        //    {
        //        return this.dataReadingService.GetDataReadings(cups, dateBegin, dateEnd, timeZone).ToList();
        //    }
        //    catch
        //    {
        //        return this.Problem("Error");
        //    }

        //}

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
