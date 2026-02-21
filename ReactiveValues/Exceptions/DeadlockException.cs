namespace ReactiveValues.Exceptions;

public sealed class DeadlockException() : Exception("A deadlock was detected while recomputing the reactive graph.");