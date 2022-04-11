﻿namespace Skyline.Protocol.Streams
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Library.Common.SafeConverters;
	using Skyline.DataMiner.Library.Protocol.Snmp.Rates;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.Extensions;

	public class StreamsProcessor
	{
		private const int GroupId = 1000;
		private readonly SLProtocol protocol;

		private readonly StreamsGetter getter;
		private readonly StreamsSetter setter;

		internal StreamsProcessor(SLProtocol protocol)
		{
			this.protocol = protocol;

			getter = new StreamsGetter(protocol);
			getter.Load();

			setter = new StreamsSetter(protocol);
		}

		internal void ProcessData()
		{
			SnmpDeltaHelper snmpDeltaHelper = new SnmpDeltaHelper(protocol, GroupId, Parameter.streamsratecalculationsmethod);

			for (int i = 0; i < getter.Keys.Length; i++)
			{
				setter.SetColumnsData[Parameter.Streams.tablePid].Add(Convert.ToString(getter.Keys[i]));

				ProcessBitRates(i, snmpDeltaHelper, minDelta: new TimeSpan(0, 0, 5), maxDelta: new TimeSpan(0, 10, 0));
			}
		}

		internal void UpdateProtocol()
		{
			setter.SetColumns();
		}

		private void ProcessBitRates(int getPosition, SnmpDeltaHelper snmpDeltaHelper, TimeSpan minDelta, TimeSpan maxDelta)
		{
			string streamPK = Convert.ToString(getter.Keys[getPosition]);
			uint octets = SafeConvert.ToUInt32(Convert.ToDouble(getter.Octets[getPosition]));
			string serializedHelper = Convert.ToString(getter.OctetsRateData[getPosition]);

			SnmpRate32 snmpRate32Helper = SnmpRate32.FromJsonString(serializedHelper, minDelta, maxDelta);
			double octetRate = snmpRate32Helper.Calculate(snmpDeltaHelper, octets, streamPK);
			double bitRate = octetRate > 0 ? octetRate / 8 : octetRate;

			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitrate].Add(bitRate);
			setter.SetColumnsData[Parameter.Streams.Pid.streamsbitratedata].Add(snmpRate32Helper.ToJsonString());
		}

		private class StreamsGetter
		{
			private readonly SLProtocol protocol;

			internal StreamsGetter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			public object[] Keys { get; private set; }

			public object[] Octets { get; private set; }

			public object[] OctetsRateData { get; private set; }

			internal void Load()
			{
				var tableData = (object[])protocol.NotifyProtocol(321, Parameter.Streams.tablePid, new uint[]
				{
					Parameter.Streams.Idx.streamsindex,
					Parameter.Streams.Idx.streamsoctetscounter,
					Parameter.Streams.Idx.streamsbitratedata,
				});

				Keys = (object[])tableData[0];
				Octets = (object[])tableData[1];
				OctetsRateData = (object[])tableData[2];
			}
		}

		private class StreamsSetter
		{
			private readonly SLProtocol protocol;

			internal StreamsSetter(SLProtocol protocol)
			{
				this.protocol = protocol;
			}

			internal Dictionary<object, List<object>> SetColumnsData { get; } = new Dictionary<object, List<object>>
			{
				{ Parameter.Streams.tablePid, new List<object>() },
				{ Parameter.Streams.Pid.streamsbitrate, new List<object>() },
				{ Parameter.Streams.Pid.streamsbitratedata, new List<object>() },
			};

			internal void SetColumns()
			{
				protocol.SetColumns(SetColumnsData);
			}
		}
	}
}
