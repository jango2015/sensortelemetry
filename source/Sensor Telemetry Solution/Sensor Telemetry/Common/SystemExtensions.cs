﻿// Copyright © 2015 Daniel Porrey
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
namespace System
{
	/// <summary>
	/// Generic extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Converts a value of one type to another.
		/// </summary>
		/// <typeparam name="T">The target type that the value will 
		/// be converted to.</typeparam>
		/// <param name="item">An instance of an object to be converted.</param>
		/// <returns>Returns the converted object of type T.</returns>
		public static T ConvertTo<T>(this object item)
		{
			T returnValue = default(T);

			returnValue = (T)Convert.ChangeType(item, typeof(T));

			return returnValue;
		}
	}
}