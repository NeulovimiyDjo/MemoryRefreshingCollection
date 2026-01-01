using System;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Resources;

namespace MyTracing
{
    public class ToFileActivityExporter : BaseExporter<Activity>
    {
        private readonly string _outputFilePath;

        public ToFileActivityExporter(string outputFilePath)
        {
            _outputFilePath = outputFilePath;
        }

        // This is a copy paste from OpenTelemetry.Exporter.ConsoleActivityExporter
        public override ExportResult Export(in Batch<Activity> batch)
        {
            foreach (var activity in batch)
            {
                this.WriteLine($"Activity.Id:          {activity.Id}");
                if (!string.IsNullOrEmpty(activity.ParentId))
                {
                    this.WriteLine($"Activity.ParentId:    {activity.ParentId}");
                }

                this.WriteLine($"Activity.ActivitySourceName: {activity.Source.Name}");
                this.WriteLine($"Activity.DisplayName: {activity.DisplayName}");
                this.WriteLine($"Activity.Kind:        {activity.Kind}");
                this.WriteLine($"Activity.StartTime:   {activity.StartTimeUtc:yyyy-MM-ddTHH:mm:ss.fffffffZ}");
                this.WriteLine($"Activity.Duration:    {activity.Duration}");
                if (activity.TagObjects.Any())
                {
                    this.WriteLine("Activity.TagObjects:");
                    foreach (var tag in activity.TagObjects)
                    {
                        var array = tag.Value as Array;

                        if (array == null)
                        {
                            this.WriteLine($"    {tag.Key}: {tag.Value}");
                            continue;
                        }

                        this.WriteLine($"    {tag.Key}: [{string.Join(", ", array.Cast<object>())}]");
                    }
                }

                if (activity.Events.Any())
                {
                    this.WriteLine("Activity.Events:");
                    foreach (var activityEvent in activity.Events)
                    {
                        this.WriteLine($"    {activityEvent.Name} [{activityEvent.Timestamp}]");
                        foreach (var attribute in activityEvent.Tags)
                        {
                            this.WriteLine($"        {attribute.Key}: {attribute.Value}");
                        }
                    }
                }

                var resource = this.ParentProvider.GetResource();
                if (resource != Resource.Empty)
                {
                    this.WriteLine("Resource associated with Activity:");
                    foreach (var resourceAttribute in resource.Attributes)
                    {
                        this.WriteLine($"    {resourceAttribute.Key}: {resourceAttribute.Value}");
                    }
                }

                this.WriteLine(string.Empty);
            }

            return ExportResult.Success;
        }

        private void WriteLine(string message)
        {
            System.IO.File.AppendAllText(_outputFilePath, message + "\n");
        }
    }
}
