namespace Bamboozlers.Classes.Func;

public delegate void BiConsumer<in T, in TU>(T t, TU u);

public delegate Task AsyncBiConsumer<in T, in TU>(T t, TU u);