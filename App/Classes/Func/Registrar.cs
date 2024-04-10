namespace Bamboozlers.Classes.Func;

public delegate bool Register<in T>(T value) where T : class;

public delegate int Unregister<in T>(T value) where T : class;