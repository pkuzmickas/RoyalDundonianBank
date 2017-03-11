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
        int atmsInUse = 0;
        Thread loggingThread;
        bool running = true;
        private Account[] ac = new Account[3];

        public Form1()
        {
            InitializeComponent();
            loggingThread = new Thread(updateLog);
            loggingThread.Start();
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            atmsInUse++;
            label5.Text = atmsInUse.ToString();
            new ATM().Show();
            
        }

        void updateLog()
        {
            while (running)
            {
                
                //updatelog
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            running = false;
        }
    }
}
