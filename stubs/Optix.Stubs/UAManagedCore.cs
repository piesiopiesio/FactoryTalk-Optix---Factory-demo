// @summary: Stubs for UAManagedCore (nodes, variables, values, NodeId, Log) - only members our code uses; shapes verified against Optix 1.7.
namespace UAManagedCore
{
    public sealed class NodeId
    {
        public NodeId(int ns, uint id) { NamespaceIndex = ns; Id = id; }
        public int NamespaceIndex { get; }
        public uint Id { get; }
        public static NodeId Empty { get; } = new(0, 0);
        public static NodeId Random(int namespaceIndex) => new(namespaceIndex, 0);
    }

    public sealed class UAValue
    {
        public UAValue(object v) { Value = v; }
        public object Value { get; }
        public static implicit operator UAValue(bool v) => new(v);
        public static implicit operator UAValue(int v) => new(v);
        public static implicit operator UAValue(uint v) => new(v);
        public static implicit operator UAValue(long v) => new(v);
        public static implicit operator UAValue(double v) => new(v);
        public static implicit operator UAValue(string v) => new(v);
        public static implicit operator UAValue(NodeId v) => new(v);
    }

    public sealed class LocalizedText { public LocalizedText(string text) { Text = text; } public LocalizedText(string text, string localeId) { Text = text; LocaleId = localeId; } public string Text { get; } public string LocaleId { get; } = ""; }

    public enum DynamicLinkMode { Read, ReadWrite, Write }

    public class VariableChangeEventArgs : EventArgs { public UAValue NewValue { get; } public UAValue OldValue { get; } }

    public interface IUANode
    {
        string BrowseName { get; set; }
        LocalizedText DisplayName { get; set; }
        NodeId NodeId { get; }
        IUANode Owner { get; }
        IEnumerable<IUANode> Children { get; }
        IUAReferences Refs { get; }
        IContext Context { get; }
        IUANode Get(string path);
        T Get<T>(string path) where T : class, IUANode;
        IUAVariable GetVariable(string path);
        IUAVariable GetOrCreateVariable(string browseName);
        IUAObject GetObject(string path);
        void Add(IUANode child);
        void Delete();
        void SetAlias(string aliasName, NodeId target);
    }

    public interface IUAReferences
    {
        void AddReference(NodeId referenceType, IUANode target);
        IEnumerable<IUAObject> GetObjects(NodeId referenceType, bool includeSubtypes);
    }

    public interface IContext { INodeFactory NodeFactory { get; } }

    public interface INodeFactory
    {
        IUAVariable MakeVariable(NodeId nodeId, string browseName, NodeId dataType, NodeId variableType, bool addToCache, object value);
    }

    public interface IUAObject : IUANode { }
    public interface IUAObjectType : IUANode { }

    public interface IUAVariable : IUANode
    {
        UAValue Value { get; set; }
        NodeId DataType { get; }
        event EventHandler<VariableChangeEventArgs> VariableChange;
    }

    /// <summary>Real Optix exposes these without an FTOptix.* using (checked on 1.7); stubbed here in the root namespace.</summary>
    public static class DynamicLinkExtensions
    {
        public static void SetDynamicLink(this IUAVariable target, IUAVariable source, DynamicLinkMode mode = DynamicLinkMode.Read) { }
        public static void SetConverter(this IUAVariable target, IUANode converter) { }
    }

    public static class Log
    {
        public static void Info(string category, string message) => Console.WriteLine($"[{category}] {message}");
        public static void Warning(string category, string message) => Console.WriteLine($"[{category}] WARN {message}");
        public static void Error(string category, string message) => Console.WriteLine($"[{category}] ERROR {message}");
    }
}

namespace UAManagedCore.OpcUa
{
    public static class DataTypes
    {
        public static readonly NodeId BaseDataType = new(0, 24), Boolean = new(0, 1), Int32 = new(0, 6), UInt32 = new(0, 7),
            Int64 = new(0, 8), Double = new(0, 11), String = new(0, 12), NodeId = new(0, 17);
    }

    public static class VariableTypes { public static readonly UAManagedCore.NodeId BaseDataVariableType = new(0, 63); }
}
