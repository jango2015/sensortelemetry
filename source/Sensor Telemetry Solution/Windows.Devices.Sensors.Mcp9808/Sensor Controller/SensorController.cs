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
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.Devices.Sensors.Interfaces;

namespace Windows.Devices.Sensors
{
	/// <summary>
	/// Creates a device of the given type as defined by T.
	/// </summary>
	public static class SensorController
	{
		public static Task<T> GetI2CDevice<T>(byte deviceAddress, I2cBusSpeed busSpeed) where T : II2CSensor
		{
			T returnValue = default(T);

			if (typeof(T) == typeof(IMcp9808))
			{
				IMcp9808 device = new Mcp9808(deviceAddress, busSpeed);
                returnValue = (T)device;
			}

			return Task<T>.FromResult(returnValue);
		}
	}
}
