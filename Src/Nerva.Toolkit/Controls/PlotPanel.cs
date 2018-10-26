using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Helpers;

namespace Nerva.Toolkit.Controls
{
    public class DataSet
    {
        public float MinY = float.MaxValue, MaxY = float.MinValue;
        public float Resolution = 60;
        public Color LineColor = Color.FromArgb(255, 255, 255);
        public List<float> Data = new List<float>();

        public bool VariableRange = false;
    }

    public class PlotPanel : Drawable
    {
        private float fh = float.NaN;
        protected DataSet[] datasets;

        public virtual void AddDataPoint(int dataset, float v)
        {
            var d = datasets[dataset].Data;
            var r = datasets[dataset].Resolution;

            d.Insert(0, v);

            while(d.Count > r + 1)
                d.RemoveAt(d.Count - 1);

            GetYValues(datasets[dataset]);
            
            //Mac does not repaint the UI at regular intervals like Linux and Windows
            //So we need to force a repaint to update the chart by invalidating the control
            //TODO: We need a way to track what tab is open and only force a repaint if the chart tab is active
            if (OS.IsMac())
                this.Invalidate();
        }

        public PlotPanel(DataSet[] datasets)
        {
            this.datasets = datasets;
        }

        protected void GetYValues(DataSet d)
        {
            if (d.VariableRange)
            {
                d.MinY = float.MaxValue;
                d.MaxY = float.MinValue;
            }
            
            foreach (var value in d.Data)
            {
                if (value > d.MaxY)
                    d.MaxY = value;

                if (value < d.MinY)
                    d.MinY = value;
            }
        }

        private void GetYValues(out float minY, out float maxY)
        {
            minY = float.MaxValue;
            maxY = float.MinValue;
            foreach (var d in datasets)
            {
                if (d.MaxY > maxY)
                    maxY = d.MaxY;

                if (d.MinY < minY)
                    minY = d.MinY;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
		{
            var g = e.Graphics;
            g.AntiAlias = true;

            if (float.IsNaN(fh))
                fh = g.MeasureString(SystemFonts.Default(), "____").Height;

            Rectangle r = new Rectangle(this.Size);

            float x0 = r.X;
            float w = r.Width;

            float y0 = r.Y + 5;
            float h = r.Height - 10;

            if (w > 0 && h > 0)
            {
                float minY, maxY, deltaY;
                GetYValues(out minY, out maxY);
                deltaY = maxY - minY;
                var now = DateTime.Now;

                foreach (var dataset in datasets)
                {
                    PointF last = new PointF();
                    bool first = true;
                    for (int i = 0; i < dataset.Data.Count; i++)
                    {
                        float value = dataset.Data[i];

                        PointF point = new PointF(
                            x0 + w - w * ((float)i / (float)dataset.Resolution),
                            y0 + h - h * (value - minY) / deltaY);

                        if (!first)
                            g.DrawLine(dataset.LineColor, last, point);

                        last = point;
                        first = false;
                    }
                }

                g.DrawText(SystemFonts.Default(), Brushes.Black, 0, y0 + h - fh, minY.ToString());
                g.DrawText(SystemFonts.Default(), Brushes.Black, 0, y0, maxY.ToString());
            }
			
			base.OnPaint(e);
		}
    }
}