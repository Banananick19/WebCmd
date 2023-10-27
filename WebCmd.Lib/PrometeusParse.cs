namespace WebCmd.Lib;

public static class PrometeusParse
{
    public static List<Metric> Parse(string mertricsRaw)
    {
        var mertrics = new List<Metric>();
        var lines = mertricsRaw.Split("\n");
        var currentMetric = new Metric();
        foreach (var line in lines)
        {
            if (line.StartsWith("#"))
            {
                if (!string.IsNullOrWhiteSpace(currentMetric.Value))
                {
                    currentMetric.Value = currentMetric.Value.Trim();
                    mertrics.Add(currentMetric);
                    currentMetric = new Metric();
                }
                var metricDescription = ParseMetricLine(line);
                switch (metricDescription.Tag)
                {
                    case MetricTag.TYPE:
                        currentMetric.Type = metricDescription;
                        break;
                    case MetricTag.HELP:
                        currentMetric.Help = metricDescription;
                        break;
                }

                currentMetric.Title = metricDescription.Title;
            }
            else
            {
                currentMetric.Value += line + "\n";
            }
        }

        return mertrics;
    }

    private static MetricDescription ParseMetricLine(string line)
    {
        var mD = new MetricDescription();
        var splited = line.Split(" ");
        var successTagParse = Enum.TryParse<MetricTag>(splited[1], out var tag);
        if (successTagParse) mD.Tag = tag;
        mD.Title = splited[2];
        mD.Description = string.Join(" ", splited[3..]).Trim();
        return mD;
    }
}

public class MetricDescription
{
    public MetricDescription(MetricTag tag, string title, string description)
    {
        Tag = tag;
        Title = title;
        Description = description;
    }

    public MetricDescription()
    {
    }

    public MetricTag Tag { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public enum MetricTag
{
    TYPE, HELP
}

public class Metric
{
    public Metric(MetricDescription help, MetricDescription type, string title, string value)
    {
        Help = help;
        Type = type;
        Title = title;
        Value = value;
    }

    public Metric()
    {
    }

    public MetricDescription Help { get; set; }
    public MetricDescription Type { get; set; }
    public string Title { get; set; }
    public string Value { get; set; }
}