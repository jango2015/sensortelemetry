// Copyright © 2015 Daniel Porrey
//
// This file is part of Sensor Telemetry.
// 
// Sensor Telemetry is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Sensor Telemetry is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Sensor Telemetry.  If not, see http://www.gnu.org/licenses/.
//
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.WindowsAzure.Mobile.Service;

namespace Porrey.SensorTelemetry.MobileService
{
	public class SensorReadingController : TableController<SensorReading>
	{
		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			SensorReadingContext context = new SensorReadingContext();
			DomainManager = new EntityDomainManager<SensorReading>(context, Request, Services);
		}

		public IQueryable<SensorReading> GetAllSensorReadings() => this.Query();

		public SingleResult<SensorReading> GetSensorReading(string id) => this.Lookup(id);
	}
}