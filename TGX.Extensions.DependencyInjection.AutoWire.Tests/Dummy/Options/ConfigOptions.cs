using System;
using System.Collections.Generic;
using System.Text;

namespace TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Options
{
    public class ConfigOptions
    {
        public string Key1 { get; set; }

        public string Key2 { get; set; }

        public int Key3 { get; set; }

        public bool Key4 { get; set; }

        public SubKey SubKey { get; set; }

    }

    public class SubKey
    {

        public string Key1 { get; set; }

        public List<string> Key2 { get; set; }

    }
}
