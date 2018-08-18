using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AngryWasp.Logger;

namespace Nerva.Toolkit.Helpers
{
    public class TaskContainer
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Task Task { get; set; }
    }

    public class TaskFactory
    {
        private ConcurrentDictionary<string, TaskContainer> containers = new ConcurrentDictionary<string, TaskContainer>();

        private static TaskFactory instance = new TaskFactory();

        public static TaskFactory Instance => instance;

        public TaskContainer this[string name]
        {
            get
            {
                TaskContainer c = null;
                if (containers.TryGetValue(name, out c))
                    return c;

                return null;
            }
        }

        public int Count => containers.Count;

        public bool RunTask(string name, string description, Action action)
        {
            //Basically, we try to add the task to the list, if that works, we start the task,
            //otherwise we write a message in the log and return
            if (containers.TryAdd(name, new TaskContainer{
                Name = name,
                Description = description}))
            {
                containers[name].Task = Task.Run(action);
                return true;
            }
            else
                return false;
        }

        public void Prune()
        {
            foreach(var c in containers)
            {
                string toRemove = null;
                
                if (c.Value.Task.IsCanceled || c.Value.Task.IsFaulted)
                {
                    //Tasks are never cancelled, so if this flag is raised, we assume an error has occured
                    Log.Instance.Write("Task {0} encountered an error", c.Key);
                    toRemove = c.Key;
                }
                else if (c.Value.Task.IsCompleted)
                {
                    //No message if task completed. Handle elsewhere with specific message
                    toRemove = c.Key;
                }

                if (toRemove != null)
                {
                    TaskContainer tc = null;
                    if (!containers.TryRemove(c.Key, out tc))
                        Log.Instance.Write(Log_Severity.Error, "Failed to remove task {0} from TaskFactory");
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb =new StringBuilder();
            foreach (var c in containers)
                sb.AppendLine(c.Value.Description);

            return sb.ToString();
        }
    }
}