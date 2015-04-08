using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudioSynthesis.Synthesis;
using AudioSynthesis.Midi;
using System.Diagnostics;

namespace DirectSoundDemo
{
    public partial class NoteViewer : UserControl
    {
        public NoteViewer()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            TimeOffset_samples = 0;
            SamplesPerPixel = (1000 / 1) * 1000; //1px per 1s)

            startTime = -1;
            endTime = -1;
        }


        HashSet<byte> channels = new HashSet<byte>();
        Dictionary<byte, int> chordCount = new Dictionary<byte, int>(); //How many simultaneous notes played, by channel
        byte minFreq = byte.MaxValue;
        byte maxFreq = byte.MinValue;

        public AudioSynthesis.Sequencer.MidiFileSequencer _mseq;
        public AudioSynthesis.Sequencer.MidiFileSequencer mseq 
        {
            get { return _mseq; }
            set
            {
                _mseq = value;

                Dictionary<byte, int> currentChordCount = new Dictionary<byte, int>();
                chordCount = new Dictionary<byte, int>();

                channels = new HashSet<byte>();
                if (_mseq != null &&  _mseq.mdata != null)
                {
                    MidiMessage[] mdata = _mseq.mdata;

                    minFreq = byte.MaxValue;
                    maxFreq = byte.MinValue;
                    foreach (MidiMessage mm in mdata)
                    {
                        if (mm.command == (int)MidiEventTypeEnum.NoteOn)
                        {
                            channels.Add(mm.channel);
                            minFreq = Math.Min(minFreq, mm.data1);
                            maxFreq = Math.Max(maxFreq, mm.data1);

                            if(currentChordCount.ContainsKey(mm.channel))
                            {
                                currentChordCount[mm.channel]++;
                            }
                            else
                            {
                                currentChordCount.Add(mm.channel, 1);
                            }
                        }
                        else if (mm.command == (int)MidiEventTypeEnum.NoteOff)
                        {
                            if(currentChordCount.ContainsKey(mm.channel))
                            {
                                
                                if (!chordCount.ContainsKey(mm.channel) || currentChordCount[mm.channel] > chordCount[mm.channel])
                                {
                                    chordCount.Remove(mm.channel);
                                    chordCount.Add(mm.channel, currentChordCount[mm.channel]);
                                }
                                currentChordCount[mm.channel]--;
                            }
                        }
                    }
                    channels.Remove(255);
                    if (maxFreq == minFreq)
                        maxFreq += 1;

                }
                //if (channels.Count == 0)
                //    this.MinimumSize = new Size(this.MinimumSize.Width, 100);
                //else
                //    this.MinimumSize = new Size(this.MinimumSize.Width, (15 + 1) * (channels.Count + 1));

            }
        }
        public double TimeOffset_samples { get; set; }
        public double SamplesPerPixel { get; set; }
        public long startTime { get; set; }
        public long endTime { get; set; }
        private int LeftMargin = 100;
        private int LeftPriMargin = 50;
        public Dictionary<byte, int> ChannelPriorities = new Dictionary<byte, int>();

