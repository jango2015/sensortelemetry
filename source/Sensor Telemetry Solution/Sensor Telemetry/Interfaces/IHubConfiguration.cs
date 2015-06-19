﻿// Copyright © 2015 Daniel Porrey
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
namespace Porrey.SensorTelemetry.Interfaces
{
	/// <summary>
	/// Defines the properties used to connect to a Microsoft
	/// Azure Service Bus Event Hub.
	/// </summary>
	public interface IHubConfiguration
	{
		/// <summary>
		/// Gets/sets the namespace containing the Event Hub
		/// that will be written to found under the Service
		/// Bus tab in the Azure portal.
		/// </summary>
		string Namepace { get; set; }

		/// <summary>
		/// Gets/sets the name of Event Hub withing the specified
		/// namespace.
		/// </summary>
		string HubName { get; set; }

		/// <summary>
		/// Gets/sets the name of the key found in the Azure Portal under
		/// Service Bus, then select the namespace, select Event Hubs, select
		/// the hub and then click Configure. The key name is found in the
		/// shared access policies section. Use a key that has Send 
		/// permissions.
		/// </summary>
		string KeyName { get; set; }

		/// <summary>
		/// Gets/sets the value of the key that is named in the
		/// KeyName property of this instance.
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// Gets/sets a a unique ID for a particular IoT device. This
		/// value is used to map to a partition in the Event
		/// Hub.
		/// </summary>
		string DeviceId { get; set; }
	}
}