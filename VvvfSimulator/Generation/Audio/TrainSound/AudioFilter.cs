using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Runtime.InteropServices;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class AudioFilter
    {
        public class MonauralFilter : ISampleProvider
        {
            private readonly ISampleProvider sourceProvider;
            private BiQuadFilter[,] filters;
            private int filterCount;
            private bool updated;
            public MonauralFilter(ISampleProvider sourceProvider, BiQuadFilter[,] filters)
            {
                this.sourceProvider = sourceProvider;
                filterCount = filters.Length;
                this.filters = filters;
            }

            public void Update(BiQuadFilter[,] filters)
            {
                this.filters = filters;
                this.filterCount = filters.Length;
                updated = true;
            }
            public WaveFormat WaveFormat
            {
                get
                {
                    return sourceProvider.WaveFormat;
                }
            }
            public int Read(float[] buffer, int offset, int count)
            {
                int samplesRead = sourceProvider.Read(buffer, offset, count);

                if (updated)
                {
                    updated = false;
                }

                for (int sample = 0; sample < samplesRead; sample++)
                {
                    for (int band = 0; band < filterCount; band++)
                    {
                        buffer[offset + sample] = filters[0, band].Transform(buffer[offset + sample]);
                    }
                }
                return samplesRead;
            }
        }

        public class CppConvolutionFilter : ISampleProvider
        {
            readonly ulong[] address;

            [DllImport("AudioFilter.dll")]
            private static extern ulong createConvolverInstance();

            [DllImport("AudioFilter.dll")]
            private static extern bool init(ulong address, long blockSize, IntPtr ir, long irLen);

            [DllImport("AudioFilter.dll")]
            private static extern void process(ulong address, IntPtr input, IntPtr output, long len);

            [DllImport("AudioFilter.dll")]
            private static extern void reset(ulong address);

            [DllImport("AudioFilter.dll")]
            private static extern void stereo2monaural(IntPtr input, long len, IntPtr outputL, IntPtr outputR);

            [DllImport("AudioFilter.dll")]
            private static extern void monaural2stereo(IntPtr inputL, IntPtr inputR, IntPtr output, long len);

            // Audio Handler

            private readonly int filterInstances;
            private readonly ISampleProvider sourceProvider;
            public WaveFormat WaveFormat
            {
                get
                {
                    return sourceProvider.WaveFormat;
                }
            }
            public CppConvolutionFilter(ISampleProvider sourceProvider, long blockSize, float[] response)
            {
                filterInstances = sourceProvider.WaveFormat.Channels;
                address = new ulong[filterInstances];


                for (int i = 0; i < filterInstances; i++)
                {
                    int bufferInBytes = Marshal.SizeOf(typeof(float)) * response.Length;
                    IntPtr bufferPtr = Marshal.AllocCoTaskMem(bufferInBytes);
                    Marshal.Copy(response, 0, bufferPtr, response.Length);

                    address[i] = createConvolverInstance();
                    init(address[i], blockSize, bufferPtr, response.Length);

                    Marshal.FreeCoTaskMem(bufferPtr);
                }

                this.sourceProvider = sourceProvider;
            }
            public int Read(float[] buffer, int offset, int count)
            {

                if(filterInstances == 1)
                {
                    int samplesRead = sourceProvider.Read(buffer, offset, count);

                    int bufferInBytes = Marshal.SizeOf(typeof(float)) * samplesRead;
                    IntPtr bufferPtr = Marshal.AllocCoTaskMem(bufferInBytes);
                    Marshal.Copy(buffer, 0, bufferPtr, samplesRead);

                    Process(0, bufferPtr, bufferPtr, samplesRead);

                    Marshal.Copy(bufferPtr, buffer, 0, samplesRead);

                    Marshal.FreeCoTaskMem(bufferInBytes);
                    return samplesRead;
                }
                else if(filterInstances == 2)
                {
                    int samplesRead = sourceProvider.Read(buffer, offset, count);

                    int bufferInBytes = Marshal.SizeOf(typeof(float)) * samplesRead;
                    IntPtr bufferPtr = Marshal.AllocCoTaskMem(bufferInBytes);
                    Marshal.Copy(buffer, 0, bufferPtr, samplesRead);

                    int bufferLRInBytes = Marshal.SizeOf(typeof(float)) * samplesRead / 2;
                    IntPtr bufferLPtr = Marshal.AllocCoTaskMem(bufferLRInBytes);
                    IntPtr bufferRPtr = Marshal.AllocCoTaskMem(bufferLRInBytes);

                    CppConvolutionFilter.StereoToMonaural(bufferPtr, samplesRead, bufferLPtr, bufferRPtr);
                    Process(0, bufferLPtr, bufferLPtr, samplesRead / 2);
                    Process(1, bufferRPtr, bufferRPtr, samplesRead / 2);
                    CppConvolutionFilter.MonauralToStereo(bufferLPtr, bufferRPtr, bufferPtr, samplesRead);

                    Marshal.Copy(bufferPtr, buffer, 0, samplesRead);

                    Marshal.FreeCoTaskMem(bufferInBytes);
                    Marshal.FreeCoTaskMem(bufferLPtr);
                    Marshal.FreeCoTaskMem(bufferRPtr);

                    return samplesRead;
                }

                return 0;
            }
            public void Reset()
            {
                for (int i = 0; i < filterInstances; i++)
                {
                    reset(address[i]);
                }
            }
            public void Process(int channel, IntPtr input, IntPtr output, long len)
            {
                process(address[channel], input, output, len);
            }

            public static void StereoToMonaural(IntPtr input, long len, IntPtr outputL, IntPtr outputR)
            {
                stereo2monaural(input, len, outputL, outputR);
            }

            public static void MonauralToStereo(IntPtr inputL, IntPtr inputR, IntPtr output, long len)
            {
                monaural2stereo(inputL, inputR, output, len);
            }
        }

    }
}