        Dictionary<byte, int> RectTop = null;
        double RectHeight = 15;

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);

            if (mseq == null || mseq.mdata == null)
            {
                string message = "No data - Load a midi file to view notes";
                Font f = new Font("Arial", 12);

                var sz = e.Graphics.MeasureString(message, f);
                e.Graphics.DrawString(message, f, Brushes.Black, new Point((int)(this.Width - sz.Width) / 2, this.Height / 2));
            }
            else
            {
                MidiMessage[] mdata = mseq.mdata;
                double time = 0;
                int maxPixel = this.Width;

                //Dictionary<int, List<Tuple<double, MidiMessage>>> last = new Dictionary<int, List<Tuple<double, MidiMessage>>>();
                Dictionary<int, List<Nullable<MidiMessage>>> last = new Dictionary<int, List<MidiMessage?>>();
                //Dictionary<int, double> currentPixel = new Dictionary<int, double>();
                //Dictionary<int, List<double>> lastTime = new Dictionary<int,List<double>>();

                


                RectTop = new Dictionary<byte, int>();
                RectHeight = (this.Height - channels.Count) / (double)(channels.Count + 1);
                if (RectHeight < 15)
                    RectHeight = 15;


                Font fontSmall = new System.Drawing.Font("Arial", 9);
                Font fontLarge = new System.Drawing.Font("Arial", 9);
                for (int fSz = 6; fSz < 24; fSz++)
                {
                    Font newFont = new System.Drawing.Font("Arial", fSz);
                    SizeF sz = e.Graphics.MeasureString("MMM", newFont);
                    if (sz.Height < RectHeight / 2 && sz.Width < (LeftPriMargin) && sz.Width < (LeftMargin - LeftPriMargin))
                        fontLarge = newFont;
                }

                var lashd = e.Graphics.MeasureString("M", fontSmall);

                int i = 0;
                foreach (byte channel in channels)
                {
                    RectTop[channel] = (int)Math.Round((i + 1) * (RectHeight + 1));
                    e.Graphics.DrawString(channel.ToString(), fontLarge, mseq.IsChannelMuted(channel) ? Brushes.Red : Brushes.Green, new Point(2, RectTop[channel] + (int)((RectHeight - lashd.Height) / 2)));

                    if(ChannelPriorities.ContainsKey(channel))
                    {
                        int pri = ChannelPriorities[channel];
                        e.Graphics.DrawString(pri.ToString(), fontLarge, mseq.IsChannelMuted(channel) ? Brushes.Red : Brushes.Green, new Point(LeftPriMargin, RectTop[channel] + (int)((RectHeight - lashd.Height) / 2)));


                    }
                    i++;
                }
                
                e.Graphics.DrawString("Ch#" , fontSmall, Brushes.Black, new Point(2, 2));
                e.Graphics.DrawString("Pri", fontSmall, Brushes.Black, new Point(LeftPriMargin, 2));
                

                foreach (MidiMessage mm in mdata)
                {
                    if (mm.channel == 255)
                        continue;
                    
                    time = (double)mm.delta;
                    
                    bool drawStart = time >= TimeOffset_samples - this.Width * SamplesPerPixel;
                    bool drawEnd = time >= TimeOffset_samples + 2 * this.Width * SamplesPerPixel;

                    if (drawEnd)
                        break;
                    if (drawStart)
                    {

                        if (last.ContainsKey(mm.channel))
                        {
                            var thisChannel = last[mm.channel];
                            var previous = thisChannel.FirstOrDefault(pmm => pmm.Value.data1 == mm.data1);
                            if (previous == null || previous.Value.data1 == 0)
                            {
                                previous = null;
                            }
                            

                            if (previous != null)
                            {
                                MidiMessage prev = previous.Value;


                                float startPixel = (float)((prev.delta - TimeOffset_samples) / SamplesPerPixel) + LeftMargin;
                                double numPixels = (time - prev.delta) / SamplesPerPixel;

                                float height = (float)(RectHeight / (chordCount[mm.channel]));
                                int index = thisChannel.Count - 1;
                                float yOffset = RectTop[mm.channel] + index * height;

                                
                                RectangleF noteRect = new RectangleF(startPixel, yOffset, (float)numPixels, height);

                                //if (thisChannel.Count > 1)
                                //{
                                //    Trace.WriteLine(String.Format("Note {0} drawn in rectangle {1} with idx {2}.", mm, noteRect, index));
                                //}

                                if (noteRect.Right > LeftMargin)
                                {
                                    if (noteRect.Left < LeftMargin)
                                        noteRect = new RectangleF(LeftMargin, noteRect.Top, noteRect.Right - LeftMargin, noteRect.Height);

                                    if (numPixels >= 1)
                                        e.Graphics.FillRectangle(GetBrush(prev, minFreq, maxFreq), noteRect);
                                }

                                if (mm.command == (int)MidiEventTypeEnum.NoteOff)
                                {
                                    
                                    string noteText = LaserMuzak.GetNoteStr(mm.data1);
                                    var sz = e.Graphics.MeasureString(noteText, fontSmall);
                                    int noteTextOffset = 1;
                                    if (numPixels > sz.Width + 2 * noteTextOffset && height >  sz.Height + 2 * noteTextOffset)
                                    {
                                        PointF loc = new PointF(startPixel + noteTextOffset, (float)yOffset + (height - sz.Height) / 2);
                                        if(loc.X >= LeftMargin)
                                            e.Graphics.DrawString(noteText, fontSmall, Brushes.Black, loc);
                                    }

                                    last[mm.channel].Remove(prev);
                                }
                            }
                        }

                        //while(last.Count > 1)
                        //    last[mm.channel].RemoveAt(0);
                        
                        if (mm.command == (int)MidiEventTypeEnum.NoteOn)
                        {
                            if (last.ContainsKey(mm.channel))
                                last[mm.channel].Add(mm);
                            else
                                last.Add(mm.channel, new List<MidiMessage?>(){mm});
                        }
                          
                        //currentPixel[mm.channel] += numPixels;
                        //lastTime[mm.channel] = time;
                    }
                }

                foreach (var Y in RectTop.Values)
                    e.Graphics.DrawLine(Pens.Black, 0, Y, this.Width, Y);

                e.Graphics.DrawLine(Pens.Black, LeftMargin, 0, LeftMargin, this.Height);
                e.Graphics.DrawLine(Pens.Black, LeftPriMargin, 0, LeftPriMargin, this.Height);

                MidiMessage endMM = mdata[mdata.Length - 1];
                double lastTime = (double)endMM.delta;
                foreach(var ch in last.Keys)
                {
                    var thisChannel = last[ch];
                    foreach (var previous in thisChannel)
                    {
                        var prev = previous.Value;

                        float startPixel = (float)((prev.delta - TimeOffset_samples) / SamplesPerPixel) + LeftMargin;
                        double numPixels = (time - prev.delta) / SamplesPerPixel;

                        float height = (float)(RectHeight / (thisChannel.Count));
                        int index = thisChannel.IndexOf(previous);
                        float yOffset = RectTop[(byte)ch] + index * height;

                        if (numPixels >= 1)
                            e.Graphics.FillRectangle(GetBrush(prev, minFreq, maxFreq), new RectangleF(startPixel, yOffset, (float)numPixels, height));
                    }

                }



                if (SamplesPerPixel > 0)
                {
                    float timeX = (float)(LeftMargin + ((currTime) - TimeOffset_samples) / SamplesPerPixel);
                    e.Graphics.DrawLine(Pens.Green, timeX, 0, timeX, this.Height);

                    if (startTime >= 0)
                    {
                        timeX = (float)(LeftMargin + ((startTime) - TimeOffset_samples) / SamplesPerPixel);
                        e.Graphics.DrawLine(Pens.White, timeX, 0, timeX, this.Height);
                    }

                    if (endTime >= 0)
                    {
                        timeX = (float)(LeftMargin + ((endTime) - TimeOffset_samples) / SamplesPerPixel);
                        e.Graphics.DrawLine(Pens.White, timeX, 0, timeX, this.Height);
                    }
                }

                //e.Graphics.DrawString((currTime * 20).ToString(), new Font("Arial", LeftMargin), Brushes.White, new Point(10, 2 * this.Height / 4));
                //e.Graphics.DrawString(mouse_us.ToString(), new Font("Arial", LeftMargin), Brushes.White, new Point(10, 3 * this.Height / 4));
            }
        }


        public void Reset()
        {
            BrushCache = new Dictionary<byte, Brush>();
            ChannelPriorities = new Dictionary<byte, int>();
        }
        static Dictionary<byte, Brush> BrushCache = new Dictionary<byte, Brush>();
        Brush GetBrush(MidiMessage m, byte minFreq, byte maxFreq)
        {

            if (m.command == (int)MidiEventTypeEnum.NoteOff)
                return Brushes.Gray;
            else if (m.command == (int)MidiEventTypeEnum.NoteOn)
            {
                if (mseq.IsChannelMuted(m.channel))
                    return Brushes.LightGray;

                if (!BrushCache.ContainsKey(m.data1))
                {
                    float frac = (m.data1 - minFreq) / ((float)(maxFreq - minFreq) * 1.2f);
                    //Color A = Color.Red;
                    //Color B = Color.Blue;

                    //Color c = MixColours(A, B, frac);
                    Color c = getColorFromHue(frac);
                    BrushCache.Add(m.data1, new SolidBrush(c));
                }
                return BrushCache[m.data1];
            }
            else
                return Brushes.Black;

        }

        private Color getColorFromHue(float a)
        {
            return HSL2RGB(a, 0.5, 0.5);
        }

        private Color MixColours(Color A, Color B, float a)
        {
            float b = 1-a;
            int R = (int)(A.R * a + B.R * b);
            int G = (int)(A.G * a + B.G * b);
            int Bl = (int)(A.B * a + B.B * b);
            return Color.FromArgb(R, G, Bl);
        }

        #region Colour from HSL
        // Given H,S,L in range of 0-1

        // Returns a Color (RGB struct) in range of 0-255

        public static Color HSL2RGB(double h, double sl, double l)
        {

            double v;

            double r, g, b;



            r = l;   // default to gray

            g = l;

            b = l;

            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);

            if (v > 0)
            {

                double m;

                double sv;

                int sextant;

                double fract, vsf, mid1, mid2;



                m = l + l - v;

                sv = (v - m) / v;

                h *= 6.0;

                sextant = (int)h;

                fract = h - sextant;

                vsf = v * sv * fract;

                mid1 = m + vsf;

                mid2 = v - vsf;

                switch (sextant)
                {

                    case 0:

                        r = v;

                        g = mid1;

                        b = m;

                        break;

                    case 1:

                        r = mid2;

                        g = v;

                        b = m;

                        break;

                    case 2:

                        r = m;

                        g = v;

                        b = mid1;

                        break;

                    case 3:

                        r = m;

                        g = mid2;

                        b = v;

                        break;

                    case 4:

                        r = mid1;

                        g = m;

                        b = v;

                        break;

                    case 5:

                        r = v;

                        g = m;

                        b = mid2;

                        break;

                }

            }

            Color rgb = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));

            return rgb;

        }
