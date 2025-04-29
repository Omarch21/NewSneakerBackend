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

                IJobDetail postingPrices = JobBuilder.Create<SneakerPricePoster>().WithIdentity("Sneaker Price poster", "group 5").Build();

                ITrigger trigger1 = (ITrigger)TriggerBuilder.Create().WithIdentity("Sneaker Price poster", "group 5")
                    .StartNow().WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()).Build();

                await scheduler1.ScheduleJob(postingPrices, trigger1);

            }catch(Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
