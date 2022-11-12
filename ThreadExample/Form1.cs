using System.Diagnostics;

namespace ThreadExample;
public partial class Form1 : Form
{
    private Task[] _tasks;
    private List<DataModel> _numberList = new();
    private List<ResultModel> _resultList = new();

    public Form1()
    {
        Control.CheckForIllegalCrossThreadCalls = false;
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        _numberList = Enumerable.Range(1, 10000).Select(s => new DataModel { Number = s }).ToList();
        dgvData.DataSource = _numberList;
    }

    private void btnSetThreadCount_Click(object sender, EventArgs e)
    {
        var threadCount = Convert.ToInt16(txtThreadCount.Text);
        _tasks = new Task[threadCount];
        AddLog("Thread Count Set To " + threadCount.ToString());
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        lstLog.Items.Clear();
        _resultList.Clear();
        dgvResult.DataSource = null;
        var timer = new Stopwatch();
        timer.Start();
        AddLog("App Started");

        var data = _numberList.Select((x, i) => new { Index = i, Value = x })
        .GroupBy(x => x.Index % _tasks.Count())
        .Select(x => x.Select(v => v.Value).ToList())
        .ToList();

        for (int i = 0; i < _tasks.Count(); i++)
        {
            var index = i;
            _tasks[index] = new Task(async () => await Process(data.ToArray()[index].ToList()));
        }

        foreach (var task in _tasks)
        {
            task.Start();
            AddLog("Data Assigned To:" + task.Id.ToString());
        }

        await Task.WhenAll(_tasks);
        timer.Stop();
        AddLog("Process Completed. Elapsed Time:" + timer.ElapsedMilliseconds + " | Result Count : " + _resultList.Count);
        timer.Reset();
        dgvResult.DataSource = _resultList.ToList();
    }

    private void AddLog(string log)
    {
        lblStatus.Text = log;
        lstLog.Items.Add(DateTime.Now.TimeOfDay + " - " + log);
        int visibleItems = lstLog.ClientSize.Height / lstLog.ItemHeight;
        lstLog.TopIndex = Math.Max(lstLog.Items.Count - visibleItems + 1, 0);
    }

    private Task Process(List<DataModel> data)
    {
        AddLog("Process Is Starting");
        foreach (var item in data)
        {
            AddLog(item.Number + " Is Processing...");
            lock (_resultList)
            {
                _resultList.Add(new ResultModel { Number = item.Number });
            }
            //Task.Delay(200).GetAwaiter().GetResult();
        }
        AddLog("Process Finished");
        return Task.CompletedTask;
    }
}
