using System;

namespace TravelGate.Extensions.DependencyInjection.AutoWire
{
	/// <summary>
	/// Use to indicate that decorated class should be left out from auto wiring.
	/// </summary>
	public class IgnoreAutoWireAttribute : Attribute
	{
	}
}
