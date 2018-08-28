using System;
using System.Windows.Forms;
using SE.MDH.DriftavbrottKlient;

namespace DriftavbrottMonitorTest
{
  public partial class Form1 : Form
  {
    private DriftavbrottMonitor monitor;
    public Form1()
    {
      InitializeComponent();
      monitor = new DriftavbrottMonitor(new [] { "ladok.uppgradering", "ladok.backup" });
      monitor.DriftavbrottStatus += Monitor_DriftavbrottStatus;

    }

    private void Monitor_DriftavbrottStatus(object sender, DriftavbrottStatusEvent args)
    {
      if (args.Status == DriftavbrottStatus.Pågående)
      {
        var action = new Action(() => { pictureBox.Image = Resource.Red;});
        pictureBox.Invoke(action);
        action = new Action(() => { pictureBox.Refresh(); });
        pictureBox.Invoke(action);
      }

      if (args.Status == DriftavbrottStatus.Upphört)
      {
        var action = new Action(() => { pictureBox.Image = Resource.Green;});
        pictureBox.Invoke(action);
        action = new Action(() => { pictureBox.Refresh(); });
        pictureBox.Invoke(action);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      monitor.DriftavbrottStatus -= Monitor_DriftavbrottStatus;
      monitor?.Dispose();
    }
  }
}