#endregion
        int currTime = -1;
        internal void updateTime(int current, int max, int voices)
        {
            currTime = current;
            this.Invalidate();
        }

        double mouse_us;
        private void NoteViewer_MouseMove(object sender, MouseEventArgs e)
        {
            //mouse_us = (e.X - LeftMargin) * MicrosecPerPixel + TimeOffset_us;
            //this.Invalidate();
        }

        public delegate void TimeSeekHandler(long seek_us, int channel);
        public event TimeSeekHandler TimeSeek;
        private void OnTimeSeek(long seek_us, int channel)
        {
            if (TimeSeek != null)
                TimeSeek(seek_us, channel);
        }

        private void NoteViewer_MouseClick(object sender, MouseEventArgs e)
        {
            int chClicked = 255;
            foreach (byte channel in RectTop.Keys)
            {
                if (e.Y < RectTop[channel] + RectHeight)
                {
                    chClicked = channel;
                    break;
                }
            }



            if (e.X < LeftPriMargin)
            {
                if (RectTop != null)
                {
                    mseq.ToggleChannelMuted(chClicked);
                    this.Invalidate();
                }
            }
            else
            {
                double seek_samp = TimeOffset_samples + (e.X - LeftMargin) * SamplesPerPixel;
                long seek = (long)seek_samp;
                OnTimeSeek(seek, chClicked);

                if (seek_samp > 0)
                {
                    if (mseq != null)
                    {
                        mseq.Seek(new TimeSpan((long)(seek_samp * 230))); //No idea where the factor of 230 comes from. Empirically determined.
                    }
                }
            }

        }

        private void NoteViewer_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        internal void AddPriority(byte channel)
        {
            if (ChannelPriorities.ContainsKey(channel))
            {
                System.Media.SystemSounds.Beep.Play();
            }
            else
            {
                int highestPri = 0;
                foreach (int pri in ChannelPriorities.Values)
                    if (pri > highestPri)
                        highestPri = pri;


                ChannelPriorities.Add(channel, highestPri + 1);
            }
        }
    }
}
