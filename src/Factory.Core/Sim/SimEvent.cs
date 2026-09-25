// @summary: Alarm/event record (fault raised/cleared, maintenance) emitted by the engine.
#nullable enable
namespace Factory.Core.Sim;

public enum SimEventKind { FaultRaised, FaultCleared, MaintenanceStarted, MaintenanceEnded }

public sealed record SimEvent(double Time, string NodeId, SimEventKind Kind, int Code, string Text);
