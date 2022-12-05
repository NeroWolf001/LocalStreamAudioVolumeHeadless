using CodeX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace LocalAudioStreamVolumeHeadless
{
    public class LocalAudioStreamVolumeHeadless : NeosMod
    {
        public override string Name => "LocalAudioStreamVolumeHeadless";
        public override string Author => "NeroWolf & LeCloutPanda";
        public override string Version => "1.0.0";

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("Set user audio stream volume to 0 in hosted sessions.", "", () => true);

        public override void OnEngineInit()
        {
            config = base.GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony($"dev.{Author}.{Name}");
            harmony.PatchAll();

            Engine.Current.OnReady += Current_OnReady;
        }
        private void Current_OnReady()
        {
            Engine.Current.WorldManager.WorldAdded += WorldAdded;
            Engine.Current.WorldManager.WorldRemoved += WorldRemoved;
        }

        private void WorldAdded(World obj) => obj.ComponentAdded += OnComponentAdded;
        private void WorldRemoved(World obj) => obj.ComponentAdded -= OnComponentAdded;

        private void OnComponentAdded(Slot arg1, Component arg2)
        {
            if (!config.GetValue(ENABLED)) return;

            if (!arg1.LocalUser.IsHost) return;

            if (arg2.GetType() == typeof(UserAudioStream<StereoSample>))
            {
                    AudioOutput audioOutput = arg1.GetComponent<AudioOutput>();
                    if (audioOutput == null) return;

                    ValueUserOverride<float> userOverride = audioOutput.Volume.OverrideForUser<float>(arg1.World.HostUser, 0);
                    userOverride.CreateOverrideOnWrite.Value = true;
                    userOverride.Default.Value = 0;

                    Slot Handle = arg1.FindChild(ch => ch.Name.Equals("Handle"), 1);
                    if (Handle.FindChild(ch => ch.Name.Equals("Local Text"), 1) != null) return;

                    TextRenderer text = Handle.AddSlot("Local Text").AttachComponent<TextRenderer>();
                    text.Text.Value = "Local Audio";
                    text.Slot.Scale_Field.Value = new BaseX.float3(0.3f, 0.3f, 0.3f);
                    text.Slot.Position_Field.Value = new BaseX.float3(0f, 0f, -0.0075f);
            }
        }
    }
}