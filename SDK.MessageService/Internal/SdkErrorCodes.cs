namespace SDK.Service
{
    public class SdkErrorCodes
    {
        private static SdkErrorCodes _instance;

        public static SdkErrorCodes Instanece => _instance ?? (_instance = new SdkErrorCodes());

        public int HTTP_REQUREST_TIMEOUT = 10000;
    }
}
