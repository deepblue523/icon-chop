namespace IconChop
{
    /// <summary>Named description preset for Auto-name (AI): short name in the UI, long text sent to the model.</summary>
    public class AutoNameAppDescriptionPreset
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";
    }
}
