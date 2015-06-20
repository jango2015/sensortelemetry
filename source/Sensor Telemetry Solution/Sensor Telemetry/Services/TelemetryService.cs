// Copyright © 2015 Daniel Porrey
//
// This file is part of the Sensor Telemetry solution.
// 
// Sensor Telemetry is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Sensor Telemetry is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Sensor Telemetry. If not, see http://www.gnu.org/licenses/.
//
using System;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;

namespace Porrey.SensorTelemetry.Services
{
	public class TelemetryService : IBackgroundService
	{
		/// <summary>
		/// Private structure used to feed the Azure Service Bus Hub. The
		/// Stream Analytics job is sensitive to changes. This structure is
		/// used to match the database.
		/// </summary>
		private struct SensorReading
		{
			public DateTimeOffset TimestampUtc;
			public int Source;
			public float Temperature;
			public int IsCritical;
			public int IsAboveUpperThreshold;
			public int IsBelowLowerThreshold;
		}

		private long _totalSent = 0;
		private long _totalFailed = 0;
		private SubscriptionToken _temperatureChangedEventToken = null;

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		[Dependency]
		protected IHubConfiguration HubConfiguration { get; set; }

		public string Name => "Telemetry";

		public Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Subscribe to temperature change event
				// ***
				_temperatureChangedEventToken = this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Subscribe((args) =>
				{
					this.OnTemperatureChangedEvent(args);
				}, ThreadOption.BackgroundThread);

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
				returnValue = false;
			}

			return Task<bool>.FromResult(returnValue);
		}

		public Task<bool> Stop()
		{
			bool returnValue = false;

			try
			{
				// ***
				// *** Unsubscribe from the events
				// ***
				if (_temperatureChangedEventToken != null)
				{
					this.EventAggregator.GetEvent<Events.TemperatureChangedEvent>().Unsubscribe(_temperatureChangedEventToken);
					_temperatureChangedEventToken.Dispose();
					_temperatureChangedEventToken = null;
				}

				returnValue = true;
			}
			catch (Exception ex)
			{
				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
				returnValue = false;
			}

			return Task<bool>.FromResult(returnValue);
		}

		private async void OnTemperatureChangedEvent(TemperatureChangedEventArgs e)
		{
			try
			{
				// ***
				// *** Send the start event
				// ***
				this.EventAggregator.GetEvent<Events.TelemetryStatusChangedEvent>().Publish(new TelemetryStatusChangedEventArgs()
				{
					Status = TelemetryChangedStatus.Sending,
					TotalSent = _totalSent,
					TotalFailed = _totalFailed
				});

				// ***
				// *** Only send telemetry events when the source is Device
				// ***
				if (e.SensorReading.Source == ApplicationSensorReadingSource.Device)
				{
					SensorReading sensorReading = new SensorReading()
					{
						Source = 1,
						TimestampUtc = e.SensorReading.TimestampUtc,
						Temperature = e.SensorReading.Temperature,
						IsCritical = e.SensorReading.IsCritical ? 1 : 0,
						IsAboveUpperThreshold = e.SensorReading.IsAboveUpperThreshold ? 1 : 0,
						IsBelowLowerThreshold = e.SensorReading.IsBelowLowerThreshold ? 1 : 0
					};

					if (await AzureServiceBusHub.SendData(this.HubConfiguration, sensorReading))
					{
						// ***
						// *** Increment the counter
						// ***
						_totalSent++;
					}
					else
					{
						// ***
						// *** Increment the failed counter
						// ***
						_totalFailed++;
					}
				}
			}
			catch (Exception ex)
			{
				// ***
				// *** Increment the failed counter
				// ***
				_totalFailed++;

				this.EventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(ex));
			}
			finally
			{
				// ***
				// *** Send the completed event
				// ***
				this.EventAggregator.GetEvent<Events.TelemetryStatusChangedEvent>().Publish(new TelemetryStatusChangedEventArgs()
				{
					Status = TelemetryChangedStatus.Completed,
					TotalSent = _totalSent,
					TotalFailed = _totalFailed
				});
			}
		}
	}
}
