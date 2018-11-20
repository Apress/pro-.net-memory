.class public CoreCLR.CIL.FinalizableClass
{
	.method public hidebysig 
        instance void M () cil managed 
    {
        .maxstack 8
        ldstr "Hello from class!"
        call void [mscorlib]System.Console::WriteLine(string)
        ret
    }

    .method family hidebysig virtual 
        instance void Finalize () cil managed 
    {
        .override method instance void [mscorlib]System.Object::Finalize()
        .maxstack 1
        .try
        {
            ldstr "Finalizing class"
            call void [mscorlib]System.Console::WriteLine(string)
            leave.s IL_0013
        }
        finally
        {
            ldarg.0
            call instance void [mscorlib]System.Object::Finalize()
            endfinally
        }
        IL_0013: ret
    }
}
