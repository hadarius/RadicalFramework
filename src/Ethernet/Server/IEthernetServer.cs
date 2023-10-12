namespace Radical.Ethernet.Server
{
    using Transit;
    using Security.Identity;
    using Invoking;

    public interface IEthernetServer
    {
        void ClearClients();
        void ClearResources();
        void Close();
        void Echo(string message);
        ITransitContext HeaderReceived(object inetdealcontext);
        ITransitContext HeaderSent(object inetdealcontext);
        bool IsActive();
        ITransitContext MessageReceived(object inetdealcontext);
        ITransitContext MessageSent(object inetdealcontext);
        void Start(MemberIdentity ServerIdentity, IMemberSecurity security = null, IInvoker echoMethod = null);
    }
}