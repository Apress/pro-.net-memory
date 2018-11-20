// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the UNMANAGEDLIBRARY_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// UNMANAGEDLIBRARY_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef UNMANAGEDLIBRARY_EXPORTS
#define UNMANAGEDLIBRARY_API __declspec(dllexport)
#else
#define UNMANAGEDLIBRARY_API __declspec(dllimport)
#endif

// This class is exported from the UnmanagedLibrary.dll
class UNMANAGEDLIBRARY_API CUnmanagedLibrary {
public:
	CUnmanagedLibrary(void);
	// TODO: add your methods here.

    int CalculateSomething(int size);
};

extern UNMANAGEDLIBRARY_API int nUnmanagedLibrary;

UNMANAGEDLIBRARY_API int fnUnmanagedLibrary(int );
