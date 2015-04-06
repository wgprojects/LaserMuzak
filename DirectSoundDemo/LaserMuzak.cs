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
            cbChordImitation.Items.Clear();
            cbChordImitation.Items.Add(ChordImitationMode.HighestThenLowest);
            cbChordImitation.Items.Add(ChordImitationMode.RoundRobin_5ms);
            cbChordImitation.Items.Add(ChordImitationMode.RoundRobin_20ms);
            cbChordImitation.SelectedIndex = 0;
        }

        enum ChordImitationMode : int
        {
            HighestThenLowest = 0,
            RoundRobin_5ms = 1,
            RoundRobin_20ms = 2

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
        int LASER_MAX_OCTAVE = 6; //Needs experimentation
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

                int octave;
                string note;
                GetNote(maxFreq, out note, out octave);

                
                if (octave >= 6)
                {
                    string max = GetNoteStr(maxFreq);
                    DialogResult dr = MessageBox.Show("Highest frequency is " + max + "\r\nTo cut off higher notes, hit Ignore.\r\nTo shift down octaves, hit Retry.", "Pitch too high", MessageBoxButtons.AbortRetryIgnore);
                    if(dr == DialogResult.Abort)
                        return;
                    else if(dr == DialogResult.Retry)
                        octaveOffset = LASER_MAX_OCTAVE - octave;
                    else if(dr == DialogResult.Ignore)
                        octaveOffset = 0;
                }

            }



          

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.FileName = midiFileName + ".muzak";
            sfd.Filter = "*.muzak|*.muzak";
            sfd.InitialDirectory = Properties.Settings.Default.midi_path;
            if(DialogResult.OK == sfd.ShowDialog())
            {
                SaveMuzak(sfd.FileName);


            }
            //SaveMuzak(mf, path);
        }

        private void WriteMuzak(string filename, Dictionary<int, Tuple<int, List<Tuple<int, int>>>> muzak)
        {
            //muzak = Dictionary<timestamp, Tuple<Duration, List<Tuple<priority, midiFreq>>>>
            //i.e. the song is divided into pieces of time, where each slice has a number of notes that should play.
            //We'll try to play them in priority order, depending on chords and what was playing previously...

            bool useW = cbAxisW.Checked;
            bool useY = cbAxisY.Checked;
            bool useX = cbAxisX.Checked;
            bool noteSplittingDisable = cbNoteSplitting.Checked;
            ChordImitationMode chordImitation = (ChordImitationMode)cbChordImitation.SelectedIndex;
            int NO_SOUND = 0;

            if(muzak.Count == 0)
            {
                MessageBox.Show("No music to write! Aborting.");
                return;
            }

            var first = muzak.First();
            while (first.Value.Item2.Count == 0)
            {
                muzak.Remove(first.Key);
                first = muzak.First();
            }

            int timeOffset = first.Key;

            List<List<Tuple<int, int>>> muzakByAxis = new List<List<Tuple<int, int>>>();
            //Each axis is a List<Tuple<int, int>>, a list of notes
            //Each note is a Tuple<duration, midiFreq>
            for (int axis = 0; axis < nAxes; axis++)
                muzakByAxis.Add(new List<Tuple<int, int>>());

            int lastTimeStamp = 0;
            foreach (var timeSlice in muzak)
            {
                var notes = timeSlice.Value.Item2; //Each note is a Tuple<Priority, MidiFreq>
                var duration = timeSlice.Value.Item1;
                int time = timeSlice.Key - timeOffset;
                //int duration = time - lastTimeStamp;
                
                if (notes.Count <= nAxes)
                {
                    //Easy case
                    var sortedNotes = notes.OrderBy(n => n.Item2).ToList(); //Sort by frequency
                    for(int axis = 0; axis < nAxes; axis++)
                    {
                        if (axis < sortedNotes.Count)
                            muzakByAxis[axis].Add(new Tuple<int, int>(duration, sortedNotes[axis].Item2));
                        else
                            muzakByAxis[axis].Add(new Tuple<int,int>(duration, NO_SOUND));
                    }
                        
                }
                else
                {
                    SortedSet<int> priorities = new SortedSet<int>();
                    foreach (var note in notes)
                        if(!priorities.Contains(note.Item1))
                            priorities.Add(note.Item1);

                    int nPri = priorities.Count;

                    switch (chordImitation)
                    {
                        case ChordImitationMode.HighestThenLowest:
                            ChordImitate_HighLow(notes, duration, muzakByAxis, priorities);
                            break;
                        case ChordImitationMode.RoundRobin_5ms:
                            ChordImitate_RoundRobin(notes, duration, muzakByAxis, priorities, 5);
                            break;
                        case ChordImitationMode.RoundRobin_20ms:
                            ChordImitate_RoundRobin(notes, duration, muzakByAxis, priorities, 20);
                            break;
                        default:
                            MessageBox.Show("Chord detected - please set the Chord Imitation mode. Aborting..");
                            return;
                    }


                    //for (int axis = 0; axis < nAxes; axis++)
                    //{
                    //    muzakByAxis[axis].Add(new Tuple<int, int>(duration, NO_SOUND));
                    //}

                }
                lastTimeStamp = time;
            }
            using (StreamWriter op = new StreamWriter(filename))
            {
                op.WriteLine("t120 #Does this work?");

                int axis = 0;
                if(useW)
                {
                    WriteMuzakAxis(op, ":W", muzakByAxis[axis]);
                    axis++;
                }
                if (useY)
                {
                    WriteMuzakAxis(op, ":Y", muzakByAxis[axis]);
                    axis++;
                }
                if (useX)
                {
                    WriteMuzakAxis(op, ":X", muzakByAxis[axis]);
                    axis++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notes">Each note is a Tuple(priority, midiFreq)</param>
        /// <param name="muzakByAxis"></param>
        private void ChordImitate_HighLow(List<Tuple<int, int>> notes, int duration, List<List<Tuple<int, int>>> muzakByAxis, SortedSet<int> priorities)
        {
            int[] pri = priorities.ToArray();
            int maxPriIdx = Math.Min(pri.Length, nAxes);

            List<Tuple<int, int>> prioritizedNotes = notes.Where(n => priorities.Take(maxPriIdx).Contains(n.Item1)).ToList();

            while(prioritizedNotes.Count() > nAxes)
            {
                foreach(int prio in priorities)
                {
                    var thisPriNotes = prioritizedNotes.Where(n => n.Item1 == prio).ToList();
                    var thisPriNotesByFreq = thisPriNotes.OrderBy(n => n.Item2).ToList();

                    if (thisPriNotesByFreq.Count > 1)
                        prioritizedNotes.Remove(thisPriNotesByFreq[thisPriNotesByFreq.Count / 2]);
                }
            }

            var priNotesByFreq = prioritizedNotes.OrderBy(n => n.Item2).ToList();

            for (int axis = 0; axis < nAxes; axis++)
            {
                muzakByAxis[axis].Add(new Tuple<int, int>(duration, priNotesByFreq[axis].Item2));
            }

        }

        /// <summary>
        /// Falls back on ChordImitate_HighLow if timing is too short for round robin!
        /// </summary>
        /// <param name="notes">Each note is a Tuple(priority, midiFreq)</param>
        /// <param name="muzakByAxis"></param>
        /// <param name="ms">milliseconds, the switching time between rounds</param>
        private void ChordImitate_RoundRobin(List<Tuple<int, int>> notes, int duration, List<List<Tuple<int, int>>> muzakByAxis, SortedSet<int> priorities, int ms)
        {
            int samplesPerRound = ms * 44100 / 1000;
            if(duration < samplesPerRound * 2) //Must be at least two rounds of round robin, or we fall back.
            {
                ChordImitate_HighLow(notes, duration, muzakByAxis, priorities);
                return;
            }

            int[] pri = priorities.ToArray();
            int maxPriIdx = Math.Min(pri.Length, nAxes);

            double[] avgFreq = new double[nAxes];
            int[] axisOut = new int[nAxes];
            List<Tuple<int, int>>[] notesSet = new List<Tuple<int, int>>[nAxes];

            for (int priIdx = 0; priIdx < maxPriIdx; priIdx++)
            {
                var set = notes.Where(n => n.Item1 == pri[priIdx]);
                notesSet[priIdx] = set.ToList();
                avgFreq[priIdx] = set.Average(n =>n.Item2); 
            }

            if(maxPriIdx < nAxes) //We have one chord on one axis - split it up to multiple axis if desired.
            {
                if (notesSet[0].Count > 1)
                {
                    if (avgFreq[0] < 50) //Low notes - split off the highest
                    {
                        Tuple<int, int> highestNote = notesSet[0].OrderByDescending(n => n.Item2).First();
                        notesSet[0].Remove(highestNote);
                        avgFreq[0] = notesSet[0].Average(n => n.Item2);

                        notesSet[maxPriIdx] = new List<Tuple<int, int>>(){highestNote};
                        avgFreq[maxPriIdx] = highestNote.Item2;
                    }
                    else //High notes - split off the lowest
                    {
                        Tuple<int, int> lowestNote = notesSet[0].OrderBy(n => n.Item2).First();
                        foreach (var note in notesSet[0])
                        {
                            if (note.Item2 < lowestNote.Item2)
                                lowestNote = note;
                        }
                        notesSet[0].Remove(lowestNote);
                        avgFreq[0] = notesSet[0].Average(n => n.Item2);

                        notesSet[maxPriIdx] = new List<Tuple<int, int>>(){lowestNote};
                        avgFreq[maxPriIdx] = lowestNote.Item2;
                    }
                    maxPriIdx++;
                }

            }

            int[] dividingFreqs = new int[] { 0, 50, 100};
            for (int priIdx = maxPriIdx; priIdx < nAxes; priIdx++)
            {
                notesSet[priIdx] = new List<Tuple<int, int>>() { new Tuple<int, int>(-1, 0) };
                avgFreq[priIdx] = dividingFreqs[priIdx];
            }

            var sortedAvgFreq = avgFreq.ToList();
            sortedAvgFreq.Sort();

            for (int priIdx = 0; priIdx < nAxes; priIdx++)
            {
                axisOut[priIdx] = sortedAvgFreq.IndexOf(avgFreq[priIdx]); 
            }

            HashSet<int> axesWritten = new HashSet<int>();
            for (int priIdx = 0; priIdx < nAxes; priIdx++)
            {
                int axisIdx = axisOut[priIdx];
                axesWritten.Add(axisIdx);
                List<Tuple<int, int>> muzakForAxis = muzakByAxis[axisIdx]; //Tuple<duration, midiFreq>

                var notesOfChord = notesSet[priIdx];

                int round = 1;
                int noteIdx;
                while(samplesPerRound * round <= duration)
                {
                    noteIdx = (round - 1) % (notesOfChord.Count);
                    muzakForAxis.Add(new Tuple<int,int>(samplesPerRound, notesOfChord[noteIdx].Item2));
                    round++;
                }
                noteIdx = (round - 1) % (notesOfChord.Count);
                round--;
                muzakForAxis.Add(new Tuple<int,int>(duration - samplesPerRound * round, notesOfChord[noteIdx].Item2));

            }


            //for (int axis = 0; axis < nAxes; axis++)
            //{
            //    if(!axesWritten.Contains(axis))
            //        for(int r=0; r<rou)
            //}

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="AxisIdentifier"></param>
        /// <param name="notes">List<Tuple<duration, MidiFreq>></param>
        private void WriteMuzakAxis(StreamWriter op, string AxisIdentifier, List<Tuple<int, int>> notes)
        {
            op.WriteLine("#######################################################");
            op.WriteLine(AxisIdentifier);
            int lastOctave = 4;
            int noteNum = 0;
            foreach(var note in notes)
            {
                int duration = note.Item1;
                int midiFreq = note.Item2;
                noteNum++;

                int octave = lastOctave;
                string noteStr = ".";
                if (midiFreq != 0)
                    GetNote(midiFreq, out noteStr, out octave);

                octave += octaveOffset;

                if (octave >= 1 && octave <= 8)
                {
                    if (octave != lastOctave)
                    {
                        op.WriteLine("o" + octave.ToString());
                        lastOctave = octave;
                    }
                }
                if ((octave >= 1 && octave <= 8) || midiFreq == 0)
                {
                    string str = String.Format("{0}44100/{1:00000} \t\t\t#{3:00000}: {0}{2} (m.f. {5}) {4:0.000} sec", noteStr, duration, midiFreq == 0 ? "" : octave.ToString(), noteNum, duration/44100f, midiFreq);
                    op.WriteLine(str);
                }
            }
        }
        private void SaveMuzak(string filename)
        {
           
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

            Dictionary<int, Tuple<int, List<Tuple<int, int>>>> muzak = new Dictionary<int, Tuple<int, List<Tuple<int, int>>>>();
            //muzak = Dictionary<timestamp, Tuple<Duration, List<Tuple<priority, midiFreq>>>>
            //i.e. the song is divided into pieces of time, where each slice has a number of notes that should play.
            //We'll try to play them in priority order, depending on chords and what was playing previously...

            SortedDictionary<int, List<MuzakNote>> currentNotes = new SortedDictionary<int, List<MuzakNote>>();


            int lastTimeStamp = 0;
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

                    


                    if (mm.command == (int)MidiEventTypeEnum.NoteOn || mm.command == (int)MidiEventTypeEnum.NoteOff)
                    {
                        int currentTimeStamp = mm.delta;
                        int timeDelta = currentTimeStamp - lastTimeStamp;
                        if (timeDelta > 0)
                        {
                            List<Tuple<int,int>> timeSlice = new List<Tuple<int,int>>();
                            muzak.Add(lastTimeStamp, new Tuple<int, List<Tuple<int,int>>>(timeDelta, timeSlice));

                            //muzak = Dictionary<timestamp, List<Tuple<priority, midiFreq>>>
                            //i.e. the song is divided into pieces of time, where each slice has a number of notes that should play.
                            //We'll try to play them in priority order, depending on chords and what was playing previously...


                            foreach (var x in currentNotes)
                            {
                                int pri = x.Key;
                                foreach (var note in x.Value)
                                    timeSlice.Add(new Tuple<int,int>(pri, note.midiFreq));
                            }


                            lastTimeStamp = mm.delta;

                        }
                        //List<List<MuzakNote>> toOutput = new List<List<MuzakNote>>();
                        //foreach (int pri in currentNotes.Keys)
                        //{
                        //    toOutput.Add(currentNotes[pri]);

                        //    Console.Write(String.Join(", ", currentNotes[pri]) + " | ");
                        //}
                        //Console.WriteLine("");
                            

                    }

                    if (mm.command == (int)MidiEventTypeEnum.NoteOn)
                    {
                        int pri = priorities[mm.channel];
                        var toStart = new MuzakNote(mm.data1, pri, mm.channel);

                        if (currentNotes.ContainsKey(pri))
                        {
                            currentNotes[pri].Add(toStart);
                        }
                        else
                        {
                            currentNotes.Add(pri, new List<MuzakNote>() { toStart });
                        }
                    }
                    else if (mm.command == (int)MidiEventTypeEnum.NoteOff)
                    {
                        MuzakNote toEnd = null;
                        bool multipleNotes = false;
                        foreach (var notes in currentNotes.Values)
                        {
                            foreach (var note in notes)
                            {
                                if (note.channel == mm.channel && note.midiFreq == mm.data1)
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

                    last[mm.channel] = mm;
                    lastTime[mm.channel] = time;
                }
            }

       
             WriteMuzak(filename, muzak);
        
        
        }

        SynthThread st;
        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadMidi("test");
        }

        string midiFileName;
        int sampleRate = -1;
        public void LoadMidi(string fileName)
        {
            midiFileName = fileName;

            MainForm pform = (MainForm)this.MdiParent;
            st = pform.sthread;
            AudioSynthesis.Sequencer.MidiFileSequencer mseq = st.mseq;
            mseq.UnMuteAllChannels();
            MidiMessage[] mdata = mseq.mdata;

            nudOffset.Value = 0;
            noteViewer1.Reset();
            noteViewer1.mseq = mseq;
            noteViewer1.Invalidate();

            //double time = 0;

            //if (mdata != null)
            //{
            //    foreach (MidiMessage mm in mdata)
            //    {
            //        time += (double)mm.delta;
            //        //The current event starts at mm.delta microseconds after the previous event.
            //        //textBox1.AppendText(mm.ToString() + " " + mm.AbsTime_ms + "usec\r\n");
            //    }

            //}
        }

        private void nudOffset_ValueChanged(object sender, EventArgs e)
        {
            noteViewer1.TimeOffset_samples = (int)nudOffset.Value;
            noteViewer1.Invalidate();
        }

        private void nudMicrosecPerPix_ValueChanged(object sender, EventArgs e)
        {
            ChangeIncrement();

            noteViewer1.SamplesPerPixel = (int)nudMicrosecPerPix.Value;
            noteViewer1.Invalidate();
        }

        private void noteViewer1_Resize(object sender, EventArgs e)
        {
            ChangeIncrement();
        }

        private void ChangeIncrement()
        {
            double width_us = noteViewer1.Width * noteViewer1.SamplesPerPixel;
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
            double width_us = noteViewer1.Width * noteViewer1.SamplesPerPixel;

            if (st != null && st.mseq != null && st.mseq.IsPlaying)
            {

                if (currentPlayingTime >= 0)
                {
                    double max = (double)(nudOffset.Value) + width_us * 7 / 8;
                    if (currentPlayingTime > max)
                    {
                        nudOffset.Value = (decimal)((double)(currentPlayingTime) - width_us * 1 / 8);
                    }
                    else if (currentPlayingTime < nudOffset.Value)
                    {
                        nudOffset.Value = currentPlayingTime;
                    }
                }
                //decimal desired = (decimal)(currentPlayingTime - width_us * 1 / 8);
                //if ((double)(desired - nudOffset.Value) > width_us * 3 / 4 || nudOffset.Value > desired)
                //{
                //    if (desired < 0)
                //        desired = 0;
                //    if (desired > nudOffset.Maximum)
                //        desired = nudOffset.Maximum;

                //    nudOffset.Value = desired;
                //}

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
