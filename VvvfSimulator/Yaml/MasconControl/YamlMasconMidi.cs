using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NextMidi.Data;
using NextMidi.Data.Score;
using NextMidi.Data.Track;
using NextMidi.DataElement;
using NextMidi.Filing.Midi;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze.YamlMasconData;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconMidi.NoteEventSimple;

namespace VvvfSimulator.Yaml.MasconControl
{
    public class YamlMasconMidi
    {
        public class NoteEventSimple
        {
            public NoteEventSimpleData On = new();
            public NoteEventSimpleData Off = new();
            public class NoteEventSimpleData
            {
                public Note_Event_Type type;
                public double time;
                public int note;
            }

            public enum Note_Event_Type
            {
                ON, OFF,
            }
        }

        public static YamlMasconData? Convert(GUI.Mascon.LoadMidi.MidiLoadData loadData)
        {
            //MIDIDataを変換
            MidiData midiData;
            try
            {
                midiData = MidiReader.ReadFrom(loadData.path);
            }
            catch
            {
                MessageBox.Show("This MIDI cannot be converted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            List<NoteEventSimple> converted_Constructs = GetTimeLine(midiData, loadData.track);
            YamlMasconData mascon_Data = new();

            double total_time = 0;

            for(int j = 0; j < loadData.priority; j++)
            {
                double pre_event_time = 0;
                List<int> selected_Data = [];
                for (int i = 0; i < converted_Constructs.Count; i++)
                {
                    NoteEventSimple data = converted_Constructs[i];

                    if (data.On.time < pre_event_time) continue;

                    double initial_wait = data.On.time - pre_event_time;
                    double play_wait = data.Off.time - data.On.time;

                    pre_event_time = data.Off.time;
                    selected_Data.Add(i);

                    if (loadData.priority != j + 1) continue;

                    // set initial
                    mascon_Data.points.Add(new YamlMasconDataPoint() { rate = 0, duration = -1, brake = false, mascon_on = true, order = 4 * i });
                    // wait initial
                    mascon_Data.points.Add(new YamlMasconDataPoint() { rate = 0, duration = initial_wait, brake = false, mascon_on = true, order = 4 * i + 1 });
                    // set play
                    double frequency = 440 * Math.Pow(2, (data.On.note - 69) / 12.0);
                    mascon_Data.points.Add(new YamlMasconDataPoint() { rate = frequency, duration = -1, brake = false, mascon_on = true, order = 4 * i + 2 });
                    // wait play
                    mascon_Data.points.Add(new YamlMasconDataPoint() { rate = 0, duration = play_wait, brake = false, mascon_on = true, order = 4 * i + 3 });

                    total_time += play_wait;
                    total_time += initial_wait;
                }

                for(int i = 0; i < selected_Data.Count; i++)
                {
                    converted_Constructs.RemoveAt(selected_Data[selected_Data.Count - i - 1]);
                }
            }

            return mascon_Data;
        }

        public static List<NoteEventSimple> GetTimeLine(MidiData midiData,int track_num)
        {
            List<NoteEventSimple> events = [];
            TempoMap sc = new(midiData);
            MidiTrack track = midiData.Tracks[track_num];

            foreach (var note in track.GetData<NoteEvent>())
            {
                NoteEventSimpleData Note_ON_Data = new()
                {
                    time = 0.001 * sc.ToMilliSeconds(note.Tick),
                    type = Note_Event_Type.ON,
                    note = note.Note
                };

                NoteEventSimpleData Note_OFF_Data = new()
                {
                    time = 0.001 * sc.ToMilliSeconds(note.Tick + note.Gate),
                    type = Note_Event_Type.OFF,
                    note = note.Note
                };

                NoteEventSimple note_event = new()
                {
                    On = Note_ON_Data,
                    Off = Note_OFF_Data
                };

                events.Add(note_event);
            }

            events.Sort((a, b) => Math.Sign(a.On.time - b.On.time));
            return events;
        }

    }
}
