using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using LiveSplit.Model;
using LiveSplit.TimeAttackPause.IO;
using LiveSplit.TimeAttackPause.UI.Components;

namespace LiveSplit.UI.Components;

public class TimeAttackPauseComponent : ControlComponent
{
    private TimeAttackPauseSettings Settings { get; set; }

    private ITimerModel Model { get; set; }

    // This object contains all of the current information about the splits, the timer, etc.
    private LiveSplitState CurrentState { get; set; }

    public override string ComponentName => "TimeAttackPause";

    public override float HorizontalWidth => 0;
    public override float MinimumWidth => 0;
    public override float VerticalHeight => 0;
    public override float MinimumHeight => 0;

    // This function is called when LiveSplit creates your component. This happens when the
    // component is added to the layout, or when LiveSplit opens a layout with this component
    // already added.
    public TimeAttackPauseComponent(LiveSplitState state) : this(state, CreateFormControl())
    {
        ContextMenuControls = new Dictionary<string, Action>
        {
            { "Export current run", SaveRun },
            { "Import run", LoadRun }
        };
    }

    private static Control CreateFormControl()
    {
        // not used anymore
        return new MenuStrip
        {
            Size = new Size(0, 0),
            Location = new Point(0, 0),
        };
    }

    private TimeAttackPauseComponent(LiveSplitState state, Control formControl) : base(state, formControl,
        ex => ErrorCallback(state.Form, ex))
    {
        Settings = new TimeAttackPauseSettings();
        Model = new TimerModel()
        {
            CurrentState = state
        };

        CurrentState = state;
    }

    private void SaveRun()
    {
        if (CurrentState.CurrentPhase == TimerPhase.Running)
        {
            Model.Pause();
        }

        // Displays a SaveFileDialog so the user can save the Run as json file
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Save Your Run"
        };
        saveFileDialog.ShowDialog();

        if (saveFileDialog.FileName == "")
        {
            return;
        }

        SplitsStateSerializer.SaveSplitsState(CurrentState, saveFileDialog.FileName);
        saveFileDialog.Dispose();
    }

    private void LoadRun()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Open Your Run"
        };
        openFileDialog.ShowDialog();

        if (openFileDialog.FileName == "")
        {
            return;
        }

        if (CurrentState.CurrentPhase != TimerPhase.NotRunning)
        {
            Model.Reset();
        }

        SplitsStateDeserializer.ImportState(openFileDialog.FileName, CurrentState, Model);
        openFileDialog.Dispose();
    }

    private static void ErrorCallback(Form form, Exception exception)
    {
        string requiredBits = Environment.Is64BitProcess ? "64" : "32";
        MessageBox.Show(form, "Error appeared: " + exception.Message, "TimeAttackPause Component Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public override Control GetSettingsControl(LayoutMode mode)
    {
        Settings.Mode = mode;
        return Settings;
    }

    public override XmlNode GetSettings(XmlDocument document)
    {
        return Settings.GetSettings(document);
    }

    public override void SetSettings(XmlNode settings)
    {
        Settings.SetSettings(settings);
    }

    // This is the function where we decide what needs to be displayed at this moment in time,
    // and tell the internal component to display it. This function is called hundreds to
    // thousands of times per second.
    public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
    {
        CurrentState = state;
    }

    // I do not know what this is for.
    public int GetSettingsHashCode()
    {
        return Settings.GetSettingsHashCode();
    }
}
