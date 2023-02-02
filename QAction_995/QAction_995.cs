using System;

using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.SNMP;

/// <summary>
///     DataMiner QAction Class: Streams Rate Calculations Method.
/// </summary>
public static class QAction
{
	/// <summary>
	///     The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			CalculationMethod rateCalculationsMethod = (CalculationMethod)Convert.ToInt32(protocol.GetParameter(Parameter.streamsratecalculationsmethod));
			SnmpDeltaHelper.UpdateRateDeltaTracking(protocol, groupId: 1000, rateCalculationsMethod);
		}
		catch (Exception ex)
		{
			protocol.Log("QA" + protocol.QActionID + "|" + protocol.GetTriggerParameter() + "|Run|Exception thrown:" + Environment.NewLine + ex, LogType.Error, LogLevel.NoLogging);
		}
	}
}