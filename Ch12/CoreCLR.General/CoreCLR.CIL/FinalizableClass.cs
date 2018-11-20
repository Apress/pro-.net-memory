using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CoreCLR.CIL
{
	public class FinalizableClass
	{
		[MethodImpl(MethodImplOptions.ForwardRef)]
		public extern void M();

        [MethodImpl(MethodImplOptions.ForwardRef)]
	    public extern void Finalize();
    }
}
