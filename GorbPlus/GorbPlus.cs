namespace GorbPlus;

public class GorbPlus : Mod, IMenuMod, IGlobalSettings<GlobalSettings>
{
    internal static GorbPlus Instance;
    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public PlayMakerFSM GorbFSM = null;

    public override void Initialize()
    {
        Instance ??= this;
        On.PlayMakerFSM.Start += (orig, self) =>
        {
            if (self.gameObject.name == "Ghost Warrior Slug" && self.FsmName == "Distance Attack")
            {
                self.GetState("Close").GetAction<FloatCompare>(3).float2 = 35f;
                self.GetState("Close").ChangeTransition("AWAY", "Away");
                self.GetState("Away").RemoveAction(5);
                var waitr = self.GetState("Away").GetAction<WaitRandom>(6);
                waitr.timeMax = gs.TimeBetweenBarrage;
                waitr.timeMin = gs.TimeBetweenBarrage;
                GorbFSM = self;
            }

            orig(self);
        };
    }

    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
    {
        float start = 0.25f, stop = 5f, step = 0.25f;
        return new List<IMenuMod.MenuEntry>()
        {
            new ()
            {
                Name = "Nail Barrage Speed",
                Description = "How much time between nail barrages",
                Values = Enumerable.Range((int)(start / step), (int)(Math.Abs(stop - start) / step) + 1).Select(x => (x * step).ToString()).ToArray(),
                Saver = i =>
                {
                    gs.TimeBetweenBarrage = ((i + (start / step)) * step);
                    if (GorbFSM != null)
                    {
                        var waitr = GorbFSM.GetState("Away").GetAction<WaitRandom>(6);
                        waitr.timeMax = gs.TimeBetweenBarrage;
                        waitr.timeMin = gs.TimeBetweenBarrage;
                    }
                },
                Loader = () => (int)((gs.TimeBetweenBarrage / step) - (start / step)),
            }
        };
    }

    public bool ToggleButtonInsideMenu => false;

    public static GlobalSettings gs { get; set; } = new GlobalSettings();
    public void OnLoadGlobal(GlobalSettings s) => gs = s;
    public GlobalSettings OnSaveGlobal() => gs;
}