using System.Windows.Forms;

namespace Chummer.Tests.xUnit.AccessChummerState;

/// <summary>
/// Defines a fixture to setup a Single Threaded Apartment (STA) for xUnit tests that would misbehave otherwise
/// </summary>
public class SetupStaThreadFixture: IAsyncLifetime
{
    private readonly Thread _staThread;

    protected SetupStaThreadFixture()
    {
        var resetEvent = new ManualResetEvent(false);
        var staThread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            Program.MainForm = new ChummerMainForm(true, true) {ShowInTaskbar = false};
            Program.MainForm.Show();
            Program.MainForm.Hide();

            resetEvent.Set();
            Application.Run();
        });
        _staThread = staThread;
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();

        resetEvent.WaitOne();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        CloseForm();
        Application.Exit();
        _staThread.Join();
        return Task.CompletedTask;

        static void CloseForm()
        {
            if (Program.MainForm.InvokeRequired)
            {
                Program.MainForm.Invoke(CloseForm);
                return;
            }
            Program.MainForm.Close();
        }
    }
}

