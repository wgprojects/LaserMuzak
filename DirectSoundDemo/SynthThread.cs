using AudioSynthesis.Synthesis;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Midi;
using NAudio.Wave;
using System.IO;
using AudioSynthesis.Bank;
using AudioSynthesis.Bank.Patches;
using System;
using System.Collections.Generic;

namespace DirectSoundDemo
{
    public sealed class SynthThread : IDisposable
    {
        private Synthesizer synth;
        private MidiFileSequencer mseq;
        private DirectSoundOut direct_out;
        private SynthWaveProvider synth_provider;

        public SynthThread()
        {
            synth = new Synthesizer(Properties.Settings.Default.SampleRate, 2, Properties.Settings.Default.BufferSize, Properties.Settings.Default.BufferCount, Properties.Settings.Default.poly);
            mseq = new MidiFileSequencer(synth);
            synth_provider = new SynthWaveProvider(synth, mseq);
            direct_out = new DirectSoundOut(Properties.Settings.Default.Latency);
            direct_out.Init(synth_provider);
        }
        public PatchBank Bank
        {
            get { return synth.SoundBank; }
        }
        public SynthWaveProvider Provider
        {
            get { return synth_provider; }
        }
        public SynthWaveProvider.PlayerState State
        {
            get { return synth_provider.state; }
        }
        public Synthesizer Synth
        {
            get { return synth; }
        }
        public void LoadBank(string bankfile)
        {
            Stop();
            synth.LoadBank(new MyFile(bankfile));
        }
        public bool SongLoaded()
        {
            return mseq.IsMidiLoaded;
        }
        public bool BankLoaded()
        {
            return synth.SoundBank != null;
        }
        public bool SequencerStarted()
        {
            return mseq.IsPlaying;
        }
        public void UnloadSong()
        {
            if (!mseq.IsPlaying)
                mseq.UnloadMidi();
        }
        public void LoadSong(string fileName)
        {
            
            if (!mseq.IsPlaying)
            {
                var mf = new MidiFile(new MyFile(fileName));
                mseq.LoadMidi(mf);
                string path = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)+".muzak");
                
                SaveMuzak(mf, path);
            }
                


            
        }

        private void SaveMuzak(MidiFile mf, string MuzakFile)
        {
            int ticksPerBeat = 96;
            if(mf.TimingStandard == MidiFile.TimeFormat.FamesPerSecond)
            {
                throw new NotImplementedException();
            }
            else if (mf.TimingStandard == MidiFile.TimeFormat.TicksPerBeat)
            {
                ticksPerBeat = mf.Division;
            }



            
            //notes start on 1/4 beat
            //duration?
            
            using(StreamWriter op = new StreamWriter(MuzakFile))
            {
                Dictionary<int, List<AudioSynthesis.Midi.Event.MidiEvent>> eventByChannel = new Dictionary<int, List<AudioSynthesis.Midi.Event.MidiEvent>>();

                List<string> axes = new List<string>(){":W", ":Y", ":X"};


                foreach(var track in mf.Tracks)
                {
                   
                    op.WriteLine("######################################################");
                    op.WriteLine("#Reading Track");
                    op.WriteLine(String.Format("# Active channels: {0}", track.ActiveChannels));
                    op.WriteLine(String.Format("# DrumInstruments: {0}", track.DrumInstruments));
                    op.WriteLine(String.Format("# EndTime: {0}", track.EndTime));
                    op.WriteLine(String.Format("# Instruments: {0}", track.Instruments));
                    op.WriteLine(String.Format("# NoteOnCount: {0}", track.NoteOnCount));


                    foreach(var e in track.MidiEvents)
                    {
                        if( e.Command == (int)MidiEventTypeEnum.PitchBend)
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

                foreach(int channel in eventByChannel.Keys)
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

                    foreach(var e in events)
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
        public bool PlaySong(string fileName)
        {
            if (synth.SoundBank == null)
                return false;
            Stop();
            mseq.LoadMidi(new MidiFile(new MyFile(fileName)));
            mseq.Play();
            Play();
            return true;
        }
        public void Play()
        {
            if (synth.SoundBank == null)
                return;
            if (mseq.IsMidiLoaded && !mseq.IsPlaying)
                mseq.Play();
            if (synth_provider.state != SynthWaveProvider.PlayerState.Playing)
            {
                synth_provider.state = SynthWaveProvider.PlayerState.Playing;
                if(direct_out.PlaybackState != PlaybackState.Playing)
                    direct_out.Play();
            }
        }
        public void Stop()
        {
            if (synth_provider.state != SynthWaveProvider.PlayerState.Stopped)
            {
                lock (synth_provider.lockObj)
                {
                    mseq.Stop();
                    synth.NoteOffAll(true);
                    synth.ResetSynthControls();
                    synth.ResetPrograms();
                    synth_provider.state = SynthWaveProvider.PlayerState.Stopped;
                    synth_provider.Reset();
                }
            }
        }
        public void TogglePause()
        {
            if (synth_provider.state == SynthWaveProvider.PlayerState.Playing)
            {
                synth_provider.state = SynthWaveProvider.PlayerState.Paused;
            }
            else if (synth_provider.state == SynthWaveProvider.PlayerState.Paused)
            {
                synth_provider.state = SynthWaveProvider.PlayerState.Playing;
            }
        }
        public void AddMessage(SynthWaveProvider.Message msg)
        {
            lock (synth_provider.lockObj)
            {
                synth_provider.msgQueue.Enqueue(msg);
            }
        }
        public bool isMuted(int channel)
        {
            return mseq.IsChannelMuted(channel);
        }
        public bool isHoldDown(int channel)
        {
            return synth.GetChannelHoldPedalStatus(channel);
        }
        public string getProgramName(int channel)
        {
            return synth.GetProgramName(channel);
        }
        public string[] getProgramNames(int bankNumber)
        {
            string[] names = new string[PatchBank.BankSize];
            for (int x = 0; x < names.Length; x++)
            {
                Patch p = synth.SoundBank.GetPatch(bankNumber, x);
                if (p == null)
                    names[x] = "Null";
                else
                    names[x] = p.Name;
            }
            return names;
        }
        public void Close()
        {
            Stop();
            direct_out.Stop();
            direct_out.Dispose();
            synth.UnloadBank();
            mseq.UnloadMidi();
        }
        public void Dispose()
        {
            Close();
        }
    }
}
