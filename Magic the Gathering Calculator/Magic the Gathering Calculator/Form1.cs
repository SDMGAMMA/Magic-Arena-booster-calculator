using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_the_Gathering_Calculator
{
    public partial class frmMagicTheGatheringCalculator : Form
    {
        // Text boxes in the designer
        private TextBox[,] txtCraftingBoxes;
        private TextBox[,] txtCollectionBoxes;
        private TextBox[] txtWildCardsBoxes;
        private TextBox[] txtCraftableBoxes;
        private TextBox[] txtCostBoxes;
        private TextBox[] txtTotalBoxes;

        // Calculation data which keeps track of the results
        private double[,] cardCopiesOwned;

        // Data from the cards that are in a set
        private int[] cardsInSet;

        // Cards opening from booster packs
        private double[] cardsOpened;
        private double[] cardsOpenedInPack;
        private int totalPacksOpened;

        // Wild cards
        private double[] wildCardChances;
        private double[] wildCards;

        // The crafting cost
        private double[] cost;

        // Other
        private double vaultPoints;
        private RoundingMethods roundValues = RoundingMethods.All;

        // Developer variable
        private int lazyIndex = 0;

        /// <summary>
        /// Initialize values
        /// </summary>
        public frmMagicTheGatheringCalculator()
        {
            InitializeComponent();

            InitializeValues();
            ResetCollection();
        }

        /// <summary>
        /// Initialize all the important variables
        /// </summary>
        private void InitializeValues()
        {
            // Set indexes
            cmbSet.SelectedIndex = 0;
            cmbBooster.SelectedIndex = 0;

            // Assign arrays
            wildCardChances = new double[] { 1.0 / 3.0, 0.2, 1.0 / 30.0, 1.0 / 30.0 };
            cost = new double[4];

            // Assign textboxes
            txtCollectionBoxes = new TextBox[,]
            {
                { txtCommonOne, txtCommonTwo, txtCommonThree, txtCommonFour },
                { txtUncommonOne, txtUncommonTwo, txtUncommonThree, txtUncommonFour },
                { txtRareOne, txtRaretwo, txtRareThree, txtRareFour },
                { txtMythicOne, txtMythicTwo, txtMythicThree, txtMythicFour }
            };

            txtCraftingBoxes = new TextBox[,]
            {
                { txtCraftCommonOne, txtCraftCommonTwo, txtCraftCommonThree, txtCraftCommonFour },
                { txtCraftUncommonOne, txtCraftUncommonTwo, txtCraftUncommonThree, txtCraftUncommonFour },
                { txtCraftRareOne, txtCraftRareTwo, txtCraftRareThree, txtCraftRareFour },
                { txtCraftMythicOne, txtCraftMythicTwo, txtCraftMythicThree, txtCraftMythicFour }
            };

            txtWildCardsBoxes = new TextBox[] { txtWildCommon, txtWildUncommon, txtWildRare, txtWildMythic };
            txtCraftableBoxes = new TextBox[] { txtCraftableCommon, txtCraftableUncommon, txtCraftableRare, txtCraftableMythic };
            txtTotalBoxes = new TextBox[] { txtTotalCommon, txtTotalUncommon, txtTotalRare, txtTotalMythic };
            txtCostBoxes = new TextBox[] { txtCostCommon, txtCostUncommon, txtCostRare, txtCostMythic };

            btnRoundValues.Text = $"Round values: {roundValues}";
        }

        /// <summary>
        /// Reset calculation data
        /// </summary>
        private void ResetCollection()
        {
            // Reset calculation data.
            cardCopiesOwned = new double[4, 4];
            wildCards = new double[4];
            cardsOpened = new double[4];
            totalPacksOpened = 0;
            vaultPoints = 0;

            for (int i = 0; i < txtCollectionBoxes.GetLength(0); i++)
            {
                for (int j = 0; j < txtCollectionBoxes.GetLength(1); j++)
                {
                    txtCollectionBoxes[i, j].Text = "0";
                }
            }

            // Limit values
            DetermineSet();
            LimitValues();
        }

        private void CalculateButton(object sender, EventArgs e)
        {
            // Limit values
            DetermineSet();
            LimitValues();

            // Open pack
            OpenPack();
        }

        /// <summary>
        /// Open a booster pack and add cards to the collection
        /// </summary>
        private void OpenPack()
        {
            // Look, idk how to make this right without a try catch, I feel like this can be better
            try
            {
                int.Parse(txtCardPacksOpened.Text);
            }
            catch (Exception)
            {
                return;
            }

            // Reset cost and add packs to the total
            cost = new double[4];
            totalPacksOpened += int.Parse(txtCardPacksOpened.Text);
            txtTotalPacksOpened.Text = totalPacksOpened.ToString();

            // Set values based on the set index
            DetermineSet();

            // Wild cards opened based on how many packs are opened.
            wildCards[0] = 0;                                               // You get no common wild cards in this category.
            wildCards[1] = (totalPacksOpened + 3) / 6;                      // Every 6 packs you get a uncommon wild card. You get the first one after 3 packs.
            wildCards[2] = totalPacksOpened / 6 - totalPacksOpened / 30;    // Every 6 packs you get a rare wild card. Every 30 packs is replaced by a mythic.
            wildCards[3] = totalPacksOpened / 30;                           // Every 30 packs you get a mythic wild card.

            // Wild cards opened based on random chance. Update text boxes
            for (int i = 0; i < wildCards.Length; i++)
            {
                wildCards[i] += wildCardChances[i] * totalPacksOpened;
                txtWildCardsBoxes[i].Text = RoundValue(roundValues != RoundingMethods.None, wildCards[i]).ToString();
            }

            for (int i = 0; i < cardCopiesOwned.GetLength(0); i++)
            {
                double total = 0;

                // If the cards opened in the rare and mythic rarities exceeds the amount you can open, 
                if (i >= 2 && cardsInSet[i] * cardsInSet.Length < cardsOpenedInPack[i] * totalPacksOpened)
                {
                    for (int j = 0; j < cardCopiesOwned.GetLength(1); j++)
                    {
                        cardCopiesOwned[i, j] = cardsInSet[i];
                    }
                }
                else
                {
                    for (int j = 0; j < int.Parse(txtCardPacksOpened.Text); j++)
                    {
                        for (int k = 0; k < (int)cardsOpenedInPack[i]; k++)
                        {
                            // Add a card to the collection
                            AddFullCard(i, 1);
                        }
                        // Add a partial card to the collection
                        AddFullCard(i, cardsOpenedInPack[i] % 1);
                    }
                }

                for (int j = 0; j < cardCopiesOwned.GetLength(1); j++)
                {
                    // Output results.
                    txtCollectionBoxes[i, j].Text = RoundValue(roundValues == RoundingMethods.All, cardCopiesOwned[i, j]).ToString();
                    
                    // Calculate total cards opened and cost.
                    total += RoundValue(roundValues == RoundingMethods.All, cardCopiesOwned[i, j]);
                    cost[i] += (cardsInSet[i] - cardCopiesOwned[i, j]) / cardsInSet[i] * int.Parse(txtCraftingBoxes[i, j].Text);
                }
                // Output results.
                txtTotalBoxes[i].Text = RoundValue(roundValues != RoundingMethods.None, total).ToString();
                txtCraftableBoxes[i].Text = wildCards[i] >= cost[i] ? "Yes" : "No";
                txtCostBoxes[i].Text = RoundValue(roundValues != RoundingMethods.None, cost[i]).ToString();
            }
            // Output vault results.
            txtVault.Text = RoundValue(roundValues != RoundingMethods.None, vaultPoints).ToString();
        }

        /// <summary>
        /// Set values based on the set index
        /// </summary>
        private void DetermineSet()
        {
            switch (cmbSet.SelectedIndex)
            {
                case (0): // Phyrexia: All will be One
                    cardsInSet = new int[] { 101, 80, 60, 20 };
                    if (cmbBooster.SelectedIndex == 0)
                    {
                        cardsOpenedInPack = new double[] { 4 + 2.0 / 3.0, 1.8, 173.0 / 210.0, 23.0 / 210.0 };
                    }
                    break;
                case (1): // The Brother's War
                    cardsInSet = new int[] { 101, 80, 63, 23 };
                    if (cmbBooster.SelectedIndex == 0)
                    {
                        cardsOpenedInPack = new double[] { 4 + 2.0 / 3.0, 1.8, 167.2 / 204.0, 23.2 / 204.0 };
                    }
                    break;
                case (2): // Domenatia United
                    cardsInSet = new int[] { 101, 80, 60, 20 };
                    if (cmbBooster.SelectedIndex == 0)
                    {
                        cardsOpenedInPack = new double[] { 4 + 2.0 / 3.0, 1.8, 173.0 / 210.0, 23.0 / 210.0 };
                    }
                    break;
                case (3): // Streets of New Capenna
                    cardsInSet = new int[] { 101, 80, 60, 20 };
                    if (cmbBooster.SelectedIndex == 0)
                    {
                        cardsOpenedInPack = new double[] { 4 + 2.0 / 3.0, 1.8, 173.0 / 210.0, 23.0 / 210.0 };
                    }
                    break;
                case (4): // Kamigawa: Neon Dynasty
                    cardsInSet = new int[] { 117, 88, 59, 18 };
                    if (cmbBooster.SelectedIndex == 0)
                    {
                        cardsOpenedInPack = new double[] { 4 + 2.0 / 3.0, 1.8, 202.0 / 240.0, 22.0 / 240.0 };
                    }
                    break;
            }

            if (cmbBooster.SelectedIndex == 1)
            {
                cardsOpenedInPack = new double[] { 4 + 2.0 / 3.0, 1.8, 0, 14.0 / 15.0 };
            }
        }

        /// <summary>
        /// Calculation to add a card to the collection. Card percentage opened is usually 1, but never higher.
        /// </summary>
        /// <param name="rarityIndex"></param>
        /// <param name="cardPercentageOpened"></param>
        private void AddFullCard(int rarityIndex, double cardPercentageOpened)
        {
            // Calculation variables.
            double card = cardPercentageOpened;
            double previousPercentage;

            for (int i = 0; i < cardCopiesOwned.GetLength(1); i++)
            {
                // Remember value.
                previousPercentage = card;
                
                // Commons and uncommons don't have duplicate protection (rarities 0 and 1 respectively).
                if (rarityIndex <= 1)
                {
                    // This calculates the chance on opening a new card for a specific duplicate amount. For example, if you have only 1 card and there are a 100 cards in a set, the calculation will look like this where card is 1:
                    // card = (100 - 1) / 100 * 1 ~ ~ ~ card = 0.99 
                    // card represents the chance to open a card for a specific duplicate amount. It starts at 1 (100%), but decreases as it is harder to collect multiple duplicates of a single card
                    card = (cardsInSet[rarityIndex] - cardCopiesOwned[rarityIndex, i]) / cardsInSet[rarityIndex] * card;
                }
                else
                {
                    // Simular to the if statement above, but the calculation adapts by substracting values to force the calculation to always add a full card (this is duplicate protection).
                    card = ((cardsInSet[rarityIndex] - cardCopiesOwned[rarityIndex, 3]) - (cardCopiesOwned[rarityIndex, i] - cardCopiesOwned[rarityIndex, 3])) / (cardsInSet[rarityIndex] - cardCopiesOwned[rarityIndex, 3]) * card;
                }

                // Add card to data.
                cardCopiesOwned[rarityIndex, i] += card;

                // Calculate the chance for the chance of opening a card for the next duplicate amount.
                card = previousPercentage - card;
            }
            
            // Vault points added
            switch (rarityIndex)
            {
                case (0):
                    vaultPoints += card;
                    break;
                case (1):
                    vaultPoints += card * 3;
                    break;
            }
        }

        /// <summary>
        /// Reset values and open packs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCalculationButton(object sender, EventArgs e)
        {
            ResetCollection();
            OpenPack();
        }

        /// <summary>
        /// Calculate the best purcase with a deck given as input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BestPurcase(object sender, EventArgs e)
        {
            calculationTimer.Start();
        }

        /// <summary>
        /// Calculate the best purcase with a deck given as input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void BestPurcaseCalculation(object sender, EventArgs e)
        {
            // Normal packs are the cheapest. To know what the best normal and mythic booster ratio is, this value needs to be calculated first.
            int highestNormalPackAmount;

            // Outcome values.
            int bestNormal;
            int bestMythic;

            // Other variables.
            float bestCost;
            int currentPackAmount = 0;

            // Start with normal packs.
            cmbBooster.SelectedIndex = 0;

            ResetCollection();

            // Open packs untill you can craft a deck.
            do
            {
                txtCardPacksOpened.Text = 1.ToString();
                currentPackAmount++;

                OpenPack();
            }
            while (!Craftable());

            // Assign values
            highestNormalPackAmount = currentPackAmount;
            txtNormal.Text = currentPackAmount.ToString();

            bestCost = highestNormalPackAmount * 10;
            bestNormal = highestNormalPackAmount;
            bestMythic = 0;

            // Check if any mythics are being used to craft the deck (magic number 0 because if index [3, 0] isn't 0, the other indexes are redundant)
            // Skip the calculation of mythic boosters if the if statement is false. When not opening mythic rares, mythic boosters are only more expensive and thus worse.
            if (int.Parse(txtCraftingBoxes[3, 0].Text) != 0)
            {
                for (int i = highestNormalPackAmount - 1; i >= 0; i--)
                {
                    // Update progress bar
                    pgbProgress.Value = (int)((double)(highestNormalPackAmount - i) / highestNormalPackAmount * 100);

                    // Reset values
                    ResetCollection();
                    currentPackAmount = 0;

                    // Open normal packs
                    for (int j = 0; j < i; j++)
                    {
                        cmbBooster.SelectedIndex = 0;
                        OpenPack();
                    }

                    // Set index to open mythic boosters
                    cmbBooster.SelectedIndex = 1;

                    // open mythic boosters
                    do
                    {
                        txtCardPacksOpened.Text = 1.ToString();
                        currentPackAmount++;

                        OpenPack();
                    }
                    while (!Craftable());

                    // Check the better price
                    if (bestCost >= i * 10 + currentPackAmount * 13)
                    {
                        // Check if the price is the same
                        txtDebug.Text = bestCost == i * 10 + currentPackAmount * 13 ? "Not the only option!" : "";

                        // Assign values
                        bestCost = i * 10 + currentPackAmount * 13;
                        bestNormal = i;
                        bestMythic = currentPackAmount;
                    }
                }

                // Reset values and input the calculated values.
                ResetCollection();

                cmbBooster.SelectedIndex = 0;
                for (int i = 0; i < bestNormal; i++)
                {
                    OpenPack();
                }

                cmbBooster.SelectedIndex = 1;
                for (int i = 0; i < bestMythic; i++)
                {
                    OpenPack();
                }
            }

            // Assign values
            txtNormal.Text = bestNormal.ToString();
            txtMythic.Text = bestMythic.ToString();

            // Progress bar complete
            pgbProgress.Value = 100;
        }

        /// <summary>
        /// Return false or true if the crafting inputs are craftable.
        /// </summary>
        /// <returns></returns>
        private bool Craftable()
        {
            for (int i = 0; i < wildCards.Length; i++)
            {
                if (wildCards[i] < cost[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Cycle rounding method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundValuesCheck(object sender, EventArgs e)
        {
            switch (roundValues)
            {
                case (RoundingMethods.None):
                    roundValues = RoundingMethods.All;
                    break;
                case (RoundingMethods.Partial):
                    roundValues = RoundingMethods.None;
                    break;
                case (RoundingMethods.All):
                    roundValues = RoundingMethods.Partial;
                    break;
            }
            btnRoundValues.Text = $"Round values: {roundValues}";

            RoundAllValues();
        }

        /// <summary>
        /// Reduce values back to their max number
        /// </summary>
        private void LimitValues()
        {
            double lowestAmount;

            for (int i = 0; i < txtCraftingBoxes.GetLength(0); i++)
            {
                for (int j = 0; j < txtCraftingBoxes.GetLength(1); j++)
                {
                    lowestAmount = Math.Min(int.Parse(txtCraftingBoxes[i, j].Text), cardsInSet[i]);
                    
                    if (j > 0)
                    {
                        lowestAmount = Math.Min(lowestAmount, int.Parse(txtCraftingBoxes[i, j - 1].Text));
                    }
                    
                    txtCraftingBoxes[i, j].Text = lowestAmount.ToString();
                }
            }
        }

        /// <summary>
        /// Attempt to round all values.
        /// </summary>
        private void RoundAllValues()
        {
            // Card copies owned. Round these values only when all is selected.
            for (int i = 0; i < cardCopiesOwned.GetLength(0); i++)
            {
                for (int j = 0; j < cardCopiesOwned.GetLength(1); j++)
                {
                    txtCollectionBoxes[i, j].Text = RoundValue(roundValues == RoundingMethods.All, cardCopiesOwned[i, j]).ToString();
                }
            }
            // Wild cards. Round these values when all or partial is selected.
            for (int i = 0; i < wildCards.Length; i++)
            {
                txtWildCardsBoxes[i].Text = RoundValue(roundValues != RoundingMethods.None, wildCards[i]).ToString();
            }
            // Total card copies owned. Round these values when all is selected.
            for (int i = 0; i < cardCopiesOwned.GetLength(0); i++)
            {
                double total = 0;
                for (int j = 0; j < cardCopiesOwned.GetLength(1); j++)
                {
                    total += RoundValue(roundValues == RoundingMethods.All, cardCopiesOwned[i, j]);
                }
                txtTotalBoxes[i].Text = RoundValue(roundValues != RoundingMethods.None, total).ToString();
            }
            // Cost values. Round these values when all or partial is selected.
            for (int i = 0; i < txtCostBoxes.Length; i++)
            {
                txtCostBoxes[i].Text = RoundValue(roundValues != RoundingMethods.None, cost[i]).ToString();
            }
            // Vault values. Round this values when all or partial is selected.
            txtVault.Text = RoundValue(roundValues != RoundingMethods.None, vaultPoints).ToString();
        }

        /// <summary>
        /// Round a specific value.
        /// </summary>
        /// <param name="roundValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private double RoundValue(bool roundValue, double value)
        {
            return roundValue ? Math.Round(value) : value;
        }

        /// <summary>
        /// Personal developer function, don't mind these hardcoded values.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LazyButton(object sender, EventArgs e)
        {
            txtCraftingBoxes = new TextBox[,]
            {
                { txtCraftCommonOne, txtCraftCommonTwo, txtCraftCommonThree, txtCraftCommonFour },
                { txtCraftUncommonOne, txtCraftUncommonTwo, txtCraftUncommonThree, txtCraftUncommonFour },
                { txtCraftRareOne, txtCraftRareTwo, txtCraftRareThree, txtCraftRareFour },
                { txtCraftMythicOne, txtCraftMythicTwo, txtCraftMythicThree, txtCraftMythicFour }
            };

            int[,] data = new int[,]
            {
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 0, 0 }
            };

            switch (lazyIndex)
            {
                case (0):
                    cmbSet.SelectedIndex = 0;
                    data = new int[,]
                    {
                        { 4, 1, 1, 1 },
                        { 6, 6, 3, 2 },
                        { 3, 3, 2, 1 },
                        { 0, 0, 0, 0 }
                    };
                    break;
                case (1):
                    cmbSet.SelectedIndex = 2;
                    data = new int[,]
                    {
                        { 0, 0, 0, 0 },
                        { 0, 0, 0, 0 },
                        { 2, 1, 0, 0 },
                        { 0, 0, 0, 0 }
                    };
                    break;
                case (2):
                    cmbSet.SelectedIndex = 4;
                    data = new int[,]
                    {
                        { 1, 1, 1, 1 },
                        { 1, 1, 1, 1 },
                        { 2, 0, 0, 0 },
                        { 0, 0, 0, 0 }
                    };
                    break;
            }
            lazyIndex = lazyIndex == 2 ? 0 : lazyIndex + 1;


            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    txtCraftingBoxes[i, j].Text = data[i, j].ToString();
                }
            }
        }
    }

    /// <summary>
    /// Rounding methods
    /// </summary>
    public enum RoundingMethods
    {
        None,
        Partial,
        All
    }
}
