namespace Radical.Ethernet
{
    using Uniques;
    using Transit;
    using Security.Identity;

    public enum StateFlag : ushort
    {
        Synced = 1,
        Edited = 2,
        Added = 4,
        Quered = 8,
        Saved = 16,
        Canceled = 32
    }

    public class EthernetOperationHandler : IDisposable
    {
        [NonSerialized]
        public ITransitContext transferContext;
        private ITransitFormatter _data;
        private EthernetContext dealContext;
        private DirectionType direction;
        private ServiceSite site;
        private ushort state;

        public EthernetOperationHandler(object data)
        {
            site = ServiceSite.Server;
            direction = DirectionType.None;
            state = ((IUniqueStructure)data).Flags;
            _data = (ITransitFormatter)data;
        }

        public EthernetOperationHandler(object data, DirectionType directionType, EthernetTransit transfer)
            : this(data)
        {
            direction = directionType;
            transferContext = transfer.Context;
            dealContext = transfer.MyHeader.Context;
        }

        public void Dispose() { }

        public void Handle(out object[] messages)
        {
            messages = null;
            switch (site)
            {
                case ServiceSite.Server:
                    switch (direction)
                    {
                        case DirectionType.Receive:

                            break;
                        case DirectionType.Send:
                            switch (state & (int)StateFlag.Synced)
                            {
                                case 0:
                                    SrvSendSync(out messages);
                                    break;
                            }
                            break;
                        case DirectionType.None:
                            switch (state & (int)StateFlag.Synced)
                            {
                                case 0:
                                    SrvSendSync(out messages);
                                    break;
                            }
                            break;
                    }
                    break;
                case ServiceSite.Client:
                    switch (direction)
                    {
                        case DirectionType.Receive:

                            break;
                        case DirectionType.Send:
                            switch (state & (int)StateFlag.Synced)
                            {
                                case 0:
                                    CltSendSync(out messages);
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        private void CltSendSync(out object[] messages)
        {
            if (direction != DirectionType.None)
                if (
                    ((state & (int)StateFlag.Edited) > 0)
                    || ((state & (int)StateFlag.Saved) > 0)
                    || ((state & (int)StateFlag.Quered) > 0)
                    || ((state & (int)StateFlag.Canceled) > 0)
                )
                {
                    transferContext.Synchronic = true;
                    dealContext.Synchronic = true;
                }

            messages = _data.GetMessage();
        }

        private void SrvSendCancel(out object[] messages)
        {
            messages = _data.GetMessage();
        }

        private void SrvSendEdit(out object[] messages)
        {
            messages = _data.GetMessage();
        }

        private void SrvSendQuery(out object[] messages)
        {
            messages = _data.GetMessage();
        }

        private void SrvSendSave(out object[] messages)
        {
            messages = _data.GetMessage();
        }

        private void SrvSendSync(out object[] messages)
        {
            if (direction != DirectionType.None)
                if (
                    ((state & (int)StateFlag.Edited) > 0)
                    || ((state & (int)StateFlag.Saved) > 0)
                    || ((state & (int)StateFlag.Quered) > 0)
                    || ((state & (int)StateFlag.Canceled) > 0)
                )
                {
                    transferContext.Synchronic = true;
                    dealContext.Synchronic = true;
                }

            messages = null;
            switch (state & (int)StateFlag.Edited)
            {
                case 2:
                    SrvSendEdit(out messages);
                    break;
            }
            switch (state & (int)StateFlag.Canceled)
            {
                case 32:
                    SrvSendCancel(out messages);
                    break;
            }
            switch ((int)state & (int)StateFlag.Saved)
            {
                case 16:
                    SrvSendSave(out messages);
                    break;
            }
            switch (state & (int)StateFlag.Quered)
            {
                case 8:
                    SrvSendQuery(out messages);
                    break;
            }
            if (messages == null)
            {
                messages = _data.GetMessage();
            }
        }
    }
}
