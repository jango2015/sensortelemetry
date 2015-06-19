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
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Porrey.SensorTelemetry.Interfaces;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace Porrey.SensorTelemetry.Common
{
	public class AzureServiceBusHub
	{
		/// <summary>
		/// Sends the given object to the specified Microsoft Azure Service Bus Event
		/// Hub. The
		/// object is serialized to JSON before being sent to the Event Hub.
		/// </summary>
		/// <typeparam name="T">The type of the object being sent.</typeparam>
		/// <param name="hubConfiguration">An instance of IHubConfiguration containing
		/// the properties needed to connect to the Event Hub</param>
		/// <param name="item">An instance of the item being serialized and sent 
		/// to the Event hub.</param>
		/// <returns></returns>
		public static async Task<bool> SendData<T>(IHubConfiguration hubConfiguration, T item)
		{
			bool returnValue = false;

			// ***
			// *** Create the SAS Token to authorize the request
			// ***
			string sas = AzureServiceBusHub.CreateSasToken(string.Format("http://{0}.servicebus.windows.net/{1}", hubConfiguration.Namepace, hubConfiguration.HubName), hubConfiguration.KeyName, hubConfiguration.Key);

			// ***
			// *** For the URI for the request
			// ***
			var uri = new Uri(String.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}/messages", hubConfiguration.Namepace, hubConfiguration.HubName, hubConfiguration.DeviceId));

			// ***
			// *** Serialize the sensor reading instance to JSON
			// ***
			string json = JsonConvert.SerializeObject(item);

			Encoding ecoding = UTF8Encoding.UTF8;
			HttpContent content = new StringContent(json);
			HttpClient httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Add("Authorization", sas);
			HttpResponseMessage result = await httpClient.PostAsync(uri, content);

			if (result.IsSuccessStatusCode)
			{
				returnValue = true;
			}

			return returnValue;
		}

		/// <summary>
		/// Create a SAS token to make use the rest API with Microsoft Azure
		/// the Service Hub.
		/// </summary>
		/// <param name="resourceUri">The URL to the Service Hub.</param>
		/// <param name="keyName">The name of the shared access policy used to write to
		/// the Event Hub.</param>
		/// <param name="key">The value of the shared access policy used to write
		/// to the Event Hub. </param>
		/// <returns>Returns the SAS token as a string.</returns>
		public static string CreateSasToken(string resourceUri, string keyName, string key)
		{
			string returnValue = string.Empty;

			TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
			var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + 3600); //EXPIRES in 1h 
			string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;
			string signature = AzureServiceBusHub.GetSha256Key(Encoding.UTF8.GetBytes(key), stringToSign);
			returnValue = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry, keyName);

			return returnValue;
		}

		/// <summary>
		/// Performs SHA256 encryption on the given data using the provided secret.
		/// </summary>
		/// <param name="secretKey">A secret key used for encryption.</param>
		/// <param name="value">The value being encrypted.</param>
		/// <returns>Returns the encrypted data as a base 64 string.</returns>
		public static string GetSha256Key(byte[] secretKey, string value)
		{
			var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
			var hash = objMacProv.CreateHash(secretKey.AsBuffer());
			hash.Append(CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8));
			return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
		}
	}
}
