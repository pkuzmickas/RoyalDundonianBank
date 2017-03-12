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
        Image[] keysPressed = new Image[16];
        Image[] keysReleased = new Image[16];
        Account[] ac;
        Account currentAccount;
        string pinEntered;
        int retriesRemaining = 3;

        public ATM(Account[] ac)
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.ac = ac;
            for (int i = 0; i < 16; i++)
            {
                keysPressed[i] = Image.FromFile(@"keyPadButtons\key" + i + "Pressed.png");
                keysReleased[i] = Image.FromFile(@"keyPadButtons\key" + i + ".png");
            }
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
                    screenButtons[i, j].Tag = new TagInfo { name = "screenButton", value = i };
                    screenButtons[i, j].Image = sideButton;



                    screenButtons[i, j].Show();
                    this.Controls.Add(screenButtons[i, j]);
                    this.Controls.SetChildIndex(screenButtons[i, j], 0);
                    //screenButtons[i, j].Click += new EventHandler(screenButtonsDown);
                }
                vertical2 += 42;
            }
        }
        private void keypadButtonsDown(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            TagInfo TI = (TagInfo)pb.Tag;
            int val = getImageNumber(TI.value);
            pb.Image = keysPressed[val];
            if (pinpanel.Visible == true && retriesRemaining>0)
            {
                if (pinlabel.Text != "****")
                {
                    pinlabel.Text += "*";
                    pinEntered+=TI.value.ToString();
                }
                
                if (pinlabel.Text == "****")
                {
                    if (currentAccount.checkPin(Int32.Parse(pinEntered)))
                    {
                        Console.WriteLine("Correct");
                    }
                    else
                    {
                        retriesRemaining--;
                        pinlabel.Text = "";
                        pinEntered = "";
                        errorlabel.Visible = true;
                        if(retriesRemaining>0)
                        errorlabel.Text = "INCORRECT! (" + retriesRemaining + " RETRIES REMAINING)";
                        else
                        {
                            errorlabel.Text = "INCORRECT! CARD EJECTED!";
                            
                            System.Timers.Timer timer = new System.Timers.Timer(1500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                            timer.Enabled = true;
                            
                        }
                    }
                    
                }
            }
        }
        private void returnToMainScreen(object sender, EventArgs e)
        {
            
            if (pinpanel.InvokeRequired)
            {
                pinpanel.Invoke(new MethodInvoker(delegate { 
                    pinpanel.Visible = false; 
                    accnumber.Enabled = true; 
                    loginpanel.Show();
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
                        loginpanel.Hide();
                        pinpanel.Visible = true;
                        accnumber.Enabled = false;

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

    }



    public class TagInfo
    {
        public string name;
        public int value;
    }
}
