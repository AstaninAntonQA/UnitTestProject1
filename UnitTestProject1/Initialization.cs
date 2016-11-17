using System;
using System.IO;

namespace Initialization
{
    public class UserData
    {
        public int TestType;
        public int GoodsCount;
        public string Login;
        public string Pass;
        public int paytype;
        public string Promo;
        public string Comment;
        public string OrderNumber;
        public int adress;
        public int final_number;
        public string DAdress;
        public int category;
        public int random;
        public int CatCount;
        public char[] Mcat;

        public void Initialization()
        {
            string[] TestingData = File.ReadAllLines(@"D://Test.txt");
            TestType = Convert.ToInt16(TestingData[1]);
            GoodsCount = Convert.ToInt16(TestingData[3]);
            Login = TestingData[5];
            Pass = TestingData[7];
            Promo = TestingData[9];
            Comment = TestingData[11];
            adress = Convert.ToInt16(TestingData[13]);
            paytype = Convert.ToInt16(TestingData[15]);
            category = Convert.ToInt16(TestingData[17]);
            random = Convert.ToInt16(TestingData[19]);
            CatCount = Convert.ToInt16(TestingData[21]);
            Mcat = TestingData[17].ToCharArray(0, TestingData[17].Length);
        }
    }
}
