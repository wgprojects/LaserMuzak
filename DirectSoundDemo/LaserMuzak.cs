using AudioSynthesis.Midi;
using AudioSynthesis.Synthesis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectSoundDemo
{
    public partial class LaserMuzak : Form
    {
        public LaserMuzak()
        {
            InitializeComponent();
        }


        int currentPlayingTime = -1;
        public void updateTime(int current, int max, int voices)
        {
            currentPlayingTime = current;
            noteViewer1.updateTime(current, max, voices);
        }
        private void LaserMuzak_Load(object sender, EventArgs e)
        {

        }




        private void SaveMuzak2(MidiFile mf, string MuzakFile)
        {
            int ticksPerBeat = 96;
            if (mf.TimingStandard == MidiFile.TimeFormat.FamesPerSecond)
            {
                throw new NotImplementedException();
            }
            else if (mf.TimingStandard == MidiFile.TimeFormat.TicksPerBeat)
            {
                ticksPerBeat = mf.Division;
            }

            using (StreamWriter op = new StreamWriter(MuzakFile))
            {
                Dictionary<int, List<AudioSynthesis.Midi.Event.MidiEvent>> eventByChannel = new Dictionary<int, List<AudioSynthesis.Midi.Event.MidiEvent>>();

                List<string> axes = new List<string>() { ":W", ":Y", ":X" };


                foreach (var track in mf.Tracks)
                {

                    op.WriteLine("######################################################");
                    op.WriteLine("#Reading Track");
                    op.WriteLine(String.Format("# Active channels: {0}", track.ActiveChannels));
                    op.WriteLine(String.Format("# DrumInstruments: {0}", track.DrumInstruments));
                    //op.WriteLine(String.Format("# noteViewer1.endTime: {0}", track.noteViewer1.endTime));
                    op.WriteLine(String.Format("# Instruments: {0}", track.Instruments));
                    op.WriteLine(String.Format("# NoteOnCount: {0}", track.NoteOnCount));


                    foreach (var e in track.MidiEvents)
                    {
                        if (e.Command == (int)MidiEventTypeEnum.PitchBend)
                        {
                            Console.WriteLine(String.Format("#\t\t\t\t{0}: {1} d1:{2} d2:{3}", e.Channel, e.Command, e.Data1, e.Data2));
                        }
                        if (e.Command == (int)MidiEventTypeEnum.NoteOff || e.Command == (int)MidiEventTypeEnum.NoteOn)
                        {
                            if (!eventByChannel.ContainsKey(e.Channel))
                                eventByChannel.Add(e.Channel, new List<AudioSynthesis.Midi.Event.MidiEvent>());

                            eventByChannel[e.Channel].Add(e);
                        }

                    }
                }

                op.WriteLine("######################################################");
                op.WriteLine("#Writing events by channel:");

                foreach (int channel in eventByChannel.Keys)
                {
                    var events = eventByChannel[channel];

                    op.WriteLine("######################################################");
                    op.WriteLine(String.Format("#Channel: {0}", channel));
                    if (axes.Count > 0)
                    {
                        op.WriteLine(axes[0]);
                        axes.RemoveAt(0);
                    }

                    int lastOctave = -1;

                    string currentNote = null;
                    int currentMidiNote = -1;
                    // int duration_ticks = 0;

                    bool silence = true;

                    //int sum = 0;
                    int lastTime = 0;

                    foreach (var e in events)
                    {
                        //Console.WriteLine(String.Format("{0} + {1} = {2} vs {3}", sum, e.DeltaTime, sum + e.DeltaTime, e.AbsTime));
                        //sum += e.DeltaTime;

                        int octave;
                        string note;


                        if (e.Command == 0x90)
                        { //NoteOn

                            GetNote(e.Data1, out note, out octave);

                            //duration_ticks = 0;

                            //op.WriteLine(String.Format("ON: {0} {1} {2}", e.Data1, e.Data2, e.DeltaTime));
                            op.WriteLine(String.Format("\t\t\t#ON: {0} - {1} {2} {3}", note, e.Data1, e.Data2, e.DeltaTime));

                            if (silence)
                            {
                                WriteNote(op, ".", e.AbsTime, ref lastTime, ticksPerBeat);

                                silence = false;
                            }
                            else //We're trying to start a new note without ending the last one.
                            {

                                WriteNote(op, currentNote, e.AbsTime, ref lastTime, ticksPerBeat);
                                op.WriteLine("\t\t#REPEATED NOTE START");
                            }


                            if (octave != lastOctave)
                            {
                                op.WriteLine("o" + octave.ToString());
                                lastOctave = octave;
                            }

                            currentNote = note;
                            currentMidiNote = e.Data1;
                            //duration_ticks = 0;

                        }
                        else if (e.Command == 0x80)
                        {
                            //NoteOff
                            op.WriteLine(String.Format("\t\t\t#OFF: {0} {1} {2}", e.Data1, e.Data2, e.DeltaTime));


                            if (silence || currentMidiNote != e.Data1)
                            { //We previously tryied to start a new note without ending the last one. Skip!
                                //duration_ticks += e.DeltaTime;
                                //Do nothing
                                op.WriteLine("\t\t#DO NOTHING");
                            }
                            else
                            {
                                WriteNote(op, currentNote, e.AbsTime, ref lastTime, ticksPerBeat);
                                //if (duration_ticks > 0)
                                //{
                                //    WriteNote(op, currentNote, duration_ticks, ticksPerBeat);
                                //}
                                //else
                                //{
                                //    op.WriteLine("\t\t#SKIPPED WRITING NOTE WITH ZERO LENGTH! - " + currentNote);
                                //}
                                currentNote = null;
                                currentMidiNote = -1;
                                //duration_ticks = 0;
                                silence = true;
                                //WriteNote(op, currentNote, duration_ticks, ticksPerBeat);

                            }
                        }
                        else
                        {
                            op.WriteLine(String.Format("#\t\t\t\t{0}: {1} d1:{2} d2:{3} dt: {4}", e.Channel, e.Command, e.Data1, e.Data2, e.DeltaTime));
                        }
                    }

                }
            }
        }



        private void WriteNote(StreamWriter op, string currentNote, int absTime, ref int lastTime, int ticksPerBeat)
        {
            int duration = absTime - lastTime;
            if (duration > 0)
            {
                op.WriteLine(String.Format("{0}{1}", currentNote, GetBeats(duration, ticksPerBeat)));
                lastTime = absTime;
            }
            else
            {
                op.WriteLine("\t\t#Skipping empty note: " + currentNote);
            }
        }

        private string GetBeats(int duration_ticks, int ticksPerBeat)
        {
            //double beatsSixteenths = 16 * duration_ticks / ticksPerBeat;
            // int beatsInv = 16 * ticksPerBeat / duration_ticks;
            return String.Format("{0}/{1}", ticksPerBeat, duration_ticks);
        }

        public static void GetNote(int midiFreq, out string note, out int octave)
        {
            string[] notes = new string[] { "c", "c#", "d", "d#", "e", "f", "f#", "g", "g#", "a", "a#", "b" };

            octave = (midiFreq / 12) - 1;

            int noteIndex = midiFreq % 12;
            note = notes[noteIndex];

        }

        public static string GetNoteStr(int midiFreq)
        {
            string note;
            int octave;
            GetNote(midiFreq, out note, out octave);
            return note + octave;

        }


        int nAxes = -1;
        int octaveOffset = 0;
        HashSet<byte> channels = new HashSet<byte>();

        private void btnSaveMuzak_Click(object sender, EventArgs e)
        {
            if(st == null || st.mseq == null)
            {
                MessageBox.Show("Not loaded - aborting");
                return;
            }
            AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
            MidiMessage[] mdata = mseq.mdata;

            bool useW = cbAxisW.Checked;
            bool useY = cbAxisY.Checked;
            bool useX = cbAxisX.Checked;

            nAxes = 0;
            if (useW)
                nAxes++;
            if (useY)
                nAxes++;
            if (useX)
                nAxes++;

            if(nAxes == 0)
            {
                MessageBox.Show("Select at least one axis");
                return;
            }





            if (noteViewer1.ChannelPriorities.Count == 0)
            {
                MessageBox.Show("No channel priorities set - aborting.");
                return;
            }

            int minPrioritiesSet = Math.Min(nAxes, channels.Count);

            if (noteViewer1.ChannelPriorities.Count < minPrioritiesSet)
            {
                if (DialogResult.OK != MessageBox.Show("Not using all axes - fewer channels than axes - okay to continue?", "Confirm", MessageBoxButtons.OKCancel))
                    return;
            }




            if (mseq != null && mseq.mdata != null)
            {

                byte minFreq = byte.MaxValue;
                byte maxFreq = byte.MinValue;
                double time;
                foreach (MidiMessage mm in mdata)
                {
                    time = (double)mm.delta;
                    if (!noteViewer1.ChannelPriorities.ContainsKey(mm.channel))
                        continue;

                    bool startOk = time >= noteViewer1.startTime || noteViewer1.startTime < 0;
                    bool endOk = time <= noteViewer1.endTime || noteViewer1.endTime < 0;
                    if (startOk && endOk)
                    {
                        if (mm.command == (int)MidiEventTypeEnum.NoteOn)
                        {
                            channels.Add(mm.channel);
                            minFreq = Math.Min(minFreq, mm.data1);
                            maxFreq = Math.Max(maxFreq, mm.data1);
                        }
                    }
                }
                channels.Remove(255);

                if(maxFreq < minFreq)
                {
                    MessageBox.Show("No notes played between start and end times on the selected channels. Aborting.");
                    return;
                }
                //Avoid frequencies too high for laser
                string max = GetNoteStr(maxFreq);
                if (DialogResult.No == MessageBox.Show("Highest frequency is " + max + "\r\nIs this okay?"))
                {
                    octaveOffset = -1;
                }

            }



          

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.muzak|*.muzak";
            sfd.InitialDirectory = Properties.Settings.Default.midi_path;
            if(DialogResult.OK == sfd.ShowDialog())
            {
                SaveMuzak(sfd.FileName);


            }
            //SaveMuzak(mf, path);
        }

        private void SaveMuzak(string filename)
        {
            bool useW = cbAxisW.Checked;
            bool useY = cbAxisY.Checked;
            bool useX = cbAxisX.Checked;


            AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
            MidiMessage[] mdata = mseq.mdata;
            
            double time = 0;
            int maxPixel = this.Width;

            Dictionary<int, MidiMessage> last = new Dictionary<int,MidiMessage>();
            Dictionary<int, double> lastTime = new Dictionary<int,double>();

           


            Dictionary<byte, int> priorities = noteViewer1.ChannelPriorities;


            
          

            //<midiFreq, 
            int[] axisPlayingPriority = new int[nAxes];
            Array.Clear(axisPlayingPriority, 0, nAxes);

            SortedDictionary<int, List<MuzakNote>> currentNotes = new SortedDictionary<int, List<MuzakNote>>();


            //List<MuzakNote>[] currentlyPlaying = new List<MuzakNote>[nAxes];
            //for (int i = 0; i < nAxes; i++)
            //    currentlyPlaying[i] = new List<MuzakNote>();

                foreach (MidiMessage mm in mdata)
                {
                    if (mm.channel == 255)
                        continue;

                    time = (double)mm.delta;
                    if (!noteViewer1.ChannelPriorities.ContainsKey(mm.channel))
                        continue;


                    bool startOk = time >= noteViewer1.startTime || noteViewer1.startTime < 0;
                    bool endOk = time <= noteViewer1.endTime || noteViewer1.endTime < 0;
                    if (startOk && endOk)
                    {

                        if (lastTime.Count == 0)
                            foreach (var c in channels)
                                lastTime.Add(c, time);

                        if (!last.ContainsKey(mm.channel))
                        {
                            last.Add(mm.channel, new MidiMessage(mm.channel, 0, 0, 0));
                        }

                        
                       
                        
                        long us = (long)(time - lastTime[mm.channel]);

                        if (mm.command == (int)MidiEventTypeEnum.NoteOff)
                        {
                            MuzakNote toEnd = null;
                            bool multipleNotes = false;
                            foreach(var notes in currentNotes.Values)
                            {
                                foreach(var note in notes)
                                {
                                    if(note.channel == mm.channel && note.midiFreq == mm.data1)
                                    {
                                        toEnd = note;
                                        multipleNotes = notes.Count > 1;
                                    }
                                }
                            }

                            if (toEnd != null)
                            {
                                if (multipleNotes)
                                {
                                    currentNotes[toEnd.priority].Remove(toEnd); //Remove note from list at this priority
                                }
                                else
                                {
                                    currentNotes.Remove(toEnd.priority); //Remove this entire priority
                                }
                            }
                        }
                        else if (mm.command == (int)MidiEventTypeEnum.NoteOn)
                        {
                            int pri = priorities[mm.channel];
                            var toStart = new MuzakNote(mm.data1, pri, mm.channel);
                            
                            if(currentNotes.ContainsKey(pri))
                            {
                                currentNotes[pri].Add(toStart);
                            }
                            else
                            {
                                currentNotes.Add(pri, new List<MuzakNote>() { toStart });
                            }
                        }


                       

                            //Dictionary<byte, MuzakNote> best = new Dictionary<byte, MuzakNote>();
                        //foreach (byte ch in channels)
                        //    if(currentlyPlaying[ch] != null)
                        //    {
                        //        best.Add(ch, currentlyPlaying[ch]);
                        //    }
                        //best.OrderBy(kp => (priorities[kp.Key])).Take(nAxes);


                        if (mm.command == (int)MidiEventTypeEnum.NoteOn || mm.command == (int)MidiEventTypeEnum.NoteOff)
                        {
                            
                            List<List<MuzakNote>> toOutput = new List<List<MuzakNote>>();
                            foreach (int pri in currentNotes.Keys)
                            {
                                toOutput.Add(currentNotes[pri]);

                                Console.Write(String.Join(", ", currentNotes[pri]) + " | ");
                            }
                            Console.WriteLine("");
                            

                        }

                        last[mm.channel] = mm;
                        lastTime[mm.channel] = time;
                    }
                }

            //if (MicrosecPerPixel > 0)
            //{
            //    float timeX = (float)(LeftMargin + ((currTime) - TimeOffset_us) / MicrosecPerPixel);
            //    e.Graphics.DrawLine(Pens.Green, timeX, 0, timeX, this.Height);

            //    if (startTime >= 0)
            //    {
            //        timeX = (float)(LeftMargin + ((startTime) - TimeOffset_us) / MicrosecPerPixel);
            //        e.Graphics.DrawLine(Pens.White, timeX, 0, timeX, this.Height);
            //    }

            //    if (endTime >= 0)
            //    {
            //        timeX = (float)(LeftMargin + ((endTime) - TimeOffset_us) / MicrosecPerPixel);
            //        e.Graphics.DrawLine(Pens.White, timeX, 0, timeX, this.Height);
            //    }
            //}

        
        
        }

        SynthThread st;
        private void btnLoad_Click(object sender, EventArgs e)
        {
            MainForm pform = (MainForm)this.MdiParent;
            st = pform.sthread;
            AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
            mseq.UnMuteAllChannels();
            MidiMessage[] mdata = mseq.mdata;

            noteViewer1.Reset();
            noteViewer1.mseq = mseq;
            noteViewer1.Invalidate();

            double time = 0;
            textBox1.Clear();

            if (mdata == null)
                textBox1.AppendText("Please load a midi file first.");
            else
            {
                foreach (MidiMessage mm in mdata)
                {
                    time += (double)mm.delta;
                    //The current event starts at mm.delta microseconds after the previous event.
                    //textBox1.AppendText(mm.ToString() + " " + mm.AbsTime_ms + "usec\r\n");
                }

            }
        }

        private void nudOffset_ValueChanged(object sender, EventArgs e)
        {
            noteViewer1.TimeOffset_us = (int)nudOffset.Value;
            noteViewer1.Invalidate();
        }

        private void nudMicrosecPerPix_ValueChanged(object sender, EventArgs e)
        {
            ChangeIncrement();

            noteViewer1.MicrosecPerPixel = (int)nudMicrosecPerPix.Value;
            noteViewer1.Invalidate();
        }

        private void noteViewer1_Resize(object sender, EventArgs e)
        {
            ChangeIncrement();
        }

        private void ChangeIncrement()
        {
            double width_us = noteViewer1.Width * noteViewer1.MicrosecPerPixel;
            nudOffset.Increment = (int)(width_us / 4);
        }

        Timer tmrFollow;
        private void cbFollow_CheckedChanged(object sender, EventArgs e)
        {
            if (cbFollow.Checked)
            {
                tmrFollow = new Timer();
                tmrFollow.Tick += tmrFollow_Tick;
                tmrFollow.Interval = 50;
                tmrFollow.Start();
            }
            else
            {
                tmrFollow.Stop();
            }
        }

        void tmrFollow_Tick(object sender, EventArgs e)
        {
            double width_us = noteViewer1.Width * noteViewer1.MicrosecPerPixel;

            if (st != null && st.mseq != null && st.mseq.IsPlaying)
            {

                decimal desired = (decimal)(currentPlayingTime - width_us * 1 / 8);
                if ((double)(desired - nudOffset.Value) > width_us * 3 / 4 || nudOffset.Value > desired)
                {
                    if (desired < 0)
                        desired = 0;
                    if (desired > nudOffset.Maximum)
                        desired = nudOffset.Maximum;

                    nudOffset.Value = desired;
                }

            }
        }

        private void noteViewer1_MouseClick(object sender, MouseEventArgs e)
        {
            //double seek_us = noteViewer1.noteViewer1.startTime_us + (e.X - 30) * noteViewer1.MicrosecPerPixel;

            //if (st != null && st.mseq != null && st.mseq.IsPlaying)
            //{
            //    AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
            //    mseq.Seek(new TimeSpan((long)(seek_us)));
            //}
        
        }

        private void noteViewer1_TimeSeek(long seek_us, int channel)
        {
            if(settingStartTime)
            {
                settingStartTime = false;
                noteViewer1.startTime = seek_us;
                lblStartTime.Text = noteViewer1.startTime.ToString();
            }

            if (settingEndTime)
            {
                settingEndTime = false;
                noteViewer1.endTime = seek_us;
                lblEndTime.Text = noteViewer1.endTime.ToString();
            }
            if (settingPriorities)
            {
                if (channel != -1)
                    noteViewer1.AddPriority((byte)channel);
            }
            UpdateStartEndTimeAndPriorityButtons();
        }

        bool settingStartTime = false;
        private void btnSetStartTime_Click(object sender, EventArgs e)
        {
            if(settingStartTime)
            {
                settingStartTime = false;
                noteViewer1.startTime = -1;
                lblStartTime.Text = "Cancelled";
            }
            else
            {
                settingStartTime = true;
                lblStartTime.Text = "Click above to set";
            }
            UpdateStartEndTimeAndPriorityButtons();
        }


        bool settingEndTime = false;
        private void btnSetEndTime_Click(object sender, EventArgs e)
        {
            if (settingEndTime)
            {
                settingEndTime = false;
                noteViewer1.endTime = -1;
                lblEndTime.Text = "Cancelled";
            }
            else
            {
                settingEndTime = true;
                lblEndTime.Text = "Click above to set";
            }
            UpdateStartEndTimeAndPriorityButtons();
        }

        private void UpdateStartEndTimeAndPriorityButtons()
        {
            btnSetEndTime.Enabled = !settingStartTime && !settingPriorities;
            btnSetStartTime.Enabled = !settingEndTime && !settingPriorities;
            btnSetChannelPriorities.Enabled = !settingStartTime && !settingEndTime;

            btnSaveMuzak.Enabled = !settingStartTime && !settingEndTime && !settingPriorities;
            noteViewer1.Invalidate();
        }

        bool settingPriorities = false;
        private void btnSetChannelPriorities_Click(object sender, EventArgs e)
        {
            if(settingPriorities)
            {

                settingPriorities = false;
                btnSetChannelPriorities.Text = "Set Channel # Priorities";

                AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
                mseq.MuteAllChannels();
                foreach (byte channel in noteViewer1.ChannelPriorities.Keys)
                    mseq.ToggleChannelMuted(channel);

            }
            else
            {
                settingPriorities = true; 
                btnSetChannelPriorities.Text = "Click again when done.";
                noteViewer1.ChannelPriorities = new Dictionary<byte, int>();

            }
            UpdateStartEndTimeAndPriorityButtons();
        }

        
    }


    public class MuzakNote
    {
        public static MuzakNote Delay(long _length, byte _channel)
        {
            MuzakNote n = new MuzakNote(0, _length, 0, _channel);
            return n;
        }
        public MuzakNote(int _midiFreq, long _length, int _pri, byte _channel)
        {
            midiFreq = _midiFreq;
            length = _length;
            priority = _pri;
            channel = _channel;
        }

        public MuzakNote(int _midiFreq, int _pri, byte _channel)
        {
            midiFreq = _midiFreq;
            priority = _pri;
            channel = _channel;
        }

        public int midiFreq;
        public long length; //us
        public int priority;
        public byte channel;

        public override string ToString()
        {
            if(midiFreq == 0)
            {
                return "." + length.ToString();
            }
            else
            {
                return String.Format("freq{0} for {1}us on ch{2} with pri{3}", midiFreq, length, channel, priority);
            }
        }
    }

}
