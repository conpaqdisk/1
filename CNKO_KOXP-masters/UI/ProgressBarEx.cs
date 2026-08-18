namespace KOXP.UI
{
    public class ProgressBarEx : ProgressBar
    {
        private SolidBrush? brush = null;

        public ProgressBarEx()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (brush == null || brush.Color != ForeColor)
                brush = new SolidBrush(ForeColor);

            Rectangle rec = new Rectangle(0, 0, Width, Height);

            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);

            rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
            rec.Height = rec.Height - 4;
            e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
        }
    }
}
