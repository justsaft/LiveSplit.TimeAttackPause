using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using LiveSplit.Model;
using LiveSplit.TimeAttackPause.DTO;

using Run = LiveSplit.TimeAttackPause.DTO.Run;

namespace LiveSplit.TimeAttackPause.IO;

public static class SplitsStateSerializer
{
    public static void SaveSplitsState(LiveSplitState state, string saveFilePath)
    {
        TimingMethod timingMethod = state.CurrentTimingMethod;

        var splits = state.Run.ToList();

        var splitsDTOs = new List<Split>();
        foreach (ISegment split in splits)
        {
            TimeSpan? time = split.SplitTime[timingMethod];
            var splitDto = new Split()
            {
                Time = time,
                Name = split.Name
            };
            splitsDTOs.Add(splitDto);
        }

        var run = new Run()
        {
            TimingMethod = timingMethod,
            Splits = splitsDTOs,
            CurrentSplitIndex = state.CurrentSplitIndex,
            CurrentSplitTime = state.CurrentSplit.SplitTime[timingMethod],
            CurrentTime = state.CurrentTime[timingMethod],
        };

        // Use xml to serialize the run
        var serializer = new XmlSerializer(typeof(Run));
        string data = "";

        using (var string_writer = new StringWriter())
        {
            var xml_writer = XmlWriter.Create(string_writer);
            serializer.Serialize(xml_writer, run);
            data = string_writer.ToString(); // The run basically
            xml_writer.Dispose();
        }

        // use newtonsoft json to serialize the run object to json
        /* string json = JsonConvert.SerializeObject(run, Newtonsoft.Json.Formatting.Indented); */
        File.WriteAllText(saveFilePath, data);
    }
}
