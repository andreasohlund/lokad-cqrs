using System;
using System.Runtime.InteropServices;

namespace Lokad.Cqrs
{
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct unit
    {

        public static readonly unit it = default(unit);
    }
}