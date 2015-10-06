using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

// User Control for 1025 game page

namespace puttgamesWP10
{
    public sealed partial class MXXVPivotItem : UserControl
    {
        private List<CheckBox> checkBoxes = new List<CheckBox>();
        private Dictionary<string, int> values = new Dictionary<string,int>();
        private bool i_full = false;
        private bool ii_full = false;
        private bool iii_full = false;
        private bool iv_full = false;
        private bool v_full = false;
        private bool vi_full = false;

        private int BONUS_I = 10;
        private int BONUS_II = 15;
        private int BONUS_III = 20;
        private int BONUS_IV = 25;
        private int BONUS_V = 30;
        private int BONUS_VI = 35;

        public MXXVPivotItem()
        {
            this.InitializeComponent();

            checkBoxes.Add(i_i);
            checkBoxes.Add(i_ii);
            checkBoxes.Add(i_iii);
            checkBoxes.Add(i_iv);
            checkBoxes.Add(i_v);
            checkBoxes.Add(i_vi);

            checkBoxes.Add(ii_i);
            checkBoxes.Add(ii_ii);
            checkBoxes.Add(ii_iii);
            checkBoxes.Add(ii_iv);
            checkBoxes.Add(ii_v);
            checkBoxes.Add(ii_vi);

            checkBoxes.Add(iii_i);
            checkBoxes.Add(iii_ii);
            checkBoxes.Add(iii_iii);
            checkBoxes.Add(iii_iv);
            checkBoxes.Add(iii_v);
            checkBoxes.Add(iii_vi);

            checkBoxes.Add(iv_i);
            checkBoxes.Add(iv_ii);
            checkBoxes.Add(iv_iii);
            checkBoxes.Add(iv_iv);
            checkBoxes.Add(iv_v);
            checkBoxes.Add(iv_vi);

            checkBoxes.Add(v_i);
            checkBoxes.Add(v_ii);
            checkBoxes.Add(v_iii);
            checkBoxes.Add(v_iv);
            checkBoxes.Add(v_v);
            checkBoxes.Add(v_vi);

            checkBoxes.Add(vi_i);
            checkBoxes.Add(vi_ii);
            checkBoxes.Add(vi_iii);
            checkBoxes.Add(vi_iv);
            checkBoxes.Add(vi_v);
            checkBoxes.Add(vi_vi);

            values.Add("i_i", 15);
            values.Add("i_ii", 10);
            values.Add("i_iii", 10);
            values.Add("i_iv", 10);
            values.Add("i_v", 10);
            values.Add("i_vi", 15);

            values.Add("ii_i", 20);
            values.Add("ii_ii", 15);
            values.Add("ii_iii", 15);
            values.Add("ii_iv", 15);
            values.Add("ii_v", 15);
            values.Add("ii_vi", 20);

            values.Add("iii_i", 25);
            values.Add("iii_ii", 20);
            values.Add("iii_iii", 20);
            values.Add("iii_iv", 20);
            values.Add("iii_v", 20);
            values.Add("iii_vi", 25);

            values.Add("iv_i", 30);
            values.Add("iv_ii", 25);
            values.Add("iv_iii", 25);
            values.Add("iv_iv", 25);
            values.Add("iv_v", 25);
            values.Add("iv_vi", 30);

            values.Add("v_i", 40);
            values.Add("v_ii", 30);
            values.Add("v_iii", 30);
            values.Add("v_iv", 30);
            values.Add("v_v", 30);
            values.Add("v_vi", 40);

            values.Add("vi_i", 45);
            values.Add("vi_ii", 35);
            values.Add("vi_iii", 35);
            values.Add("vi_iv", 35);
            values.Add("vi_v", 35);
            values.Add("vi_vi", 45);
        }

        private void checkbox_Clicked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            TextBlock tb = new TextBlock();
            string preName = chkbox.Name.Substring(0, chkbox.Name.IndexOf("_"));

            // check the row
            if ( preName == "i")
            {
                tb = total_i;
            }
            else if (preName == "ii")
            {
                tb = total_ii;
            }
            else if (preName == "iii")
            {
                tb = total_iii;
            }
            else if (preName == "iv")
            {
                tb = total_iv;
            }
            else if (preName == "v")
            {
                tb = total_v;
            }
            else if (preName == "vi")
            {
                tb = total_vi;
            }

