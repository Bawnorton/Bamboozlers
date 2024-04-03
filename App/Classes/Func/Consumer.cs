namespace Bamboozlers.Classes.Func;

/// <summary>
///     Function that takes an argume and returns no value.
/// </summary>
public delegate void Consumer<in T>(T arg);

public delegate Task AsyncConsumer<in T>(T arg);