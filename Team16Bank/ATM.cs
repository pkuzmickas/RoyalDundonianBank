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
    public partial class ATM : Form
    {
        //Initialise an array of picture boxes for the screen side buttons
        private PictureBox[,] screenButtons = new PictureBox[4, 2];
        //Initialise an array of picture boxes for the keypad
        private PictureBox[,] keyPadButtons = new PictureBox[4, 4];
        //Array of images to store all the images used for the keypad when they are pressed
        Image[] keysPressed = new Image[18];
        //Array of images to store all the images used for the keypad when they are not pressed
        Image[] keysReleased = new Image[18];
        //Array of images to store the cash in/out animation
        Image[] cashOutIMG = new Image[8];
        //Array of images to store the receipt in/out animation
        Image[] receiptOutIMG = new Image[6];
        //Array of Accounts to store all the available accounts
        Account[] ac;
        //Account object used to store the account in use
        Account currentAccount;
        //String variable used to store the entered pin
        string pinEntered;
        //Bool variable which is used to enable and disable the keypad
        bool controlsEnabled = true;
        //Int variable used to select the numbers of tries you ahve till the card will be rejected
        int retriesRemaining = 3;
        //Int variable used to change the image used for the cash out animation
        private int cashNumber = 0;
        //Int variable used to change the image used for the cash in animation
        private int cashNumberRevers = 6;
        ////Int variable used to change the image used for the receipt out animation
        private int receiptNumber = 1;
        //Bool variable used to check if the receipt was requested and also to check if the user took the money,if the user did not take the money they will pulled back in and readded to the balance
        bool receiptNeeded = false, moneyTaken = true;
        //Object of the main bank system used to interact with the main controll unit
        Form1 bank;
        //Bool variable used to trigger the semaphore and on to trigger the delays needed to how how we handle the data race
        bool isFixed = true, isSimulation = false;
        //Bool variable used to know if the delays are applyed at the momment normaly it is true when a cash out request is triggered
        public bool simulating = false;

        public ATM(Account[] ac, Form1 mainBank, string name, bool isSimulation, bool isFixed)
        {
            //Initialiaze all the components created using the ToolBox
            InitializeComponent();

            this.isFixed = isFixed;
            this.isSimulation = isSimulation;
            //Sets the bank object to an instance of the main bank computer
            bank = mainBank;
            //Changes the name of the ATM
            this.Text = "ATM" + name;

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            //Gets the array of accounts from the bank system
            this.ac = ac;
            //For loop used to initialise all the array elemts of the ATM GUI
            for (int i = 0; i < 16; i++)
            {
                //Imports all the images for the keypad when the keys are pressed
                keysPressed[i] = Image.FromFile(@"keyPadButtons\key" + i + "Pressed.png");
                //Imports all the images for the keypad when the keys are not pressed
                keysReleased[i] = Image.FromFile(@"keyPadButtons\key" + i + ".png");
                //Imports all the images for the cash out animation
                if (i < 8)
                {
                    cashOutIMG[i] = Image.FromFile(@"keyPadButtons\cash" + i + ".png");
                }
                //Imports all the images for the receipt out animation
                if (i < 6)
                {
                    receiptOutIMG[i] = Image.FromFile(@"keyPadButtons\end" + (i + 1) + ".png");
                }
            }
            //Imports images for pressed keys
            keysPressed[16] = Properties.Resources.screenSideButton0Pressed;
            keysPressed[17] = Properties.Resources.screenSideButton1Pressed;
            //Imports images for realeased keys 
            keysReleased[16] = Image.FromFile(@"keyPadButtons\screenSideButton0.png");
            keysReleased[17] = Image.FromFile(@"keyPadButtons\screenSideButton1.png");
        }

        private void ATM_Load(object sender, EventArgs e)
        {
            //the horizontal point the starting possition for generating the key pad buttons
            int horizotal = 28;
            //the vertical point the starting possition for generating the key pad buttons
            int vertical = 271;
            //the horizontal point the starting possition for generating the screen buttons
            int horizotal2 = 12;
            //the vertical point the starting possition for generating the screen buttons
            int vertical2 = 110;
            //Used to keep track of which image needs to be used for a specific button
            int fileNumber = 0;
            //For loop used to initialised array elements of the GUI
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //Gets an image from the array that contains all the button images
                    Image image = keysReleased[fileNumber];
                    //Initialise the picture box 
                    keyPadButtons[i, j] = new PictureBox();
                    //Sets the size of the keypad buttons
                    keyPadButtons[i, j].Size = new Size(32, 25);
                    //Sets the location of the keypad buttons
                    keyPadButtons[i, j].Location = new Point(horizotal, vertical);
                    //Int variable used to keep track of each keypad button
                    int val = i * 3 + (j + 1);
                    //calculating the val to know with which button the code is interacting
                    if (j == 3) val = -(val - (i + 1) * 3 + i);
                    //Resets val
                    if (val == 11) val = 0;
                    //Initialises the tag class of the picture boxes
                    keyPadButtons[i, j].Tag = new TagInfo { name = "keyPadButton", value = val };
                    //Sets the background image of the button
                    keyPadButtons[i, j].Image = image;
                    //Calculates the possition of the next button
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
                    //Make the button visible
                    keyPadButtons[i, j].Show();
                    //Add the button to the controler
                    this.Controls.Add(keyPadButtons[i, j]);
                    //Place the button on top of all the other GUI elements
                    this.Controls.SetChildIndex(keyPadButtons[i, j], 0);
                    //Add mouse down event for the button to make the interaction with the buttons possible
                    keyPadButtons[i, j].MouseDown += new MouseEventHandler(keypadButtonsDown);
                    //Add mouse up event for the button to make the interaction with the buttons possible
                    keyPadButtons[i, j].MouseUp += new MouseEventHandler(keypadButtonsUp);
                    //Increment the image tracker 
                    fileNumber++;
                }
            }
            //Stretch the image to fit the picture boxes for some buttons
            //Adjust the size of the button for some picture boxes
            keyPadButtons[0, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[0, 3].Size = new Size(37, 25);
            keyPadButtons[1, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[1, 3].Size = new Size(37, 25);
            keyPadButtons[2, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[2, 3].Size = new Size(37, 25);
            keyPadButtons[3, 3].SizeMode = PictureBoxSizeMode.StretchImage;
            keyPadButtons[3, 3].Size = new Size(37, 25);
            //For loop used to initialise array elements of the GUI
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    //Changes the horizontal possition of the screen button to make them apper of both sides of the screen
                    if (j == 1)
                    {
                        horizotal2 = 400;
                    }
                    else
                    {
                        horizotal2 = 12;
                    }
                    //Extract the images for the side screen buttons
                    Image sideButton = Image.FromFile(@"keyPadButtons\screenSideButton" + j + ".png");
                    //Initialise the picture boxes for the side screen buttons
                    screenButtons[i, j] = new PictureBox();
                    //Set the size of the side screen buttons
                    screenButtons[i, j].Size = new Size(25, 17);
                    //Sets the possition of the side screen buttons
                    screenButtons[i, j].Location = new Point(horizotal2, vertical2);
                    //Initialise the tag for each side screen button
                    screenButtons[i, j].Tag = new TagInfo { name = "screenButton", value = i + j * 3 };
                    //Sets the backgroung image of the side screen buttons
                    screenButtons[i, j].Image = sideButton;


                    //Make the side screen buttons visible
                    screenButtons[i, j].Show();
                    //Add the side screen buttons to the controler
                    this.Controls.Add(screenButtons[i, j]);
                    //Place the button on top of all the other GUI elements
                    this.Controls.SetChildIndex(screenButtons[i, j], 0);
                    //Add mouse down event for the button to make the interaction with the buttons possible
                    screenButtons[i, j].MouseDown += new MouseEventHandler(screenButtonsDown);
                    //Add mouse up event for the button to make the interaction with the buttons possible
                    screenButtons[i, j].MouseUp += new MouseEventHandler(screenButtonsUp);
                }
                //Changing the vertical possition of the side screen buttons
                vertical2 += 42;
            }
        }
        //Event handler for side screen buttons
        private void screenButtonsDown(object sender, EventArgs e)
        {
            //Creates a variable to hold the clicked button
            PictureBox pb = (PictureBox)sender;
            //Creates a variable to hold the tag informations of the buttons to make the interaction with the tag easier
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
            //Creates a variable to hold the clicked button
            PictureBox pb = (PictureBox)sender;
            //Creates a variable to hold the tag informations of the buttons to make the interaction with the tag easier
            TagInfo TI = (TagInfo)pb.Tag;
            //Changing the image of the button when it is clicked
            if (TI.value > 2)
            {
                pb.Image = keysReleased[17];
            }
            else
            {
                pb.Image = keysReleased[16];
            }
            //Checks if the keypad is active
            if (controlsEnabled)
            {
                //Checks if the user is in the cash out panel
                if (cashpanel.Visible)
                {   
                    //Variable used to keep track of the value of the cash out request
                    int cashToTakeAmount = 0;   
                    //Checks how much money were requested using the keypad
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
                            //Case when the users wants a custome ammount
                        case 5:
                            {
                                othercashpanel.Visible = true;
                                cashpanel.Visible = false;
                                break;
                            }

                    }
                    //If the values is diffrent then 5, that means that if the user does not click on other the withdraw operation will start
                    if (TI.value != 5)
                    {
                        //If the user has 
                        if (currentAccount.getBalance() >= cashToTakeAmount)
                        {
                            //Normal ATM mechanic without any data race or fix
                            if (!isSimulation)
                            {
                                currentAccount.decrementBalance(cashToTakeAmount);
                                cashOutAnimation();
                                moneyTaken = false;
                                bank.logInfo(this.Text + ": user " + accnumber.Text + " has just withdrawn " + cashToTakeAmount + "$ from their account.");
                                bank.logInfo(this.Text + ": user " + accnumber.Text + " new balance: " + currentAccount.getBalance() + "$");
                                if (receiptNeeded) receiptOutAnimation();
                                EndCash.Visible = true;
                                cashpanel.Visible = false;
                            }
                                //Triggered when simulation without fix is active
                            else if(!isFixed)
                            {
                                cashpanel.Visible = false;
                                badsimulationpanel.Visible = true;
                                System.Timers.Timer t = new System.Timers.Timer(500);
                                t.Elapsed += (sender1, ev) => sleepTest(sender1, ev, cashToTakeAmount);
                                t.Enabled = true;

                            }
                                //Triggered when simulation with semaphore fix is active
                            else
                            {
                                cashpanel.Visible = false;
                                goodsimulationpanel.Visible = true;
                                System.Timers.Timer t = new System.Timers.Timer(500);
                                t.Elapsed += (sender1, ev) => sleepTest(sender1, ev, cashToTakeAmount);
                                t.Enabled = true;
                            }

                        }
                            //Triggered when the user does not have enough cash
                        else
                        {
                            EndCashBad.Visible = true;
                            cashpanel.Visible = false;
                            System.Timers.Timer timer = new System.Timers.Timer(1500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                            timer.Enabled = true;

                        }
                        //Return to the initial screen
                        if (!isSimulation)
                        {
                            takemoneypanel.Visible = true;
                            othercashpanel.Visible = false;
                            System.Timers.Timer timer = new System.Timers.Timer(2500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(showTakeCash);
                            timer.Enabled = true;
                        }
                    }
                }
                //Checks if teh main panel is visible/active
                if (mainpanel.Visible)
                {
                    //Checks which screen side button was pressed
                    switch (TI.value)
                    {
                            //Sets the cash without receipt panel to visible/active
                        case 0:
                            {

                                mainpanel.Visible = false;
                                cashpanel.Visible = true;

                                break;
                            }
                            //Sets the balance without receipt panel to visible/active
                        case 1:
                            {
                                mainpanel.Visible = false;
                                balancepanel.Visible = true;
                                balancelabel.Text = currentAccount.getBalance().ToString();
                                break;
                            }
                            //Sets the cash with receipt panel to visible/active
                        case 3:
                            {
                                receiptNeeded = true;
                                mainpanel.Visible = false;
                                cashpanel.Visible = true;

                                break;
                            }
                            //Sets the balance with receipt panel to visible/active
                        case 4:
                            {
                                receiptNeeded = true;
                                mainpanel.Visible = false;
                                balancepanel.Visible = true;
                                balancelabel.Text = currentAccount.getBalance().ToString();
                                receiptOutAnimation();
                                break;
                            }
                            //Sets pin panel to visible/active
                        case 5:
                            {
                                mainpanel.Visible = false;
                                currentpinpanel.Visible = true;

                                break;
                            }

                    }
                }
                    //Checks if the show balance panel is visible
                else if (balancepanel.Visible)
                {
                    switch (TI.value)
                    {
                        //Case when users presses yes when he is asked is he wants to do another operation
                        case 4:
                            {

                                balancepanel.Visible = false;
                                mainpanel.Visible = true;
                                receiptNeeded = false;
                                break;
                            }
                            //Case when users is pressing NO when he is asked if he want to do an other operation 
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

      
        //Method used to how the data race and the fix using semaphores
        private void sleepTest(object sender, EventArgs e, int cashToTakeAmount)
        {
            //Checks if this is the first call of the method
            if (!simulating)
            {
                //Checks if this is the first call of the method
                if (!currentAccount.simulationStarted)
                {
                    currentAccount.simulationStarted = true;
                    simulating = true;
                }
                //Waits for the 2nd ATM
                else
                {
                    currentAccount.simulationStarted = false;
                }
            }
            //If both ATM reached the withdraw operation it will trigger the code and it will show the data race without fix and with the fix depand son which simulation is selected
            if (!currentAccount.simulationStarted)
            {
                ((System.Timers.Timer)sender).Enabled = false;

                if (isFixed)
                {
                    Form1.semaphore.WaitOne();
                    
                }
                if (currentAccount.getBalance() < cashToTakeAmount)
                {
                    try
                    {
                        EndCashBad.Invoke(new MethodInvoker(delegate
                        {

                            EndCashBad.Visible = true;
                            badsimulationpanel.Visible = false;
                            goodsimulationpanel.Visible = false;
                            System.Timers.Timer timer2 = new System.Timers.Timer(1500);
                            timer2.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                            timer2.Enabled = true;
                        }));
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    int tempBalance = currentAccount.getBalance();
                    Thread.Sleep(2500);

                    tempBalance -= cashToTakeAmount;
                    currentAccount.setBalance(tempBalance);
                    try
                    {
                        simulatingpanel.Invoke(new MethodInvoker(delegate
                        {
                            simulatingpanel.Visible = true;
                            badsimulationpanel.Visible = false;
                            goodsimulationpanel.Visible = false;
                            simulating = false;

                            Console.WriteLine(this.Text + ": " + currentAccount.getBalance());

                            bank.logInfo(this.Text + ": user " + accnumber.Text + " has just withdrawn " + cashToTakeAmount + "$ from their account.");
                            if (isFixed) bank.logInfo(this.Text + ": user " + accnumber.Text + " new balance: " + currentAccount.getBalance() + "$");
                            cashOutAnimation();
                            moneyTaken = false;
                            if (receiptNeeded) receiptOutAnimation();
                            System.Timers.Timer timer = new System.Timers.Timer(2500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(showTakeCash);
                            timer.Enabled = true;


                        }));
                    }
                    catch (Exception ex)
                    {

                    }

                    if (isFixed) Form1.semaphore.Release();
                    else
                    {
                        Thread.Sleep(1000);
                        bank.logInfo(this.Text + ": user " + accnumber.Text + " new balance: " + currentAccount.getBalance() + "$");

                    }
                }
            }
            
            
        }
        //Method handeling the press event on a keypad button
        private void keypadButtonsDown(object sender, EventArgs e)
        {
            //Creates a variable to hold the clicked button
            PictureBox pb = (PictureBox)sender;
            //Creates a variable to hold the tag informations of the buttons to make the interaction with the tag easier
            TagInfo TI = (TagInfo)pb.Tag;
            int val = getImageNumber(TI.value);
            pb.Image = keysPressed[val];
            if (controlsEnabled)
            {
                //Checks if the user pressed the cancel button tp return to the login in panel
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
                    //Checks if the user pressed a numerical key
                    if (TI.value >= 0 && TI.value <= 9)
                    {
                        pinEntered += TI.value.ToString();
                        currentpinlabel.Text += "*";

                    }
                    //Checks if the user pressed the clear button to remove a character from the pin
                    if (TI.value == -2)
                    {
                        currentpinlabel.Text = currentpinlabel.Text.Substring(0, currentpinlabel.Text.Length - 1);
                        pinEntered = pinEntered.Substring(0, pinEntered.Length - 1);

                    }
                    //Checks if a full pin was entered
                    if (currentpinlabel.Text == "****")
                    {
                        //Checks if the pin is correct
                        if (currentAccount.checkPin(Int32.Parse(pinEntered)))
                        {
                            System.Timers.Timer timer = new System.Timers.Timer(500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(showNewPinScreen);
                            timer.Enabled = true;


                        }
                            //Triggers when the pin entered is incorrect
                        else
                        {
                            retriesRemaining--;
                            currentpinlabel.Text = "";
                            pinEntered = "";
                            errorpinlabel.Visible = true;
                            if (retriesRemaining > 0)
                                errorpinlabel.Text = "INCORRECT! (" + retriesRemaining + " RETRIES REMAINING)";
                                //Triggers when the pin entered is incorrect and the user used all his tries
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
                    //Checks if the confirm pin panel of the change pin operation is visible
                else if (newpinpanel.Visible)
                {
                    //Checks if the user pressed a numerical key
                    if (TI.value >= 0 && TI.value <= 9)
                    {
                        pinEntered += TI.value.ToString();
                        newpinlabel.Text += "*";

                    }
                    //Checks if the user pressed the clear button to remove a character from the pin
                    if (TI.value == -2)
                    {
                        newpinlabel.Text = newpinlabel.Text.Substring(0, newpinlabel.Text.Length - 1);
                        pinEntered = pinEntered.Substring(0, pinEntered.Length - 1);

                    }
                    //Checks if a full pin was entered
                    if (newpinlabel.Text == "****")
                    {
                        controlsEnabled = false;
                        currentAccount.setPin(Int32.Parse(pinEntered));
                        successlabel.Visible = true;
                        System.Timers.Timer timer = new System.Timers.Timer(1500);
                        timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                        timer.Enabled = true;

                    }

                }
                    //Checks if the other option of the cash panel is active/visible
                else if (othercashpanel.Visible)
                {
                    //Checks if the key pad key pressed is enter
                    if (TI.value == -3)
                    {
                        //Checks if there was a value entered in the other cash operation
                        if (cashamountlabel.Text != "")
                        {
                            //Checks if the users has enough money is his back account
                            if (currentAccount.getBalance() >= Int32.Parse(cashamountlabel.Text))
                            {
                                //Normal ATM withdraw(with out a data race)
                                if (!isSimulation)
                                {
                                    currentAccount.decrementBalance(Int32.Parse(cashamountlabel.Text));
                                    cashOutAnimation();
                                    moneyTaken = false;
                                    bank.logInfo(this.Text + ": user " + accnumber.Text + " has just withdrawn " + Int32.Parse(cashamountlabel.Text) + "$ from their account.");
                                    bank.logInfo(this.Text + ": user " + accnumber.Text + " new balance: " + currentAccount.getBalance() + "$");
                                    if (receiptNeeded) receiptOutAnimation();
                                    EndCash.Visible = true;
                                    othercashpanel.Visible = false;
                                }
                                else if (!isFixed)
                                {
                                    othercashpanel.Visible = false;
                                    badsimulationpanel.Visible = true;
                                    System.Timers.Timer t = new System.Timers.Timer(500);
                                    t.Elapsed += (sender1, ev) => sleepTest(sender1, ev, Int32.Parse(cashamountlabel.Text));
                                    t.Enabled = true;

                                }
                                else
                                {
                                    othercashpanel.Visible = false;
                                    goodsimulationpanel.Visible = true;
                                    System.Timers.Timer t = new System.Timers.Timer(500);
                                    t.Elapsed += (sender1, ev) => sleepTest(sender1, ev, Int32.Parse(cashamountlabel.Text));
                                    t.Enabled = true;
                                }
                                

                            }
                                //TRiggers when the users does not have enough money
                            else
                            {
                                EndCashBad.Visible = true;
                                othercashpanel.Visible = false;
                                System.Timers.Timer timer1 = new System.Timers.Timer(1500);
                                timer1.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                                timer1.Enabled = true;
                            }
                            //Return to the initial panel
                            if (!isSimulation)
                            {
                                takemoneypanel.Visible = true;
                                othercashpanel.Visible = false;
                                System.Timers.Timer timer1 = new System.Timers.Timer(2500);
                                timer1.Elapsed += new System.Timers.ElapsedEventHandler(showTakeCash);
                                timer1.Enabled = true;
                                
                            }
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
                    //Checks if a full pin was not entered and if the user is still pressing numerical keys the triggered code will add the key to the pin string
                    if (pinlabel.Text != "****" && TI.value >= 0 && TI.value <= 9)
                    {

                        pinlabel.Text += "*";
                        pinEntered += TI.value.ToString();
                    }
                    //Checks if the user pressed the clear button to remove a charater from the pin
                    if (TI.value == -2)
                    {
                        pinlabel.Text = pinlabel.Text.Substring(0, pinlabel.Text.Length - 1);
                        pinEntered = pinEntered.Substring(0, pinEntered.Length - 1);

                    }
                    //Checks if a full pin was entered
                    if (pinlabel.Text == "****")
                    {
                        //Checks if the pin is correct and it it is then the triggered code will give the user acces to the acount
                        if (currentAccount.checkPin(Int32.Parse(pinEntered)))
                        {

                            System.Timers.Timer timer = new System.Timers.Timer(500);
                            timer.Elapsed += new System.Timers.ElapsedEventHandler(showMainScreen);
                            timer.Enabled = true;
                            controlsEnabled = false;
                            bank.logInfo(this.Text + ": Successful login for account: " + accnumber.Text);
                        }
                            //Triggered when the pin is incorrect
                        else
                        {
                            retriesRemaining--;
                            pinlabel.Text = "";
                            pinEntered = "";
                            errorlabel.Visible = true;
                            if (retriesRemaining > 0)
                                errorlabel.Text = "INCORRECT! (" + retriesRemaining + " RETRIES REMAINING)";
                                //Triggered when the pin is incorrect and the user used his last try
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
        //Method used to display the main screen
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
        private void showTakeCash(object sender, EventArgs e)
        {
            try
            {
                takemoneypanel.Invoke(new MethodInvoker(delegate
                {
                    takemoneypanel.Visible = true;
                    EndCash.Visible = false;
                    goodsimulationpanel.Visible = false;
                    badsimulationpanel.Visible = false;
                    simulatingpanel.Visible = false;
                    System.Timers.Timer timer = new System.Timers.Timer(3000);
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(returnToMainScreen);
                    timer.Enabled = true;
                }));
            }
            catch (Exception ex)
            {

            }
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
                    takemoneypanel.Visible = false;
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
                    simulatingpanel.Visible = false;
                    
                    bank.logInfo(this.Text + ": user " + accnumber.Text + " has just ended their session.");
                    cashamountlabel.Text = "";
                    newpinlabel.Text = "";
                    currentpinlabel.Text = "";
                    pinlabel.Text = "";
                    pinEntered = "";
                    accnumber.Text = "";
                    loginlabel.Text = "Please insert your card ((insert the account number))";
                    retriesRemaining = 3;
                    if (!moneyTaken)
                    {
                        cashInAnimation();
                        moneyTaken = true;
                    }
                    
                }));
            }

            ((System.Timers.Timer)sender).Enabled = false;



        }
        private void keypadButtonsUp(object sender, EventArgs e)
        {
            //Creates a variable to hold the clicked button
            PictureBox pb = (PictureBox)sender;
            //Creates a variable to hold the tag informations of the buttons to make the interaction with the tag easier
            TagInfo TI = (TagInfo)pb.Tag;
            int val = getImageNumber(TI.value);
            pb.Image = keysReleased[val];
        }
        //Method used for triggering the pull cash animation
        private void cashInAnimation()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(cashAnimationRevers);
            timer.Enabled = true;


        }


        //This method is cycling through the animation images to create the animation
        private void cashAnimationRevers(object sender, System.EventArgs e)
        {
            var timer = (System.Windows.Forms.Timer)sender;
            Image image = cashOutIMG[cashNumberRevers];
            pictureBox3.BackgroundImage = image;
            cashNumberRevers--;
            if (cashNumberRevers == 0)
            {
                pictureBox3.BackgroundImage = Properties.Resources.nocash;
                timer.Stop();
                cashNumberRevers = 6;
            }
        }
        //Method used for triggering the cash out animation
        private void cashOutAnimation()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 50;
            timer.Tick += new EventHandler(cashAnimation);
            timer.Start();


        }

        //Method used for triggering the receipt out animation
        private void receiptOutAnimation()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += new EventHandler(receiptAnimation);
            timer.Start();


        }

        //This method is cycling through the animation images to create the animation
        private void cashAnimation(object sender, System.EventArgs e)
        {
            var timer = (System.Windows.Forms.Timer)sender;
            Image image = cashOutIMG[cashNumber];

            pictureBox3.BackgroundImage = image;
            cashNumber++;
            if (cashNumber == 7)
            {
                timer.Stop();
                cashNumber = 0;
            }
        }
        //This method is cycling through the animation images to create the animation
        private void receiptAnimation(object sender, System.EventArgs e)
        {
            var timer = (System.Windows.Forms.Timer)sender;
            Image image = receiptOutIMG[receiptNumber - 1];
            pictureBox4.BackgroundImage = image;
            receiptNumber++;
            if (receiptNumber == 6)
            {
                receiptNumber = 1;
                timer.Stop();
            }
        }

        //Method used to detect which button was pressed
        private int getImageNumber(int TIValue)
        {
            int decreaser = 1;

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
        //Method used to handle clickes on the button1
        private void button1_Click(object sender, EventArgs e)
        {
            //Checks if the text field is empty
            if (accnumber.Text != "")
            {
                //Try to parse the text field to an int to see if the account number is correct
                try
                {

                    int input = Int32.Parse(accnumber.Text);
                    //looks for the account
                    currentAccount = findAccount(input);
                    //Checks if the account exists
                    //If it exists the the system will display the login system(pin panel)
                    if (currentAccount != null)
                    {
                        loginpanel.Visible = false;
                        pinpanel.Visible = true;
                        accnumber.Enabled = false;
                        button1.Enabled = false;

                    }
                        //Triggered when the account is not in the system
                    else
                    {
                        loginlabel.Text = "The card could not be read ((invalid account number))";
                        accnumber.Text = "";
                    }
                }
                    //Triggered if the entered string can not be parsed to an integer
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
        //Method which handels the clicks on pictureBox3(cash withdraw port)
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (!moneyTaken) { 
                pictureBox3.BackgroundImage = Properties.Resources.nocash;
                moneyTaken = true;
                
            }
        }
        //Method which handels the clicks on pictureBox4
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            pictureBox4.BackgroundImage = Properties.Resources.noend;

        }
        //Method which handels the clicks on label43
        private void label43_Click(object sender, EventArgs e)
        {

        }
        //Handler for clicking the exit button
        private void ATM_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Turn the ATM off
            bank.turnOffATM();
        }

    }


    //Tag class which contains all the tag elements of a GUI element
    public class TagInfo
    {
        public string name;
        public int value;
    }
}
