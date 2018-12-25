﻿namespace Assets.Tools.UnityTools.Interfaces
{
    public interface IValidator<TData>
    {
        bool Validate(TData data);
    }

    public interface IValidator
    {
        bool Validate();
    }
}