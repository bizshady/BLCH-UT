using System;
using Eto.Drawing;
using Eto.Forms;
using Nerva.Toolkit.Controls;

namespace Nerva.Toolkit.Content
{
    public class ChartPage
	{
        private StackLayout mainControl;
        public StackLayout MainControl => mainControl;

        private PlotPanel hrPlot = new PlotPanel(
            new DataSet[] {
                new DataSet {
                    LineColor = Color.FromArgb(0, 255, 0),
                    Resolution = 60
                },
                new DataSet {
                    LineColor = Color.FromArgb(0, 0, 255),
                    Resolution = 60
                }
            }
        );

        public PlotPanel HrPlot => hrPlot;

        public void ConstructLayout()
        {
            mainControl = new StackLayout
            {
                Padding = new Padding(0, 10),
                Spacing = 10,
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items = 
                {
                    new StackLayoutItem(hrPlot, true)
                }
            };
        }
    }
}