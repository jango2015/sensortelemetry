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
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Porrey.SensorTelemetry.Common;
using Porrey.SensorTelemetry.Interfaces;
using Porrey.SensorTelemetry.Models;
using Porrey.SensorTelemetry.Repositories;
using Porrey.SensorTelemetry.Services;
using Porrey.SensorTelemetry.Shared.Events;
using Porrey.SensorTelemetry.Shared.Models;
using Porrey.SensorTelemetry.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Devices.I2c;
using Windows.Devices.Sensors;
using Windows.Devices.Sensors.Interfaces;
using Windows.UI.Xaml;

namespace Porrey.SensorTelemetry
{
	public sealed partial class App : MvvmUnityAppBase
	{
		public App()
		{
			this.InitializeComponent();
		}

		protected override void OnContainerRegistration(IUnityContainer container)
		{
			// ***
			// *** Add general application objects
			// ***
			container.RegisterInstance<IApplicationInstanceIdentity>(new ApplicationInstanceIdentity(), new ContainerControlledLifetimeManager());
			container.RegisterInstance<IEventAggregator>(new EventAggregator(), new ContainerControlledLifetimeManager());
			container.RegisterInstance<INavigationService>(this.NavigationService);
			container.RegisterInstance<ISessionStateService>(this.SessionStateService);
			container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));
			container.RegisterType<IApplicationSettingsRepository, ApplicationSettingsRepository>(new ContainerControlledLifetimeManager());

			// ***
			// *** Devices
			// ***
			container.RegisterType<ITemperatureRepository, Mcp9808TemperatureRepository>(MagicValue.Device.Local, new ContainerControlledLifetimeManager());
			container.RegisterType<ITemperatureRepository, NullTemperatureRepository>(MagicValue.Device.Null, new ContainerControlledLifetimeManager());

			// ***
			// *** Configure the pins
			// ***
			IGpioPinConfiguration gpioPinConfiguration = new GpioPinConfiguration()
			{
				BluePinNumber = 18,             // *** Blue LED
				GreenLedPinNumber = 23,         // *** Green LED
				RedLedPinNumber = 12,           // *** Red LED				
				YellowPinNumber = 16,           // *** Blue LED
				PushButtonPinNumber = 5,        // *** Push Button Pin
				AlertPinNumber = 6,             // *** Alert Pin
			};

			container.RegisterInstance<IGpioPinConfiguration>(gpioPinConfiguration, new ContainerControlledLifetimeManager());

			// ***
			// *** Set the hub configuration values
			// ***
			IHubConfiguration hubConfiguration = new HubConfiguration()
			{
				Namepace = "{YOUR NAME SPACE HERE}",
				HubName = "{YOUR HUB NAME HERE}",
				DeviceId = "iot1",
				KeyName = "{YOUR SHARED POLICY KEY NAME HERE}",
				Key = "{YOUR SHARED KEY HERE}"
			};
			container.RegisterInstance<IHubConfiguration>(hubConfiguration, new ContainerControlledLifetimeManager());

			// ***
			// *** Set the mobile services configuration
			// ***
			IMobileServicesConfiguration mobileServicesConfiguration = new MobileServicesConfiguration()
			{
				Url = "{YOUR MOBILE SERVICES URL HERE}",
				ApplicationKey = "{YOUR APPLICATION KEY HERE}"
			};
			container.RegisterInstance<IMobileServicesConfiguration>(mobileServicesConfiguration, new ContainerControlledLifetimeManager());

			// ***
			// *** Background Services
			// ***
			container.RegisterType<IBackgroundService, DebugConsoleService>(MagicValue.BackgroundService.Debug, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, NotificationRelayService>(MagicValue.BackgroundService.Relay, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, LedService>(MagicValue.BackgroundService.Led, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, PushButtonMonitoringService>(MagicValue.BackgroundService.PushButtonMonitor, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, AlertPinMonitoringService>(MagicValue.BackgroundService.AlertPinMonitor, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, TelemetryService>(MagicValue.BackgroundService.Telemetry, new ContainerControlledLifetimeManager());
			container.RegisterType<IBackgroundService, TimerService>(MagicValue.BackgroundService.Timer, new ContainerControlledLifetimeManager());
			
			// ***
			// *** The Debug COnsoel Service also doubles as the IDebugConsoleProvider
			// ***
			container.RegisterType<IDebugConsoleProvider>(new InjectionFactory((c) =>
			{
				return (IDebugConsoleProvider)c.Resolve<IBackgroundService>(MagicValue.BackgroundService.Debug);
			}));

			// ***
			// *** View Models
			// ***
			container.RegisterType<MainPageViewModel, MainPageViewModel>(new ContainerControlledLifetimeManager());
			container.RegisterType<StartPageViewModel, StartPageViewModel>(new ContainerControlledLifetimeManager());
		}

		protected override void OnApplicationInitialize(IActivatedEventArgs args)
		{
			base.OnApplicationInitialize(args);
		}

		protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
		{
			// ***
			// *** Navigate to the main page
			// ***
			this.NavigationService.Navigate(MagicValue.Views.StartPage, null);
			Window.Current.Activate();

			return Task.FromResult<object>(null);
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
			eventAggregator.GetEvent<Events.DebugEvent>().Publish(new DebugEventArgs(e.Exception));
			e.Handled = true;
		}
	}
}
