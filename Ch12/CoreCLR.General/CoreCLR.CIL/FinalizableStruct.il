.class public sealed CoreCLR.CIL.FinalizableStruct
	extends System.ValueType
{
    .method public hidebysig virtual 
        instance void Finalize () cil managed 
    {
        .try
        {
            ldstr "Finalizing boxed struct."
            call void [mscorlib]System.Console::WriteLine(string)
            leave.s exit
        }
        finally
        {
            ldarg.0
            call instance void [mscorlib]System.Object::Finalize()
            endfinally
        }
        exit: ret
    }

	.method public hidebysig 
        instance void M () cil managed 
    {
        ldstr "Hello from struct!"
        call void [mscorlib]System.Console::WriteLine(string)
        ret
    }
}
