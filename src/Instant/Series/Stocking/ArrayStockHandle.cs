namespace Radical.Instant.Series.Stocking
{
    using System;
    using Extracting;
    using System.Runtime.InteropServices;

    public class ArrayStockHandle
    {
        public int sizeStruct = 0;
        public Type typeStruct = null;

        public ArrayStockHandle(Type t)
        {
            typeStruct = t;
            sizeStruct = Marshal.SizeOf(t);
        }

        public unsafe void* GetPtr(object[] structure)
        {
            GCHandle pinn = GCHandle.Alloc(structure, GCHandleType.Pinned);
            IntPtr address = pinn.AddrOfPinnedObject();
            return address.ToPointer();
        }

        public object PtrToStructure(IntPtr pointer)
        {
            return Extract.PointerToStructure(pointer, typeStruct, 0);
        }

        public void PtrToStructure(IntPtr pointer, object structure)
        {
            if (structure != null)
                Extract.PointerToStructure(pointer, structure);
            else
                structure = Extract.PointerToStructure(pointer, typeStruct, 0);
        }

        public unsafe void ReadArray(ref object[] buffer, byte* source, int index, int count)
        {
            if (index < 0)
                index = 0;
            int bufferIndex = index;
            if (buffer == null)
            {
                buffer = new object[count];
                bufferIndex = 0;
            }
            else if (buffer.Length - index < count)
            {
                count = buffer.Length - index;
            }
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            int offset = index * sizeStruct;
            for (int i = 0; i < count; i++)
            {
                int arrayoffset = i + bufferIndex;
                if (buffer[arrayoffset] != null)
                    Extract.PointerToStructure(
                        source + ((i * sizeStruct) + offset),
                        buffer[arrayoffset]
                    );
                else
                    buffer[arrayoffset] = Extract.PointerToStructure(
                        source,
                        typeStruct,
                        ((i * sizeStruct) + offset)
                    );
            }
        }

        public unsafe void ReadArray(
            ref object[] buffer,
            int destIndex,
            byte* source,
            int index,
            int count
        )
        {
            if (index < 0)
                index = 0;
            if (buffer == null)
            {
                buffer = new object[count];
                destIndex = 0;
            }
            else if (buffer.Length - destIndex - index < count)
                count = buffer.Length - destIndex - index;
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            int offset = index * sizeStruct;

            for (int i = 0; i < count; i++)
            {
                int arrayoffset = i + destIndex;
                if (buffer[arrayoffset] != null)
                    Extract.PointerToStructure(
                        source + ((i * sizeStruct) + offset),
                        buffer[arrayoffset]
                    );
                else
                    buffer[arrayoffset] = Extract.PointerToStructure(
                        source,
                        typeStruct,
                        ((i * sizeStruct) + offset)
                    );
            }
        }

        public unsafe void ReadArray(ref object[] buffer, IntPtr source, int index, int count)
        {
            if (index < 0)
                index = 0;
            int bufferIndex = index;
            if (buffer == null)
            {
                buffer = new object[count];
                bufferIndex = 0;
            }
            else if (buffer.Length - index < count)
            {
                count = buffer.Length - index;
            }
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            int offset = index * sizeStruct;

            for (int i = 0; i < count; i++)
            {
                int arrayoffset = i + bufferIndex;
                if (buffer[arrayoffset] != null)
                    Extract.PointerToStructure(
                        source + ((i * sizeStruct) + offset),
                        buffer[arrayoffset]
                    );
                else
                    buffer[arrayoffset] = Extract.PointerToStructure(
                        source,
                        typeStruct,
                        ((i * sizeStruct) + offset)
                    );
            }
        }

        public int SizeOf(object t)
        {
            return sizeStruct;
        }

        public unsafe void StructureToPtr(object s, IntPtr pointer)
        {
            s.StructureTo((byte*)pointer);
        }

        public unsafe void WriteArray(
            byte* destination,
            int srcIndex,
            ref object[] buffer,
            int index,
            int count
        )
        {
            if (index < 0)
                index = 0;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count)
                count = buffer.Length - index;

            int offset = index * sizeStruct;

            for (int i = 0; i < count; i++)
            {
                int arrayoffset = i + srcIndex;
                Extract.StructureToPointer(
                    buffer[arrayoffset],
                    destination + ((i * sizeStruct) + offset)
                );
            }
        }

        public unsafe void WriteArray(byte* destination, ref object[] buffer, int index, int count)
        {
            if (index < 0)
                index = 0;
            int bufferIndex = index;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count)
                count = buffer.Length - index;

            int offset = index * sizeStruct;

            for (int i = 0; i < count; i++)
            {
                int arrayoffset = i + bufferIndex;
                Extract.StructureToPointer(
                    buffer[arrayoffset],
                    destination + ((i * sizeStruct) + offset)
                );
            }
        }

        public unsafe void WriteArray(IntPtr destination, ref object[] buffer, int index, int count)
        {
            if (index < 0)
                index = 0;
            int bufferIndex = index;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count)
                count = buffer.Length - index;

            int offset = index * sizeStruct;

            for (int i = 0; i < count; i++)
            {
                int arrayoffset = i + bufferIndex;
                Extract.StructureToPointer(
                    buffer[arrayoffset],
                    destination + ((i * sizeStruct) + offset)
                );
            }
        }
    }
}
