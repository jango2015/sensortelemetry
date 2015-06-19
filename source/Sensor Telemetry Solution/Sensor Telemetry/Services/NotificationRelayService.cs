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
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;

namespace Porrey.SensorTelemetry.Services
{
	/// <summary>
	/// This service relays commands between the internal Event Aggregator and Signal
	/// so that messages sent are relayed to and from other clients.
	/// </summary>
	public class NotificationRelayService : IBackgroundService
	{
		private HubConnection _hubConnection { get; set; }
		private IHubProxy _proxy { get; set; }

		private EventRelayMap<TemperatureChangedEventArgs> _temperatureChangedEventMap = null;
		private EventRelayMap<DeviceCommandEventArgs> _deviceCommandEventMap = null;

		public string Name => "Notification Relay";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IMobileServicesConfiguration MobileServicesConfiguration { get; set; }

		[Dependency]
		protected IApplicationInstanceIdentity ApplicationInstanceIdentity { get; set; }

		public async Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Configure and connect to the SignalR hub
				// ***
				_hubConnection = new HubConnection(this.MobileServicesConfiguration.Url);
				_hubConnection.Headers["x-zumo-application"] = this.MobileServicesConfiguration.ApplicationKey;
				this._proxy = this._hubConnection.CreateHubProxy("RelayHub");
				await this._hubConnection.Start();

				// ***
				// *** Link the TemperatureChangedEventArgs to SignalR
				// ***
				_temperatureChangedEventMap = new EventRelayMap<TemperatureChangedEventArgs>(this.ApplicationInstanceIdentity,
												_hubConnection,
												_proxy,
												this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>(),
												(e) => { e.SensorReading.Source = ApplicationSensorReadingSource.Cloud;  return e; },
												(e) => { return e.SensorReading.Source == ApplicationSensorReadingSource.Device; },
												(e) => { return e; },
												(e) => { return e.SensorReading.Source == ApplicationSensorReadingSource.Device; });

				// ***
				// *** Link the DeviceCommandEventArgs to SignalR
				// ***
				_deviceCommandEventMap = new EventRelayMap<DeviceCommandEventArgs>(this.ApplicationInstanceIdentity,
												_hubConnection,
												_proxy,
												this.EventAggregator.GetEvent<Events.DeviceCommandEvent>(),
												(e) => { return e; },
												(e) => { return true; },
												(e) => { return e; },
												(e) => { return true; });

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}

			return returnValue;
		}

		public Task<bool> Stop()
		{
			bool returnValue = false;

			// ***
			// *** Release the proxy
			// ***
			_proxy = null;

			// ***
			// *** Release the hub
			// ***
			if (_hubConnection != null)
			{
				_hubConnection.Dispose();
				_hubConnection = null;
			}

			return Task<bool>.FromResult(returnValue);
		}
	}
}
