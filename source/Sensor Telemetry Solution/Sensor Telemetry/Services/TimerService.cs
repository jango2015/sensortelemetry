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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;

namespace Porrey.SensorTelemetry.Services
{
	/// <summary>
	/// Sends out a timer event every 500 ms
	/// </summary>
	public class TimerService : IBackgroundService
	{
		private Timer _timer = null;

		public string Name => "Timer";

		[Dependency]
		protected IEventAggregator EventAggregator { get; set; }

		public Task<bool> Start()
		{
			bool returnValue = false;

			try
			{
				_timer = new Timer(this.TimerCallback, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
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
				if (_timer != null)
				{
					_timer.Change(Timeout.Infinite, Timeout.Infinite);
					_timer = null;
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

		private void TimerCallback(object state)
		{
			this.EventAggregator.GetEvent<Events.TimerEvent>().Publish(new EventArgs());
		}
	}
}
