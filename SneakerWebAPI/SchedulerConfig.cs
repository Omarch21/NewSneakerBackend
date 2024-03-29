using Quartz;
using Quartz.Impl;
using System.Net.NetworkInformation;

namespace SneakerWebAPI
{
    public class SchedulerConfig
    {
        public static async void Start()
        {
            try
            {
                ISchedulerFactory schedulerFactory1 = new StdSchedulerFactory();
                IScheduler scheduler1 = await schedulerFactory1.GetScheduler();

                await scheduler1.Start();

                IJobDetail job1 = JobBuilder.Create<SneakerPricePoster>().WithIdentity("Sneaker Price poster", "group 5").Build();

                ITrigger trigger1 = (ITrigger)TriggerBuilder.Create().WithIdentity("Sneaker Price poster", "group 5")
                    .StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()).Build();

                await scheduler1.ScheduleJob(job1, trigger1);

                Console.WriteLine("hi");
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                IScheduler scheduler = await schedulerFactory.GetScheduler();

                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<ResellUpdater>().WithIdentity("resell updater", "group 1").Build();

                ITrigger trigger = (ITrigger)TriggerBuilder.Create().WithIdentity("Resell updater trigger", "group 1")
                    .StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()).Build();

               await scheduler.ScheduleJob(job, trigger);

            }catch(Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
