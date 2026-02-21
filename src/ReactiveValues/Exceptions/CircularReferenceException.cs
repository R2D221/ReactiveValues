namespace ReactiveValues.Exceptions;

public sealed class CircularReferenceException() : Exception("There's a circular reference in the reactive graph.");
