namespace TaylorBot.Net.Core.Tasks
{
    public class OnceRunner
    {
        private readonly object lockObject = new object();
        private bool isHandled = false;

        public bool AllowOnce()
        {
            if (!isHandled)
            {
                lock (lockObject)
                {
                    if (!isHandled)
                    {
                        isHandled = true;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
