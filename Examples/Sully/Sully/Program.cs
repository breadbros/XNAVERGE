using System;

namespace Sully
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SullyGame game = new SullyGame())
            {
                game.Run();
            }
        }
    }
#endif
}