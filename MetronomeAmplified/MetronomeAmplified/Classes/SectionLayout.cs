using System;
using Xamarin.Forms;

namespace MetronomeAmplified.Classes
{
    public delegate void SectionMeasuredHandler();

    public class SectionLayout : AbsoluteLayout
    {
        private bool sizeValid = false;
        private double width, height;

        public bool SizeIsValid { get { return sizeValid; } }
        public double LayoutWidth { get { return width; } }
        public double LayoutHeight { get { return height; } }

        public Section section;
        
        protected override void OnSizeAllocated(double widthConstraint, double heightConstraint)
        {
            base.OnSizeAllocated(widthConstraint, heightConstraint);
            if (widthConstraint == width && heightConstraint == height) return;
            width = widthConstraint;
            height = heightConstraint;
            sizeValid = true;
            MeasurePerformed?.Invoke();
        }

        public event SectionMeasuredHandler MeasurePerformed;

    }
}
