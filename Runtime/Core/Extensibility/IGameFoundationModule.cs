using System.Collections;

public interface IGameFoundationModule
{
    int Order { get; }
    IEnumerator Initialize();
}
