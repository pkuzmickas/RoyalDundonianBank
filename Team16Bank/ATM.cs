using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Team16Bank
{
    public partial class ATM : Form
    {
        private PictureBox[,] screenButtons = new PictureBox[4, 2];
        private PictureBox[,] keyPadButtons = new PictureBox[4, 4];
        private Label account = new Label();
        Image[] keysPressed = new Image[18];
        Image[] keysReleased = new Image[18];
        Image[] cashOutIMG = new Image[8];
        Image[] receiptOutIMG = new Image[6];
        Account[] ac;
        Account currentAccount;
        string pinEntered;
        bool controlsEnabled = true;
        int retriesRemaining = 3;
        private int cashNumber = 0;
        private int receiptNumber = 1;
        bool receiptNeeded = false;
        Form1 bank;

        public ATM(Account[] ac, Form1 mainBank, string name)
        {
            InitializeComponent();
            bank = mainBank;
            this.Text = "ATM" + name;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.ac = ac;
            for (int i = 0; i < 16; i++)
            {
                keysPressed[i] = Image.FromFile(@"keyPadButtons\key" + i + "Pressed.png");
                keysReleased[i] = Image.FromFile(@"keyPadButtons\key" + i + ".png");
                if (i < 8)
                {
                    cashOutIMG[i] = Image.FromFile(@"keyPadButtons\cash" + i + ".png");
                }
                if (i < 6)
                {
                    receiptOutIMG[i] = Image.FromFile(@"keyPadButtons\end" + (i+1) + ".png");
                }
            }
            
            keysPressed[16] = Properties.Resources.screenSideButton0Pressed;
            keysPressed[17] = Properties.Resources.screenSideButton1Pressed;
            keysReleased[16] = Image.FromFile(@"keyPadButtons\screenSideButton0.png");
            keysReleased[17] = Image.FromFile(@"keyPadButtons\screenSideButton1.png");
        }

        private void ATM_Load(object sender, EventArgs e)
        {
            int horizotal = 28;
            int vertical = 271;
            int horizotal2 = 12;
            int vertical2 = 110;
            int fileNumber = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    Image image = keysReleased[fileNumber];
                    keyPadButtons[i, j] = new PictureBox();
                    keyPadButtons[i, j].Size = new Size(32, 25);
                    keyPadButtons[i, j].Location = new Point(horizotal, vertical);
                    int val = i * 3 + (j + 1);
                    if (j == 3) val = -(val - (i + 1) * 3 + i);
                    if (val == 11) val = 0;
                    keyPadButtons[i, j].Tag = new TagInfo { name = "keyPadButton", value = val };
                    keyPadButtons[i, j].Image = image;

                    if ((j + 1) % 4 == 0)
                    {
                        vertical += 30;
                        horizotal = 28;
                    }
                    else
                    {
                        if (j == 2) horizotal += 44;
                        else horizotal += 37;
                    }
                    keyPadButtons[i, j].Show();
                    this.Controls.Add(keyPadButtons[i, j]);
                    this.Controls.SetChildIndex(keyPadButtons[i, j], 0);
                    keyPadButtons[i, j].MouseDown += new MouseEventHandler(keypadButtonsDown);
                    keyPadButtons[i, j].MouseUp += new MouseEventHandler(keypadButtonsUp);
                    fileNumber++;
                }
            }
            keyPadButtons[0, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[0, 3].Size = new Size(37, 25);
            keyPadButtons[1, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[1, 3].Size = new Size(37, 25);
            keyPadButtons[2, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[2, 3].Size = new Size(37, 25);
            keyPadButtons[3, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[3, 3].Size = new Size(37, 25);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (j == 1)
                    {
                        horizotal2 = 400;
                    }
                    else
                    {
                        horizotal2 = 12;
                    }
                    Image sideButton = Image.FromFile(@"keyPadButtons\screenSideButton" + j + ".png");
                    screenButtons[i, j] = new PictureBox();
                    screenButtons[i, j].Size = new Size(25, 17);
                    screenButtons[i, j].Location = new Point(horizotal2, vertical2);
                    screenButtons[i, j].Tag = new TagInfo { name = "screenButton", value = i + j * 3 };
                    screenButtons[i, j].Image = sideButton;



                    screenButtons[i, j].Show();
                    this.Controls.Add(screenButtons[i, j]);
                    this.Controls.SetChildIndex(screenButtons[i, j], 0);
                    screenButtons[i, j].MouseDown += new MouseEventHandler(screenButtonsDown);
                    screenButtons[i, j].MouseUp += new MouseEventHandler(screenButtonsUp);
                }
                vertical2 += 42;
            }
        }

        private void screenButtonsDown(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            TagInfo TI = (TagInfo)pb.Tag;
            if (TI.value > 2)
            {
                pb.Image = keysPressed[17];
            }
            else
            {
                pb.Image = keysPressed[16];
            }

        }
        private void screenButtonsUp(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            TagInfo TI = (TagInfo)pb.Tag;

            if (TI.value > 2)
            {
                pb.Image = keysReleased[17];
            }
            else
            {
                pb.Image = keysReleased[16];
            }
            if (controlsEnabled)
            {
                if (cashpanel.Visible)
                {
                    int cashToTakeAmount = 0;
                    switch (TI.value)
                    {
                        case 0:
                            cashToTakeAmount = 5;
                            break;
                        case 1:
                            cashToTakeAmount = 20;
                            break;
                        case 2:
                            cashToTakeAmount = 100;
                            break;
                        case 3:
                            cashToTakeAmount = 10;
                            break;
                        case 4:
                            cashToTakeAmount = 50;
                            break;
                        case 5:
                            {
                                othercashpanel.Visible = true;
                                cashpanel.Visible = false;
                                break;
                            }

                    }
                    if (TI.value != 5)
                    {
                        if (currentAccount.getBalance() >= cashToTakeAmount)
                        {
                            currentAccount.decrementBalance(cashToTakeAmount);
                            cashOutAnimation();
                            if (receiptNeeded) receiptOutAnimation();
                            EndCash.Visible = true;
                            cashpanel.Visible = false;
                            
                        }
                        else
                        {
                            EndCashBad.Visible = true;
                            cashpanel.Visible = false;
                            
                        }
                        System.Timers.Timer timer = new System.Timers.Timer(2500);
                        timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                        timer.Enabled = true;
                    }
                }

                if (mainpanel.Visible)
                {
                    switch (TI.value)
                    {
                        case 0:
                            {

                                mainpanel.Visible = false;
                                cashpanel.Visible = true;

                                break;
                            }
                        case 1:
                            {
                                mainpanel.Visible = false;
                                balancepanel.Visible = true;
                                balancelabel.Text = currentAccount.getBalance().ToString();
                                break;
                            }
                        case 3:
                            {
                                receiptNeeded = true;
                                mainpanel.Visible = false;
                                cashpanel.Visible = true;

                                break;
                            }
                        case 4:
                            {
                                receiptNeeded = true;
                                mainpanel.Visible = false;
                                balancepanel.Visible = true;
                                balancelabel.Text = currentAccount.getBalance().ToString();
                                receiptOutAnimation();
                                break;
                            }
                        case 5:
                            {
                                mainpanel.Visible = false;
                                currentpinpanel.Visible = true;

                                break;
                            }

                    }
                }
                else if (balancepanel.Visible)
                {
                    switch (TI.value)
                    {
                        case 4:
                            {

                                balancepanel.Visible = false;
                                mainpanel.Visible = true;
                                receiptNeeded = false;
                                break;
                            }
                        case 5:
                            {
                                balancepanel.Visible = false;
                                EndSesh.Visible = true;
                                System.Timers.Timer timer = new System.Timers.Timer(2500);
                                timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                                timer.Enabled = true;
                                break;
                            }

                    }
                }
            }
        }
        

        private void keypadButtonsDown(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            TagInfo TI = (TagInfo)pb.Tag;
            int val = getImageNumber(TI.value);
            pb.Image = keysPressed[val];
            if (controlsEnabled)
            {

                if (TI.value == -1 && !loginpanel.Visible)
                {
                    pinpanel.Visible = false;
                    EndSesh.Visible = true;
                    System.Timers.Timer timer = new System.Timers.Timer(2500);
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                    timer.Enabled = true;
                    controlsEnabled = false;
                }
                else if (currentpinpanel.Visible && retriesRemaining > 0)
                {
                    if (TI.value >= 0 && TI.value <= 9)
                    {
                        pinEntered += TI.value.ToString();
                        currentpinlabel.Text += "*";

                    }
                    if (TI.value == -2)
                    {
                        currentpinlabel.Text = currentpinlabel.Text.Substring(0, currentpinlabel.Text.Length - 1);
                        pinEntered = pinEntered.Substring(0, pinEntered.Length - 1);

                    }
                    if (currentpinlabel.Text == "****")
                    {
                        if (currentAccount.checkPin(Int32.Parse(pinEntered)))
                        {
                            System.Timers.Timer timer = new System.Timers.Timer(500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(showNewPinScreen);
                            timer.Enabled = true;
                            
                            
                        }
                        else
                        {
                            retriesRemaining--;
                            currentpinlabel.Text = "";
                            pinEntered = "";
                            errorpinlabel.Visible = true;
                            if (retriesRemaining > 0)
                                errorpinlabel.Text = "INCORRECT! (" + retriesRemaining + " RETRIES REMAINING)";
                            else
                            {
                                errorpinlabel.Text = "INCORRECT! CARD EJECTED!";

                                System.Timers.Timer timer = new System.Timers.Timer(1500);
                                timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                                timer.Enabled = true;

                            }
                        }
                    }

                }
                else if (newpinpanel.Visible)
                {
                    if (TI.value >= 0 && TI.value <= 9)
                    {
                        pinEntered += TI.value.ToString();
                        newpinlabel.Text += "*";

                    }
                    if (TI.value == -2)
                    {
                        newpinlabel.Text = newpinlabel.Text.Substring(0, newpinlabel.Text.Length - 1);
                        pinEntered = pinEntered.Substring(0, pinEntered.Length - 1);

                    }
                    if (newpinlabel.Text == "****")
                    {
                        currentAccount.setPin(Int32.Parse(pinEntered));
                        successlabel.Visible = true;
                        System.Timers.Timer timer = new System.Timers.Timer(1500);
                        timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                        timer.Enabled = true;

                    }

                }
                else if (othercashpanel.Visible)
                {
                    if (TI.value == -3)
                    {
                        if (cashamountlabel.Text != "")
                        {
                            if (currentAccount.getBalance() >= Int32.Parse(cashamountlabel.Text))
                            {
                                currentAccount.decrementBalance(Int32.Parse(cashamountlabel.Text));
                                cashOutAnimation();
                                if (receiptNeeded) receiptOutAnimation();
                                EndCash.Visible = true;
                                othercashpanel.Visible = false;
                                
                            }
                            else
                            {
                                EndCashBad.Visible = true;
                                othercashpanel.Visible = false;
                            }
                            System.Timers.Timer timer = new System.Timers.Timer(2500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                            timer.Enabled = true;
                        }
                    }
                    else if (TI.value == -2 && cashamountlabel.Text != "")
                    {
                        cashamountlabel.Text = cashamountlabel.Text.Substring(0, cashamountlabel.Text.Length - 1);
                    }
                    else if (TI.value >= 0 && TI.value <= 9 && (TI.value != 0 || cashamountlabel.Text != ""))
                    {
                        cashamountlabel.Text += TI.value.ToString();
                    }

                }
                else if (pinpanel.Visible == true && retriesRemaining > 0)
                {
                    if (pinlabel.Text != "****" && TI.value>=0 && TI.value<=9)
                    {
                        
                        pinlabel.Text += "*";
                        pinEntered += TI.value.ToString();
                    }
                    if (TI.value == -2)
                    {
                        pinlabel.Text = pinlabel.Text.Substring(0, pinlabel.Text.Length - 1);
                        pinEntered = pinEntered.Substring(0, pinEntered.Length - 1);

                    }
                    if (pinlabel.Text == "****")
                    {
                        if (currentAccount.checkPin(Int32.Parse(pinEntered)))
                        {

                            System.Timers.Timer timer = new System.Timers.Timer(500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(showMainScreen);
                            timer.Enabled = true;
                            controlsEnabled = false;
                            bank.logInfo(this.Text + ": Successful login for account: " + accnumber.Text);
                        }
                        else
                        {
                            retriesRemaining--;
                            pinlabel.Text = "";
                            pinEntered = "";
                            errorlabel.Visible = true;
                            if (retriesRemaining > 0)
                                errorlabel.Text = "INCORRECT! (" + retriesRemaining + " RETRIES REMAINING)";
                            else
                            {
                                errorlabel.Text = "INCORRECT! CARD EJECTED!";
                                bank.logInfo(this.Text + ": Pin rejected for account: " + accnumber.Text);
                                System.Timers.Timer timer = new System.Timers.Timer(1500);
                                timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                                timer.Enabled = true;

                            }
                        }

                    }

                }
            }
        }
        private void showMainScreen(object sender, EventArgs e)
        {

            pinpanel.Invoke(new MethodInvoker(delegate
            {
                pinpanel.Visible = false;
                mainpanel.Visible = true;
                controlsEnabled = true;
                pinlabel.Text = "";
                pinEntered = "";
                retriesRemaining = 3;
            }));

            ((System.Timers.Timer)sender).Enabled = false;
        }
        private void showNewPinScreen(object sender, EventArgs e)
        {

            currentpinpanel.Invoke(new MethodInvoker(delegate
            {
                currentpinpanel.Visible = false;
                newpinpanel.Visible = true;
                currentpinlabel.Text = "";
                pinEntered = "";
                retriesRemaining = 3;
            }));

            ((System.Timers.Timer)sender).Enabled = false;
        }
        private void returnToMainScreen(object sender, EventArgs e)
        {

            if (pinpanel.InvokeRequired)
            {
                pinpanel.Invoke(new MethodInvoker(delegate
                {
                    receiptNeeded = false;
                    pinpanel.Visible = false;
                    EndCash.Visible = false;
                    EndSesh.Visible = false;
                    balancepanel.Visible = false;
                    othercashpanel.Visible = false;
                    mainpanel.Visible = false;
                    cashpanel.Visible = false;
                    currentpinpanel.Visible = false;
                    newpinpanel.Visible = false;
                    accnumber.Enabled = true;
                    button1.Enabled = true;
                    controlsEnabled = true;
                    loginpanel.Visible = true;
                    EndCashBad.Visible = false;
                    errorlabel.Visible = false;
                    errorpinlabel.Visible = false;
                    successlabel.Visible = false;
                    cashamountlabel.Text = "";
                    newpinlabel.Text = "";
                    currentpinlabel.Text = "";
                    pinlabel.Text = "";
                    pinEntered = "";
                    accnumber.Text = "";
                    loginlabel.Text = "Please insert your card ((insert the account number))";
                    retriesRemaining = 3;
                }));
            }

            ((System.Timers.Timer)sender).Enabled = false;



        }
        private void keypadButtonsUp(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            TagInfo TI = (TagInfo)pb.Tag;
            int val = getImageNumber(TI.value);
            pb.Image = keysReleased[val];
        }

        private void cashOutAnimation()
        {
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(cashAnimation);
            timer.Start();


        }

        private void receiptOutAnimation()
        {
            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(receiptAnimation);
            timer.Start();


        }

        private void cashAnimation(object sender, System.EventArgs e)
        {
            var timer = (Timer)sender;
            Image image = cashOutIMG[cashNumber];
            
            pictureBox3.BackgroundImage = image;
            cashNumber++;
            if (cashNumber == 7)
            {
                timer.Stop();
                cashNumber = 0;
            }
        }
        private void receiptAnimation(object sender, System.EventArgs e)
        {
            var timer = (Timer)sender;
            Image image = receiptOutIMG[receiptNumber-1];
            pictureBox4.BackgroundImage = image;
            receiptNumber++;
            if (receiptNumber == 6)
            {
                receiptNumber = 1;
                timer.Stop();
            }
        }    


        private int getImageNumber(int TIValue)
        {
            int decreaser = 1;

            // I'm subtracting a minus from a minus just for lolz
            if (TIValue >= 4 && TIValue < 7)
            {
                decreaser = 0;
            }
            else if (TIValue >= 7 && TIValue < 10)
            {
                decreaser = -1;
            }
            else if (TIValue == -1)
            {
                decreaser = -4;
            }
            else if (TIValue == -2)
            {
                decreaser = -9;
            }
            else if (TIValue == -3)
            {
                decreaser = -14;
            }
            else if (TIValue == -4)
            {
                decreaser = -19;
            }
            else if (TIValue == 0)
            {
                decreaser = -13;
            }
            else if (TIValue >= 10)
            {
                decreaser = -2;
            }
            int val = TIValue - decreaser;
            return val;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (accnumber.Text != "")
            {
                try
                {
                    int input = Int32.Parse(accnumber.Text);
                    currentAccount = findAccount(input);
                    if (currentAccount != null)
                    {
                        loginpanel.Visible = false;
                        pinpanel.Visible = true;
                        accnumber.Enabled = false;
                        button1.Enabled = false;

                    }
                    else
                    {
                        loginlabel.Text = "The card could not be read ((invalid account number))";
                        accnumber.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    loginlabel.Text = "The card could not be read ((invalid account number))";
                    accnumber.Text = "";
                }

            }
        }

        /*
         *    this method promts for the input of an account number
         *    the string input is then converted to an int
         *    a for loop is used to check the enterd account number
         *    against those held in the account array
         *    if a match is found a referance to the match is returned
         *    if the for loop completest with no match we return null
         * 
         */
        private Account findAccount(int input)
        {



            for (int i = 0; i < this.ac.Length; i++)
            {
                if (ac[i].getAccountNum() == input)
                {
                    return ac[i];
                }
            }

            return null;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            pictureBox3.BackgroundImage = Properties.Resources.nocash;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            pictureBox4.BackgroundImage = Properties.Resources.noend;

        }

        private void ATM_FormClosed(object sender, FormClosedEventArgs e)
        {
            
            bank.turnOffATM();
        }

    }



    public class TagInfo
    {
        public string name;
        public int value;
    }
}
