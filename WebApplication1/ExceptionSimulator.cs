namespace WebApplication1
{
    public class ExceptionSimulator
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);
       
        public static void Trigger(string message, double failureChance = 0.2) {
            if (random.NextDouble() <= failureChance) { 
                throw new Exception(message);
            }
        }
    }
}
