﻿using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security;

namespace Radical.Instant.Series.Stocking.MMF;

using Extracting;

#if !NET40Plus

public sealed class MMViewAccessor : IDisposable
{
    MMView _view;

    internal MMViewAccessor(MMView memoryMappedView)
    {
        this._view = memoryMappedView;
    }

    public SafeMMViewHandle SafeMemoryMappedViewHandle
    {
        [SecurityCritical]
        get { return this._view.SafeMemoryMappedViewHandle; }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposeManagedResources)
    {
        if (_view != null)
            _view.Dispose();
        _view = null;
    }

    internal static unsafe void PtrToStructure(byte* ptr, object structure)
    {
        Extracting.Extract.PointerToStructure(ptr, structure);
    }

    internal static unsafe object PtrToNewStructure(byte* ptr, Type t)
    {
        return Extracting.Extract.PointerToStructure(ptr, t, 0);
    }

    internal static unsafe void StructureToPtr(object structure, byte* ptr)
    {
        Extracting.Extract.StructureToPointer(structure, ptr);
    }

    internal unsafe void Write(long position, object structure)
    {
        int itemSize = Marshal.SizeOf(structure.GetType());
        if (position > this._view.Size - itemSize)
            throw new ArgumentOutOfRangeException("position", "");

        try
        {
            byte* ptr = null;
            _view.SafeMemoryMappedViewHandle.AcquireIntPtr(ref ptr);
            ptr += _view.ViewStartOffset;
            StructureToPtr(structure, ptr + position);
        }
        finally
        {
            _view.SafeMemoryMappedViewHandle.ReleaseIntPtr();
        }
    }

    internal unsafe void WriteArray(long position, object[] buffer, int index, int count)
    {
        Type t = null;
        if (buffer != null && buffer.Length > 0)
            t = buffer[0].GetType();
        else
            throw new ArgumentOutOfRangeException("position");

        uint itemSize = (uint)Marshal.SizeOf(t);

        if (position > this._view.Size - (itemSize * count))
            throw new ArgumentOutOfRangeException("position");

        try
        {
            byte* ptr = null;
            _view.SafeMemoryMappedViewHandle.AcquireIntPtr(ref ptr);
            ptr += _view.ViewStartOffset + position;

            for (var i = 0; i < count; i++)
                StructureToPtr(buffer[index + i], ptr + (i * itemSize));
        }
        finally
        {
            _view.SafeMemoryMappedViewHandle.ReleaseIntPtr();
        }
    }

    internal unsafe object Read(long position, object structure, int itemSize, Type t)
    {
        if (position > this._view.Size - itemSize)
            throw new ArgumentOutOfRangeException("position", "");
        try
        {
            byte* ptr = null;
            _view.SafeMemoryMappedViewHandle.AcquireIntPtr(ref ptr);
            ptr += _view.ViewStartOffset;
            if (structure == null)
                structure = PtrToNewStructure(ptr + position, t);
            else
                PtrToStructure(ptr + position, structure);
        }
        finally
        {
            _view.SafeMemoryMappedViewHandle.ReleaseIntPtr();
        }
        return structure;
    }

    internal unsafe void ReadArray(
        long position,
        object[] buffer,
        int index,
        int count,
        int itemSize,
        Type t
    )
    {
        if (position > this._view.Size - (itemSize * count))
            throw new ArgumentOutOfRangeException("position");
        try
        {
            byte* ptr = null;
            _view.SafeMemoryMappedViewHandle.AcquireIntPtr(ref ptr);
            ptr += _view.ViewStartOffset + position;

            if (buffer == null || buffer.Length < 1)
            {
                buffer = new object[count];
                for (var i = 0; i < count; i++)
                    buffer[index + i] = PtrToNewStructure(ptr + (i * itemSize), t);
            }
            else
                for (var i = 0; i < count; i++)
                    PtrToStructure(ptr + (i * itemSize), buffer[index + i]);
        }
        finally
        {
            _view.SafeMemoryMappedViewHandle.ReleaseIntPtr();
        }
    }
}
#endif
