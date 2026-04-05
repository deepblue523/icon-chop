namespace IconChop
{
    /// <summary>PictureBox that paints an optional overlay after the image (e.g. drag preview).</summary>
    internal sealed class ReferenceImagePictureBox : PictureBox
    {
        public Action<Graphics>? PaintOverlay { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            PaintOverlay?.Invoke(e.Graphics);
        }
    }
}
