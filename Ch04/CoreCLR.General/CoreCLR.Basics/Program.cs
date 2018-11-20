using System;

namespace CoreCLR.Basics
{
    //.class private auto ansi beforefieldinit CoreCLR.HelloWorld.Program
    // extends [System.Runtime]System.Object
    // {
    // // Token: 0x06000001
    // .method private hidebysig static 
    // void Main (
    // string[] args
    // ) cil managed 
    // {
    // // Header Size: 1 byte
    // // Code Size: 11 (0xB) bytes
    // .maxstack 8
    // .entrypoint
    // 
    // /* 7201000070   */ IL_0000: ldstr     "Hello World!"
    // /* 280C00000A   */ IL_0005: call      void [System.Console]System.Console::WriteLine(string)
    // /* 2A           */ IL_000A: ret
    // } // end of method Program::Main
    // 
    // } // end of class CoreCLR.HelloWorld.Program
    // 
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
