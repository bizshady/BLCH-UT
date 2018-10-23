using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;

namespace Nerva.Toolkit.Controls
{
    public class FixedWidthPlotPanel : GenericPlotPanel<int>
    {
        public FixedWidthPlotPanel(DataSet<int>[] datasets) : base (datasets) { }

        public override void AddDataPoint(int dataset, int k, float v)
        {
            base.AddDataPoint(dataset, k, v);

            while (datasets[dataset].Data.Peek().K > datasets[dataset].Resolution)
                datasets[dataset].Data.Dequeue();

            GetYValues(datasets[dataset]);
        }

        protected override float CalculateXIncrement(float w, int k, DataSet<int> ds) => w * ((float)k / (float)ds.Resolution);
    }

    public class TimeWidthPlotPanel : GenericPlotPanel<DateTime>
    {
        public TimeWidthPlotPanel(DataSet<DateTime>[] datasets) : base (datasets) { }

        public override void AddDataPoint(int dataset, DateTime k, float v)
        {
            base.AddDataPoint(dataset, k, v);

            double ts = 0;

            while ((ts = (DateTime.Now - datasets[dataset].Data.Peek().K).TotalSeconds) > datasets[dataset].Resolution * 60)
                datasets[dataset].Data.Dequeue();

            GetYValues(datasets[dataset]);
        }

        protected override float CalculateXIncrement(float w, DateTime k, DataSet<DateTime> ds) => (w * (float)(DateTime.Now - k).TotalMinutes / ds.Resolution);
    }

    public class DataSet<T>
    {
        public float MinY, MaxY;
        public float Resolution = 1;
        public Color LineColor = Color.FromArgb(255, 255, 255);
        public Queue<DataPoint<T>> Data = new Queue<DataPoint<T>>(60);
    }

    public class DataPoint<T>
    {
        public T K;
        public float V;
    }

    public abstract class GenericPlotPanel<T> : Drawable
    {
        private float fh = float.NaN;
        protected DataSet<T>[] datasets;

        public virtual void AddDataPoint(int dataset, T k, float v)
        {
            var d = datasets[dataset].Data;
            var r = datasets[dataset].Resolution;

            d.Enqueue(new DataPoint<T>
            {
                K = k,
                V = v
            });
        }

        public GenericPlotPanel(DataSet<T>[] datasets)
        {
            this.datasets = datasets;
        }

        protected void GetYValues(DataSet<T> d)
        {
            d.MinY = float.MaxValue;
            d.MaxY = float.MinValue;
            foreach (var value in d.Data)
            {
                if (value.V > d.MaxY)
                    d.MaxY = value.V;

                if (value.V < d.MinY)
                    d.MinY = value.V;
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

        protected abstract float CalculateXIncrement(float w, T k, DataSet<T> ds);

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
                    foreach (var value in dataset.Data)
                    {
                        PointF point = new PointF(
                            x0 + w - CalculateXIncrement(w, value.K, dataset),
                            y0 + h - h * (value.V - minY) / deltaY);

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

    public class FloatDataSet
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
        protected FloatDataSet[] datasets;

        public virtual void AddDataPoint(int dataset, float v)
        {
            var d = datasets[dataset].Data;
            var r = datasets[dataset].Resolution;

            d.Insert(0, v);

            while(d.Count > r + 1)
                d.RemoveAt(d.Count - 1);

            GetYValues(datasets[dataset]);
        }

        public PlotPanel(FloatDataSet[] datasets)
        {
            this.datasets = datasets;
        }

        protected void GetYValues(FloatDataSet d)
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