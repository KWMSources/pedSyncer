using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace pedSyncer.Task
{
    public class TaskRunning
    {
        public static ConcurrentQueue<TaskRunning> TaskList { get; set; }
        public Action Function = null;
        internal bool isExecuted = false;

        public TaskRunning(Action function)
        {
            Function = function;
            TaskRunning.TaskList.Enqueue(this);
        }

        public void RunSameThread(Action task)
        {
            new TaskRunning(task);
        }

        [HandleProcessCorruptedStateExceptionsAttribute]
        public static void OnTick()
        {
            int MaxBatch = 100;
            try
            {
                ConcurrentQueue<TaskRunning> TaskList = TaskRunning.TaskList;
                int max = 0;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                if (!TaskList.IsEmpty)
                {
                    do
                    {
                        max++;
                        if (TaskList.TryDequeue(out TaskRunning task))
                        {
                            try
                            {
                                task.isExecuted = true;
                                task.Function();
                                task = null;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error TaskRunning " + ex);
                            }

                            if (timer.ElapsedMilliseconds >= 1)
                            {
                                Console.WriteLine("Time execution ConcurrentQueu Task: " + timer.ElapsedTicks / 10000 + "MS in thread ID: " + Thread.CurrentThread.ManagedThreadId);
                                Console.WriteLine("StackTrace : " + Environment.StackTrace);
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (max < MaxBatch && timer.ElapsedMilliseconds < 1 && !TaskList.IsEmpty);
                    timer.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR SERVEUR TaskRunning ConcurrentQueue Function: " + ex.ToString());
            }
        }
    }
}
