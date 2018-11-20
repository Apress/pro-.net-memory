// This is the main DLL file.

#include "stdafx.h"
#include "MemoryLeaks.Library.h"
#include "..\UnmanagedLibrary\UnmanagedLibrary.h"


int MemoryLeaksLibrary::Class1::DoSomething(int size)
{
    CUnmanagedLibrary* lib = new CUnmanagedLibrary();
    return lib->CalculateSomething(size);
}