            // check whether to add or substract
            if ((bool)chkbox.IsChecked)
            {
                tb.Text = (Convert.ToInt32(tb.Text) + values[chkbox.Name]).ToString();
            }
            else
            {
                tb.Text = (Convert.ToInt32(tb.Text) - values[chkbox.Name]).ToString();
            }

            // check if there is a full row
            // i
            if (!i_full && (bool)i_i.IsChecked && (bool)i_ii.IsChecked && (bool)i_iii.IsChecked && (bool)i_iv.IsChecked && (bool)i_v.IsChecked && (bool)i_vi.IsChecked)
            {
                i_full = true;
                total_i.Text = (Convert.ToInt32(total_i.Text) + BONUS_I).ToString();
            }
            else if (i_full && (!(bool)i_i.IsChecked || !(bool)i_ii.IsChecked || !(bool)i_iii.IsChecked || !(bool)i_iv.IsChecked || !(bool)i_v.IsChecked || !(bool)i_vi.IsChecked))
            {
                i_full = false;
                total_i.Text = (Convert.ToInt32(total_i.Text) - BONUS_I).ToString();
            }

            // ii
            if (!ii_full && (bool)ii_i.IsChecked && (bool)ii_ii.IsChecked && (bool)ii_iii.IsChecked && (bool)ii_iv.IsChecked && (bool)ii_v.IsChecked && (bool)ii_vi.IsChecked)
            {
                ii_full = true;
                total_ii.Text = (Convert.ToInt32(total_ii.Text) + BONUS_II).ToString();
            }
            else if (ii_full && (!(bool)ii_i.IsChecked || !(bool)ii_ii.IsChecked || !(bool)ii_iii.IsChecked || !(bool)ii_iv.IsChecked || !(bool)ii_v.IsChecked || !(bool)ii_vi.IsChecked))
            {
                ii_full = false;
                total_ii.Text = (Convert.ToInt32(total_ii.Text) - BONUS_II).ToString();
            }

            // iii
            if (!iii_full && (bool)iii_i.IsChecked && (bool)iii_ii.IsChecked && (bool)iii_iii.IsChecked && (bool)iii_iv.IsChecked && (bool)iii_v.IsChecked && (bool)iii_vi.IsChecked)
            {
                iii_full = true;
                total_iii.Text = (Convert.ToInt32(total_iii.Text) + BONUS_III).ToString();
            }
            else if (iii_full && (!(bool)iii_i.IsChecked || !(bool)iii_ii.IsChecked || !(bool)iii_iii.IsChecked || !(bool)iii_iv.IsChecked || !(bool)iii_v.IsChecked || !(bool)iii_vi.IsChecked))
            {
                iii_full = false;
                total_iii.Text = (Convert.ToInt32(total_iii.Text) - BONUS_III).ToString();
            }

            // iv
            if (!iv_full && (bool)iv_i.IsChecked && (bool)iv_ii.IsChecked && (bool)iv_iii.IsChecked && (bool)iv_iv.IsChecked && (bool)iv_v.IsChecked && (bool)iv_vi.IsChecked)
            {
                iv_full = true;
                total_iv.Text = (Convert.ToInt32(total_iv.Text) + BONUS_IV).ToString();
            }
            else if (iv_full && (!(bool)iv_i.IsChecked || !(bool)iv_ii.IsChecked || !(bool)iv_iii.IsChecked || !(bool)iv_iv.IsChecked || !(bool)iv_v.IsChecked || !(bool)iv_vi.IsChecked))
            {
                iv_full = false;
                total_iv.Text = (Convert.ToInt32(total_iv.Text) - BONUS_IV).ToString();
            }
            // v
            if (!v_full && (bool)v_i.IsChecked && (bool)v_ii.IsChecked && (bool)v_iii.IsChecked && (bool)v_iv.IsChecked && (bool)v_v.IsChecked && (bool)v_vi.IsChecked)
            {
                v_full = true;
                total_v.Text = (Convert.ToInt32(total_v.Text) + BONUS_V).ToString();
            }
            else if (v_full && (!(bool)v_i.IsChecked || !(bool)v_ii.IsChecked || !(bool)v_iii.IsChecked || !(bool)v_iv.IsChecked || !(bool)v_v.IsChecked || !(bool)v_vi.IsChecked))
            {
                v_full = false;
                total_v.Text = (Convert.ToInt32(total_v.Text) - BONUS_V).ToString();
            }
            // vi
            if (!vi_full && (bool)vi_i.IsChecked && (bool)vi_ii.IsChecked && (bool)vi_iii.IsChecked && (bool)vi_iv.IsChecked && (bool)vi_v.IsChecked && (bool)vi_vi.IsChecked)
            {
                vi_full = true;
                total_vi.Text = (Convert.ToInt32(total_vi.Text) + BONUS_VI).ToString();
            }
            else if (vi_full && (!(bool)vi_i.IsChecked || !(bool)vi_ii.IsChecked || !(bool)vi_iii.IsChecked || !(bool)vi_iv.IsChecked || !(bool)vi_v.IsChecked || !(bool)vi_vi.IsChecked))
            {
                vi_full = false;
                total_vi.Text = (Convert.ToInt32(total_vi.Text) - BONUS_VI).ToString();
            }

