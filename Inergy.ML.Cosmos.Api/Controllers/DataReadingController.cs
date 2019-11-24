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
        
        /// <summary>
        /// Método Get: obtener datos de series temporales
        /// </summary>
        /// <param name="cups">Cups o identificador</param>
        /// <param name="dateBegin">Fecha de inicio</param>
        /// <param name="dateEnd">fecha de fin</param>
        /// <param name="timeZone">Zona horaria</param>
        /// <returns>Enumerable de objetos de series temporales</returns>
        [HttpGet]
        public ActionResult<IEnumerable<DataReading>> Get(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone = "Europe/Madrid")
        {
            try
            {
                return this.dataReadingService.GetDataReadings(cups, dateBegin, dateEnd, timeZone).ToList();
            }
            catch (Exception exception)
            {
                return this.Problem(exception.Message);
            }
        }

        /// <summary>
        /// Método Create: insertar series temporales
        /// </summary>
        /// <param name="dataReadings">Lista de series temporales</param>
        /// <param name="timeZone">Zona horaria</param>
        /// <returns>Lista de series temporales insertados</returns>
        [HttpPost]
        public IActionResult Create(IEnumerable<DataReading> dataReadings, string timeZone = "Europe/Madrid")
        {
            try
            {
                this.dataReadingService.CreateDataReadings(dataReadings, timeZone);

                return this.Ok(dataReadings);
            }
            catch (Exception exception)
            {
                return this.Problem(exception.Message);
            }
        }

        /// <summary>
        /// Método Put: actualizar series tenporales
        /// </summary>
        /// <param name="dataReadings">Lista de series temporales</param>
        /// <param name="timeZone">Zona horaria</param>
        /// <returns>Lista de series temporales actualizados</returns>
        [HttpPut]
        public IActionResult Update(IEnumerable<DataReading> dataReadings, string timeZone = "Europe/Madrid")
        {
            try
            {
                this.dataReadingService.UpdateDataReadings(dataReadings, timeZone);

                return this.Ok(dataReadings);
            }
            catch (Exception exception)
            {
                return this.Problem(exception.Message);
            }
        }

        /// <summary>
        /// Método Delete: eliminar series temporales
        /// </summary>
        /// <param name="cups">Cups o identificador</param>
        /// <param name="dateBegin">Fecha de inicio</param>
        /// <param name="dateEnd">fecha de fin</param>
        /// <param name="timeZone">Zona horaria</param>
        /// <returns>Response Ok</returns>
        [HttpDelete]
        public IActionResult Delete(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone = "Europe/Madrid")
        {
            try
            {
                this.dataReadingService.DeleteDataReadings(cups, dateBegin, dateEnd, timeZone);

                return this.Ok();
            }
            catch (Exception exception)
            {
                return this.Problem(exception.Message);
            }
        }
    }
}
