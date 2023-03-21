using ScottPlot;
using System;
using System.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace AudioAnalyser
{
    public class AudioFile
    {
        public string FileName;

        SoundPlayer song_to_play;

        public int chunkID;
        public int fileSize;
        public int riffType;


        // chunk 1
        public int fmtID;
        public int fmtSize; // bytes for this chunk (expect 16 or 18)

        // 16 bytes coming...
        public int fmtCode;
        public int channels;
        public int sampleRate;
        public int byteRate;
        public int fmtBlockAlign;
        public int bitDepth;

        public int fmtExtraSize;

        public int dataID;
        public int bytes;

        public int bytesForSamp;
        public int nValues;

        public float[] LData;
        public float[] RData;

        public double Length;

        public AudioFile(FileInfo info)
        {
            FileName = info.Name;
            try
            {
                using (FileStream fs = File.Open(info.FullName, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);

                    this.song_to_play = new SoundPlayer(info.FullName);

                    // chunk 0
                     chunkID = reader.ReadInt32();
                     fileSize = reader.ReadInt32();
                     riffType = reader.ReadInt32();


                    // chunk 1
                     fmtID = reader.ReadInt32();
                     fmtSize = reader.ReadInt32(); // bytes for this chunk (expect 16 or 18)

                    // 16 bytes coming...
                     fmtCode = reader.ReadInt16();
                     channels = reader.ReadInt16();
                     sampleRate = reader.ReadInt32();
                     byteRate = reader.ReadInt32();
                     fmtBlockAlign = reader.ReadInt16();
                     bitDepth = reader.ReadInt16();

                    if (fmtSize == 18)
                    {
                        // Read any extra values
                        fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(fmtExtraSize);
                    }

                    // chunk 2
                    dataID = reader.ReadInt32();
                    bytes = reader.ReadInt32();

                    // DATA!
                    byte[] byteArray = reader.ReadBytes(bytes);

                    bytesForSamp = bitDepth / 8;
                    nValues = bytes / bytesForSamp;

                    float[] asFloat = null;
                    switch (bitDepth)
                    {
                        case 64:
                            double[]
                                asDouble = new double[nValues];
                            Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
                            asFloat = Array.ConvertAll(asDouble, e => (float)e);
                            break;
                        case 32:
                            asFloat = new float[nValues];
                            Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
                            break;
                        case 16:
                            Int16[]
                                asInt16 = new Int16[nValues];
                            Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
                            asFloat = Array.ConvertAll(asInt16, e => e / (float)(Int16.MaxValue + 1));
                            break;
                        default:
                            return;
                    }

                    switch (channels)
                    {
                        case 1:
                            LData = asFloat;
                            RData = null;
                            return;
                        case 2:
                            // de-interleave
                            int nSamps = nValues / 2;
                            LData = new float[nSamps];
                            RData = new float[nSamps];
                            for (int s = 0, v = 0; s < nSamps; s++)
                            {
                                LData[s] = asFloat[v++];
                                RData[s] = asFloat[v++];
                            }
                            return;
                        default:
                            return;
                    }
                }
            }
            catch
            {
                throw new Exception();
            }

        }
        public TimeSpan GetLength()
        {
            this.Length = (double)this.bytes / (double)(this.sampleRate * channels * this.bytesForSamp);
            return TimeSpan.FromSeconds(Math.Round(this.Length,0));
        }
        public void DrawPlot(WpfPlot plot)
        {
            plot.Reset();
            if(this.LData!=null)
                plot.Plot.AddSignal(this.LData);
            if(this.RData!=null)
                plot.Plot.AddSignal(this.RData);
            plot.Refresh();
        }
        public void PlaySong()
        {
            this.song_to_play.PlaySync();
        }
        public double GetInterval()
        {
            double val = 1 / (double)this.sampleRate;
            val *= 1e3;
            return val;
        }

    }
}
