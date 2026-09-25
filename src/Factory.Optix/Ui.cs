// @summary: Widget factory for generated screens (pixels): boxes, labels, buttons, bindings, formatters, click -> NetLogic method.
#region Using directives
using System;
using System.Globalization;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Core;
#endregion

public static class Ui
{
    public const string Font = "Arial";

    public static Rectangle Box(IUANode parent, string name, double x, double y, double w, double h,
        uint fill, uint border = OptixNames.LineArgb, float borderPx = 1)
    {
        var r = InformationModel.Make<Rectangle>(name);
        r.LeftMargin = (float)x;
        r.TopMargin = (float)y;
        r.Width = (float)w;
        r.Height = (float)h;
        r.FillColor = new Color(fill);
        r.BorderColor = new Color(border);
        r.BorderThickness = borderPx;
        parent.Add(r);
        return r;
    }

    /// <summary>Full-size background (Stretch/Stretch), first child so it stays behind everything.</summary>
    public static Rectangle Background(IUANode parent, string name, uint fill)
    {
        var r = InformationModel.Make<Rectangle>(name);
        r.HorizontalAlignment = HorizontalAlignment.Stretch;
        r.VerticalAlignment = VerticalAlignment.Stretch;
        r.FillColor = new Color(fill);
        r.BorderThickness = 0;
        parent.Add(r);
        return r;
    }

    public static Label Text(IUANode parent, string name, string text, double x, double y,
        float size = 14, uint color = OptixNames.TitleArgb, bool bold = false)
    {
        var l = InformationModel.Make<Label>(name);
        l.Text = text;
        l.LeftMargin = (float)x;
        l.TopMargin = (float)y;
        l.FontSize = size;
        l.FontFamily = Font;
        l.TextColor = new Color(color);
        if (bold) l.FontWeight = FontWeight.Bold;
        parent.Add(l);
        return l;
    }

    /// <summary>Live value label: format uses {0}, {1}... fed by project variable paths (see OptixNames.ModelVar).</summary>
    public static Label Value(IUANode parent, string name, string format, double x, double y, float size, params string[] sources)
    {
        var l = Text(parent, name, "", x, y, size, OptixNames.DataArgb);
        Format(l.TextVariable, format, sources);
        return l;
    }

    /// <summary>Neutral gray command button (style guide: no color unless it means something), min. 40 px high.</summary>
    public static Button Button(IUANode parent, string name, string text, double x, double y, double w, double h = 44)
    {
        var b = InformationModel.Make<Button>(name);
        b.Text = text;
        b.LeftMargin = (float)x;
        b.TopMargin = (float)y;
        b.Width = (float)w;
        b.Height = (float)Math.Max(40, h);
        b.FontSize = 15;
        b.FontFamily = Font;
        b.BackgroundColor = new Color(OptixNames.ButtonArgb);
        b.TextColor = new Color(OptixNames.TitleArgb);
        parent.Add(b);
        return b;
    }

    public static void Link(IUAVariable target, string sourcePath)
    {
        var source = Project.Current.GetVariable(sourcePath);
        if (source != null) target.SetDynamicLink(source);
        else Log.Warning("Ui", "Missing variable " + sourcePath);
    }

    /// <summary>StringFormatter converter: "OEE {0} %" + Source0..N linked to the given variables.</summary>
    public static void Format(IUAVariable target, string format, params string[] sourcePaths)
    {
        var formatter = InformationModel.Make<StringFormatter>("Formatter");
        formatter.Format = format;
        for (var i = 0; i < sourcePaths.Length; i++)
        {
            var src = InformationModel.MakeVariable("Source" + i, OpcUa.DataTypes.BaseDataType);
            formatter.Refs.AddReference(FTOptix.CoreBase.ReferenceTypes.HasSource, src);
            var source = Project.Current.GetVariable(sourcePaths[i]);
            if (source != null) src.SetDynamicLink(source);
            else Log.Warning("Ui", "Missing variable " + sourcePaths[i]);
        }
        target.SetConverter(formatter);
    }

    /// <summary>ExpressionEvaluator converter with one source, e.g. bar width = "{0} * 1.8".</summary>
    public static void Expression(IUAVariable target, string expression, string sourcePath)
    {
        var ee = InformationModel.MakeObject<ExpressionEvaluator>("Expression");
        ee.Expression = expression;
        var src = InformationModel.MakeVariable("Source0", OpcUa.DataTypes.BaseDataType);
        ee.Refs.AddReference(FTOptix.CoreBase.ReferenceTypes.HasSource, src);
        var source = Project.Current.GetVariable(sourcePath);
        if (source != null) src.SetDynamicLink(source);
        else Log.Warning("Ui", "Missing variable " + sourcePath);
        target.SetConverter(ee);
    }

