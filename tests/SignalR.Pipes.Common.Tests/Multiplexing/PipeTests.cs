//using SignalR.Pipes.Common.Pipelines;
//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.IO.Pipelines;
//using System.IO.Pipes;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace SignalR.Pipes.Common.Multiplexing
//{
//    public class PipeTests
//    {
//        unsafe Sun ReadData(in ReadOnlySequence<byte> buffer)
//        {
//            var reader = new SequenceReader<byte>(buffer);

//            if (reader.TryRead<Sun>(out var result))
//            {
//                return result;
//            }
//            return default;
//        }

//        enum K : byte
//        {
//            S,
//            K
//        }

//        struct Sun
//        {
//            public K Enum { get; set; }

//            public int S { get; set; }

//            public ushort S2 { get; set; }
//        }

//        private class DuplexPipe : IDuplexPipe
//        {
//            internal DuplexPipe(PipeReader input, PipeWriter output)
//            {
//                Input = input;
//                Output = output;
//            }

//            public PipeReader Input { get; }

//            public PipeWriter Output { get; }
//        }

//        [Fact]
//        public async Task Test()
//        {
//            void WriteData(in IBufferWriter<byte> bufferWriter)
//            {
//                var binaryWriter = new BinaryWriter(bufferWriter);
//                var str = new FrameHeader
//                {
//                    ChannelId = 2,
//                    FrameType = FrameType.Data,
//                    Size = 10
//                };
//                binaryWriter.Write(ref str);
//            }

            

//            var incoming = new Pipe();
//            var outgoing = new Pipe();

//            var duplexPipe = new DuplexPipe(incoming.Reader, outgoing.Writer);
//            var virtualStream = new VirtualDuplexPipeManager(duplexPipe);

//            WriteData(incoming.Writer);
//            await incoming.Writer.FlushAsync();
//            var mem = incoming.Writer.GetMemory(10);
//            byte[] b = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
//            b.CopyTo(mem);
//            await incoming.Writer.WriteAsync(mem);
//            await incoming.Writer.FlushAsync();

//            await Task.Delay(1000000);
//        }

//        [Fact]
//        public async Task Test2()
//        {
//            var server = new NamedPipeServerStream("testtesttest", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous);
//            var serverConnectTask = server.WaitForConnectionAsync();

//            var client = new NamedPipeClientStream(".", "testtesttest", PipeDirection.InOut, System.IO.Pipes.PipeOptions.Asynchronous);
//            await client.ConnectAsync();

//            await serverConnectTask;

//            var serverPipe = server.AsDuplexPipe();
//            var clientPipe = client.AsDuplexPipe();

//            var serverVirtual = new VirtualDuplexPipeManager(serverPipe);
//            var clientVirtual = new VirtualDuplexPipeManager(clientPipe);

//            await clientVirtual.ConnectAsync("testname");

//            await Task.Delay(10000000);
//        }
//    }
//}
