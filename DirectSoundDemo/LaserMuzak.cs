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


        int time = -1;
        public void updateTime(int current, int max, int voices)
        {
            time = current;
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

                    int sum = 0;
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

        void GetNote(int midiFreq, out string note, out int octave)
        {
            string[] notes = new string[] { "c", "c#", "d", "d#", "e", "f", "f#", "g", "g#", "a", "a#", "b" };

            octave = (midiFreq / 12) - 1;

            int noteIndex = midiFreq % 12;
            note = notes[noteIndex];

        }

        private void btnSaveMuzak_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.muzak";
            sfd.InitialDirectory = Properties.Settings.Default.midi_path;
            if(DialogResult.OK == sfd.ShowDialog())
            {
                SaveMuzak(sfd.FileName);


            }
            //SaveMuzak(mf, path);
        }

        private void SaveMuzak(string filename)
        {
            
        }

        SynthThread st;
        private void btnLoad_Click(object sender, EventArgs e)
        {
            MainForm pform = (MainForm)this.MdiParent;
            st = pform.sthread;
            AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
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

                decimal desired = (decimal)(time - width_us * 2 / 8);
                if ((double)(desired - nudOffset.Value) > width_us / 2 || nudOffset.Value > desired)
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
}
