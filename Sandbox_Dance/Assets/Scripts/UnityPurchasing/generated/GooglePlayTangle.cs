// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("45a9KnXHV0gUDR4Tj8FBpNt6MKRu3F98blNYV3TYFtipU19fX1teXU72q/bXZBzmyKAJhuI/4K6mC3h7GOXnqxK+MEK/7xctsmU1Mq4F/iNOE+5Vk/EUFouy1NcOWIwZQg1RYE5NuAxtnrSetmBB4GtN1ooxCtboHACmdgfBPxQe40FRQt3zMWupBiqJ1mWNRFbxEFdrcZbsoIm2mCIJ6Mw+wMIrXB1yf/4gPY9huzbi00+o7NqsVS3Bi/0dr/vXiyADiR8u+tSLWldPJExU4Fat+FyDLoqlJKdJ04CjYdY7AM7Zpx+c3r5sbb6Jt2Al3F9RXm7cX1Rc3F9fXuqRGiTA6Bw/WSQdAdPxO9NJALShGeerPfILrxrV2IVvGBb8eVxdX15f");
        private static int[] order = new int[] { 13,13,11,12,11,10,11,11,12,9,11,11,13,13,14 };
        private static int key = 94;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
