using System;
using UnityEngine;

[Serializable]
public class Observer<T>
{

    [SerializeField] T value;
    [SerializeField] Action<T> OnValueChanged;

    public T Value
    {
        get => value;
        set => Set(value);
    }

    public Observer(T val, Action<T> callback = null)
    {
        value = val;
        if (callback != null)
            OnValueChanged += callback;
    }

    void Set(T val)
    {
        if (Equals(value, val))
            return;

        value = val;
        Invoke();
    }

    public void Invoke()
    {
        OnValueChanged?.Invoke(value);
    }

    public void AddListener(Action<T> callback)
    {
        if (callback == null)
            return;

        OnValueChanged += callback;
    }

    public void RemoveListener(Action<T> callback)
    {
        if (callback == null)
            return;

        OnValueChanged -= callback;
    }
    
}
