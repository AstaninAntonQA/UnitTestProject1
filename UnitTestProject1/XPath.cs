namespace XPath
{
    public class mainpage
    {
        public string OrderNumberPath = "//div[@id='center']/div/div[2]/div[2]/div";
        public string Categories(int i)
        {
            string[] names = new string[8];
            names[0] = "Продукты";
            names[1] = "Товары для дома";
            names[2] = "Товары для детей";
            names[3] = "Товары для животных";
            names[4] = "Спортивное питание";
            names[5] = "Все для спорта";
            names[6] = "Товары для отдыха";
            names[7] = "Одежда и обувь";
            return names[i];
        }
    }



    public class catalog
    {
        public string CartXPath = "basketDesktop";
        public string GoodFromCatalog(int i)
        {
            string Good = "//div[@id='sort']/div[" + i + "]/div/div/div[5]";
            return Good;
        }
    }



    public class cart
    {
        public string DeliverAdressPath(int adress)
        {
            adress++;
            string Adress = "//div[@id='delivery-type-and-address-select-block']/div/div[" + adress + "]/label/span";
            return Adress;
        }



        public string PayTypePath(int type)
        {
            string Type = "//div[@class='payments']/div/div[" + type + "]/label/span";
            return Type;
        }



        public string PromocodePath = "//div[@id='basket-page-promo-code']/div[1]/div[2]/div/input";
        public string CommentPath = "//div[@id='basket-page-promo-code']/div[2]/textarea";
        public string CartOrderPath = "testOrder";
        public string FinalOrderPath = "//div[@class='modal-body']/div[5]/div[2]/div";
    }
}
