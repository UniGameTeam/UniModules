﻿namespace UniGreenModules.UniNodeSystem.Runtime
{
    using global::UniStateMachine.Runtime;
    using Interfaces;
    using UniCore.Runtime.Interfaces;
    using UniStateMachine.Runtime;
    using UniTools.UniRoutine.Runtime;

    public class NodeRoutineExecutor : INodeExecutor<IContext>
    {   
        public void Execute(IUniNode node, IContext context)
        {
            if (node.IsActive)
                return;

            StateLogger.LogState($"GRAPH NODE {node.ItemName} Type {node.GetType()}: STARTED");

            var inputValue = node.Input;
            inputValue.Add(context);

            var awaiter = node.Execute(context);
            var disposable = awaiter.RunWithSubRoutines(node.RoutineType);

            //cleanup actions
            var lifeTime = node.LifeTime;
            lifeTime.AddDispose(disposable);
        }

        public void Stop(IUniNode node)
        {
            //node already stoped
            if (!node.IsActive)
                return;

            StateLogger.LogState($"GRAPH NODE  {node.ItemName} Type {node.GetType()}: STOPED");

            node.Exit();
        }
    }
}
