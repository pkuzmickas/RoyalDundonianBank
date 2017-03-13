using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Team16Bank
{
    public partial class Form1 : Form
    {
        private int atmsAvailable = 537;
        private int atmsInUse = 0;
        private Account[] ac = new Account[3];
        List<Thread> threadList = new List<Thread>();

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            availableLabel.Text = atmsAvailable.ToString();
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            atmsInUse++;
            atmsAvailable--;
            availableLabel.Text = atmsAvailable.ToString();
            atmslabel.Text = atmsInUse.ToString();
            Thread newThread = new Thread(() =>
            {
                Application.Run(new ATM(ac, this, atmsInUse.ToString(), false, true));

            });
            newThread.Start();
            threadList.Add(newThread);

        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (Thread t in threadList)
            {
                t.Abort();
            }
        }

        public void turnOffATM()
        {
            atmsInUse--;
            atmsAvailable++;

            atmslabel.Invoke(new MethodInvoker(delegate
            {
                availableLabel.Text = atmsAvailable.ToString();
                atmslabel.Text = atmsInUse.ToString();
            }));

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        public void logInfo(string text)
        {
            log.Invoke(new MethodInvoker(delegate
            {
                if (log.Text == "Listening for events...")
                {
                    log.Text = text;
                }
                else
                {
                    log.Text += "\r\n" + text;
                }
            }));

        }

        private void button2_Click(object sender, EventArgs e)
        {
            atmsInUse += 2;
            atmsAvailable -= 2;
            availableLabel.Text = atmsAvailable.ToString();
            atmslabel.Text = atmsInUse.ToString();
            Thread newThread = new Thread(() =>
            {
                Application.Run(new ATM(ac, this, (atmsInUse - 1).ToString(), true, false));
            });
            newThread.Start();
            threadList.Add(newThread);
            Thread newThread2 = new Thread(() =>
             {
                 Application.Run(new ATM(ac, this, atmsInUse.ToString(), true, false));
             });
            newThread2.Start();
            threadList.Add(newThread2);
        }
    }
}
