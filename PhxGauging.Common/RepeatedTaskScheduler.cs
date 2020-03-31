using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PhxGauging.Common
{
    public class RepeatedTaskScheduler
    {
        internal class ScheduledTask
        {
            internal string Name { get; set; }
            internal Action Action { get; set; }
            internal DateTime LastRunDateTime { get; set; }
            internal TimeSpan Interval { get; set; }
            internal Func<bool> ConditionFunc { get; set; }
        }

        private Task schedulerTask;
        private bool schedulerRunning;
        private List<ScheduledTask> scheduledTasks = new List<ScheduledTask>();

        public void RegisterAction(string name, Action action, TimeSpan interval)
        {
            RegisterAction(name, action, interval, () => true, true);
        }

        public void RegisterAction(string name, Action action, TimeSpan interval, bool runImmediately)
        {
            RegisterAction(name, action, interval, () => true, runImmediately);
        }

        public void RegisterAction(string name, Action action, TimeSpan interval, Func<bool> conditionFunc)
        {
            RegisterAction(name, action, interval, conditionFunc, true);
        }

        public void RegisterAction(string name, Action action, TimeSpan interval, Func<bool> conditionFunc, bool runImmediately)
        {
            ScheduledTask scheduledTask = new ScheduledTask();
            scheduledTask.Name = name;
            scheduledTask.Action = action;
            scheduledTask.Interval = interval;
            scheduledTask.ConditionFunc = conditionFunc;
            scheduledTask.LastRunDateTime = DateTime.Now;

            if (!runImmediately)
            {
                scheduledTask.LastRunDateTime -= interval;
            }

            scheduledTasks.Add(scheduledTask);
        }

        public void UnregisterAction(string name)
        {
            ScheduledTask scheduledTask = scheduledTasks.FirstOrDefault(t => t.Name == name);

            if (scheduledTask == null)
                return;

            scheduledTasks.Remove(scheduledTask);
        }

        public void ResetAction(string name)
        {
            var task = scheduledTasks.FirstOrDefault(t => t.Name == name);

            if (task == null)
                return;

            task.LastRunDateTime = DateTime.Now;
        }

        public void Start()
        {
            if (schedulerRunning)
                return;

            schedulerRunning = true;

            /*Thread thread = new Thread(() =>
            {
                while (schedulerRunning)
                {
                    for (int i = 0; i < scheduledTasks.Count; i++)
                    {
                        try
                        {
                            ScheduledTask scheduledTask = scheduledTasks[i];
                            DateTime now = DateTime.Now;

                            if ((now - scheduledTask.LastRunDateTime).TotalMilliseconds < scheduledTask.Interval
                                || !scheduledTask.ConditionFunc())
                                continue;

                            scheduledTask.LastRunDateTime = now;
                            scheduledTask.Action();
                        }
                        catch (Exception ex)
                        {

                        }
                        //Application.DoEvents();
                    }
                    Thread.Sleep(25);
                    //Application.DoEvents();

                }

            });
            thread.IsBackground = true;
            thread.Priority=ThreadPriority.BelowNormal;
            thread.Start();*/

            schedulerTask = new Task(() =>
            {
                while (schedulerRunning)
                {
                    for (int i = 0; i < scheduledTasks.Count; i++)
                    {
                        try
                        {
                            ScheduledTask scheduledTask = scheduledTasks[i];
                            DateTime now = DateTime.Now;

                            if ((now - scheduledTask.LastRunDateTime) < scheduledTask.Interval
                                || !scheduledTask.ConditionFunc())
                                continue;

                            scheduledTask.LastRunDateTime = now;
                            scheduledTask.Action();
                        }
                        catch (Exception ex)
                        {
                            
                        }
                        //Application.DoEvents();
                    }
                    //Application.DoEvents();

                }
            });

            schedulerTask.Start();
        }

        public void Stop()
        {
            schedulerRunning = false;
        }
    }
}