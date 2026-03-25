using Common.Enums;
using Common.Events;
using Common.Interfaces;
using Common.Models;

namespace test2;

public class Class2 : AsyncModule
{
    public readonly Guid guid = Guid.NewGuid();
    public override ModuleIdentity Identity => new("для второго теста", guid, "1.0f1");

    public override void Render(IUiBuilder ui)
    {
        Bus.Publish(new ProgressEvent(Identity, 0, $"Загружено {0}%"));
        ui.AddAction("Test", async () =>
        {
            Bus.Publish(new LogEvent("Start Heavy Task", ELog.Basic));

            bool isStarted = await Context.Execute(ETaskWeight.Heavy, async () =>
            {
                for (int i = 0; i <= 100; i += 1)
                {
                    await Task.Delay(500);
                    Bus.Publish(new ProgressEvent(Identity, i, $"Загружено {i}%"));
                }
            });

            if (isStarted)
            {
                Bus.Publish(new LogEvent("Heavy Task Succesfuly", ELog.Basic));
                ui.AddInfo("Heavy Task Succesfuly");
            }
        });
    }

    protected override async Task<bool> OnInitAsync()
    {
        return true;
    }
}