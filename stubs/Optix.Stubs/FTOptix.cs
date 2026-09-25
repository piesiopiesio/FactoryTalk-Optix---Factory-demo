// @summary: Stubs for FTOptix.* namespaces (NetLogic base + tasks, Project, InformationModel, UI items, alarms, ResourceUri).
using UAManagedCore;

namespace FTOptix.NetLogic
{
    [AttributeUsage(AttributeTargets.Method)] public sealed class ExportMethodAttribute : Attribute { }

    public abstract class BaseNetLogic
    {
        public IUAObject LogicObject { get; }
        public IUANode Owner { get; }
        public virtual void Start() { }
        public virtual void Stop() { }
    }

    public sealed class PeriodicTask : IDisposable
    {
        public PeriodicTask(Action action, int periodMs, IUANode owner) { }
        public void Start() { }
        public void Dispose() { }
    }

    public sealed class LongRunningTask : IDisposable
    {
        public LongRunningTask(Action<LongRunningTask> action, IUANode owner) { }
        public bool IsCancellationRequested => false;
        public void Start() { }
        public void Dispose() { }
    }
}

namespace FTOptix.HMIProject
{
    public static class Project { public static IUAObject Current { get; } }

    public static class InformationModel
    {
        public static T Make<T>(string browseName) where T : IUANode => default;
        public static T MakeObject<T>(string browseName) where T : IUAObject => default;
        public static IUAObject MakeObject(string browseName, NodeId typeId) => null;
        public static IUAObjectType MakeObjectType(string browseName) => null;
        public static IUAVariable MakeVariable(string browseName, NodeId dataType) => null;
        public static IUAVariable GetVariable(NodeId id) => null;
        public static IUAObject GetObject(NodeId id) => null;
    }
}

namespace FTOptix.Core
{
    public interface Folder : IUAObject { }

    public sealed class ResourceUri
    {
        public static ResourceUri FromProjectRelativePath(string path) => new();
        public string Uri => "";
    }
}

namespace FTOptix.CoreBase
{
    public static class DynamicLinkExtensions
    {
        public static void SetDynamicLink(this IUAVariable target, IUAVariable source, DynamicLinkMode mode = DynamicLinkMode.Read) { }
    }
}

namespace FTOptix.UI
{
    public enum HorizontalAlignment { Left, Center, Right, Stretch }
    public enum VerticalAlignment { Top, Center, Bottom, Stretch }

    public interface Item : IUAObject
    {
        float Width { get; set; }
        float Height { get; set; }
        float LeftMargin { get; set; }
        float TopMargin { get; set; }
        HorizontalAlignment HorizontalAlignment { get; set; }
        VerticalAlignment VerticalAlignment { get; set; }
    }

    public interface Panel : Item { }

    public interface Rectangle : Item
    {
        Color FillColor { get; set; }
        IUAVariable FillColorVariable { get; }
        Color BorderColor { get; set; }
        float BorderThickness { get; set; }
        float CornerRadius { get; set; }
    }

    public interface Label : Item
    {
        string Text { get; set; }
        IUAVariable TextVariable { get; }
        float FontSize { get; set; }
        Color TextColor { get; set; }
    }
}

namespace FTOptix.NativeUI { internal static class Placeholder { } }

namespace FTOptix.Alarm
{
    public interface DigitalAlarm : IUAObject
    {
        IUAVariable InputValueVariable { get; }
        string Message { get; set; }
        bool AutoAcknowledge { get; set; }
        bool AutoConfirm { get; set; }
    }
}
