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
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Mobile.Service;
using Porrey.SensorTelemetry.Shared.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;

namespace Porrey.SensorTelemetry.MobileService.Controllers
{
	/// <summary>
	/// SignalR Relay Hub
	/// </summary>
	public class RelayHub : Hub
	{
		public ApiServices Services { get; set; }

		/// <summary>
		/// Sends a sensor reading to all connected clients.
		/// </summary>
		/// <param name="e">The temperature changed event arguments.</param>
		public void SendTemperatureChangedEvent(TemperatureChangedEventArgs e)
		{
			this.Clients.All.OnTemperatureChangedEvent(e);
		}

		/// <summary>
		/// Sends a device command to all connected clients.
		/// </summary>
		/// <param name="e">The device command event arguments.</param>
		public void SendDeviceCommandEvent(DeviceCommandEventArgs e)
		{
			this.Clients.All.OnDeviceCommandEvent(e);
		}

		/// <summary>
		/// This method is used for testing the service
		/// </summary>
		/// <param name="message">A message to be sent.</param>
		/// <returns>Returns the message that was passed to the method.</returns>
		public string Ping(string message)
		{
			this.Clients.All.OnPing(message);
			return message;
		}
	}

	public class TestClass
	{
		public string message { get; set; }
	}
}