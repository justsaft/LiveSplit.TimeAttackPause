#nullable enable
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using LiveSplit.Model;

using Run = LiveSplit.TimeAttackPause.DTO.Run;

/* using Newtonsoft.Json; */

namespace LiveSplit.TimeAttackPause.IO;

public static class SplitsStateDeserializer
{
    public static void ImportState(string filepath, LiveSplitState state, ITimerModel timerModel)
    {
        // read all the text from the file as string
        string data = File.ReadAllText(filepath);
        var serializer = new XmlSerializer(typeof(Run));
        Run? runDto;

        /* if (data.IsNullOrEmpty())
        {
            throw new Exception("Imported file was empty or null.");
        } */

        /* var string_reader = new StringReader(data); */
        using (var stream_reader = new StreamReader(data))
        {
            var xml_reader = XmlReader.Create(stream_reader);
            /* try
            { runDto = (Run)xml_reader.ReadContentAsObject(); }
            catch (FormatException)
            {  }
            catch (InvalidCastException)
            {  }
            catch (InvalidOperationException)
            {  } */

            runDto = (Run)serializer.Deserialize(xml_reader);

            xml_reader.Dispose();

            if (runDto == null)
            {
                /* throw new Exception("Couldn't deserialize run (runDto was null)"); */
                return;
            }
        }

        /* JsonConvert.DeserializeObject<Run>(data); */

        timerModel.Start();

        int i = 0;
        foreach (ISegment segment in state.Run) // ISegment is a split
        {
            segment.SplitTime = new Time(runDto.TimingMethod, runDto.Splits[i].Time);
            i += 1;
        }

        state.CurrentSplitIndex = runDto.CurrentSplitIndex;
        state.AdjustedStartTime = TimeStamp.Now - runDto.CurrentTime.GetValueOrDefault(TimeSpan.Zero);
        state.IsGameTimeInitialized = true;
        timerModel.Pause();
    }
}
