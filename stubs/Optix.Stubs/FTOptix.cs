// @summary: Stubs for FTOptix.* namespaces (NetLogic, Project, InformationModel, UI widgets, converters, events, alarms) - verified against Optix 1.7 build.
using UAManagedCore;

namespace FTOptix.NetLogic
{
    [AttributeUsage(AttributeTargets.Method)] public sealed class ExportMethodAttribute : Attribute { }

    public abstract class BaseNetLogic
    {
        public IUAObject LogicObject { get; }
        public IUANode Owner { get; }
        public FTOptix.Core.Session Session { get; }
        public virtual void Start() { }
        public virtual void Stop() { }
    }

    public sealed class PeriodicTask : IDisposable
    {
        public PeriodicTask(Action action, int periodMs, IUANode owner) { }
        public void Start() { }
        public void Dispose() { }
    }

    public static class ObjectTypes { public static readonly NodeId NetLogic = new(3, 1); }
}


namespace FTOptix.HMIProject
{
    public static class Project { public static IUAObject Current { get; } }

    public static class InformationModel
    {
        public static T Make<T>(string browseName) where T : IUANode => default;
        public static T MakeObject<T>(string browseName) where T : IUAObject => default;
        public static IUAObject MakeObject(string browseName) => null;
        public static IUAObject MakeObject(string browseName, NodeId typeId) => null;
        public static IUAObjectType MakeObjectType(string browseName) => null;
        public static T MakeObjectType<T>(string browseName) where T : IUAObjectType => default;
        public static IUAVariable MakeVariable(string browseName, NodeId dataType) => null;
        public static T MakeVariable<T>(string browseName, NodeId dataType) where T : IUAVariable => default;
        public static IUAVariable GetVariable(NodeId id) => null;
        public static IUAObject GetObject(NodeId id) => null;
    }
}

namespace FTOptix.Core
{
    public interface Folder : IUAObject { }
    public static class DataTypes { public static readonly NodeId VariablePointer = new(5, 1); }
    public interface NodePointer : IUAVariable { }
    public interface User : IUAObject { string LocaleId { get; set; } }   // CheatSheet users-groups: user.LocaleId = locale
    public interface Group : IUAObject { }
    public static class ReferenceTypes { public static readonly NodeId HasGroup = new(6, 1); }
    public enum ChangePasswordResultCode { Success, WrongOldPassword, PasswordAlreadyUsed, PasswordTooShort, UserNotFound, UnsupportedOperation }
    public sealed class ChangePasswordResult { public ChangePasswordResultCode ResultCode { get; } }
    public sealed class Session
    {
        public User User { get; }
        public ChangePasswordResult ChangePassword(string userName, string newPassword, string oldPassword) => new();
    }

    public struct Color
    {
        public Color(uint argb) { ARGB = argb; }
        public uint ARGB { get; }
    }

    public sealed class ResourceUri
    {
        public static ResourceUri FromProjectRelativePath(string path) => new();
        public string Uri => "";
    }
}

namespace FTOptix.CoreBase
{
    public static class ReferenceTypes { public static readonly NodeId HasSource = new(2, 1), HasDynamicLink = new(2, 3); }
    public static class Objects { public static readonly NodeId VariableCommands = new(2, 2); }

    public interface StringFormatter : IUAObject { string Format { get; set; } }
    public interface ExpressionEvaluator : IUAObject { string Expression { get; set; } }

    public interface EventHandler : IUAObject { IUANode MethodsToCall { get; } }
}

namespace FTOptix.UI
{
    public enum HorizontalAlignment { Left, Center, Right, Stretch }
    public enum VerticalAlignment { Top, Center, Bottom, Stretch }
    public enum FontWeight { Normal, Bold }

    public static class ObjectTypes { public static readonly NodeId MouseClickEvent = new(4, 1); }

    public interface Item : IUAObject
    {
        float Width { get; set; }
        IUAVariable WidthVariable { get; }
        float Height { get; set; }
        IUAVariable HeightVariable { get; }
        float LeftMargin { get; set; }
        IUAVariable LeftMarginVariable { get; }
        float TopMargin { get; set; }
        IUAVariable TopMarginVariable { get; }
        bool Visible { get; set; }
        bool Enabled { get; set; }
        IUAVariable VisibleVariable { get; }
        bool HitTestVisible { get; set; }
        HorizontalAlignment HorizontalAlignment { get; set; }
        VerticalAlignment VerticalAlignment { get; set; }
    }

    public interface Panel : Item { }
    public interface ScreenType : IUAObjectType, Panel { }
    public interface ColumnLayout : Item { float VerticalGap { get; set; } }

    public interface Rectangle : Item
    {
        FTOptix.Core.Color FillColor { get; set; }
        IUAVariable FillColorVariable { get; }
        FTOptix.Core.Color BorderColor { get; set; }
        float BorderThickness { get; set; }
        float CornerRadius { get; set; }
    }

    public interface Label : Item
    {
        string Text { get; set; }
        IUAVariable TextVariable { get; }
        float FontSize { get; set; }
        string FontFamily { get; set; }
        FontWeight FontWeight { get; set; }
        bool WordWrap { get; set; }
        FTOptix.Core.Color TextColor { get; set; }
    }

    public interface Button : Item
    {
        string Text { get; set; }
        float FontSize { get; set; }
        string FontFamily { get; set; }
        FTOptix.Core.Color BackgroundColor { get; set; }
        FTOptix.Core.Color TextColor { get; set; }
    }

    public interface NavigationPanelItem : IUAObject
    {
        string Title { get; set; }
        NodeId Panel { get; set; }
    }

    public interface NavigationPanel : Item
    {
        IUANode Panels { get; }
        int CurrentTabIndex { get; set; }
    }
}

namespace FTOptix.NativeUI { internal static class Placeholder { } }

namespace FTOptix.DataLogger
{
    public interface VariableToLog : IUAVariable { }
    public interface DataLogger : IUAObject { }
}

namespace FTOptix.Alarm
{
    public interface DigitalAlarm : IUAObject
    {
        IUAVariable InputValueVariable { get; }
        string Message { get; set; }
        bool AutoAcknowledge { get; set; }
        bool AutoConfirm { get; set; }
        ushort Severity { get; set; }
    }
}
