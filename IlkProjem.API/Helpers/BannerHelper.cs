namespace IlkProjem.API.Helpers;

public static class BannerHelper
{
    public static readonly string Logo = @"
 '||''|.                    '||          |                        
  ||   ||   ....   .. ...    ||  ..     |||    ... ...   ... ...  
  ||'''|.  '' .||   ||  ||   || .'     |  ||    ||'  ||   ||'  || 
  ||    || .|' ||   ||  ||   ||'|.    .''''|.   ||    |   ||    | 
 .||...|'  '|..'|' .||. ||. .||. ||. .|.  .||.  ||...'    ||...'  
                                                ||        ||      
                                               ''''      ''''     ";

    public static void PrintLogo()
    {
        Console.WriteLine(Logo);
    }
}
