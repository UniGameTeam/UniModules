﻿namespace UniTools.UniRoutine.Runtime
{
    using System.Collections;
    using UniGreenModules.UniCore.Runtime.Interfaces;

    public static class UniRoutineExtension {

        public static IDisposableItem RunWithSubRoutines(this IEnumerator enumerator, 
            RoutineType routineType = RoutineType.Update)
        {
            return ExecuteRoutine(enumerator,routineType);   
        }
        
        public static IDisposableItem ExecuteRoutine(
            this IEnumerator enumerator, 
            RoutineType routineType = RoutineType.Update,
            bool moveNextImmediately = false)
        {
            return Execute(enumerator,routineType,moveNextImmediately).ToDisposable();
        }
	
        public static RoutineValue Execute(
            this IEnumerator enumerator, 
            RoutineType routineType = RoutineType.Update,
            bool moveNextImmediately = false)
        {
            return UniRoutineManager.RunUniRoutine(enumerator,routineType,moveNextImmediately);
        }

        
    }
}