            // update grand total
            grandTotal.Text = (Convert.ToInt32(total_i.Text) +
                               Convert.ToInt32(total_ii.Text) +
                               Convert.ToInt32(total_iii.Text) +
                               Convert.ToInt32(total_iv.Text) +
                               Convert.ToInt32(total_v.Text) +
                               Convert.ToInt32(total_vi.Text)).ToString();
        }

        private void CheckAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TextBlock s = sender as TextBlock;
            string preName = s.Name.Substring(0, s.Name.IndexOf("_"));
            CheckBox cOne = new CheckBox();
            CheckBox cTwo = new CheckBox();
            CheckBox cThree = new CheckBox();
            CheckBox cFour = new CheckBox();
            CheckBox cFive = new CheckBox();
            CheckBox cSix = new CheckBox();

            if (preName == "i")
            {
                cOne = i_i;
                cTwo = i_ii;
                cThree = i_iii;
                cFour = i_iv;
                cFive = i_v;
                cSix = i_vi;
            }
            else if (preName == "ii")
            {
                cOne = ii_i;
                cTwo = ii_ii;
                cThree = ii_iii;
                cFour = ii_iv;
                cFive = ii_v;
                cSix = ii_vi;
            }
            else if (preName == "iii")
            {
                cOne = iii_i;
                cTwo = iii_ii;
                cThree = iii_iii;
                cFour = iii_iv;
                cFive = iii_v;
                cSix = iii_vi;
            }
            else if (preName == "iv")
            {
                cOne = iv_i;
                cTwo = iv_ii;
                cThree = iv_iii;
                cFour = iv_iv;
                cFive = iv_v;
                cSix = iv_vi;
            }
            else if (preName == "v")
            {
                cOne = v_i;
                cTwo = v_ii;
                cThree = v_iii;
                cFour = v_iv;
                cFive = v_v;
                cSix = v_vi;
            }
            else if (preName == "vi")
            {
                cOne = vi_i;
                cTwo = vi_ii;
                cThree = vi_iii;
                cFour = vi_iv;
                cFive = vi_v;
                cSix = vi_vi;
            }
            // check if all boxes are checked, if not check them all, if yes, uncheck all
            if ((bool)cOne.IsChecked && (bool)cTwo.IsChecked && (bool)cThree.IsChecked && (bool)cFour.IsChecked && (bool)cFive.IsChecked && (bool)cSix.IsChecked)
            {
                cOne.IsChecked = false;
                cTwo.IsChecked = false;
                cThree.IsChecked = false;
                cFour.IsChecked = false;
                cFive.IsChecked = false;
                cSix.IsChecked = false;
            }
            else
            {
                if (!(bool)cOne.IsChecked) { cOne.IsChecked = true; }
                if (!(bool)cTwo.IsChecked) { cTwo.IsChecked = true; }
                if (!(bool)cThree.IsChecked) { cThree.IsChecked = true; }
                if (!(bool)cFour.IsChecked) { cFour.IsChecked = true; }
                if (!(bool)cFive.IsChecked) { cFive.IsChecked = true; }
                if (!(bool)cSix.IsChecked) { cSix.IsChecked = true; }
            }

        }

        public int getScore()
        {
            return Convert.ToInt32(grandTotal.Text);
        }

        public string getState()
        {
            string state = "";
            for (int i = 0; i < checkBoxes.Count; ++i)
            {
                state += checkBoxes[i].IsChecked.ToString() + ";";
            }
            return state;
        }
        public void setState(string state)
        {

            for (int i = 0; i < checkBoxes.Count; ++i)
            {
                string boxState = state.Substring(0, state.IndexOf(";"));
                checkBoxes[i].IsChecked = bool.Parse(boxState);
                state = state.Substring(state.IndexOf(";") + 1);
            }

        }
    }
}
