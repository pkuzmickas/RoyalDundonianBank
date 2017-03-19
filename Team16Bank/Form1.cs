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
        //Int variable used to show the number of ATM available
        private int atmsAvailable = 537;
        //Int variable used to show the number of ATM active/used at the moment
        private int atmsInUse = 0;
        //Array of accounts used to store accounts
        private Account[] ac = new Account[3];
        //List containg all the threads and instanciate
        List<Thread> threadList = new List<Thread>();
        //Semaphore variable used for semaphore implementation
        public static Semaphore semaphore;

        public Form1()
        {
            //Initialize main components created using the toolbox
            InitializeComponent();
            //Set the style of the bank system form
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            //Set the label that is showing the number of available ATMs
            availableLabel.Text = atmsAvailable.ToString();
            //Initialize 3 accounts
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
            //Initialize the semaphore
            semaphore = new Semaphore(1,1);
        }
        //Event handler for clicks on button1(normal ATM button)
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


        //Event handler for closing the form
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (Thread t in threadList)
            {
                t.Abort();
            }
        }
        //Method used to savely close an ATM instance
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
        //Event handler for closing the form
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        //Method used to display all the informations about the ATMs in the log window
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
        //Event handler for clicks on button2(Simulated broken ATMs button)
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
        //Event handler for clicks on button2(Simulated broken ATMs with semaphore fix button)
        private void button3_Click(object sender, EventArgs e)
        {
            //Keep track of the ATMs to always be 2
            atmsInUse += 2;
            atmsAvailable -= 2;
            //Set the number of the available ATMs to 2
            availableLabel.Text = atmsAvailable.ToString();
            atmslabel.Text = atmsInUse.ToString();
            //Creates a new Thread which will run one ATM
            Thread newThread = new Thread(() =>
            {
                Application.Run(new ATM(ac, this, (atmsInUse - 1).ToString(), true, true));
            });
            newThread.Start();
            threadList.Add(newThread);
            //Creates a new Thread which will run one ATM
            Thread newThread2 = new Thread(() =>
            {
                Application.Run(new ATM(ac, this, atmsInUse.ToString(), true, true));
            });
            newThread2.Start();
            threadList.Add(newThread2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        
    }
    
}
