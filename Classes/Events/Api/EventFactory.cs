using Bamboozlers.Classes.Events.Impl;

namespace Bamboozlers.Classes.Events.Api;

public static class EventFactory
{
    public static Event<T> CreateArrayBacked<T>(Func<T[], T> invokerFactory)
    {
        return EventFactoryImpl.CreateArrayBacked(invokerFactory);
    }
    
    public static Event<T> CreateWithPhases<T>(Func<T[], T> invokerFactory, string[] phases)
    {
        EventFactoryImpl.EnsureContainsDefault(phases);
        EventFactoryImpl.EnsureNoDuplicates(phases);
        
        var arrayBacked = CreateArrayBacked(invokerFactory);
        
        for(var i = 1; i < phases.Length; i++)
        {
            arrayBacked.AddPhaseOrdering(phases[i - 1], phases[i]);
        }
        
        return arrayBacked;
    }
}