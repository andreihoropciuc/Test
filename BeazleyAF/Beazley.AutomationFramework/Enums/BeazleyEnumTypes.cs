namespace Beazley.AutomationFramework.Enums
{
    public enum AlertResolveType
    {
        Dismiss = 0,
        Accept = 1,
        TreatAfter = 2
    }

    public enum ContainerType
    {
        Element,
        Alert,
        Frame,
        Window
    }

    public enum SortType
    {
        Ascending,
        Descending,
        None
    }

    public enum AlertAction
    {
        Accept,
        Dismiss
    }
}
