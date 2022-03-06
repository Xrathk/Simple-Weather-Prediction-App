namespace Console_GettingAPIData
{
    public static class UtilityMethods
    {
        /// <summary>
        /// Prints a formatted intro for the program.
        /// </summary>
        /// <param name="verbose">Signifies whether the intro should be consise (default) or expanded.</param>
        public static void PrintIntro(bool verbose = false)
        {
            Console.WriteLine("\n==============================================");
            Console.WriteLine("==== Weather Forecast Console Application ====");
            if (verbose)
            {
                Console.WriteLine("==== Author: AK95 ============================");
                Console.WriteLine("==== Copyright: December 2021 © ==============");
            }
            Console.WriteLine("==============================================\n");
        }

    }
}
