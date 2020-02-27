using System;
using System.Collections.Generic;
using System.Text;
using TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Interfaces;

namespace TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Services
{
	public class IgnoredServiceByDerivedInterface : IDerivedIgnoredInterface
	{
	}
}
