using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace AudioAnalyser
{
    internal class Frame
    {
        public int imin;
        public int imax;

        public Frame(int imin, int imax)
        {
            this.imin = imin;
            this.imax = imax;
        }
        public void SetOverlap(int overlaping)
        {
            int len = imax - imin + 1;
            imin -= len * overlaping/100;
            imax += len * overlaping / 100;
        }
        public double Volume(AudioFile a)
        {
            double summ = 0;
            for(int i = imin;i<=imax;i++)
            {
                summ += Math.Pow(a.LData[i], 2);
            }
            return summ / (imax - imin + 1);
        }
        public double STE(AudioFile a)
        {
            return Math.Pow(Volume(a), 2);
        }
        public double ZCR(AudioFile a)
        {
            double summ = 0;
            for (int i = imin+1; i <= imax; i++)
            {
                summ += Math.Abs(Math.Sign(a.LData[i]) - Math.Sign(a.LData[i-1]));
            }
            return summ / 2 * (imax - imin + 1);
        }
        public double SR(AudioFile a)
        {
            return Math.Pow(Volume(a), 2);
        }
    }

    internal class ListOfFrames
    {
        public AudioFile audio;
        public List<Frame> frames;
        public ListOfFrames(AudioFile audio)
        {
            this.frames = new List<Frame>();
            this.audio = audio;
        }
        public void Generate(int Length, int overlap)
        {
            for(int i = 0; i <audio.LData.Length/Length - 1;i++)
            {
                frames.Add(new Frame(i * Length, (i + 1) * Length - 1));
            }
            if (audio.LData.Length % Length != 0)
                frames.Add(new Frame(frames[frames.Count - 1].imax, audio.LData.Length - 1));
            for (int i = 1; i < frames.Count - 1; i++)
                frames[i].SetOverlap(overlap);
        }
        public (double,int) GetDetails()
        {
            int x = frames.Count / 2;
            return ((frames[x].imax - frames[x].imin + 1)*audio.GetInterval(), frames[x].imax - frames[x].imin + 1);
        }

        public List<double> Volume()
        {
            List<double> ret = new List<double>();
            foreach (var f in frames)
                ret.Add(f.Volume(this.audio));
            return ret;
        }
        public List<double> STE()
        {
            List<double> ret = new List<double>();
            foreach (var f in frames)
                ret.Add(f.STE(this.audio));
            return ret;
        }
        public List<double> ZCR()
        {
            List<double> ret = new List<double>();
            foreach (var f in frames)
                ret.Add(f.ZCR(this.audio));
            return ret;
        }
        public List<double> SR()
        {
            List<double> ret = new List<double>();
            foreach (var f in frames)
                ret.Add(f.SR(this.audio));
            return ret;
        }
    }
}
