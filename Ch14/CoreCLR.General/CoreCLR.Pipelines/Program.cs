using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreCLR.Pipelines
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Listing 14-70
            var pool = MemoryPool<byte>.Shared;
            var options = new PipeOptions(pool: pool, minimumSegmentSize: 128);
            var pipe = new Pipe(options);

            await AsynchronousGetMemoryUsage(pipe);
            SynchronousGetSpanUsage(pipe);
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-71
        static async Task SynchronousUsage(Pipe pipe)
        {
            pipe.Writer.Write(new byte[] { 1, 2, 3 }.AsReadOnlySpan());
            await pipe.Writer.FlushAsync();

            var result = await pipe.Reader.ReadAsync();
            byte[] data = result.Buffer.ToArray();
            pipe.Reader.AdvanceTo(result.Buffer.End);
            //pipe.Reader.Complete();
            data.Print();
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-72
        static void SynchronousGetSpanUsage(Pipe pipe)
        {
            Span<byte> span = pipe.Writer.GetSpan(2);
            span[0] = 1;
            span[1] = 2;
            pipe.Writer.Advance(2);
            pipe.Writer.FlushAsync().GetAwaiter().GetResult();

            var result = pipe.Reader.ReadAsync().GetAwaiter().GetResult();
            byte[] data = result.Buffer.ToArray();
            pipe.Reader.AdvanceTo(result.Buffer.End);
            data.Print();
            pipe.Reader.Complete();
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-73
        static async Task AsynchronousGetMemoryUsage(Pipe pipe)
        {
            Memory<byte> memory = pipe.Writer.GetMemory(minimumLength: 2);
            memory.Span[0] = 1;
            memory.Span[1] = 2;
            Console.WriteLine(memory.Length);
            pipe.Writer.Advance(4);
            await pipe.Writer.FlushAsync();

            Memory<byte> memory2 = pipe.Writer.GetMemory(2);
            memory2.Span[0] = 3;
            memory2.Span[1] = 4;
            pipe.Writer.Advance(4);
            await pipe.Writer.FlushAsync();
            //pipe.Writer.Complete(); close the pipeline from writer side (so reader will not expect more data)

            var result = await pipe.Reader.ReadAsync();
            byte[] data = result.Buffer.ToArray(); // ToArray gets all segments and copies to a new array
            pipe.Reader.AdvanceTo(result.Buffer.End);
            data.Print(); // 1,2,0,0,1,2,0,0
            //pipe.Reader.Complete(); no more reads possible
        }

        ///////////////////////////////////////////////////////////////////////
        // Listing 14-74
        static async Task Process(Pipe pipe)
        {
            PipeReader reader = pipe.Reader;
            var readResult = await reader.ReadAsync();
            var readBuffer = readResult.Buffer;
            SequencePosition consumed;
            SequencePosition examined;
            try
            {
                ProcessBuffer(in readBuffer, out consumed, out examined);
            }
            finally
            {
                reader.AdvanceTo(consumed, examined);
            }
        }

        private static void ProcessBuffer(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = sequence.Start;
            examined = sequence.End;
            if (sequence.IsSingleSegment)
            {
                // Consume buffer as single span
                var span = sequence.First.Span;
                Consume(in span);
            }
            else
            {
                // Consume buffer as collections of spans
                foreach (var segment in sequence)
                {
                    var span = segment.Span;
                    Consume(in span);
                }
            }
            // out consumed - to which position we have already consumed the data (and do not need them anymore)
            // out examined - to which position we have already analyzed the data (data between consumed and examined will be provided again when new data arrives)
        }

        private static void Consume(in ReadOnlySpan<byte> span) // No defensive copy as ReadOnlySpan is readonly struct
        {
            //...
        }

        private static void ProcessWithBufferReader(in ReadOnlySequence<byte> sequence, out SequencePosition consumed, out SequencePosition examined)
        {
            var byteReader = BufferReader.Create(sequence);
            while (!byteReader.End)
            {
                byteReader.Read();
                // Consume...

                consumed = byteReader.Position;
                examined = byteReader.Position; // or less if Peek was used
            }
        }
    }

    static class ArrayExtensions
    {
        public static void Print<T>(this T[] array)
        {
            Console.WriteLine(string.Join(",", array));
        }
    }
}
