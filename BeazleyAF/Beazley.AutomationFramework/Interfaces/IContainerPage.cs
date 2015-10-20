using Beazley.AutomationFramework.Enums;
using Beazley.AutomationFramework.InfrastructureObjects;

namespace Beazley.AutomationFramework.Interfaces
{
    public interface IContainerPage
    {
        ContainerType Type { get; set; }

        ContainerWebElement Container { get; set; }
    }
}
