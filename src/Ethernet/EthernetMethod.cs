namespace Radical.Ethernet
{
    using Invoking;

    [Serializable]
    public class EthernetMethod : Invoker
    {
        public EthernetMethod(string MethodName, object TargetClassObject, params object[] parameters)
            : base(TargetClassObject, MethodName)
        {
            base.ParameterValues = parameters;
        }

        public EthernetMethod(string MethodName, string TargetClassName, params object[] parameters)
            : base(TargetClassName, MethodName)
        {
            base.ParameterValues = parameters;
        }
    }
}
