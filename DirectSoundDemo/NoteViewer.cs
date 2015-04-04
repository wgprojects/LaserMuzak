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

namespace DirectSoundDemo
{
    public partial class NoteViewer : UserControl
    {
        public NoteViewer()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            TimeOffset_us = 0;
            MicrosecPerPixel = (1000 / 1) * 1000; //1px per 1s)

            startTime = -1;
            endTime = -1;
        }


        HashSet<byte> channels = new HashSet<byte>();
        byte minFreq = byte.MaxValue;
        byte maxFreq = byte.MinValue;

        public AudioSynthesis.Sequencer.MidiFileSequencer _mseq;
        public AudioSynthesis.Sequencer.MidiFileSequencer mseq 
        {
            get { return _mseq; }
            set
            {
                _mseq = value;

                channels = new HashSet<byte>();
                if (_mseq != null &&  _mseq.mdata != null)
                {
                    MidiMessage[] mdata = _mseq.mdata;

                    minFreq = byte.MaxValue;
                    maxFreq = byte.MinValue;
                    foreach (MidiMessage mm in mdata)
                    {
                        channels.Add(mm.channel);
                        if (mm.command == (int)MidiEventTypeEnum.NoteOn)
                        {
                            minFreq = Math.Min(minFreq, mm.data1);
                            maxFreq = Math.Max(maxFreq, mm.data1);
                        }
                    }
                    channels.Remove(255);

                }
                if (channels.Count == 0)
                    this.MinimumSize = new Size(this.MinimumSize.Width, 100);
                else
                    this.MinimumSize = new Size(this.MinimumSize.Width, (15 + 1) * (channels.Count + 1));

            }
        }
        public double TimeOffset_us { get; set; }
        public double MicrosecPerPixel { get; set; }
        public long startTime { get; set; }
        public long endTime { get; set; }
        private int LeftMargin = 80;
        private int LeftPriMargin = 40;
        public Dictionary<byte, int> ChannelPriorities = new Dictionary<byte, int>();

        Dictionary<byte, int> RectTop = null;
        double RectHeight = 1;

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);

            if (mseq == null || mseq.mdata == null)
            {
                string message = "No data - Load a midi file, then press Load here in LaserMuzak";
                Font f = new Font("Arial", 12);

                var sz = e.Graphics.MeasureString(message, f);
                e.Graphics.DrawString(message, f, Brushes.Black, new Point((int)(this.Width - sz.Width) / 2, this.Height / 2));
            }
            else
            {
                MidiMessage[] mdata = mseq.mdata;
                double time = 0;
                int maxPixel = this.Width;

                Dictionary<int, MidiMessage> last = new Dictionary<int,MidiMessage>();
                Dictionary<int, double> currentPixel = new Dictionary<int, double>();
                Dictionary<int, double> lastTime = new Dictionary<int,double>();

                


                RectTop = new Dictionary<byte, int>();
                RectHeight = (this.Height - channels.Count) / (double)(channels.Count + 1);
                if (RectHeight < 1)
                    RectHeight = 1;

                int i = 0;
                foreach (byte channel in channels)
                {
                    RectTop[channel] = (int)Math.Round((i + 1) * (RectHeight + 1));
                    e.Graphics.DrawString(channel.ToString(), new Font("Arial", 9), mseq.IsChannelMuted(channel)?Brushes.Red : Brushes.Green, new Point(2, RectTop[channel]));

                    if(ChannelPriorities.ContainsKey(channel))
                    {
                        int pri = ChannelPriorities[channel];
                        e.Graphics.DrawString(pri.ToString(), new Font("Arial", 9), mseq.IsChannelMuted(channel) ? Brushes.Red : Brushes.Green, new Point(LeftPriMargin, RectTop[channel]));


                    }
                    i++;
                }

                e.Graphics.DrawString("Ch# | Pri" , new Font("Arial", 9), Brushes.Black, new Point(2, 2));
                var lashd = e.Graphics.MeasureString("W", new Font("Arial", 9));

                foreach (MidiMessage mm in mdata)
                {
                    if (mm.channel == 255)
                        continue;
                    
                    time = (double)mm.delta;
                    
                    if (time >= TimeOffset_us)
                    {

                        if (lastTime.Count == 0)
                            foreach (var c in channels)
                                lastTime.Add(c, time);

                        if (!last.ContainsKey(mm.channel))
                        {
                            last.Add(mm.channel, new MidiMessage(mm.channel, 0, 0, 0));
                            currentPixel.Add(mm.channel, LeftMargin);
                            //lastTime.Add(mm.channel, time);
                        }



                        float startPixel = (float)((lastTime[mm.channel] - TimeOffset_us) / MicrosecPerPixel) + LeftMargin;
                        double numPixels = (time - lastTime[mm.channel]) / MicrosecPerPixel;

                        if (numPixels >= 1)
                            e.Graphics.FillRectangle(GetBrush(last[mm.channel], minFreq, maxFreq), new RectangleF(startPixel, RectTop[mm.channel], (float)numPixels, (int)RectHeight));
                            //e.Graphics.FillRectangle(GetBrush(last[mm.channel], minFreq, maxFreq), new RectangleF((float)currentPixel[mm.channel], RectTop[mm.channel], (float)numPixels, (int)RectHeight));

                        currentPixel[mm.channel] += numPixels;
                        last[mm.channel] = mm;
                        lastTime[mm.channel] = time;
                    }
                }

                if (MicrosecPerPixel > 0)
                {
                    float timeX = (float)(LeftMargin + ((currTime) - TimeOffset_us) / MicrosecPerPixel);
                    e.Graphics.DrawLine(Pens.Green, timeX, 0, timeX, this.Height);

                    if (startTime >= 0)
                    {
                        timeX = (float)(LeftMargin + ((startTime) - TimeOffset_us) / MicrosecPerPixel);
                        e.Graphics.DrawLine(Pens.White, timeX, 0, timeX, this.Height);
                    }

                    if (endTime >= 0)
                    {
                        timeX = (float)(LeftMargin + ((endTime) - TimeOffset_us) / MicrosecPerPixel);
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



            if (e.X < LeftMargin)
            {
                if (RectTop != null)
                {
                    mseq.ToggleChannelMuted(chClicked);
                    this.Invalidate();
                }
            }
            else
            {
                double seek_us = TimeOffset_us + (e.X - LeftMargin) * MicrosecPerPixel;
                long seek = (long)seek_us;
                OnTimeSeek(seek, chClicked);

                if (mseq != null)
                {
                    mseq.Seek(new TimeSpan((long)(seek_us * 230))); //No idea where the factor of 230 comes from. Empirically determined.
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
