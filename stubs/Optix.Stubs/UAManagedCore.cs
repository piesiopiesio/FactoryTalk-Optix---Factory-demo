// @summary: Stubs for UAManagedCore (nodes, variables, values, NodeId, Log, Color) - only members our code uses.
namespace UAManagedCore
{
    public sealed class NodeId { public NodeId(int ns, uint id) { Ns = ns; Id = id; } public int Ns { get; } public uint Id { get; } }

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
    }

    public sealed class LocalizedText { public LocalizedText(string text) { Text = text; } public string Text { get; } }

    public enum DynamicLinkMode { Read, ReadWrite, Write }

    public class VariableChangeEventArgs : EventArgs { public UAValue NewValue { get; } public UAValue OldValue { get; } }

    public interface IUANode
    {
        string BrowseName { get; set; }
        NodeId NodeId { get; }
        IUANode Owner { get; }
        IEnumerable<IUANode> Children { get; }
        IUANode Get(string path);
        T Get<T>(string path) where T : class, IUANode;
        IUAVariable GetVariable(string path);
        IUAObject GetObject(string path);
        void Add(IUANode child);
        void Delete();
    }

    public interface IUAObject : IUANode { }
    public interface IUAObjectType : IUANode { }

    public interface IUAVariable : IUANode
    {
        UAValue Value { get; set; }
        NodeId DataType { get; }
        event EventHandler<VariableChangeEventArgs> VariableChange;
    }

    public struct Color
    {
        public Color(uint argb) { ARGB = argb; }
        public Color(byte a, byte r, byte g, byte b) { ARGB = (uint)(a << 24 | r << 16 | g << 8 | b); }
        public uint ARGB { get; }
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
        public static readonly NodeId Boolean = new(0, 1), Int32 = new(0, 6), UInt32 = new(0, 7), Int64 = new(0, 8), Double = new(0, 11), String = new(0, 12);
    }
}
