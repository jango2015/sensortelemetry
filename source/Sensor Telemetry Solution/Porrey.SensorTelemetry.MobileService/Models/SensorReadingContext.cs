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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;

namespace Porrey.SensorTelemetry.MobileService
{
	public class SensorReadingContext : DbContext
	{
		private const string connectionStringName = "Name=MS_TableConnectionString";

		public SensorReadingContext()
			: base(connectionStringName)
		{
		}

		public DbSet<SensorReading> SensorReadings { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			string schema = ServiceSettingsDictionary.GetSchemaName();
			if (!string.IsNullOrEmpty(schema))
			{
				modelBuilder.HasDefaultSchema(schema);
			}

			modelBuilder.Conventions.Add(
				new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
					"ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
		}
	}
}
