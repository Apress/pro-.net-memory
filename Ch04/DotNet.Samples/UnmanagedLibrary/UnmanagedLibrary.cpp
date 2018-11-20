// UnmanagedLibrary.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "UnmanagedLibrary.h"


// This is an example of an exported variable
int nUnmanagedLibrary=0;

// This is an example of an exported function.
int fnUnmanagedLibrary(int size)
{
    return size;
}

// This is the constructor of a class that has been exported.
// see UnmanagedLibrary.h for the class definition
CUnmanagedLibrary::CUnmanagedLibrary()
{
    return;
}

int CUnmanagedLibrary::CalculateSomething(int size)
{
    int* buffer = new int[size];
    return 2 * size;
}