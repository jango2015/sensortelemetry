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
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Config;

namespace Porrey.SensorTelemetry.MobileService
{
	public static class WebApiConfig
	{
		public static void Register()
		{
			ConfigOptions options = new ConfigOptions();
			HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

			// To display errors in the browser during development, uncomment the following
			// line. Comment it out again when you deploy your service for production use.
			//config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

			// ***
			// *** Add SignalR
			// ***
			SignalRExtensionConfig.Initialize();
		}
	}
}

