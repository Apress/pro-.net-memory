using System.Runtime.CompilerServices;

namespace CoreCLR.CIL
{
    public struct FinalizableStruct
    {
        [MethodImpl(MethodImplOptions.ForwardRef)]
        public extern void M();

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public extern void Finalize();
    }
}