using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RSA_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Random r = new Random();
        string alfabetRU = " абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ0123456789";
        BigInteger p, q, n, exp, d, func;

        (BigInteger, BigInteger, BigInteger) NOD(BigInteger a, BigInteger b)
        {
            if (b == 0)
                return (a, 1, 0);
            else
            {
                (BigInteger c, BigInteger x, BigInteger y) = NOD(b, a % b);
                return (c, y, x - y * (a / b));
            }
        }

        BigInteger Converter01ToInt(string s)
        {
            BigInteger res = 0;
            for (int i = s.Length - 1, j = 0; i >= 0; --i, ++j)
                if (s[i] == '1')
                    res += BigInteger.Pow(2, j);

            return res;
        }

        string ConverterIntTo01(BigInteger s)
        {
            Stack<int> st = new Stack<int>();
            string res = "";
            while (s > 0)
            {
                st.Push((int)(s % 2));
                s = (s - s % 2) / 2;
            }
            while (st.Count > 0)
                res += st.Pop();
            return res;
        }

        BigInteger RandBigIntByLen(int len) //Возвращает случайное нечетное число
        {
            string res = "1";
            while (res.Length < len)
            {
                res += r.Next(2).ToString();
            }
            var v = Converter01ToInt(res);
            return v % 2 == 0 ? v + 1 : v; //если четное то +1

        }
        BigInteger ExpModN(BigInteger a, BigInteger exp, BigInteger n)
        {
            if (exp == 0)
                return new BigInteger(1);
            string e = ConverterIntTo01(exp);
            BigInteger res = a % n;

            for (int j = 1; j < e.Length; ++j)
            {
                if (e[j] == '1')
                    res = ((res * res) * a) % n;
                else
                    res = (res * res) % n;
            }
            return res;
        }


        bool TestMR(BigInteger R)
        {
            if (R == 2 || R == 3)
                return true;

            if (R < 2 || R % 2 == 0)
                return false;

            //Разложение на s и t
            BigInteger t = R - 1, s = 0;
            while (t % 2 == 0)
            {
                t /= 2;
                s += 1;
            }

            BigInteger a;
            for (int k = 0; k < 20; ++k)
            {
                //Случайное число размера BigInt (подглядел в интернете)
                RNGCryptoServiceProvider r = new RNGCryptoServiceProvider();
                byte[] arrb = new byte[R.ToByteArray().Length];
                do
                {
                    r.GetBytes(arrb);
                    a = new BigInteger(arrb);
                } while (a < 2 || a >= R - 2);

                // далее тест согласно методичке
                if (R % a == 0)
                    return false;

                BigInteger b = ExpModN(a, t, R);

                if (b == 1 || b == R - 1)
                {
                    continue;
                }
                else
                {
                    for (int i = 1; i < s; ++i)
                    {
                        b = ExpModN(b, 2, R);

                        if (b == R - 1)
                            break;
                        if (b == 1)
                            return false;
                    }
                    if (b != R - 1)
                        return false;
                }
            }
            return true;
        }
        BigInteger PrimeGenerator(int len) //длина в битах
        {
            var res = RandBigIntByLen(len);
            while (!TestMR(res))
            {
                res = RandBigIntByLen(len);
            }
            return res;
        }

        void GenerateKeys(int len) //длина в битах
        {
            p = PrimeGenerator(len / 2);
            q = PrimeGenerator(len / 2);
            while (p == q)
                q = PrimeGenerator(len / 2);
            n = p * q;
            func = (p - 1) * (q - 1);
            exp = PrimeGenerator(len / 3);
            while (exp == p || exp == q || func % exp == 0)
                exp = PrimeGenerator(len / 3);
            d = NOD(func, exp).Item3;
            if (d < 0)
                d += func;
        }
       
        string StringToBigint(string txt, string alf)
        {
            string res = "";
            foreach (var a in txt)
            {
                if (!alf.Contains(a))
                {
                    MessageBox.Show("Не все символы можно закодировать");
                    return "";
                }
                res += (alf.IndexOf(a) + 10).ToString();
            }
            return res;
        }


        string BigintToString(BigInteger txt, string alf)
        {
            string res = "";
            string r = txt.ToString();
            int i = 0;
            while (i < r.Length)
            {
                res += alf[(int.Parse((r[i++].ToString() + r[i++].ToString())) - 10) % alf.Length];
            }
            return res;
        }

        string ShifrRSA(BigInteger txt, BigInteger exp, BigInteger n)
        {
            return ExpModN(txt, exp, n).ToString();
        }

        BigInteger DeShifrRSA(BigInteger txt, BigInteger exp, BigInteger d)//если значение txt больше n, то преобразование невозможно
        {
            if (txt >= d)
            {
                throw new Exception();
            }
            return ExpModN(txt, exp, d);
        }

        private void btn_Generate_Click(object sender, RoutedEventArgs e)
        {
            int len;
            if (int.TryParse(txtB_Len.Text,out len) && len < 1100 && len > 100)
            {
                GenerateKeys(len);
                round.ToolTip = $"p = {p}\nq = {q}\nn = {n}\nfunc = {func}\nexp = {exp}\nd = {d}";
                round.Fill = Brushes.Green;
                return;
            }
            round.ToolTip = "";
            round.Fill = Brushes.Red;
            MessageBox.Show("Введите целое число");
        }

        private void btn_Shifr_Click(object sender, RoutedEventArgs e)
        {
            if(round.Fill!= Brushes.Red)
            {
                if (BigInteger.TryParse(StringToBigint(txtB_Orig.Text, alfabetRU), out BigInteger msg))
                {
                    txtB_Shifr.Text = ShifrRSA(msg,exp,n);
                }
                return;
            }
            MessageBox.Show("Сгенерируйте ключ");
        }

        private void btn_DeShifr_Click(object sender, RoutedEventArgs e)
        {
            if(BigInteger.TryParse(txtB_Shifr.Text, out BigInteger msg))
            {
                try
                {
                    txtB_DeShifr.Text = BigintToString(DeShifrRSA(msg, d, n), alfabetRU);
                }
                catch
                {
                    MessageBox.Show("Преобразование невозможно");
                }
            }
        }
    }
}
