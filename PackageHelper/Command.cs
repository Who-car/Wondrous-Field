namespace PackageHelper
{
    public static class Command
    {
        public static readonly byte[] Join = { 0x4A, 0x4F, 0x49, 0x4E };           //[JOIN]
        public static readonly byte[] Bye = { 0x42, 0x59, 0x45, 0x20 };            //[BYE ]
        public static readonly byte[] CreateSession = { 0x43, 0x52, 0x53, 0x4E };  //[CRSN]
        public static readonly byte[] NameTheLetter = { 0x53, 0x41, 0x59, 0x4C };  //[SAYL]
        public static readonly byte[] NameTheWord = { 0x53, 0x41, 0x59, 0x57 };    //[SAYW]
        public static readonly byte[] SendMessage = { 0x4D, 0x53, 0x47, 0x20 };    //[MSG ]
        public static readonly byte[] Post = { 0x50, 0x4F, 0x53, 0x54 };           //[POST]
    }
}
