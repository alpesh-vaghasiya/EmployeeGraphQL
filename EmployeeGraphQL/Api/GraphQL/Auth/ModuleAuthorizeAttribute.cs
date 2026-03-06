using System.Reflection;
using HotChocolate.Types.Descriptors;

namespace Api.GraphQL.Auth;

[AttributeUsage(AttributeTargets.Method)]
public class ModuleAuthorizeAttribute : ObjectFieldDescriptorAttribute
{
    public string[] ModuleCodes { get; }
    public string[] Actions { get; }

    public ModuleAuthorizeAttribute(string[] moduleCodes, string[] actions)
    {
        ModuleCodes = moduleCodes;
        Actions = actions;
    }

    protected override void OnConfigure(
        IDescriptorContext context,
        IObjectFieldDescriptor descriptor,
        MemberInfo member)
    {
        descriptor.Extend().OnBeforeCreate(d =>
        {
            d.ContextData["ModuleAuthorize"] = this;
        });
    }
}