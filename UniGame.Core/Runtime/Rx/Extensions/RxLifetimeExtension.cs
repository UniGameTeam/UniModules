﻿namespace UniGreenModules.UniCore.Runtime.Rx.Extensions
{
    using System;
    using Common;
    using DataFlow.Interfaces;
    using Interfaces;
    using ObjectPool.Runtime;
    using ObjectPool.Runtime.Interfaces;

    public static class RxLifetimeExtension 
    {

        public static T AddTo<T>(this T disposable, ILifeTime lifeTime)
            where T : class, IDisposable
        {
            if (disposable != null)
                lifeTime.AddDispose(disposable);
            return disposable;
        }
        
        public static ICompletionSource AddTo(this ILifeTime lifeTime, Action cleanupAction)
        {
            var disposableAction = ClassPool.Spawn<DisposableAction>();
            disposableAction.Initialize(cleanupAction);
            lifeTime.AddDispose(disposableAction);
            return disposableAction;
        }
        
    }
}
