namespace ReactiveValues.Exceptions;

public sealed class FrozenReactiveGraphException() : Exception("The reactive graph can't be observed or modified while a watcher notification is in progress.");
