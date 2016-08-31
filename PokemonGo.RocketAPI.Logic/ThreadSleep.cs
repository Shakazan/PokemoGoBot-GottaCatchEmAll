using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI.Logic
{
    public static class ThreadSleep
    {
        public static int f_sleep(int time)
        {

            System.DateTime now = System.DateTime.Now;
            System.TimeSpan duration = new System.TimeSpan(0, 0, 0, time);
            System.DateTime then = now.Add(duration);


            Thread this_thread = Thread.CurrentThread;
            while (then > DateTime.Now)
            {
                //MessageBox.Show("NewTime:" + then + "Now:" + DateTime.Now);
                //we do not want to use this because it's said to mess up multithreading
                Application.DoEvents();

                //Thread.CurrentThread.Sleep(10);
                Thread.Sleep(10);
            }

            return 1;
        }
    }
}
