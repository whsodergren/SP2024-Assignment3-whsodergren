namespace SP2024_Assignment3_whsodergren.Models
{
    public class Helper
    {
        public static string GetSentimentDescription(double sentiment)
        {
            if (sentiment >= 0.8)
                return "Very Positive";
            else if (sentiment >= 0.6)
                return "Positive";
            else if (sentiment >= 0.4)
                return "Slightly Positive";
            else if (sentiment >= -0.2 && sentiment <= 0.2)
                return "Neutral";
            else if (sentiment >= -0.4)
                return "Slightly Negative";
            else if (sentiment >= -0.6)
                return "Negative";
            else
                return "Very Negative";
        }
    }
}
