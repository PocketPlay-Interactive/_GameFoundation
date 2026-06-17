using System.Collections;
using UnityEngine;

public abstract class GameFoundationModuleBehaviour : MonoBehaviour, IGameFoundationModule
{
    [SerializeField] private int order;

    public virtual int Order => order;

    public abstract IEnumerator Initialize();
}
