using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AudioFile audio;
        public MainWindow()
        {
            InitializeComponent();
            audio = null;
            lframes = null;
            this.EnableUI(false);
        }
        private void LoadSong(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Wav Files Only (*.wav)|*.wav";
            dialog.Title = "Choose .WAV File";

            if (dialog.ShowDialog() == true)
            {
                FileInfo file_info = new FileInfo(dialog.FileName);
                audio = new AudioFile(file_info);
                SetFileInfo();
                audio.GetLength();
            }
            this.DrawPlot();
            this.EnableUI(true);
        }
        private void SetFileInfo()
        {
            NameLabel.Content = audio.FileName;
            SizeLabel.Content = $"{Math.Round(audio.bytes / 1e6, 2)} MB";
            LengthLabel.Content = audio.GetLength();
            switch (audio.channels)
            {
                case 1:
                    ChanelsLabel.Content = "Mono";
                    break;
                case 2:
                    ChanelsLabel.Content = "Stereo";
                    break;
                default:
                    ChanelsLabel.Content = "Undefined";
                    break;
            }
            SampleLabel.Content = $"{audio.sampleRate} Hz";
            IntervalLabel.Content = $"{Math.Round(audio.GetInterval(),4)} ms";
        }
        private void DrawPlot()
        {
            if (audio != null)
            {
                audio.DrawPlot(WavPlot);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (audio != null)
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var t = new Thread(() => audio.PlaySong());
                    t.Start();
                }));

            else
                MessageBox.Show("Load Song to play!\nPress: File -> Open -> and select audio File", "No Song!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        ListOfFrames lframes;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lframes = new ListOfFrames(this.audio);
            lframes.Generate((int)SliderQframe.Value, (int)SliderOverlap.Value);
            this.SetFrameDetails(lframes);
            this.RefreshData();
        }

        private void SetFrameDetails(ListOfFrames l)
        {
            var t = l.GetDetails();
            FrameLengthLabel.Content = $"{Math.Round(t.Item1,1)} ms";
            ImpulseCountLabel.Content = $"{t.Item2}";
            FramesQCountLabel.Content = $"{l.frames.Count}";
            this.SetAudioClipDetails(l);
        }

        private void FrameLevelComboBox(object sender, SelectionChangedEventArgs e)
        {
            RefreshData();
        }
        private void RefreshData()
        {
            if (audio == null || lframes == null)
                return;
            List<double> Data = null;
            switch (FrameCombo.SelectedIndex)
            {
                case 0:
                    Data = lframes.Volume();
                    break;
                case 1:
                    Data = lframes.STE();
                    break;
                case 2:
                    Data = lframes.ZCR();
                    break;
                case 3:
                    Data = lframes.SR(5,0.1);
                    break;
                case 4:
                    Data = lframes.F0AUTO();
                    break;
                case 5:
                    Data = lframes.F0AMDF();
                    break;
                default:
                    return;
            }
            FrameLevelPlot.Reset();
            FrameLevelPlot.Plot.AddSignal(Data.ToArray());
            FrameLevelPlot.Refresh();
        }
        private void SetAudioClipDetails(ListOfFrames l)
        {
            this.VSTDLabel.Content = $"{Math.Round(l.VSTD(),4)}";
            this.VDRLabel.Content = $"{Math.Round(l.VDR(),4)}";
        }
        private void EnableUI(bool enable)
        {
            this.Play.IsEnabled = enable;
            this.ButtonFramesApply.IsEnabled = enable;
            this.FrameCombo.IsEnabled = enable;
            this.thresholdBox.IsEnabled = enable;
        }
    }

}
