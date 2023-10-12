namespace Radical.Instant.Series.Stocking
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Buffers;
    using Security.Identity;
    using Extracting;

    public unsafe class StockContext : IStockContext, IDisposable
    {
        public IntPtr receivingBlockHandle;
        public IntPtr sendingBlockHandle;
        public int ClientWaitCount = 0;
        public int ReadCount = 0;
        public int ServerWaitCount = 0;
        public int WriteCount = 0;
        private byte[] receivingBlock = new byte[0];
        private byte[] sendingBlock = new byte[0];

        public StockContext() { }

        public int BlockOffset { get; set; } = 16;

        public long BlockSize { get; set; } = 0;

        public long BufferSize { get; set; } = 1048576;

        public int ClientCount { get; set; } = 1;

        public byte[] DeserialBlock
        {
            get
            {
                byte[] result = null;
                lock (receivingBlock)
                {
                    BlockSize = 0;
                    result = receivingBlock;
                    receivingBlock = new byte[0];
                }
                return result;
            }
        }

        public int DeserialBlockId { get; set; } = 0;

        public IntPtr DeserialBlockPtr
        {
            get
            {
                return GCHandle.FromIntPtr(receivingBlockHandle).AddrOfPinnedObject() + BlockOffset;
            }
        }

        public int Items { get; set; } = 1;

        public string File { get; set; }

        public long FreeSize { get; set; } = 0;

        public long ItemCapacity { get; set; } = -1;

        public int ItemCount { get; set; } = -1;

        public int ItemSize { get; set; } = -1;

        public int NodeCount { get; set; } = 50;

        public int ObjectPosition { get; set; } = 0;

        public int ObjectsLeft { get; set; } = 0;

        public string Path { get; set; }

        public ushort SectorId { get; set; } = 0;

        public byte[] SerialBlock
        {
            get { return sendingBlock; }
            set
            {
                sendingBlock = value;
                if (sendingBlock != null && BlockOffset > 0)
                {
                    long size = sendingBlock.Length - BlockOffset;
                    new byte[] { (byte)'D', (byte)'A', (byte)'T', (byte)'A' }.CopyTo(
                        sendingBlock,
                        0
                    );
                    size.GetBytes().CopyTo(sendingBlock, 4);
                    ObjectPosition.GetBytes().CopyTo(sendingBlock, 12);
                    sendingBlockHandle = GCHandle.ToIntPtr(
                        GCHandle.Alloc(sendingBlock, GCHandleType.Pinned)
                    );
                }
            }
        }

        public int SerialBlockId { get; set; } = 0;

        public IntPtr SerialBlockPtr
        {
            get { return GCHandle.FromIntPtr(sendingBlockHandle).AddrOfPinnedObject(); }
        }

        public int ServerCount { get; set; } = 1;

        public ServiceSite Site { get; set; }

        public ushort StockId { get; set; } = 0;

        public long UsedSize { get; set; } = 0;

        public void Dispose()
        {
            if (!receivingBlockHandle.Equals(IntPtr.Zero))
            {
                GCHandle gc = GCHandle.FromIntPtr(receivingBlockHandle);
                gc.Free();
            }
            if (!sendingBlockHandle.Equals(IntPtr.Zero))
            {
                GCHandle gc = GCHandle.FromIntPtr(sendingBlockHandle);
                gc.Free();
            }
            receivingBlock = null;
            sendingBlock = null;
        }

        public object ReadStock(IStock stock)
        {
            if (stock != null)
            {
                stock.ReadHeader();
                BufferSize = stock.BufferSize;
                byte[] bufferread = new byte[BufferSize];
                GCHandle handler = GCHandle.Alloc(bufferread, GCHandleType.Pinned);
                IntPtr rawpointer = handler.AddrOfPinnedObject();
                stock.Read(rawpointer, BufferSize, 0L);
                ReceiveBytes(bufferread, BufferSize);
                handler.Free();
            }
            return DeserialBlock;
        }

        public IntPtr ReadStockPtr(IStock stock)
        {
            if (stock != null)
            {
                stock.ReadHeader();
                BufferSize = stock.BufferSize;
                receivingBlock = new byte[BufferSize];

                GCHandle handler = GCHandle.Alloc(receivingBlock, GCHandleType.Pinned);
                receivingBlockHandle = GCHandle.ToIntPtr(handler);
                IntPtr rawpointer = handler.AddrOfPinnedObject();
                stock.Read(rawpointer, BufferSize, 0);
                ReceiveBytes(rawpointer, BufferSize);
            }
            return DeserialBlockPtr;
        }

        public MarkupType ReceiveBytes(byte[] buffer, int received)
        {
            MarkupType noiseKind = MarkupType.None;
            lock (receivingBlock)
            {
                int offset = 0,
                    length = received;
                bool inprogress = false;

                if (BlockSize == 0)
                {
                    BlockSize = BitConverter.ToInt64(buffer, 4);
                    DeserialBlockId = BitConverter.ToInt32(buffer, 12);
                    receivingBlock = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(receivingBlock, GCHandleType.Pinned);
                    receivingBlockHandle = GCHandle.ToIntPtr(gc);
                    offset = BlockOffset;
                    length -= BlockOffset;
                }
                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = received;
                    noiseKind = buffer.SeekMarkup(out endPosition, SeekDirection.Backward);
                }
                int destid = (receivingBlock.Length - ((int)BlockSize + length));
                if (inprogress)
                {
                    fixed (void* msgbuff = buffer)
                    {
                        Extracting.Extract.CopyBlock(
                            GCHandle
                                .FromIntPtr(receivingBlockHandle)
                                .AddrOfPinnedObject()
                                .ToPointer(),
                            (ulong)destid,
                            msgbuff,
                            (ulong)offset,
                            (ulong)length
                        );
                    }
                }
            }
            return noiseKind;
        }

        public MarkupType ReceiveBytes(byte[] buffer, long received)
        {
            MarkupType noiseKind = MarkupType.None;
            lock (receivingBlock)
            {
                int offset = 0,
                    length = (int)received;
                bool inprogress = false;
                if (BlockSize == 0)
                {
                    BlockSize = BitConverter.ToInt64(buffer, 4);
                    DeserialBlockId = BitConverter.ToInt32(buffer, 12);
                    receivingBlock = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(receivingBlock, GCHandleType.Pinned);
                    receivingBlockHandle = GCHandle.ToIntPtr(gc);
                    offset = BlockOffset;
                    length -= BlockOffset;
                }

                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = received;
                    noiseKind = buffer.SeekMarkup(out endPosition, SeekDirection.Backward);
                }

                int destid = (receivingBlock.Length - ((int)BlockSize + length));
                if (inprogress)
                {
                    fixed (byte* msgbuff = buffer)
                    {
                        Extracting.Extract.CopyBlock(
                            GCHandle
                                .FromIntPtr(receivingBlockHandle)
                                .AddrOfPinnedObject()
                                .ToPointer(),
                            (ulong)destid,
                            msgbuff,
                            (ulong)offset,
                            (ulong)length
                        );
                    }
                }
            }
            return noiseKind;
        }

        public void ReceiveBytes(IntPtr buffer, long received)
        {
            lock (receivingBlock)
            {
                BlockSize = *((int*)(buffer + 4));
                DeserialBlockId = *((int*)(buffer + 12));
            }
        }

        public void WriteStock(IStock stock)
        {
            if (stock != null)
            {
                GCHandle handler = GCHandle.Alloc(SerialBlock, GCHandleType.Pinned);
                IntPtr rawpointer = handler.AddrOfPinnedObject();
                stock.BufferSize = SerialBlock.Length;
                stock.WriteHeader();
                stock.Write(rawpointer, SerialBlock.Length);
                handler.Free();
            }
        }

        public void WriteStockPtr(IStock stock)
        {
            if (stock != null)
            {
                stock.BufferSize = BlockSize;
                stock.WriteHeader();
                stock.Write(SerialBlockPtr, BlockSize);
            }
        }
    }
}
