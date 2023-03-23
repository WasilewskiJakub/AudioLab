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
        public double F0AUTO(AudioFile a,int l)
        {
            double sum = 0;
            for (int i = imin; i <= imax - l; i++)
            {
                sum += (double)a.LData[i] * a.LData[i + l];
                //sum.Item2 += (double)Math.Abs(a.LData[i + l] - a.LData[i]);
            }
            return sum;   
        }
        public double F0AMDF(AudioFile a, int l)
        {
            double sum = 0;
            for (int i = imin; i <= imax - l; i++)
            {
                //sum.Item1 += (double)a.LData[i] * a.LData[i + l];
                sum += (double)Math.Abs(a.LData[i + l] - a.LData[i]);
            }
            return sum;
        }
        public double AVGAmplitude(AudioFile a)
        {
            double summ = 0;
            for (int i = imin; i <= imax; i++)
            {
                summ += a.LData[i];
            }
            return summ / (imax - imin + 1);
        }
        public double VarianceAmplitude(AudioFile a)
        {
            double summ = 0;
            double M = this.AVGAmplitude(a);
            for (int i = imin; i <= imax; i++)
            {
                summ += Math.Pow(a.LData[i]-M,2);
            }
            return summ / (imax - imin + 1);
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
        public List<double> SR(double threshold, double zcrRatio)
        {
            double[] ret = new double[frames.Count];
            for(int i = 0; i <frames.Count;i++)
            {
                double frameRMS = frames[i].Volume(audio);
                double frameZCR = frames[i].ZCR(audio);
                if (frameRMS < threshold && frameZCR < zcrRatio)
                {
                    ret[i] = 1;
                }
            }
            return ret.ToList();
        }
        public List<double> F0AUTO()
        {
            double[,] tab = new double[this.GetDetails().Item2, frames.Count];
            for(int l = 0; l <this.GetDetails().Item2;l++)
            {
                for(int j=0; j<frames.Count; j++ )
                {
                    tab[l, j] = frames[j].F0AUTO(audio, l);
                }
            }
            List<double> ret = new List<double>();
            for(int i = 0; i < this.GetDetails().Item2;i++)
            {
                double sum = 0 ;
                for(int j = 0; j <frames.Count;j++)
                {
                    sum += tab[i, j];
                }
                ret.Add(sum);
            }
            return ret;
        }

        public List<double> F0AMDF()
        {
            double[,] tab = new double[this.GetDetails().Item2, frames.Count];
            for (int l = 0; l < this.GetDetails().Item2; l++)
            {
                for (int j = 0; j < frames.Count; j++)
                {
                    tab[l, j] = frames[j].F0AMDF(audio, l);
                }
            }
            List<double> ret = new List<double>();
            for (int i = 0; i < this.GetDetails().Item2; i++)
            {
                double sum = 0;
                for (int j = 0; j < frames.Count; j++)
                {
                    sum += tab[i, j];
                }
                ret.Add(sum);
            }
            return ret;
        }

        public List<double> AVGAmplitude()
        {
            List<double> ret = new List<double>();
            foreach (var f in frames)
                ret.Add(f.AVGAmplitude(this.audio));
            return ret;
        }
        public double VDR()
        {
            (double MinValue, double MaxValue) tmp = (double.MaxValue, double.MinValue);
            foreach(var f in frames)
            {
                double val = f.Volume(this.audio);
                tmp.MinValue = val < tmp.MinValue ? val : tmp.MinValue;
                tmp.MaxValue = val > tmp.MaxValue? val : tmp.MaxValue;
            }
            return (tmp.MaxValue - tmp.MinValue) / tmp.MaxValue;
        }
        public double VSTD()
        {
            var list = this.Volume();
            double avg = 0;
            foreach (var v in list)
                avg += v;
            avg /= list.Count;
            double sum = 0; 
            foreach(var v in list)
            {
                sum += Math.Pow(v - avg, 2);
            }
            sum/= list.Count;
            return Math.Sqrt(sum);
        }
    }
}
