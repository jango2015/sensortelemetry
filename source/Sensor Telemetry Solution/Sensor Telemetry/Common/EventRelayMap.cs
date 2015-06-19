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
using System.Diagnostics;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Practices.Prism.PubSubEvents;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Shared.Models;

namespace Porrey.SensorTelemetry.Common
{
	/// <summary>
	/// Defines a method for transforming an
	/// </summary>
	/// <typeparam name="T">The type of an object that inherits from EventRelayArgs.</typeparam>
	/// <param name="e">An instance of the event arguments of type T.</param>
	/// <returns></returns>
	public delegate T TransformArgumentDelegate<T>(T e) where T : EventRelayArgs;

	/// <summary>
	/// Defines a method for determining if an event with event arguments of type T
	/// should be routed.
	/// </summary>
	/// <typeparam name="T">The type of an object that inherits from EventRelayArgs.</typeparam>
	/// <param name="e">An instance of the event arguments of type T.</param>
	/// <returns></returns>
	public delegate bool ShouldRouteEventDelegate<T>(T e) where T : EventRelayArgs;

	/// <summary>
	/// Creates a map between a PubSub event and a SignalR event allowing
	/// event messages to flow back and forth between these two services.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EventRelayMap<T> : IDisposable where T : EventRelayArgs
	{
		private SubscriptionToken _subscriptionToken = null;

		/// <summary>
		/// Creates an instance of EventMap.
		/// </summary>
		/// <param name="applicationInstanceIdentity">An instance of IApplicationInstanceIdentity
		/// uniquely identifying the current application instance.</param>
		/// <param name="hubConnection">A reference to the SingalR hub connection object.</param>
		/// <param name="proxy">A reference to the SignalR proxy used for sending
		/// data to the hub.</param>
		/// <param name="eventTarget">A PubSubEvent instance representing the event to be mapped.</param>
		/// <param name="inboundTransformAction">A TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed inbound.</param>
		/// <param name="shouldRouteInbound">A ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.</param>
		/// <param name="outboundTransformAction">A TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed outbound.</param>
		/// <param name="shouldRouteOutbound">A ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.</param>
		public EventRelayMap(IApplicationInstanceIdentity applicationInstanceIdentity, HubConnection hubConnection, IHubProxy proxy,
						PubSubEvent<T> eventTarget,
						TransformArgumentDelegate<T> inboundTransformAction, ShouldRouteEventDelegate<T> shouldRouteInbound,
						TransformArgumentDelegate<T> outboundTransformAction, ShouldRouteEventDelegate<T> shouldRouteOutbound)
		{
			this.ApplicationInstanceIdentity = applicationInstanceIdentity;
			this.HubConnection = hubConnection;
			this.Proxy = proxy;
			this.Event = eventTarget;
			this.InboundTransformAction = inboundTransformAction;
			this.ShouldRouteInbound = shouldRouteInbound;
			this.OutboundTransformAction = outboundTransformAction;
			this.ShouldRouteOutbound = shouldRouteOutbound;

			this.ExternalEventTarget = string.Format("Send{0}", this.Event.GetType().Name);
			this.ExternalEventSource = string.Format("On{0}", this.Event.GetType().Name);

			this.MapEventFromExternalToInternal();
			this.MapEventFromInternalToExternal();
		}

		/// <summary>
		/// Gets/sets an instance of IApplicationInstanceIdentity
		/// uniquely identifying the current application instance.
		/// </summary>
		public IApplicationInstanceIdentity ApplicationInstanceIdentity { get; set; }

		/// <summary>
		/// Gets/sets a PubSubEvent instance representing the event to be mapped.
		/// </summary>
		public PubSubEvent<T> Event { get; set; }

		/// <summary>
		/// Gets/sets the TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed inbound.
		/// </summary>
		public TransformArgumentDelegate<T> InboundTransformAction { get; set; }

		/// <summary>
		/// Gets/sets the ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.
		/// </summary>
		public ShouldRouteEventDelegate<T> ShouldRouteInbound { get; set; }

		/// <summary>
		/// Gets/sets the TransformArgumentDelegate method used to transform the 
		/// event arguments before being routed outbound.
		/// </summary>
		public TransformArgumentDelegate<T> OutboundTransformAction { get; set; }

		/// <summary>
		/// Gets/sets the ShouldRouteEventDelegate method that determines if the event
		/// should be routed. Note this method is called prior to the transformation.
		/// </summary>
		public ShouldRouteEventDelegate<T> ShouldRouteOutbound { get; set; }

		/// <summary>
		/// Gets/sets a reference to the SingalR hub connection object.
		/// </summary>
		protected HubConnection HubConnection { get; set; }

		/// <summary>
		/// Get/sets a refere3nmce to the subscription token instance
		/// created when the event is subscribed to. This object is used to 
		/// unsubscribe from the event.
		/// </summary>
		protected IHubProxy Proxy { get; set; }

		/// <summary>
		/// Gets/sets the name of the SignalR method called to send event data
		/// to the subscribed client.
		/// </summary>
		protected string ExternalEventTarget { get; set; }

		/// <summary>
		/// Gets/sets the name of the SignalR event that is called
		/// by the SignalR hub.
		/// </summary>
		protected string ExternalEventSource { get; set; }

		/// <summary>
		/// Links an event from the Event Aggregator to a SignalR 
		/// event. This map sends internal events to external 
		/// applications.
		/// </summary>
		private void MapEventFromInternalToExternal()
		{
			_subscriptionToken = this.Event.Subscribe((e) =>
			{
				this.OnProcessOutboundMessage(e);
			});
		}

		/// <summary>
		/// Links an inbound SignalR event to an outbound Event Aggregator
		/// event passing through the given transformation. This map brings
		/// events from outside applications into this instance of the 
		/// application.
		/// </summary>
		private void MapEventFromExternalToInternal()
		{
			Proxy.On<T>(this.ExternalEventSource, (e) =>
			{
				this.OnProcessInboundMessage(e);
			});
		}

		/// <summary>
		/// Processes an inbound message.
		/// </summary>
		/// <param name="e">The data received from the event.</param>
		protected void OnProcessInboundMessage(T e)
		{
			Debug.WriteLine("Received event of type '{0}'.", typeof(T).Name);

			try
			{
				// ***
				// *** An inbound message will have a relay count of 0
				// *** 
				if (e.RelayCount == 1)
				{
					// ***
					// *** Only forward events that came from another
					// *** application instance.
					// ***
					if (e.SenderKey != this.ApplicationInstanceIdentity.Key)
					{
						if (this.ShouldRouteInbound(e))
						{
							T newE = this.InboundTransformAction(e);

							// ***
							// *** Publish internally on the Event Aggregator
							// ***
							this.Event.Publish(newE);
						}
					}
				}
			}
			catch
			{
				// ***
				// *** Discard this event
				// ***
			}
		}

		/// <summary>
		/// Processes an outbound event.
		/// </summary>
		/// <param name="e">The data received from the event.</param>
		protected void OnProcessOutboundMessage(T e)
		{
			try
			{
				// ***
				// *** Any message originating from this application will
				// *** have an empty SenderKey and a relay count of 0
				// ***
				if (string.IsNullOrWhiteSpace(e.SenderKey) && e.RelayCount == 0)
				{
					if (this.ShouldRouteOutbound(e))
					{
						// ***
						// *** Transform the item
						// ***
						T newE = this.OutboundTransformAction(e);

						// ***
						// *** Create the SignalR object to
						// *** wrap the actual item being sent.
						// ***
						newE.SenderKey = this.ApplicationInstanceIdentity.Key;
						newE.RelayCount++;

						// ***
						// *** Send to the SignalR hub
						// ***
						Debug.WriteLine("Sending event of type '{0}'.", typeof(T).Name);
						this.Proxy.Invoke(this.ExternalEventTarget, newE);
					}
				}
			}
			catch
			{
				// ***
				// *** Discard this event
				// ***
			}
		}

		/// <summary>
		/// CLeans up the internal objects.
		/// </summary>
		public void Dispose()
		{
			if (_subscriptionToken != null)
			{
				this.Event.Unsubscribe(this._subscriptionToken);
				_subscriptionToken.Dispose();
				_subscriptionToken = null;
			}
		}
	}
}
