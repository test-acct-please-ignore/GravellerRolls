using System.Diagnostics;

class GravellerRolls
{
    #region Constants
    const int TOTAL_ROLLS = 1000000000;

    #endregion

    #region Fields

    UInt64 CoreCount = 0;

    UInt64[] HighestRollsPerThread;

    Task<UInt64>[] TaskList;

    #endregion

    public GravellerRolls()
    {
        CoreCount = (UInt64) Environment.ProcessorCount - 1;

        if (CoreCount == 0 ) CoreCount = 1;

        HighestRollsPerThread = new UInt64[CoreCount];

        TaskList = new Task<UInt64>[CoreCount];
    }

    /// <summary>
    /// Calculation function ran by each thread participating in the rolling shenanagins.
    /// </summary>
    /// <param name="rolls"></param>
    /// <returns></returns>
    private static UInt64 RollThread(UInt64 rolls)
    {
        UInt64 highestRolls = 0;
        Random random = new();

        // Start rolls
        for (UInt64 i = 0; i < rolls; i++)
        {   
            // Roll a d4 die, 231 times.   
            UInt64 paralyzeRolls = RollD4(231);

            if (paralyzeRolls > highestRolls)
                highestRolls = paralyzeRolls;
        }

        return highestRolls;
    }

    /// <summary>
    /// Rolls a d4 some number of times, and returns the number of '1's rolled.
    /// </summary>
    /// <param name="rolls">How many times the d4 is rolled.</param>
    /// <returns></returns>
    private static UInt64 RollD4(int rolls)
    {
        Random random = new();
        UInt64[] counts = [0,0,0,0];

        for (int i = 0; i < rolls; i++)
        {
            // Rolls the d4, incrementing the corresponding count
            counts[random.Next(4)]++;
        }

        return counts[0];
    }

    /// <summary>
    /// Does the work, obviously.
    /// </summary>
    /// <returns></returns>
    private UInt64 DoTheWork()
    {
        // Determine how many rolls should be allocated per thread.
        UInt64 totalRollsPerThread = TOTAL_ROLLS / CoreCount;
        UInt64 remainderRolls = TOTAL_ROLLS % CoreCount;

        // Create the tasks and start them.
        for (UInt64 i = 0; i < CoreCount; i++)
        {            
            if (i == 0) // Include remainder rolls in first Task.
            {
                Task<UInt64> task = Task.Run<UInt64>(() =>
                {
                    return RollThread(totalRollsPerThread + remainderRolls);
                });

                TaskList[i] = task;
            }
            else // Other threads have it slightly easier, lucky them.
            {
                Task<UInt64> task = Task.Run<UInt64>(() =>
                {
                    return RollThread(totalRollsPerThread);
                });

                TaskList[i] = task;
            }            
        }

        UInt64 comparedHighest = 0;
        
        // Iterate over each thread's result, choosing the highest out of all of them.
        foreach (Task<UInt64> task in TaskList)
        {
            UInt64 result = task.Result;
            if (result > comparedHighest)
                comparedHighest = result;
        }

        return comparedHighest;
    }

    /// <summary>
    /// The main function, duh.
    /// </summary>
    /// <param name="args">Arguments, if for some reason I want to change this to allow passing in a roll count.</param>
    public static void Main(string[] args)
    {
        Console.WriteLine(string.Format("Welcome. Let's roll the dice...{0} times!", TOTAL_ROLLS));        

        GravellerRolls gravellerRolls = new();
        
        Stopwatch watch = new();
        watch.Start();

        UInt64 result = gravellerRolls.DoTheWork();

        watch.Stop();

        //spinFlag = false; // Stop spinner thread.

        Console.WriteLine(string.Format("Calculations completed in {0} seconds! Highest # of '1' Rolls: {1}", watch.ElapsedMilliseconds / 1000, result));

        Console.WriteLine("Press any key to exit...");
        Console.Read();
    }
}
