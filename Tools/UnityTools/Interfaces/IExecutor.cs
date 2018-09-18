﻿using System.Collections;

namespace Assets.Scripts.StateMachine
{
    public interface IExecutor : ICommonExecutor<IEnumerator> { }

    public interface ICommonExecutor<TData>
    {
        void Execute(TData data);
        void Stop();
    }
}