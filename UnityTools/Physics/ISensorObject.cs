﻿using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Assets.Tools.UnityTools.Physics
{
    public interface ISensorObject
    {
        IReadOnlyCollection<Collision> CollisionData { get; }
        IReadOnlyCollection<Collider> TriggersData { get; }
        
        LayerMask CollisionMask { get; }
        
        IReadOnlyReactiveProperty<bool> TriggerConnectionChanged { get;  }
        IReadOnlyReactiveProperty<bool> CollisionConnectionChanged { get;  }
        
        Collider Collider { get; }
        Collider LastTriggerObject { get;  }
        Collision LastCollisionObject { get;}
        
        Vector3 Position { get; }
        Transform Transform { get; }
        
        void SetCollisionMask(int mask);
        void SetCollisionMask(string[] mask);

        void Reset();
    }
}