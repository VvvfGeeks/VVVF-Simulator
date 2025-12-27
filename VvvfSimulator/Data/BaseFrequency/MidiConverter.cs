using NextMidi.Data;
using NextMidi.Data.Score;
using NextMidi.Data.Track;
using NextMidi.DataElement;
using NextMidi.Filing.Midi;
using System;
using System.Collections.Generic;
using static VvvfSimulator.Data.BaseFrequency.Struct;
using static VvvfSimulator.Data.BaseFrequency.MidiConverter.NoteEventSimple;

namespace VvvfSimulator.Data.BaseFrequency
{
    public class MidiConverter
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

        public static Struct? Convert(GUI.BaseFrequency.LoadMidi.MidiLoadData loadData)
        {
            //MIDIDataを変換
            MidiData midiData = MidiReader.ReadFrom(loadData.path);

            List<NoteEventSimple> converted_Constructs = GetTimeLine(midiData, loadData.track);
            Struct Data = new();

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
                    Data.Points.Add(new Point() { Rate = 0, Duration = -1, Brake = false, PowerOn = true, Order = 4 * i });
                    // wait initial
                    Data.Points.Add(new Point() { Rate = 0, Duration = initial_wait, Brake = false, PowerOn = true, Order = 4 * i + 1 });
                    // set play
                    double frequency = 440 * Math.Pow(2, (data.On.note - 69) / 12.0);
                    Data.Points.Add(new Point() { Rate = frequency, Duration = -1, Brake = false, PowerOn = true, Order = 4 * i + 2 });
                    // wait play
                    Data.Points.Add(new Point() { Rate = 0, Duration = play_wait, Brake = false, PowerOn = true, Order = 4 * i + 3 });

                    total_time += play_wait;
                    total_time += initial_wait;
                }

                for(int i = 0; i < selected_Data.Count; i++)
                {
                    converted_Constructs.RemoveAt(selected_Data[selected_Data.Count - i - 1]);
                }
            }

            return Data;
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
