using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ExtensionModuleNoticeAttribute : Attribute
{
    public readonly string Message;

    public ExtensionModuleNoticeAttribute(string message)
    {
        Message = message;
    }
}
