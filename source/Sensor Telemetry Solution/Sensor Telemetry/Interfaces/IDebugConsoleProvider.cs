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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Porrey.SensorTelemetry.Models;
using Porrey.SensorTelemetry.Shared.Models;

namespace Porrey.SensorTelemetry.Interfaces
{
	public interface IDebugConsoleProvider
	{
		ObservableCollection<DebugEventArgs> Items { get; }
		Task Refresh();		
		Task Clear();
	}
}
