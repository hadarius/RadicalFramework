using System.Reflection;
using System.Threading.Tasks;

namespace Radical.Invoking
{
    using Uniques;

    public interface IInvoker : IUnique
    {
        string Name { get; set; }

        string QualifiedName { get; set; }

        object TargetObject { get; set; }

        MethodInfo Info { get; set; }

        ParameterInfo[] Parameters { get; set; }

        object[] ParameterValues { get; set; }

        InvokerDelegate MethodInvoker { get; }

        Delegate Method { get; }

        Task Publish(params object[] parameters);
        Task Publish(bool firstAsTarget, object target, params object[] parameters);

        object Invoke(params object[] parameters);
        object Invoke(bool firstAsTarget, object target, params object[] parameters);

        Task<object> InvokeAsync(params object[] parameters);
        Task<object> InvokeAsync(bool firstAsTarget, object target, params object[] parameters);

        Task<T> InvokeAsync<T>(params object[] parameters);
        Task<T> InvokeAsync<T>(bool firstAsTarget, object target, params object[] parameters);
    }
}
