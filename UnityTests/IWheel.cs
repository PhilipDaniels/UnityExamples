using System.Diagnostics;

namespace UnityTests
{
    public interface IWheel
    {
        string Name { get; set; }
    }

    [DebuggerDisplay("{Name}")]
    public class DefaultWheel : IWheel
    {
        public string Name { get; set; }

        public DefaultWheel()
        {
            Name = Names.DefaultWheelName;
        }
    }

    [DebuggerDisplay("{Name}")]
    public class OverrideWheel : IWheel
    {
        public string Name { get; set; }

        public OverrideWheel()
        {
            Name = Names.OverrideWheelName;
        }
    }
}