    /// <summary>Horizontal or vertical fill bar driven by a 0..100 % variable (belt occupancy, station progress).</summary>
    public static Rectangle Bar(IUANode parent, string name, double x, double y, double w, double h, bool vertical, uint fill, string percentPath)
    {
        var bar = Box(parent, name, x, y, vertical ? w : 0, vertical ? 0 : h, fill, fill, 0);
        var full = vertical ? h : w;
        var k = (full / 100.0).ToString("0.###", CultureInfo.InvariantCulture);
        Expression(vertical ? bar.HeightVariable : bar.WidthVariable, "{0} * " + k, percentPath);
        return bar;
    }

    /// <summary>MouseClick -> call an [ExportMethod] on a model object (same structure Studio writes for Events).</summary>
    public static void OnClick(IUANode widget, IUANode target, string method, params (string Name, NodeId Type, object Value)[] args)
    {
        var eh = InformationModel.MakeObject<FTOptix.CoreBase.EventHandler>("OnMouseClick");
        widget.Add(eh);
        eh.GetOrCreateVariable("ListenEventType").Value = FTOptix.UI.ObjectTypes.MouseClickEvent;
        var mc = InformationModel.MakeObject("MethodContainer1");
        eh.MethodsToCall.Add(mc);
        var objPtr = InformationModel.MakeVariable<NodePointer>("ObjectPointer", OpcUa.DataTypes.NodeId);
        objPtr.Value = target.NodeId;
        mc.Add(objPtr);
        var methodName = InformationModel.MakeVariable("Method", OpcUa.DataTypes.String);
        methodName.Value = method;
        mc.Add(methodName);
        var inputArgs = InformationModel.MakeObject("InputArguments");
        mc.Add(inputArgs);
        foreach (var a in args)
        {
            var v = inputArgs.Context.NodeFactory.MakeVariable(NodeId.Random(inputArgs.NodeId.NamespaceIndex),
                a.Name, a.Type, OpcUa.VariableTypes.BaseDataVariableType, false, a.Value);
            inputArgs.Add(v);
        }
    }

    /// <summary>MouseClick -> built-in VariableCommands.Set(variable, value): the Studio "Set variable value" action.</summary>
    public static void OnClickSet(IUANode widget, string variablePath, object value)
    {
        var target = Project.Current.GetVariable(variablePath);
        if (target == null) { Log.Warning("Ui", "Missing command variable " + variablePath); return; }
        var eh = InformationModel.MakeObject<FTOptix.CoreBase.EventHandler>("OnMouseClick");
        widget.Add(eh);
        eh.GetOrCreateVariable("ListenEventType").Value = FTOptix.UI.ObjectTypes.MouseClickEvent;
        var mc = InformationModel.MakeObject("MethodContainer1");
        eh.MethodsToCall.Add(mc);
        var objPtr = InformationModel.MakeVariable<NodePointer>("ObjectPointer", OpcUa.DataTypes.NodeId);
        objPtr.Value = InformationModel.GetObject(FTOptix.CoreBase.Objects.VariableCommands).NodeId;
        mc.Add(objPtr);
        var methodName = InformationModel.MakeVariable("Method", OpcUa.DataTypes.String);
        methodName.Value = "Set";
        mc.Add(methodName);
        var inputArgs = InformationModel.MakeObject("InputArguments");
        mc.Add(inputArgs);
        var toModify = InformationModel.MakeVariable("VariableToModify", FTOptix.Core.DataTypes.VariablePointer);
        toModify.Value = target.NodeId;
        inputArgs.Add(toModify);
        var v = InformationModel.MakeVariable("Value", target.DataType);
        if (value is bool b) v.Value = b;
        else v.Value = Convert.ToInt32(value);
        inputArgs.Add(v);
        var index = InformationModel.MakeVariable("ArrayIndex", OpcUa.DataTypes.UInt32);
        index.Value = 0u;
        inputArgs.Add(index);
    }

    public static (string, NodeId, object) Str(string name, string value) => (name, OpcUa.DataTypes.String, value);
    public static (string, NodeId, object) Int(string name, int value) => (name, OpcUa.DataTypes.Int32, value);
}
